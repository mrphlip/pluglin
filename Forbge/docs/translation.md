# Translations
Any text that can be shown to the player in-game, can be translated to different languages.

These are all handled in Forbge by the `Forbge.Translation` class.

# Option 1: No translations
If you don't care about translation, you can just set the `Default` property to your text, and call it a day. This text will then be shown regardless of what langauge the player has selected.
```cs
relicbuilder.name.Default = "Sample relic";
```

# Option 2: Provide translations
Alternatively, if you _do_ care about translation, you can provide the text in multiple languages, and the game will select the appropriate one based on the player's selected language.

If the player selects a lanauge which you haven't provided a translation, it will use whichever translation you provided first, as a fallback.
```cs
relicbuilder.name["en"] = "Sample relic";
relicbuilder.name["fr"] = "Exemple de relique";
relicbuilder.name["de"] = "Beispiel eines Relikts";
```
You can also provide _both_ translations for specific languages, _and_ `Default`, to provide an explicit fallback message for langauges that do not have a translation available.

# Language codes
The available language codes you can provide translations for are currently (as of Peglin 2.0.12):
* `en`: English
* `de`: German (Deutsch)
* `es`: Spanish (Español)
* `fr`: French (Français)
* `it`: Italian (Italiano)
* `pl`: Polish (Polski)
* `pt-BR`: Brazilian Portuguese (Português do Brasil)
* `ru`: Russian (Русский)
* `ua`: Ukranian (Українська)
* `ko`: Korean (한국어)
* `ja`: Japanese (日本語)
* `zh-CN`: Chinese Simplified (简体中文)
* `zh-TW`: Chinese Traditional (繁體中文)
