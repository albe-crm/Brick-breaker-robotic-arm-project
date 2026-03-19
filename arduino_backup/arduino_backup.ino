#include <Servo.h>
#include <SPI.h>
#include <WiFiNINA.h>
#include <WiFiUdp.h>

// --- YOUR NETWORK SETTINGS ---
char ssid[] = "albe_hotspotto";     
char pass[] = "pass-word"; 

// --- IP SETTINGS ---
unsigned int localPort = 4242;      // Port Arduino listens on
const char* unityIP = "10.134.27.212"; // REPLACE with your PC's IP Address (you can find it in commandprompt:   ipconfig | findstr /i "IPv4")
unsigned int unityPort = 4241;      // Port Unity listens on

//IMPORTANT: GO TO WINDOWS FIREWALL SETTINGS AND IN "INBOUND RULES" ALLOW UNITY CONNECTION FROM PROPERTIES /////////////////////////
WiFiUDP udp;

Servo myservo;
const int servoOnPin = 7, PWMPin = 5, posOffset = 0,
          forceAnalogInPin = A2, ampliSwitchPin = 8, gainSelectPin = 4,
          buttonPin1 = 3, buttonPin2 = 2,
          angleSensorPin = A1; 

// Angle calibration (raw 0..1023)  -> mapped to 0..100
int posSensorMin  = 760;   // 760 exo4 adjust from your real extremes
int posSensorMax  = 300;   // 300 exo4 adjust from your real extremes



int targetAngle = 50,   // Desired target angle 0-100 (received from Unity)
    assistanceLevel = 1, // 0 = transparent, 1 = low assist, 2 = high assist (start with assist enabled!)
    minAngle = 0, maxAngle = 100,
    forceOffset = 520, forceThreshold = 100, forceDesired = 0; //520 offset for exo4

float forceIs;
float positionDesiredFloat = 0.0;  // Use float to accumulate small changes
int positionDesired = 0;  // Integer version for servo
byte currentAngle = 0;  // Current angle mapped to 0-100 for Unity

// assistanceLevel 0 = fully transparent (detached), 1 = low assist (low gain), 2 = high assist (high gain)
const float admittanceGains[3] = {0.0, 0.01, 0.05};

// Target tracking
byte previousTargetAngle = 0;
bool targetReached = false;
int targetDeadband = 10;  // Deadband around target where exo becomes transparent


void setup(){
    Serial.begin(9600);
    while(!Serial);

    // Connect to WiFi
    while (WiFi.status() != WL_CONNECTED) {
    Serial.print("Connecting to ");
    Serial.println(ssid);
    WiFi.begin(ssid, pass);
    delay(1000);
    }
    Serial.println("Connected to WiFi!");
    printWifiStatus();

    delay(5000);//time to copy ip

    udp.begin(localPort);

    // Security mechanism (buttons to power up/down)
    pinMode(buttonPin1, INPUT_PULLUP);
    pinMode(buttonPin2, INPUT_PULLUP);
    attachInterrupt(digitalPinToInterrupt(buttonPin1), PowerDown, FALLING);
    attachInterrupt(digitalPinToInterrupt(buttonPin2), PowerUp, FALLING);
    pinMode(gainSelectPin, OUTPUT);
    digitalWrite(gainSelectPin, LOW);
    pinMode(ampliSwitchPin, OUTPUT);
    digitalWrite(ampliSwitchPin, HIGH);
    pinMode(servoOnPin, OUTPUT);
    digitalWrite(servoOnPin, LOW);
}

