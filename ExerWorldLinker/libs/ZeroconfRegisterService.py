from Zeroconf import *
import socket

desc = {'path':'/~paulsm/','pyLinker':'true'}
info = ServiceInfo("_controller._tcp.local.", "PyLinker._controller._tcp.local.", socket.inet_aton("10.0.1.2"), 80, 0, 0, desc, "ash-2.local.")

r = Zeroconf()
print "Registration of a service..."
r.registerService(info)
print "Waiting..."
