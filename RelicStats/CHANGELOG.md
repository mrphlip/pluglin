# 1.5.0
* Update for Peglin 2.0.10
	* Add new relic Lightning Grease

# 1.4.1
* Update for Peglin 2.0.9
	* Bugfix for internal change that broke the hooks for calculating orb damage

# 1.4.0
* Update for Peglin 2.0.5
	* 7 new relics added
		* Spooky Haglin's Scary Hat counts the gold added to the board
		* Chain of Reaction and Duck (and Cover) count the Ballusion granted
		* Counter Tack counts the damage dealt
		* Weave of Fate counts the extra Ballusion added
		* Gustav's Holster counts the extra damage added
		* Beleagured Boots does not have anything useful to count
	* Update to Crystal Catalyst for the new relic behaviour
	* Don't count stats for relics when they're disabled by Relic Rotation
	* Some bug fixes for the tooltips on the Run Summary screen
	* Bug fix for relics that count damage added/removed by affecting orb stats, which was broken by a new optimisation refactor in the game.
	* Choosing not to also add stats for all the new Act 4 challenges, even though they look like relics... maybe I will revisit this decision later but for now I don't think they're worth the effort.

# 1.3.0
* Update for Peglin 1.1.19 & 1.1.24
	* Fix up a ton of relics for the big damage refactor
		* The delay in this release is mostly due to spending time being intimidated by the size of this refactor
	* Add counters for the new relics
		* Grinding Monstera only counts the max HP that is actually realised from hitting the special pegs
		* Focused Blast counts the total bomb damage dealt to the targeted enemy
			* And a few other bomb relics have been tweaked to handle their interaction with this relic correctly
		* Froggo Amulet coutns the max HP gained
		* A Bad Slime counts the damage added/removed, in the same way as A Good Slime
		* Split Packages counts the damage added by the extra double-hit pegs
		* Big Fish counts the extra damage dealt
		* No obvious stats to count for Critiqualibrium or Collusion Detection
	* Other updates
		* Update for Basalt Toadem adding 5 instead of 4 max HP
		* Bugfix for Glorious SuffeRing, which was accidentally using the Endless DevouRing stats instead (so it would calculate peg buffs as debuffs instead)

# 1.2.1
* Bugfix for Alien's Rock counting other sources of splash damage (like Sappers exploding)

# 1.2.0
* Update for Peglin 1.1.9
	* Please don't look up how many Peglin releases there have been since I last got around to updating this
* Fixed Alien's Rock and Aliensrock to not overcount on enemies that died in the attack
	* Also, updated them to work after some refactored code in the game
* Fixed Refresh Perspective's new interaction with Necorbmancer
	* The stats should only count the +1 from the relic and not the +5/7/9 from the orb, even though both are applied at once

# 1.1.0
* Update for Peglin 1.1.0
* New relics added:
	* Pincer Maneuver in the affects-projectiles too-hard bucket
	* Gopher Gold counts how many orbs have been granted Multiball, like Matryoshka Shell
	* Tender Cactus counts how much damage has been dealt
	* Status Symbol counts status effects doubled, and damage lost due to debuff
	* No obvious stats to count for Viridian Trinket
* Existing relics changed:
	* Skulltimate Wrath and Endless DevouRing updated for new behaviour
	* Consuming Chalice now counts spinesse stacks added
	* Strange Brew counts all three additional peg hits
	* Eye of Turtle removed a no-longer-needed workaround for a bug fixed in 1.1.0
* No change needed for: Pocketwatch, Dungeon Die, Parallel Boomiverse, Bag of Orange Pegs, Is Dis Your Card, Subtraction Reaction, Super Boots, Alien's Rock
* No stats to change for: Constricting Chains, Matryoshka Shell, Heavy Hand, Beleaguered Boots, Wand of Skulltimate Greed

# 1.0.1
* Fix stats for Crystal Catalyst – was previously only counting "5 per act, per poisoned enemy" not "5 per act, per stack".

# 1.0.0
* Bumping version number up to full-release status, now that the mod has been tested by players other than myself.
* Updated for Peglin 1.0.6
	* Added stats for new relic Parallel Boomiverse
	* Added new relic Safety Pegulations to the "changes projectile behaviour" too-hard basket
	* Updated stats tracking for Orbert's Story for its modified behaviour
	* Updated tooltip for Recombombulator to show a regular black bomb icon
	* Hero's Backpack, Perfected Reactant and Perfect Forger did not require changes to work with the new version
	* Constricting Chains, Heavy Hand, Haglin's Satchel and Molten Gold do not have stat trackers to update
* Fixed a bug that was making the run summary relic tooltips also show up in the Encirclepedia run history section.

# 0.2.0
* Fix a bug with Spheridae's Fate that was causing slowdown
* Add stats for:
	* Cursed Mask
	* Rallying Heart
	* Slimy Salve
	* Glorious SuffeRing
	* Pumpkin Pi
	* Axe Me Anything
	* A Good Slime
	* Adventurine
	* Endless DevouRing
	* Refresh Perspective
	* Subtraction Reaction
* Only relics still on the todo list are the ones that affect projectile behaviour:
	* Overwhammer
	* Grabby Hand
	* Prime Rod of Frost
	* Au Auger

# 0.1.0
* Initial public release
* Keeps stats for how effective your relics are
* Mouse over the relics in-game or at the post-game summary to see the stats
* Stats are implemented for 129 of 184 relics
* 11 tricker ones are still todo, but I still want to implement them in a future version
* 44 are ones it doesn't really make sense to have stats for (eg the ones that just increase your max HP)
