'''
Send msg over an available (open) WiFi network.
From: https://medium.com/analytics-vidhya/esp8266-python-connection-in-arduino-based-system-5d4a308bd79b
'''

import time
import socket

ip = '192.168.4.1' # unless you have re-assigned it!
port = 80          # NOTE. Check out this value

conn = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
conn.connect((ip, port))

print('Enter an angle value in the [0 90] range to move the EduExo, or `q` to quit.')
while True:
    ans = input('Desired angle elbow: ')
    if ans == 'q' or ans == 'Q':
        break
    else:
        conn.send(ans.encode())
    time.sleep(0.5) #[s]
    from_server = conn.recv(1024) #[bytes]
    print(from_server.decode())