TARGET_DEBUG=bin/Debug/netstandard2.1/DataExtract.dll

ALLCODE=Plugin.cs

$(TARGET_DEBUG): DataExtract.csproj $(ALLCODE) $(ALLASSETS)
	dotnet build --configuration Debug
debug: $(TARGET_DEBUG)

all: debug

install: $(TARGET_DEBUG)
	cp $(TARGET_DEBUG) ../BepInEx/plugins

uninstall:
	rm -f ../BepInEx/plugins/DataExtract.dll

clean:
	rm -rf bin obj

.PHONY: all debug install uninstall clean
