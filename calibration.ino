/* Arduino program to read the position sensor.*/

const int servoOnPin = 7, servoAnalogInPin = A1;
int posIs,posIsDeg,posServo1 = 0, posServo2 = 90,
 posSensor1 = 871, posSensor2 = 468;

void setup(){
Serial.begin(9600);
while(!Serial);
pinMode(servoOnPin, OUTPUT);
digitalWrite(servoOnPin, HIGH);
}


void loop(){
posIs = analogRead(servoAnalogInPin);
 posIsDeg = map(posIs, posSensor1, posSensor2, posServo1, posServo2);
 Serial.print("Position[deg]:");
 Serial.println(posIsDeg);
 delay(100);
 }
