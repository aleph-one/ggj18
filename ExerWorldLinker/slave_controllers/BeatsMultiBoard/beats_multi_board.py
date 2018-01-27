#!/usr/bin/env python
# -*- coding: utf-8 -*-

#
"""
Beat's Multi Board, teensy/arduino based
"""

from threading import Thread

################################################### exer.world variables & 'callbacks'
running = False
lastValues = None
lastData = [0,0,0,0,0,0,0,0,0,0,0,0]

thread = None

def setup():
    global running

    print("beats multi board teensy/arduino homebrew is setup and ready")
    running = True
    thread = Thread(target = bootstrap_midi_device)
    thread.start()

def update():
    global lastValues

    values = dict()
    values["pedal_0"] = (lastData[0]*1.0+1)/127.0
    values["pedal_1"] = (lastData[1]*1.0+1)/127.0
    values["pedal_2"] = (lastData[2]*1.0+1)/127.0
    values["pedal_3"] = (lastData[3]*1.0+1)/127.0
    values["pedal_4"] = (lastData[4]*1.0+1)/127.0
    values["pedal_5"] = (lastData[5]*1.0+1)/127.0
    values["pedal_6"] = (lastData[6]*1.0+1)/127.0
    values["pedal_7"] = (lastData[7]*1.0+1)/127.0
    values["pedal_8"] = (lastData[8]*1.0+1)/127.0
    values["pedal_9"] = (lastData[9]*1.0+1)/127.0
    values["pedal_10"] = (lastData[10]*1.0+1)/127.0
    values["pedal_11"] = (lastData[11]*1.0+1)/127.0

    # print ("received values and filled in: " + str(values))
    lastValues = values
    return values

def destroy():
    global running

    print("beats multi board teensy/arduino midi device (beat kunz's homebrew micro) destroy called")

    running = False

    midiin.close_port()
    del midiin

################################################### custom code
def bootstrap_midi_device():

    port = sys.argv[1] if len(sys.argv) > 1 else 0
    try:
        midiin, port_name = open_midiport(port)
    except (EOFError, KeyboardInterrupt):
        sys.exit()

    print("Attaching Teensy/arduino MIDI input callback handler. port_name: " + port_name)

    midiin.set_callback(MidiInputHandler(port_name))

    # TODO: kill thread ...

    print("Entering main loop. Press Control-C to exit.")

    while True:
        time.sleep(0.05)
        update()

    print("Exit.")


#import logging
import sys
import time

from rtmidi.midiutil import open_midiport

#log = logging.getLogger('test_midiin_callback')
#logging.basicConfig(level=logging.DEBUG)

class MidiInputHandler(object):
    def __init__(self, port):
        self.port = port
        self._wallclock = time.time()

    def __call__(self, event, data=None):
        global lastData
        message, deltatime = event
        self._wallclock += deltatime
        # if (message[1] == 2): # ignore noise pedal ;)
        print("[%s] @%0.6f %r" % (self.port, self._wallclock, message))
        lastData[message[1]] = message[2]
        update()


if __name__ == '__main__':
    setup()
