TARGET_DEBUG=bin/Debug/netstandard2.1/ForbgeSample.dll
TARGET_RELEASE=bin/Release/netstandard2.1/ForbgeSample.dll

ALLCODE=$(wildcard *.cs)
ASSETS=$(wildcard assets/*.png)

$(TARGET_DEBUG): ForbgeSample.csproj $(ALLCODE) $(ASSETS)
	dotnet build --configuration Debug
debug: $(TARGET_DEBUG)

$(TARGET_RELEASE): ForbgeSample.csproj $(ALLCODE) $(ASSETS)
	dotnet build --configuration Release
release: $(TARGET_RELEASE)

ForbgeSample.zip: $(TARGET_RELEASE) icon.png README.md CHANGELOG.md manifest.json
	rm -f ForbgeSample.zip
	zip -j ForbgeSample.zip $(TARGET_RELEASE) icon.png README.md CHANGELOG.md manifest.json
zip: ForbgeSample.zip

all: debug release zip

install: $(TARGET_DEBUG)
	cp $(TARGET_DEBUG) ../BepInEx/plugins

instrelease: $(TARGET_RELEASE)
	cp $(TARGET_RELEASE) ../BepInEx/plugins

uninstall:
	rm -f ../BepInEx/plugins/ForbgeSample.dll

clean:
	rm -rf bin obj ForbgeSample.zip

.PHONY: all debug release zip install instrelease uninstall clean
