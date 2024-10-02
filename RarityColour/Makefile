TARGET_DEBUG=bin/Debug/netstandard2.1/RarityColour.dll
TARGET_RELEASE=bin/Release/netstandard2.1/RarityColour.dll

ALLCODE=Plugin.cs
ALLASSETS=assets assets/ItemBackground1.png assets/ItemBackground2.png assets/ItemBackground3.png assets/ItemBackgroundSold.png assets/ShopBackground.png

$(TARGET_DEBUG): RarityColour.csproj $(ALLCODE) $(ALLASSETS)
	dotnet build --configuration Debug
debug: $(TARGET_DEBUG)

$(TARGET_RELEASE): RarityColour.csproj $(ALLCODE) $(ALLASSETS)
	dotnet build --configuration Release
release: $(TARGET_RELEASE)

RarityColour.zip: $(TARGET_RELEASE) icon.png README.md CHANGELOG.md manifest.json
	rm -f RarityColour.zip
	zip -j RarityColour.zip $(TARGET_RELEASE) icon.png README.md CHANGELOG.md manifest.json
zip: RarityColour.zip

all: debug release zip

install: $(TARGET_DEBUG)
	cp $(TARGET_DEBUG) ../BepInEx/plugins

instrelease: $(TARGET_RELEASE)
	cp $(TARGET_RELEASE) ../BepInEx/plugins

assets:
	mkdir -p assets

assets/ItemBackground1.png: srcassets/Item_Background_Lv1.png
	convert srcassets/Item_Background_Lv1.png -channel R -evaluate Divide 0.5 -channel GB -evaluate Divide 0.25 assets/ItemBackground1.png

assets/ItemBackground2.png: srcassets/Item_Background_Lv2.png
	convert srcassets/Item_Background_Lv2.png -channel RGB -evaluate Divide 0.5 assets/ItemBackground2.png

assets/ItemBackground3.png: srcassets/Item_Background_Lv3.png
	convert srcassets/Item_Background_Lv3.png -channel R -evaluate Divide 0.9 -channel G -evaluate Divide 0.55 -channel B -evaluate Divide 0.3 assets/ItemBackground3.png

assets/ItemBackgroundSold.png: srcassets/Item_Background_Vanilla.png
	convert srcassets/Item_Background_Vanilla.png -channel R -evaluate Divide 0.5 -channel GB -evaluate Divide 0.25 assets/ItemBackgroundSold.png

assets/ShopBackground.png: srcassets/Shop_Background.png
	convert srcassets/Shop_Background.png -channel RG -evaluate Divide 0.25 -channel B -evaluate Divide 0.5 assets/ShopBackground.png

clean:
	rm -rf bin obj assets RarityColour.zip

.PHONY: all debug release zip install instrelease clean
