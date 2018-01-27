import sys
import glob
import serial
from random import random
ser = None

# NOTE: just try 'start_random_routine'
# NOTE: currently on port COM15 hard coded

import time

################################################### exer.world variables & 'callbacks'
running = False
lastValues = None

values = dict()


def setup():
    global running
    global values
    running = True
    
    values = dict()
    values["relay1"] = 0
    values["relay2"] = 0
    values["relay3"] = 0
    values["relay4"] = 0
    values["relay5"] = 0
    values["relay6"] = 0
    values["relay7"] = 0
    values["relay8"] = 0
    
    # start_random_routine()

def setValue(name, value):
    global values
    
    # sanitize value. should only be from 0 to 1
    
    value = round(value)
    values[name] = value
    
    applyValues()
    
    
def update():
    global lastValues
    global values

    # values = dict()

    lastValues = values
    return values


def destroy():
    global running
    running = False

    
################################################### custom code

def list_serial_ports():
    global ser
    """ Lists serial port names

        :raises EnvironmentError:
            On unsupported or unknown platforms
        :returns:
            A list of the serial ports available on the system
    """
    print("now listing all serial ports")
    if sys.platform.startswith('win'):
        ports = ['COM%s' % (i + 1) for i in range(256)]
    elif sys.platform.startswith('linux') or sys.platform.startswith('cygwin'):
        # this excludes your current terminal "/dev/tty"
        ports = glob.glob('/dev/tty[A-Za-z]*')
    elif sys.platform.startswith('darwin'):
        ports = glob.glob('/dev/tty.*')
    else:
        raise EnvironmentError('Unsupported platform')

    if ser != None and ser.isOpen():
        ser.close()
        
    result = []
    for port in ports:
        try:
            ser = serial.Serial(port)
            ser.close()
            result.append(port)
        except (OSError, serial.SerialException):
            pass
            
    print("result of open com ports", result)
    return result

def connect_to_com(com_name):
    global ser
    if ser != None and ser.isOpen():
        ser.close()
    
    if ser == None:
        ser = serial.Serial()
    ser.port = com_name
    ser.baudrate = 19200
    if ser.isOpen() == False:
        ser.open()
    
def send_message(data, cmd=3, address=0, com_name = None):
    global ser
    
    if com_name == None:
        result = list_serial_ports()
        com_name = result[0]
    
    print("send_message",data,cmd,address,com_name)
    
    if ser == None or ser.isOpen() == False:
        connect_to_com(com_name)
        
    print("2")
    message = bytearray([cmd,address,data])     # Message already created
    print("3")
    
    lrc = 0
    for b in message:
        lrc ^= b
    message.append(lrc)
    ser.write(message)

def applyValues():
    global values
    data = 0
    data = data + (values["relay1"]*(2**0))
    data = data + (values["relay2"]*(2**1))
    data = data + (values["relay3"]*(2**2))
    data = data + (values["relay4"]*(2**3))
    data = data + (values["relay5"]*(2**4))
    data = data + (values["relay6"]*(2**5))
    data = data + (values["relay7"]*(2**6))
    data = data + (values["relay8"]*(2**7))
    data = int(data)
    send_message(data)
    
    
def randomize(com_name = None, address=0):
    global values
    values["relay1"] = round(random())
    values["relay2"] = round(random())
    values["relay3"] = round(random())
    values["relay4"] = round(random())
    values["relay5"] = round(random())
    values["relay6"] = round(random())
    values["relay7"] = round(random())
    values["relay8"] = round(random())
    applyValues()
    

run_routine = False
def start_random_routine(com_name = None):
    global ser
    
    print("Start random routine")
    run_routine = True
    while run_routine:
        try:
            randomize(com_name)
        except Exception as e:
            print("sth went wrong. try to restart serial port", e)
            ser.close()
            pass
        time.sleep(random()*0.5+0.5)
    
if __name__ == "__main__":
    setup()
