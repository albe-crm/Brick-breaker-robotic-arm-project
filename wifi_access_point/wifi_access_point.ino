  /*

    WiFi Web Server

    A simple web server that lets you send/receive msgs via WiFi.
    This sketch will create a new access point (with no password).
    It will then launch a new server and print out the IP address
    to the Serial monitor. 
    You have to create a client application (e.g. with Python) to
    communicate with the Arduino server. See the `wifi_client.py`
    script for an example of client application.

    Adapted from
    https://tinyurl.com/arduino-access-point
    created 25 Nov 2012
    by Tom Igoe

  */

  #include <SPI.h>
  #include <WiFiNINA.h> // install from the library tab in the right bar of the Arduino IDE
  /* IMPORTANT! To run this file you first have to create a `arduino_secrets.h` file.
                To do so, in the Arduino IDE:
                - click on the three dots in the top right corner
                - select 'New Tab' and enter the 'arduino_secrets.h' as file name
                - in 'arduino_secrets.h' type the WiFi SSID (i.e. network name) and password as

                  #define SECRET_SSID "EDUEXON1WIFI" // please adapt the exo number!
                  #define SECRET_PASS "eduexon1"     // please adapt the exo number!
  */
  #include "arduino_secrets.h"
  ///////please enter your sensitive data in the Secret tab/arduino_secrets.h
const char* ssid = "EDUEXON4WIFI";
const char* password = "eduexon4";   // your network password (use for WPA, or use as key for WEP)
  int keyIndex = 0;             // your network key Index number (needed only for WEP)
                                // NOTE. Consider removing.

  int status = WL_IDLE_STATUS;

  WiFiServer server(80);

  void setup() {
    //Initialize serial and wait for port to open:
    Serial.begin(9600);
    /*while (!Serial) {
      ; // wait for serial port to connect. Needed for native USB port only
    }*/

    Serial.println("Access Point Web Server");
    Serial.print("ciao");
    // check for the WiFi module:
    if (WiFi.status() == WL_NO_MODULE) {
      Serial.println("Communication with WiFi module failed!");
      // don't continue
      while (true);
    }

    String fv = WiFi.firmwareVersion();
    if (fv < WIFI_FIRMWARE_LATEST_VERSION) {
      Serial.println("Please upgrade the firmware");
    }

    // by default the local IP address of will be 192.168.4.1
    // you can override it with the following:
    // WiFi.config(IPAddress(10, 0, 0, 1));

    // print the network name (SSID);
    Serial.print("Creating access point named: ");
    Serial.println(ssid);

    // Create open network. Change this line if you want to create an WEP network:
    //WiFi.noLowPowerMode();
    status = WiFi.beginAP(ssid); //, pass);

    if (status != WL_AP_LISTENING) {
      Serial.println("Creating access point failed");
      // don't continue
      while (true);
    }

    // wait 10 seconds for connection:
    delay(10000);
    // start the web server on port 80
    server.begin();
    // you're connected now, so print out the status
    printWiFiStatus();
  }

  void loop() {

    // compare the previous status to the current status
    if (status != WiFi.status()) {
      // it has changed update the variable
      status = WiFi.status();

      if (status == WL_AP_CONNECTED) {
        // a device has connected to the AP
        Serial.println("Device connected to AP");
      } else {
        // a device has disconnected from the AP, and we are back in listening mode
        Serial.println("Device disconnected from AP");
      }
    }

    WiFiClient client = server.available();   // listen for incoming clients

    if (client) {                             // if you get a client,
      Serial.println("new client");           // print a message out the serial port
      
      // You can send an HTTP header (if simulating a web server) or any other message here
      client.println("Hello from Arduino!"); // Send actual data (the message) here

      while (client.connected()) {            // loop while the client's connected
        if (client.available()) {             // if there's bytes to read from the client,
          String request = client.readStringUntil('\r');
          Serial.println(request);
          client.flush();
          client.println("Got it!");
        }
      }

      // close the connection:
      client.stop();
      Serial.println("client disconnected");
    }
  }

  void printWiFiStatus() {
    // Print the SSID of the network you're attached to:
    Serial.print("SSID: ");
    Serial.println(WiFi.SSID());
    // Print your WiFi shield's IP address:
    IPAddress ip = WiFi.localIP();
    Serial.print("IP Address: ");
    Serial.println(ip);
  }