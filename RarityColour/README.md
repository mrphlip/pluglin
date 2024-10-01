# Rarity Colour

A Peglin mod that re-colours the buttons for purchasing orbs and relics, to show the item's rarity.

Once installed, post-battle upgrades and shops will have the buttons recoloured, in the standard MMORPG colour scheme:

* Common orbs and relics will be shown in the standard reddish-brown colour
* Uncommon orbs will be shown in green
* Rare orbs and rare (miniboss) relics will be shown in blue
* Boss relics will be shown in purple
* Special (event) orbs and relics will be shown in gold

## How to install

* Install from Thunderstore using [r2modman](https://thunderstore.io/c/peglin/p/ebkr/r2modman/)
* For manual installation, install [BepInEx](https://thunderstore.io/c/peglin/p/BepInEx/BepInExPack_Peglin/), then download the mod and unzip `RarityColour.dll` into the `BepInEx\plugins` folder.

## How to build from source

* Install [dotnet](https://dotnet.microsoft.com/en-us/download) and [ImageMagick](https://imagemagick.org/)
* Get the source code from [GitHub](https://github.com/mrphlip/pluglin)
* Copy (or symlink) your Peglin install into the root of the repository.
* Run `make`

Note that the current `Makefile` as-written will probably only work on Linux. But it's not doing anything fancy, I'm sure everything would build fine on Windows with some tinkering.
