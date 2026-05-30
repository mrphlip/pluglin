# Custom Relics
To create a custom relic, create an object of the `Forbge.CustomRelic` class, and set its various properties. This will create the relic, and add it to the relic pools so it can be collected in-game.

You will then need to make patches to the game (eg using Harmony) to implement whatever effect the relic will need to have.

# Example
```cs
Forbge.CustomRelic static samplerelic;
private static void Register() {
	samplerelic = new Forbge.CustomRelic("Sample relic");
	samplerelic.Name.Default = "Sample relic";
	samplerelic.Description.Default = "This relic does sample things";
	samplerelic.Rarity = Relics.RelicRarity.COMMON;
	samplerelic.Sprite = Forbge.AssetHelper.MakeSprite("samplerelic.png");
}

[HarmonyPatch(... whatever place the relic needs to activate ...)]
private static void CheckRelic() {
	if (samplerelic.AttemptUseRelic()) {
		... have some effect ...
	}
}
```

# CustomRelic constructor
The `new CustomRelic` constructor takes one parameter, an internal name for the relic.

Whatever you set it to, it should not change in future versions. If you do want to rename the relic, you can do so by changing the `Name` property, but still pass the same name to the constructor. This is because the internal name controls things like, whether the relic is unlocked in Custom Start, or whether it has the "new relic" icon when offered to the player.

If you change the internal name here, the game will treat it as a different relic that will need to be unlocked again. And if the player has a savegame in progress that includes that relic, they will not be able to continue the game.

# CustomRelic properties
These properties can be set on the CustomRelic to configure the relic:
* `Forbge.Translation Name`: the name of the relic. Should be short and snappy. This can be translated, see the docs on [translations](translation.md) for more details. This must be set for all relics.
* `Forbge.Translation Description`: the description of the relic, shown in the tooltip. Should explain what the relic does. This can also be translated, and can contain [styling](styling.md). This must also be set for all relics.
* `Forbge.Translation LockedDescription` and `PeglinUI.Tooltip.LockedDescriptionPresets LockedDescriptionPreset`: these control what is shown in the tooltip for the relic in Custom Start if you don't have it unlocked yet. The default option is to say "You haven't encountered this relic yet", which is appropriate for most normal relics. But if you want to add some Secret relics that have special unlock conditions for Custom Start, you can either pick a different preset hint with `LockedDescriptionPreset`, or provide your own text in `LockedDescription`.
* `Relics.RelicRarity Rarity`: how rare is this relic, by default. Note: to give the relic different rarity/availability on different classes, see [classes](classes.md). Will default to `COMMON` if not set.
	* `Relics.RelicRarity.COMMON`: Common relic, appears in chests, shops, and the free relics at the start of the game
	* `Relics.RelicRarity.RARE`: Rare relic, appears in miniboss rewards, and occasionally in chests
	* `Relics.RelicRarity.BOSS`: Boss relic, appears in act-end boss rewards
	* `Relics.RelicRarity.NONE`: Scenario relic, does not appear normally, and must be granted elsewhere (usually by an event scenario)
	* `Relics.RelicRarity.UNAVAILABLE`: Relic does not appear in-game at all, usually used with class-specific rarity or for Secret relics
	* Note that "shop-only" is a special condition that cannot be set here as a general rarity, it can _only_ be set on a per-class basis.
* `UnityEngine.Sprite Sprite`: The image to show for the relic. See the docs on [assets](assets.md) for how to load these. This should be an image that is exactly 16x16 pixels in size, as other sizes may not display correctly.
* `UnityEngine.AudioClip UseSfx`: A sound-effect to play whenever the relic is used. Should be used sparingly. Forbge currently doesn't have a helper for loading audio clips, so if you want to use this you will have to figure that out on your own.
* `bool Intro`: If this is set to true, the relic can appear as the start-of-game bonus relic when the game is in "intro" mode (ie before the player has beaten the game). After the player has beaten the game once, the start-of-game bonus relic becomes a choice of 3 relics from the entire Common pool. I imagine most players who are installing mods have probably beaten the game already, so this is unlikely to matter, but it is here if you want to set it.
* `int? Countdown`: Set this if you want your relic's effect to be a "Every 3 times a thing happens, get an effect" style of relic. If this is set, the relic will have a number displayed, which will count down each time `AttemptUseRelic` is called, and only when the countdown reaches zero, will `AttemptUseRelic` return `true`. This defaults to `null` which will disable the countdown.
* `int? UsesPerShot`, `int? UsesPerBattle`, `int? UsesPerRun`: Set these if you want your relic to have a limited number of uses before it is disabled. The number of remaining uses will be shown on the relic, if it is greater than 1. These also default to `null`, meaning unlimited uses.

# Relic object and RelicManager
The `Relics.RelicManager` class is a part of Peglin which handles many relic-related actions, which you will likely need to interact with.

Some of the methods on the RelicManager will want you to pass a `Relics.Relic` object which is the main Peglin object that represents the relic, and others will want a `Relics.RelicEffect` value, which is an identifier number for each relic. You can get these via `CustomRelic.Relic` and `CustomRelic.Effect`, respectively.

Two calls in particular that are particularly useful, also have convenience methods on the `CustomRelic` object:
* `CustomRelic.AttemptUseRelic()`: Check whether you can use this relic. This will check whether the player has the relic, that it hasn't been disabled, and will also handle the countdowns and limited-uses properties. If all the checks are successful, it will return `true` and also make the icon for the relic blink in the UI.
* `CustomRelic.RelicEffectActive()`: Checks only whether the player owns the relic, and it hasn't been disabled. Does not perform any countdown or limited-use logic, and does not blink the UI.

Generally, you will want to use `AttemptUseRelic` for any relics where you're reacting to a specific event ("every time X happens, do Y"), and use `RelicEffectActive` for any relics that make a continuous change to the game (eg "all orbs get +1/+1"... don't need to blink the icon in the UI every time the game calculates the current orb's stats).
