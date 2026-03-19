#include <Servo.h>
#include <SPI.h>
#include <WiFiNINA.h>
#include <WiFiUdp.h>

// --- YOUR NETWORK SETTINGS ---
char ssid[] = "albe_hotspotto";     
char pass[] = "pass-word"; 

// --- IP SETTINGS ---
unsigned int localPort = 4242;      // Port Arduino listens on
const char* unityIP = "10.80.184.212"; // REPLACE with your PC's IP Address from commandprompt "ipconfig"
unsigned int unityPort = 4241;      // Port Unity listens on
WiFiUDP udp;

Servo myservo;

// Pins
const int servoOnPin     = 7;
const int PWMPin         = 5;
const int angleSensorPin = A1;
const int fsrPin         = A2;

// Angle calibration (raw 0..1023)  -> mapped to 0..100
int posSensorMin  = 950;   // adjust from your real extremes
int posSensorMax  = 510;   // adjust from your real extremes

// Servo mapping (0..180). Swap min/max if direction is inverted.
int posServoMin = 180;
int posServoMax = 0;
int posOffset   = 0;

// One-shot behavior
const int deadband01 = 10;                 // close enough -> no push
const unsigned long pulseOnMs = 20;        // duration of the single nudge

bool oneShotMode = true;                   // one nudge per new command/target
int gentleOneShotStep01 = 4;               // start small
int strongOneShotStep01 = 6;               // start small

// FSR intent gating
int fsrThreshold = 60;
int fsrFullScale = 400;
float fsrFilterAlpha = 0.2f;

// ============================================================
// B) INTERNAL STATE (DON'T EDIT)
// ============================================================

int currentAngle01 = 50;
int targetAngle01  = 50;
char assistCmd      = '0';

int fsrBaseline = 0;
float fsrFiltered = 0.0f;

bool oneShotDone = false;

// ============================================================
// C) SETUP / LOOP
// ============================================================

void setup() {
  Serial.begin(9600);
  pinMode(servoOnPin, OUTPUT);
  digitalWrite(servoOnPin, HIGH);
  
   while (WiFi.status() != WL_CONNECTED) {
    Serial.print("Connecting to ");
    Serial.println(ssid);
    WiFi.begin(ssid, pass);
    delay(2000);
  }
  Serial.println("Connected to WiFi!");
  printWifiStatus();

  udp.begin(localPort);



  myservo.detach();

  // FSR baseline calibration (RELAX during boot)
  long sum = 0;
  const int N = 50;
  for (int i = 0; i < N; i++) {
    sum += analogRead(fsrPin);
    delay(5);
  }
  fsrBaseline = (int)(sum / N);
  fsrFiltered = (float)fsrBaseline;
}

void loop() {
  // 1) Send current angle to Unity as ONE RAW BYTE (0..100)
  int rawAngle = analogRead(angleSensorPin);

  float t = 0.0f;
  if (posSensorMin != posSensorMax) {
    t = (rawAngle - posSensorMin) / (float)(posSensorMax - posSensorMin);
  }
  t = constrain(t, 0.0f, 1.0f);
  currentAngle01 = (byte)round(t * 100.0f);

  // 2) Send current angle to Unity as ONE RAW BYTE (0..100) via WiFi
  udp.beginPacket(unityIP, unityPort);
  udp.write((byte)currentAngle01);  // Send as single byte
  udp.endPacket();

  // 3) Receive command from Unity via WiFi: 1 or 2 bytes
  //    - 1 byte: assist level only ('0', '1', or '2')
  //    - 2 bytes: assist level + target angle (0-100)
  int packetSize = udp.parsePacket();
  if (packetSize >= 1) {
    char newCmd = (char)udp.read();  // First byte: assist level ('0', '1', or '2')
    
    byte newTarget = targetAngle01;  // Keep current target by default
    if (packetSize >= 2) {
      newTarget = (byte)udp.read();  // Second byte: target angle (0-100)
    }
    
    // Flush any remaining bytes
    while (udp.available() > 0) {
      udp.read();
    }

    // New command or target => allow one new assist nudge
    if (newCmd != assistCmd || newTarget != targetAngle01) {
      assistCmd = newCmd;
      targetAngle01 = newTarget;
      oneShotDone = false;  // Reset to allow new assist action
      
      Serial.print("Assist: ");
      Serial.print(newCmd);
      Serial.print(" Target: ");
      Serial.println(newTarget);
    }
  }

  // 3) Read & filter FSR (force sensor - detects user's physical push)
  int fsrRaw = analogRead(fsrPin);
  fsrFiltered = (1.0f - fsrFilterAlpha) * fsrFiltered + fsrFilterAlpha * (float)fsrRaw;

  // 4) Apply servo assist based on assist level and target angle
  applyAssistOneShotWithFSR();

  delay(10);
}

// ============================================================
// D) HELPERS
// ============================================================

int angle01ToServoCmd(int angle01) {
  angle01 = constrain(angle01, 0, 100);
  int desiredServoPos = map(angle01, 0, 100, posServoMin, posServoMax);
  int servoCmd = desiredServoPos + posOffset;
  return constrain(servoCmd, 0, 180);
}

float computeUserIntent01() {
  float x = fabs(fsrFiltered - (float)fsrBaseline);

  if (x <= (float)fsrThreshold) return 0.0f;

  float den = (float)(fsrFullScale - fsrThreshold);
  if (den < 1.0f) den = 1.0f;

  float u = (x - (float)fsrThreshold) / den;
  if (u < 0.0f) u = 0.0f;
  if (u > 1.0f) u = 1.0f;
  return u;
}

// ============================================================
// E) ONE-SHOT ASSIST (transparent unless actively pulsing)
// ============================================================

void applyAssistOneShotWithFSR() {
  // ALWAYS default to transparent
  if (myservo.attached()) myservo.detach();

  if (assistCmd == '0') return;
  if (oneShotMode && oneShotDone) return;

  // Must be pushing
  float intent01 = computeUserIntent01();
  if (intent01 <= 0.0f) return;

  int error01 = (int)targetAngle01 - (int)currentAngle01;
  int absErr = abs(error01);

  // If already close, count as done
  if (absErr <= deadband01) {
    oneShotDone = true;
    return;
  }

  int step01 = (assistCmd == '1') ? gentleOneShotStep01 : strongOneShotStep01;
  int dir = (error01 > 0) ? 1 : -1;

  int newCmd01 = (int)currentAngle01 + dir * step01;

  // avoid overshoot
  if (dir > 0) newCmd01 = min(newCmd01, (int)targetAngle01);
  else         newCmd01 = max(newCmd01, (int)targetAngle01);

  // ONE pulse
  myservo.attach(PWMPin);
  myservo.write(angle01ToServoCmd(newCmd01));
  delay((int)pulseOnMs);
  myservo.detach();

  oneShotDone = true;      // lock until next new cmd/target
}

void printWifiStatus() {
  Serial.print("SSID: ");
  Serial.println(WiFi.SSID());
  IPAddress ip = WiFi.localIP();
  Serial.print("Arduino IP Address: "); // IMPORTANT: Copy this into Unity!
  Serial.println(ip);
}
