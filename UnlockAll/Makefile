TARGET_DEBUG=bin/Debug/netstandard2.1/UnlockAll.dll

ALLCODE=$(wildcard *.cs)

$(TARGET_DEBUG): UnlockAll.csproj $(ALLCODE) $(ASSETS)
	dotnet build --configuration Debug
debug: $(TARGET_DEBUG)

install: $(TARGET_DEBUG)
	cp $(TARGET_DEBUG) ../BepInEx/plugins

uninstall:
	rm -f ../BepInEx/plugins/UnlockAll.dll

clean:
	rm -rf bin obj

.PHONY: debug install uninstall clean
