all: bin/Debug/netstandard2.1/RarityColour.dll

bin/Debug/netstandard2.1/RarityColour.dll: RarityColour.csproj Plugin.cs Assets.cs
	dotnet build

install: all
	cp bin/Debug/netstandard2.1/RarityColour.dll ../BepInEx/plugins

Assets.cs: gen_assets_cs.sh assets assets/ItemBackground1.png assets/ItemBackground2.png assets/ItemBackground3.png assets/ItemBackgroundSold.png assets/ShopBackground.png
	./gen_assets_cs.sh

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
	rm -rf bin obj assets Assets.cs

.PHONY: all install clean
