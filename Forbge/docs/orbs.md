# Custom Orbs
To create a custom orb, create an object via the `Forbge.CustomOrb` class, and set its various properties. This will create the orb, and add it to the orb pools so it can be collected in-game.

Orbs in Peglin are [Unity game objects](https://docs.unity3d.com/ScriptReference/GameObject.html) and so many of the standard Unity properties for game objects can be used here. `CustomOrb` itself is a [component](https://docs.unity3d.com/ScriptReference/Component.html) of the orb object, and inherits all of the properties and methods you would expect it to have.

# Example
```cs
Forbge.CustomOrb sampleorb;
private static void Register() {
	sampleorb = Forbge.CustomOrb.Create("Sample orb");
	sampleorb.Name.Default = "Sample orb";
	Forbge.Translation desc = sampleorb.AddDescription();
	desc.Default = "This orb does sample things";
	sampleorb.Rarity = PachinkoBall.OrbRarity.COMMON;
	sampleorb.Sprite = Forbge.AssetHelper.MakeSprite("sampleorb.png");
}
```

# CustomOrb factory
The `CustomOrb.Create` factory takes one mandatory parameter, an internal name for the orb.

Whatever you set it to, it should not change in future versions. If you do want to rename the orb, you can do so by changing the `Name` property, but still pass the same name to the factory. This is because the internal name controls things like, whether the orb is unlocked in Custom Start, or whether it has the "new orb" icon when offered to the player.

If you change the internal name here, the game will treat it as a different orb that will need to be unlocked again. And if the player has a savegame in progress, they will not be able to continue the game.

The `Create` factory also takes a second optional parameter, which controls what type of attack this orb performs (eg projectile, targeted, etc). See the section on attack properties, below.

# CustomOrb properties
These properties can be set on the CustomOrb to configure the orb:
* `Forbge.Translation Name`: the name of the orb. Should be short and snappy. This can be translated, see the docs on [translations](translation.md) for more details. This must be set for all orbs.
* `Forbge.Translation AddDescription()`, `Forbge.Translation GetDescription(int ix)`, `void RemoveDescription(int ix)`, `int DescriptionCount`: These methods and properties maintain a list of translation objects for the description of the orb. These descriptions are all shown together in the orb's tooltip. For example, if you have an orb with three effects, you should create three descriptions here, and it will be shown as a tooltip with three bullet points.
* `Forbge.Translation LockedDescription` and `PeglinUI.Tooltip.LockedDescriptionPresets LockedDescriptionPreset`: these control what is shown in the tooltip for the orb in Custom Start if you don't have it unlocked yet. The default option is to say "You haven't encountered this orb yet", which is appropriate for most orbs. But if you want to add some special orbs that have secret unlock conditions for Custom Start, you can either pick a different preset hint with `LockedDescriptionPreset`, or provide your own text in `LockedDescription`.
* `PachinkoBall.OrbRarity Rarity`: how rare is this orb, by default. Note: to give the orb different rarity/availability on different classes, see [classes](classes.md). Will default to `COMMON` if not set.
	* `PachinkoBall.OrbRarity.COMMON`: Common orb, appears frequently in battle rewards and shops
	* `PachinkoBall.OrbRarity.UNCOMMON`: Uncommon orb, appears less frequently in battle rewards and shops
	* `PachinkoBall.OrbRarity.RARE`: Rare orb, appears infrequently in battle rewards and shops
	* `PachinkoBall.OrbRarity.SPECIAL`: Special orb, does not appear normally, and must be granted elsewhere (usually by an event scenario)
	* `PachinkoBall.OrbRarity.NOT_PRESENT`: Orb does not appear in-game at all, usually used with class-specific rarity
* `int Damage`, `int Crit`: The orbs basic damage stats per peg
* `UnityEngine.Sprite Sprite`: The image to show for the orb. See the docs on [assets](assets.md) for how to load these. For a normal orb this should be 16x16 pixels in size, though other sizes can be used (but be sure to adjust the `Radius` below).
* `int Level`: What level the orb is. Normally you won't need to change this, as it will be set by default: to `-1` for non-upgradable orbs, or for `1`, `2`, `3` automatically counting up for upgradable orbs. See the section on upgradable orbs, below.
* `float Radius`: How large is this orb. For normal-sized orbs (16x16 pixels), this should be set to `PachinkoBall.DEFAULT_ORB_COLLIDER_RADIUS`, which is the default. For orbs of different sizes, this should be scaled accordingly, eg a 12x12 pixel orb should have `Radius = PachinkoBall.DEFAULT_ORB_COLLIDER_RADIUS * 0.75f`. This controls how large the orb is in the peg-board, and also directly controls whether the orb counts for relics like Big Fish and Small Packages.
* `float Mass`: How heavy is this orb. This controls how much inertia it has in collisions. For most orbs this should be the default of `1.2`, but it can be increased for dense orbs (eg Bouldorb is `2.0`) or decreased for light orbs (eg Infernorb is `0.8`).
* `float Bounciness`: How bouncy is this orb. The default is `0` but for extra bouncy orbs this can be increased (eg Rubborb is `0.85`).
* `float Friction`: How much friction this orb experiences in flight. The default is `0.4`.
* `float Scale`: Can be used to make orbs larger or smaller. The default is `0.6`, but if you were to, for example, halve this to `0.3`, then a 32x32 pixel sprite would show up at the size of a normal orb.

# Unity components
There are a lot of other values that can be controlled on the various components that make up the orb game object, including many that haven't been lifted out above. You can reach various components via quick-access properties on the `CustomOrb` object:
* `gameObject`: the main Unity Game Object itself, this is a [standard property](https://docs.unity3d.com/ScriptReference/Component-gameObject.html) on Unity components
* `Ball`: the `PachinkoBall` component, which mostly includes properties about how the orb behaves bouncing between the pegs
* `Attack`: The `Battle.Attacks.Attack` component, which mostly includes properties about how the orb deals damage, and how it appears in the enemy area. This will usually be a _subclass_ of `Battle.Attacks.Attack` depending on what type of attack it is (projectile, targeted, etc)
* `Collider`: The [CircleCollider2D](https://docs.unity3d.com/ScriptReference/CircleCollider2D.html) component that controls how it collides with pegs on the board
* `Rididbody`: The [Rigidbody2D](https://docs.unity3d.com/ScriptReference/Rigidbody2D.html) component that controls how it behaves after a collision
* `Renderer`: The [SpriteRenderer](https://docs.unity3d.com/ScriptReference/SpriteRenderer.html) component that controls how the orb is displayed on the screen
* `Animator`: The [Animator](https://docs.unity3d.com/ScriptReference/Animator.html) component that can be used to add animation to the orb's sprite

In addition to these, there are a large number of other components that can be [added to the orb](https://docs.unity3d.com/ScriptReference/GameObject.AddComponent.html) to provide additional behaviours. Look through the `Battle.Pachinko.BallBehaviours` namespace to find components to add things like Multiball, or adding statuses when you hit pegs, or many other behaviours. You can, of course, also implement your own new behaviours.

# Upgradable orbs
To make an upgradable orb, first create the orb as normal with all of its Level 1 details. Then, call the method `CustomOrb.CreateLevel()`, which will duplicate the orb, setting it to Level 2, and return the new orb. Make any changes you need for level 2, then call `CreateLevel` again on the Level 2 orb, to make a Level 3.

For example:
```cs
CustomOrb sample = CustomOrb.Create("sample");
sample.Name.Default = "Sample Orb";
var desc = sample.AddDescription();
desc.Default = "Does very little";
sample.Damage = 1;
sample.Crit = 1;

var lvl2 = sample.CreateLevel();
lvl2.Damage = 2;
lvl2.Crit = 2;
lvl2.GetDescription(0).Default = "Does a bit";

var lvl3 = lvl2.CreateLevel();
lvl3.Damage = 10;
lvl3.Crit = 10;
lvl3.GetDescription(0).Default = "Does a lot";
```

# Attack properties
TODO: this has not yet been implemented
## Projectile attacks
## Targeted attacks
## All-enemies attacks
## Special attacks
