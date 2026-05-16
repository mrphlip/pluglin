TARGET_DEBUG=bin/Debug/netstandard2.1/Forbge.dll
TARGET_DEBUGSYM=bin/Debug/netstandard2.1/Forbge.pdb
TARGET_RELEASE=bin/Release/netstandard2.1/Forbge.dll
TARGET_RELEASESYM=bin/Release/netstandard2.1/Forbge.pdb

ALLCODE=$(wildcard *.cs)
ASSETS=$(wildcard assets/*.png)

$(TARGET_DEBUG) $(TARGET_DEBUGSYM): Forbge.csproj $(ALLCODE) $(ASSETS)
	dotnet build --configuration Debug
debug: $(TARGET_DEBUG)

$(TARGET_RELEASE) $(TARGET_RELEASESYM): Forbge.csproj $(ALLCODE) $(ASSETS)
	dotnet build --configuration Release
release: $(TARGET_RELEASE)

Forbge.zip: $(TARGET_RELEASE) $(TARGET_RELEASESYM) icon.png README.md CHANGELOG.md manifest.json
	rm -f Forbge.zip
	zip -j Forbge.zip $(TARGET_RELEASE) $(TARGET_RELEASESYM) icon.png README.md CHANGELOG.md manifest.json
zip: Forbge.zip

all: debug release zip

install: $(TARGET_DEBUG) $(TARGET_DEBUGSYM)
	mkdir -p ../BepInEx/plugins/mrphlip-Forbge
	cp $(TARGET_DEBUG) $(TARGET_DEBUGSYM) ../BepInEx/plugins/mrphlip-Forbge

instrelease: $(TARGET_RELEASE)
	mkdir -p ../BepInEx/plugins/mrphlip-Forbge
	cp $(TARGET_RELEASE) $(TARGET_RELEASESYM) ../BepInEx/plugins/mrphlip-Forbge

uninstall:
	rm -rf ../BepInEx/plugins/mrphlip-Forbge

clean:
	rm -rf bin obj Forbge.zip

.PHONY: all debug release zip install instrelease uninstall clean
