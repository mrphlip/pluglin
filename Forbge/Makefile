TARGET_DEBUG=bin/Debug/netstandard2.1/Forbge.dll
TARGET_RELEASE=bin/Release/netstandard2.1/Forbge.dll

ALLCODE=Plugin.cs

$(TARGET_DEBUG): Forbge.csproj $(ALLCODE)
	dotnet build --configuration Debug
debug: $(TARGET_DEBUG)

$(TARGET_RELEASE): Forbge.csproj $(ALLCODE)
	dotnet build --configuration Release
release: $(TARGET_RELEASE)

Forbge.zip: $(TARGET_RELEASE) icon.png README.md CHANGELOG.md manifest.json
	rm -f Forbge.zip
	zip -j Forbge.zip $(TARGET_RELEASE) icon.png README.md CHANGELOG.md manifest.json
zip: Forbge.zip

all: debug release zip

install: $(TARGET_DEBUG)
	mkdir -p ../BepInEx/plugins/mrphlip-Forbge
	cp $(TARGET_DEBUG) ../BepInEx/plugins/mrphlip-Forbge

instrelease: $(TARGET_RELEASE)
	mkdir -p ../BepInEx/plugins/mrphlip-Forbge
	cp $(TARGET_RELEASE) ../BepInEx/plugins/mrphlip-Forbge

uninstall:
	rm -rf ../BepInEx/plugins/mrphlip-Forbge

clean:
	rm -rf bin obj Forbge.zip

.PHONY: all debug release zip install instrelease uninstall clean
