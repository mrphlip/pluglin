TARGET_DEBUG=bin/Debug/netstandard2.1/RelicStats.dll
TARGET_RELEASE=bin/Release/netstandard2.1/RelicStats.dll

ALLCODE=Plugin.cs

$(TARGET_DEBUG): RelicStats.csproj $(ALLCODE)
	dotnet build --configuration Debug
debug: $(TARGET_DEBUG)

$(TARGET_RELEASE): RelicStats.csproj $(ALLCODE)
	dotnet build --configuration Release
release: $(TARGET_RELEASE)

RelicStats.zip: $(TARGET_RELEASE) icon.png README.md CHANGELOG.md manifest.json
	rm -f RelicStats.zip
	zip -j RelicStats.zip $(TARGET_RELEASE) icon.png README.md CHANGELOG.md manifest.json
zip: RelicStats.zip

all: debug release zip

install: $(TARGET_DEBUG)
	cp $(TARGET_DEBUG) ../BepInEx/plugins

instrelease: $(TARGET_RELEASE)
	cp $(TARGET_RELEASE) ../BepInEx/plugins

clean:
	rm -rf bin obj RelicStats.zip

.PHONY: all debug release zip install instrelease clean
