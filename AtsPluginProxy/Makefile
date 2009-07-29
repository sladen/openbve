# Paul Sladen, 2009-07-29

CFLAGS += -shared -fPIC
LDFLAGS += -shared

AtsPluginProxy.so: AtsPluginProxy.o
	$(CC) -o $@ $(LDFLAGS) -ldl $<

%.o: %.cpp
	$(CC) -c -o $@ $(CFLAGS) $<

clean:
	-rm -f AtsPluginProxy.so AtsPluginProxy.o

.PHONY: clean