# Custom Relics
To create a custom relic, create an object of the `Forbge.RelicBuilder` class, set its various properties, and then call `Register`. This will create the relic, and add it to the relic pools so it can be collected in-game.

You will then need to make patches to the game (eg using Harmony) to implement whatever effect the relic will need to have.

# Example
```cs
Relics.Relic samplerelic;
private static void Register() {
	Forbge.RelicBuilder builder = new Forbge.RelicBuilder();
	builder.name.Default = "Sample relic";
	builder.description.Default = "This relic does sample things";
	builder.rarity = Relics.RelicRarity.COMMON;
	builder.sprite = Forbge.AssetHelper.MakeSprite("samplerelic.png");
	samplerelic = builder.Register();
}

[HarmonyPatch(... whatever place the relic needs to activate ...)]
private static void CheckRelic() {
	if (Forbge.Managers.relicManager.AttemptUseRelic(samplerelic.effect)) {
		... have some effect ...
	}
}
```

# RelicBuilder properties
These properties can be set on the RelicBuilder before calling `Register`:
* `name`: the name of the relic. Should be short and snappy. This can be translated, see the docs on [translations](translation.md) for more details. This must be set for all relics.
* `description`: the description of the relic, shown in the tooltip. Should explain what the relic does. This can also be translated, and can contain [styling](styling.md). This must also be set for all relics.
* `lockedDescription` and `lockedDescriptionPreset`: these control what is shown in the tooltip for the relic in Custom Start if you don't have it unlocked yet. The default option is to say "You haven't encountered this relic yet", which is appropriate for most normal relics. But if you want to add some Secret relics that have special unlock conditions for Custom Start, you can either pick a different preset hint with `lockedDescriptionPreset`, or provide your own text in `lockedDescription`.
* `rarity`: how rare is this relic, by default. Note: to give the relic different rarity/availability on different classes, see [classes](classes.md). Will default to `COMMON` if not set.
	* `Relics.RelicRarity.COMMON`: Common relic, appears in chests, shops, and the free relics at the start of the game
	* `Relics.RelicRarity.RARE`: Rare relic, appears in miniboss rewards, and occasionally in chests
	* `Relics.RelicRarity.BOSS`: Boss relic, appears in act-end boss rewards
	* `Relics.RelicRarity.NONE`: Special relic, does not appear in-game, and must be granted elsewhere (usually by an event scenario)
	* `Relics.RelicRarity.UNAVAILABLE`: Relic does not appear in-game at all, usually used with class-specific rarity
	* Note that "shop-only" is a special condition that cannot be set here as a general rarity, it can _only_ be set on a per-class basis.
* `sprite`: The image to show for the relic. See the docs on [assets](assets.md) for how to load these. This should be an image that is exactly 16x16 pixels in size, as other sizes may not display correctly.
* `useSfx`: A sound-effect to play whenever the relic is used. Should be used sparingly. Forbge currently doesn't have a helper for loading audio clips, so if you want to use this you will have to figure that out on your own.
* `intro`: If this is set to true, the relic can appear as the start-of-game bonus relic when the game is in "intro" mode (ie before the player has beaten the game). After the player has beaten the game once, the start-of-game bonus relic becomes a choice of 3 relics from the entire Common pool. I imagine most players who are installing mods have probably beaten the game already, so this is unlikely to matter, but it is here if you want to set it.
* `countdown`: Set this if you want your relic's effect to be a "Every 3 times a thing happens, get an effect" style of relic. If this is set, the relic will have a number displayed, which will count down each time `RelicManager.AttemptUseRelic` is called, and only when the countdown reaches zero, will `AttemptUseRelic` return `true`. This defaults to `null` which will disable the countdown.
* `usesPerShot`, `usesPerBattle`, `usesPerRun`: Set these if you want your relic to have a limited number of uses before it is disabled. The number of remaining uses will be shown on the relic, if it is greater than 1.

# Relic object and RelicManager
The `RelicBuilder.Register()` method will return a `Relics.Relic` object, which is a class from Peglin proper. All its properties will already be set up by the builder according to your settings. You should keep a hold of this, as you will need it to make calls to `RelicManager`, later.

Note that some calls on `RelicManager` will want you to pass the entire `Relic` object as a parameter, others will want you to pass the value of the `Relic.effect` field, which is a magic ID that identifies the relic.

Two calls in particular that you should pay attention to are:
* `RelicManager.AttemptUseRelic(relic.effect)`: Check whether you can use this relic. This will check whether the player has the relic, that it hasn't been disabled, and will also handle the countdowns and limited-uses properties. If all the checks are successful, it will return `true` and also make the icon for the relic blink in the UI.
* `RelicManager.RelicEffectActive(relic.effect)`: Checks only whether the player owns the relic, and it hasn't been disabled. Does not perform any countdown or limited-use logic, and does not blink the UI.

Generally, you will want to use `AttemptUseRelic` for any relics where you're reacting to a specific event ("every time X happens, do Y"), and use `RelicEffectActive` for any relics that make a continuous change to the game (eg "all orbs get +1/+1"... don't need to blink the icon in the UI every time the game calculates the current orb's stats).
