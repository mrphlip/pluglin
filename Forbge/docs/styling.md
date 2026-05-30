# Styling
Styling tags can be applied to most text that is displayed in-game, especially in tooltip descriptions.

For example:
```cs
relic.Description["en"] = "Throwing a <sprite name=\"BOMB\"> will <style=heal>heal 10</style>";
```

Note that some styles can also cause additional information to appear. For example, using `<style=ballwark>` in a relic description can make an additional tooltip appear explaining what Ballwark is, this is caused automatically by the style tag.

# Basic styling
There are a number of basic tags for doing general styling, see the [Unity TextMeshPro docs](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.2/manual/RichTextSupportedTags.html).

# Specific styling
* `<style=activate>...</style>`: Activating pegs
* `<style=balance>...</style>`: Ballance status effect
* `<style=balltiplier>...</style>`: Balltiplier status effect
* `<style=ballwark>...</style>`: Ballwark status effect
* `<style=blind>...</style>`: Darcness status effect
* `<style=bramble>...</style>`: Tangled status effect
* `<style=confuse>...</style>`: Conefused status effect
* `<style=crystal>...</style>`: Mentions of crystals in events
* `<style=damage>...</style>`: Damage dealt/received
* `<style=destroy>...</style>`: Effects that destroy pegs
* `<style=dmg_bonus>...</style>`: Increases to damage dealt
* `<style=dmg_limit>...</style>`: Intangiball status effect
* `<style=dmg_negative>...</style>`: Decreases to damage dealt
* `<style=dodge>...</style>`: Ballusion status effect
* `<style=durable>...</style>`: Durable pegs
* `<style=echo>...</style>`: Echo effects
* `<style=egg>...</style>`: Egg
* `<style=enemyBonusDmg>...</style>`: Increases to damage received
* `<style=enemyNegDmg>...</style>`: Decreases to damage received
* `<style=exploitaball>...</style>`: Exploitaball status effect
* `<style=finesse>...</style>`: Spinesse status effect
* `<style=fire>...</style>`: Mentions of fire in events
* `<style=gold>...</style>`: Gold, gained or collected
* `<style=heal>...</style>`: Healing, in orb/relic descriptions
* `<style=heal2>...</style>`: Healing, in event scenarios
* `<style=hidden>...</style>`: Very faint text, in event scenarios
* `<style=hit>...</style>`: Hitting pegs
* `<style=lightning>...</style>`: Lightning, in event scenarios
* `<style=loseOrb>...</style>`: Orb removal, in event scenarios
* `<style=morbid>...</style>`: Morbid effects
* `<style=multiball>...</style>`: Multiball effects
* `<style=objectOfInterest>...</style>`: Assorted highlights, in event scenarios
* `<style=orberos>...</style>`: Orboros, in event scenarios
* `<style=overflow>...</style>`: Overflow effects
* `<style=peglin>...</style>`: Peglin, in event scenarios
* `<style=persist>...</style>`: Persist effects
* `<style=pierce>...</style>`: Piercing effects
* `<style=player_poison>...</style>`: Spinfection status effect, on the player
* `<style=poison>...</style>`: Spinfection status effect, on enemies
* `<style=pop>...</style>`: Popping pegs
* `<style=refresh>...</style>`: Refresh effects
* `<style=shield>...</style>`: Ballwark status effect
* `<style=slime>...</style>`: Slime effects
* `<style=stone>...</style>`: Pebballs, in event scenarios
* `<style=strength>...</style>`: Muscircle status effect
* `<style=subtle>...</style>`: Faint text, in event scenarios
* `<style=transpherency>...</style>`: Transpherency status effect
* `<style=upgradeOrb>...</style>`: Orb upgrades, in event scenarios
* `<style=water>...</style>`: Waterfall, in event scenarios

# Icons
* `<sprite name="BALANCE">`: Ballance icon
* `<sprite name="BALLUSION">`: Ballusion icon
* `<sprite name="BALLWARK">`: Ballwark icon
* `<sprite name="BLIND">`: Darcness icon
* `<sprite name="BOMB">`: Black/red bomb, indicating a bomb of either type
* `<sprite name="BOMB_REGULAR">`: Entirely black bomb, indicating regular bombs only
* `<sprite name="BULLET">`: A small dot, for bullet points
* `<sprite name="COIN_ONLY">`: A small gold coin
* `<sprite name="CONFUSION">`: Conefusion icon
* `<sprite name="CRIT_PEG">`: Yellow crit peg
* `<sprite name="DULL_PEG">`: A grey dull peg
* `<sprite name="EXPLOITABALL">`: Exploitaball icon
* `<sprite name="FINESSE">`: Spinesse icon
* `<sprite name="GOLD">`: A pile of gold coins
* `<sprite name="INTANGIBALL">`: Intangiball icon
* `<sprite name="LIFELINK">`: Spintertwined icon
* `<sprite name="LONG_PEG">`: Rectangular brick peg
* `<sprite name="MAX_HP_PEG">`: Peg with a green plus
* `<sprite name="PEG">`: Ordinary round peg
* `<sprite name="PEG_CLEARED">`: Small dot, marking a peg that has been hit
* `<sprite name="PEG_COIN">`: Peg containing gold
* `<sprite name="PEG_CRIT_ACTIVE">`: Red peg, when a crit has been hit
* `<sprite name="PEG_RESPAWNED">`: Slightly dulled peg, that has been hit and refreshed
* `<sprite name="PEG_SHIELD">`: A shield that could be put on a peg
* `<sprite name="PEG_SHIELDED">`: A peg with a shield
* `<sprite name="POISON">`: Spinfection icon
* `<sprite name="PS_BUTTONS">`: Four buttons, with square, triangle, circle and cross symbols
* `<sprite name="REFLECT">`: Rebound icon
* `<sprite name="REFRESH_PEG">`: Green refresh peg
* `<sprite name="RIGGED_BOMB">`: Entirely red bomb, indicating rigged bombs only
* `<sprite name="SLIME_DAMAGE">`: Double-damage slime peg (orange)
* `<sprite name="SLIME_HEAL">`: Healing slime peg (green)
* `<sprite name="SLIME_LIGHTNING">`: Lightning slime peg (purple)
* `<sprite name="SLIME_POISON">`: Poison slime peg (light blue)
* `<sprite name="SLIME_RUBBER">`: Rubber slime peg (magenta)
* `<sprite name="SLIME_SHIELD">`: Damage reduction slime peg (dark blue)
* `<sprite name="SPINFECTION_PEG">`: Peg with spinfection icon
* `<sprite name="STRENGTH">`: Muscircle icon
* `<sprite name="SWITCH_BUTTONS">`: Four buttons, with Y, X, A, B text
* `<sprite name="TEMP_CRIT">`: Dotted yellow crit peg
* `<sprite name="TEMP_REFRESH">`: Dotted green refresh peg
* `<sprite name="THORNED">`: Tangled icon
* `<sprite name="VINE_PEG">`: Green vine peg from Betsy
* `<sprite name="XBOX_BUTTONS">`: Four buttons, with X, Y, B, A text