void loop(){

    int rawAngle = analogRead(angleSensorPin);

    float t = 0.0f;
    if (posSensorMin != posSensorMax) {
    t = (rawAngle - posSensorMin) / (float)(posSensorMax - posSensorMin);
    }
    t = constrain(t, 0.0f, 1.0f);
    currentAngle = (byte)round(t * 100.0f);

    // 2) Send current angle to Unity as ONE RAW BYTE (0..100) via WiFi
    udp.beginPacket(unityIP, unityPort);
    udp.print(currentAngle);  // Send as single byte //use udp.print if doesn't work (also change unity code to accept it as ascii and not byte)
    udp.endPacket();

    // 3) Receive command from Unity via WiFi: 1 or 2 bytes
    //    - 1 byte: assist level ('0', '1', or '2' as ASCII char)
    //    - 2 bytes: assist level + target angle (0-100 as raw byte)
    int packetSize = udp.parsePacket();
    if (packetSize >= 1) {
    char assistChar = (char)udp.read();  // First byte: assist level ('0', '1', or '2')

    // Convert ASCII char to int (e.g., '0'->0, '1'->1, '2'->2)
    int newAssistLevel = assistChar - '0';
    if (newAssistLevel >= 0 && newAssistLevel <= 2) {
        assistanceLevel = newAssistLevel;
    }

    if (packetSize >= 2) {
        byte newTargetAngle = (byte)udp.read();  // Second byte: target angle (0-100)
        if (newTargetAngle != previousTargetAngle) {
            targetAngle = newTargetAngle;
            previousTargetAngle = newTargetAngle;
            targetReached = false;  // Reset flag when target changes
            
            // Initialize positionDesired to current position so we start from here
            positionDesiredFloat = (float)currentAngle;
            positionDesired = currentAngle;
        }
    }

    // Flush any remaining bytes
    while (udp.available() > 0) {
        udp.read();
    }

    // Serial.print("Received - Assist: ");
    // Serial.print(assistanceLevel);
    // Serial.print(" Target: ");
    // Serial.println(targetAngle);
    }

    // Get admittance gain based on current assistance level
    float admittanceGain = admittanceGains[constrain(assistanceLevel, 0, 2)];

    int forceRaw = analogRead(forceAnalogInPin);
    forceIs = (float)(forceRaw - forceOffset);


    // Check if we've reached the target (only set once, not continuously)
    if (!targetReached && abs(targetAngle - currentAngle) <= targetDeadband) {
        targetReached = true;
        positionDesiredFloat = (float)targetAngle;  // Snap to target, not oscillating currentAngle
        positionDesired = targetAngle;
    }

    // Only allow movement if target not reached (or target changed)
    if (!targetReached && abs(forceIs) > forceThreshold) {

        // Ensure positionDesiredFloat never lags behind currentAngle
        // This must happen BEFORE calculating the delta
        if (targetAngle > currentAngle && positionDesiredFloat < currentAngle) {
            positionDesiredFloat = (float)currentAngle;
        } else if (targetAngle < currentAngle && positionDesiredFloat > currentAngle) {
            positionDesiredFloat = (float)currentAngle;
        }

        float previousPosition = positionDesiredFloat;
        
        // Calculate delta and clamp to ±5 max
        float delta = admittanceGain * (forceIs);
        delta = constrain(delta, -5.0f, 5.0f);
        
        float newPositionDesired = positionDesiredFloat + delta;
        
        // Clamp newPositionDesired to valid range before checking direction
        newPositionDesired = constrain(newPositionDesired, (float)minAngle, (float)maxAngle);

        // Only update position if moving CLOSER to target (simpler logic)
        bool movingTowardsTarget = abs(newPositionDesired - targetAngle) < abs(previousPosition - targetAngle);

        // Debug: show calculation
        // Serial.print(" rawF=");
        // Serial.print(forceRaw);
        // Serial.print(",");
        Serial.print(" force=");
        Serial.print(forceIs);
        Serial.print(",");
        // Serial.print("gain=");
        // Serial.print(admittanceGain, 4);
        // Serial.print(",");
        Serial.print(" assist?");
        Serial.print(movingTowardsTarget);
        Serial.print(",");
        Serial.print(" delta=");
        Serial.print(delta, 4);
        Serial.print(",");
        Serial.print(" CurAng=");
        Serial.print(currentAngle);
        Serial.print(",");
        Serial.print(" PosDes=");
        Serial.println(newPositionDesired);
        // Serial.print(",");
        // Serial.print("forceIs:");
        // Serial.println(forceIs);



        if (movingTowardsTarget) {
            positionDesiredFloat = newPositionDesired;

            // Don't overshoot target
            if (previousPosition < targetAngle && positionDesiredFloat > targetAngle) {
                positionDesiredFloat = targetAngle;
            } else if (previousPosition > targetAngle && positionDesiredFloat < targetAngle) {
                positionDesiredFloat = targetAngle;
            }

            // Apply min/max limits
            positionDesiredFloat = constrain(positionDesiredFloat, (float)minAngle, (float)maxAngle);
            positionDesired = (int)round(positionDesiredFloat);

            myservo.attach(PWMPin);
            // Servo is velocity-based: 90 = neutral, >90 = push up, <90 = push down
            // Map delta (-5 to +5) to servo range centered on 90
            int servoCommand = 90 + (int)(delta * 18.0f);  // delta * 18 maps ±5 to ±90
            servoCommand = constrain(servoCommand, 0, 180);
            myservo.write(servoCommand);
            
            // Debug: see what's actually being sent
            Serial.print(" SERVO=");
            Serial.println(servoCommand);
        } else {
            myservo.detach();  // Not moving towards target, detach
        }

    } else { myservo.detach(); }


    //debug info
    // Serial.print("target:");
    // Serial.print(targetAngle);
    // Serial.print(",");
    // Serial.print("target reached?:");
    // Serial.print(targetReached);
    // Serial.print(",");

    //for calibration
    // Serial.print(" rawF=");
    // Serial.print(forceRaw);
    // Serial.print(",");
    // Serial.print("angle raw?:");
    // Serial.println(rawAngle);

    //Serial.print(",");
    // Serial.print("position des:");
    // Serial.print(positionDesired);
    // Serial.print(",");
    // Serial.print("forceIs:");
    // Serial.print(forceIs);
    // Serial.print(",");
    // Serial.print("angle");
    // Serial.println(currentAngle);
    delay(50);

    
}

void PowerUp() {
digitalWrite(servoOnPin, HIGH);
}
void PowerDown() {
digitalWrite(servoOnPin, LOW);
}


void printWifiStatus() {
  Serial.print("SSID: ");
  Serial.println(WiFi.SSID());
  IPAddress ip = WiFi.localIP();
  Serial.print("Arduino IP Copy this into UNITY: "); // IMPORTANT: Copy this into Unity!
  Serial.println(ip);
}