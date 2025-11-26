#include <WiFiNINA.h>
#include <Servo.h>

Servo myservo;

const int servoPin = 7;
const int sensorPin = A1;

// Sensor calibration
int rawMin = 468;   //  posSensor2
int rawMax = 871;   //  posSensor1

// Servo target positions
int servoUp = 5;     // like posDesired1
int servoDown = 90;  // like posDesired2

char unityCommand = 'O';

const char* ssid     = "albe_hotspotto";
const char* password = "secretpass";

WiFiServer server(80);

void setup() {
  Serial.begin(9600);
  while (!Serial);

  Serial.println("Connecting to WiFi...");
  while (WiFi.begin(ssid, password) != WL_CONNECTED) {
    delay(1000);
    Serial.print(".");
  }
  Serial.println("\nConnected!");

  server.begin();
  Serial.print("Server started at IP: ");
  Serial.println(WiFi.localIP());

  // Initial servo test
  myservo.attach(servoPin);
  myservo.write(10);
  delay(500);
  myservo.write(0);
  delay(500);
  myservo.detach();
}

void loop() {
  WiFiClient client = server.available();

  if (client) {
    Serial.println("Client connected!");

    while (client.connected()) {

      // -------------------------
      // 1. READ SENSOR & SEND ANGLE
      // -------------------------
      int raw = analogRead(sensorPin);
      int angle = map(raw, rawMin, rawMax, 0, 180);
      angle = constrain(angle, 0, 180);

      client.print(angle);
      client.print('\n');

      // Debug
      Serial.print("Sent Angle: ");
      Serial.println(angle);

      // -------------------------
      // 2. RECEIVE UNITY COMMAND
      // -------------------------
      if (client.available()) {
        unityCommand = client.read();
        Serial.print("Received command: ");
        Serial.println(unityCommand);

        handleCommand(unityCommand);
      }

      delay(100);
    }

    client.stop();
    Serial.println("Client disconnected.");
  }
}


// ==========================================
// COMMAND HANDLING (U = up, D = down, O = off)
// ==========================================

void handleCommand(char cmd) {
  switch (cmd) {

    case 'U':    // Move servo to UP position
      if (!myservo.attached()) myservo.attach(servoPin);
      myservo.write(servoUp);
      break;

    case 'D':    // Move servo DOWN
      if (!myservo.attached()) myservo.attach(servoPin);
      myservo.write(servoDown);
      break;

    case 'O':    // Stop assist: detach servo
      if (myservo.attached()) myservo.detach();
      break;
  }
}
