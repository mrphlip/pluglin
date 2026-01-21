TARGET_DEBUG=bin/Debug/netstandard2.1/CustomCruciball.dll
TARGET_RELEASE=bin/Release/netstandard2.1/CustomCruciball.dll

ALLCODE=Plugin.cs Utils.cs CustomUI.cs Assets.cs State.cs Constants.cs
ALLASSETS=assets/unchecked.png assets/checked.png

$(TARGET_DEBUG): CustomCruciball.csproj $(ALLCODE) $(ALLASSETS)
	dotnet build --configuration Debug
debug: $(TARGET_DEBUG)

$(TARGET_RELEASE): CustomCruciball.csproj $(ALLCODE) $(ALLASSETS)
	dotnet build --configuration Release
release: $(TARGET_RELEASE)

CustomCruciball.zip: $(TARGET_RELEASE) icon.png README.md CHANGELOG.md manifest.json
	rm -f CustomCruciball.zip
	zip -j CustomCruciball.zip $(TARGET_RELEASE) icon.png README.md CHANGELOG.md manifest.json
zip: CustomCruciball.zip

all: debug release zip

install: $(TARGET_DEBUG)
	cp $(TARGET_DEBUG) ../BepInEx/plugins

instrelease: $(TARGET_RELEASE)
	cp $(TARGET_RELEASE) ../BepInEx/plugins

clean:
	rm -rf bin obj CustomCruciball.zip

.PHONY: all debug release zip install instrelease clean
