# Relic Stats

A Peglin mod that keeps stats for your relics, so you can see how effective they are in your game. See exactly how much damage you've gained (or lost) to the Weighted Chip. How much money you've gotten back from that Navigationflation you bought. Or how much healing you've gotten from the Popping Corn.

Mouse over the relics in-game or at the post-game summary to see the stats.

## How to install

* Install from Thunderstore using [r2modman](https://thunderstore.io/c/peglin/p/ebkr/r2modman/)
* For manual installation, install [BepInEx](https://thunderstore.io/c/peglin/p/BepInEx/BepInExPack_Peglin/), then download the mod and unzip `RelicStats.dll` into the `BepInEx\plugins` folder.

## How to build from source

* Install [dotnet](https://dotnet.microsoft.com/en-us/download)
* Get the source code from [GitHub](https://github.com/mrphlip/pluglin)
* Copy (or symlink) your Peglin install into the root of the repository.
* Run `make`

Note that the current `Makefile` as-written will probably only work on Linux. But it's not doing anything fancy, I'm sure everything would build fine on Windows with some tinkering.
