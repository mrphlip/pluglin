using HarmonyLib;
using System;

namespace RelicStats;

public class ConsolationPrize : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.NONE;

	public override void Used() {}
	public override string Tooltip {
		get {
			count++;
			return $"{count} time{Utils.Plural(count)} consoled";
		}
	}
}

public class EnhancedGunpowder : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BOMB_SPLASH;
	public override string Tooltip => $"{count} <sprite name=\"BOMB\"> exploded";
}

public class AlchemistCookbook : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.PEG_TO_BOMB;
	public override string Tooltip => $"{count} <sprite name=\"BOMB_REGULAR\"> created";
}

public class Cookie : HealingCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.HEAL_ON_REFRESH_POTION;
	public override int? HealAmount => (int?)Utils.GetResource<Relics.RelicManager>()?.REFRESH_POTION_HEAL_AMOUNT;
}

public class WellDoneSteak : HealingCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.HEAL_ON_RELOAD;
	public override int? HealAmount => (int)Relics.RelicManager.HEAL_ON_RELOAD_AMOUNT;
}

public class BagOfOrangePegs : MultDamageCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.PEG_CLEAR_DAMAGE_SCALING;
}

public class LightShaftPotion : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.REFRESH_ALSO_CRIT;
	public override string Tooltip => $"{count} <sprite name=\"REFRESH_PEG\"> crits";
}

public class HeavyShaftPotion : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.CRIT_ALSO_REFRESH;
	public override string Tooltip => $"{count} <sprite name=\"CRIT_PEG\"> refreshes";
}

public class WeightedChip : MultDamageCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.SLOT_MULTIPLIERS;
}

public class OldSaltShaker : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.DAMAGE_BONUS_SLIME_FLAT;
	public override int Step => 10;
	public override string Tooltip => $"{count} <style=damage>damage added</style>";
}

public class GiftThatKeepsGiving : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.UNPOPPABLE_PEGS;
}

public class RoundGuard : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.NO_DAMAGE_ON_RELOAD;
	public override int Step => Math.Max((int)(Hooks.damageBeingDealt ?? 0), 0);
	public override string Tooltip => $"{count} <style=damage>damage avoided</style>";

}

[HarmonyPatch]
public class Refillibuster : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.REFRESH_DAMAGES_PEG_COUNT;

	public Refillibuster() {
		Battle.Enemies.Enemy.OnAllEnemiesDamaged += new Battle.Enemies.Enemy.DamageAllEnemies(this.DamageAllEnemies);
	}

	private bool _active = false;
	public override void Used() {
		_active = true;
	}
	[HarmonyPatch(typeof(Battle.PegManager), "ResetPegs")]
	[HarmonyPostfix]
	private static void AfterResetPegs() {
		((Refillibuster)Tracker.trackers[Relics.RelicEffect.REFRESH_DAMAGES_PEG_COUNT])._active = false;
	}
	private void DamageAllEnemies(float damageAmount) {
		if (_active) {
			count += Utils.EnemyDamageCount() * (int)damageAmount;
			_active = false;
		}
	}

	public override string Tooltip => $"{count} <style=damage>damage dealt</style>";
}

[HarmonyPatch]
public class MatryoshkaDoll : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.MATRYOSHKA;
	public override void Used() {}
	[HarmonyPatch(typeof(BattleController), "ArmBallForShot")]
	[HarmonyPostfix]
	private static void ArmOrb() {
		if (Tracker.HaveRelic(Relics.RelicEffect.MATRYOSHKA)) {
			((MatryoshkaDoll)Tracker.trackers[Relics.RelicEffect.MATRYOSHKA]).count += 1;
		}
	}
	public override string Tooltip => $"{count} orb{Utils.Plural(count)} duplicated";
}

[HarmonyPatch]
public class Recombombulator : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BOMBS_RESPAWN;

	private bool _active = false;
	public override void Used() {
		_active = true;
	}
	[HarmonyPatch(typeof(Bomb), "Reset", new Type[]{})]
	[HarmonyPostfix]
	private static void ResetBomb() {
		Recombombulator t = (Recombombulator)Tracker.trackers[Relics.RelicEffect.BOMBS_RESPAWN];
		if (t._active)
			t.count++;
	}
	[HarmonyPatch(typeof(Battle.PegManager), "ResetPegs")]
	[HarmonyPostfix]
	private static void EndResetPegs() {
		Recombombulator t = (Recombombulator)Tracker.trackers[Relics.RelicEffect.BOMBS_RESPAWN];
		t._active = false;
	}
	public override string Tooltip => $"{count} <sprite name=\"BOMB\"> refreshed";
}

public class ShortFuse : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BOMBS_ONE_HIT;
	public override string Tooltip => $"{count} <sprite name=\"BOMB\"> exploded";
}

public class StrangeBrew : PegDamageCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.POTION_PEGS_COUNT;
}

public class LuckyPenny : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADDITIONAL_CRIT2;
}

public class ThreeExtraCrits : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADDITIONAL_CRIT3;
}

[HarmonyPatch]
public class RefreshingPunch : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.REFRESH_PEGS_SPLASH;
	public override string Tooltip => $"{count} <sprite name=\"REFRESH_PEG\"> exploded";

	// The code can call AttemptUseRelic(REFRESH_PEGS_SPLASH) twice for the same peg
	// so instead hook this call site
	public override void Used() {}
	[HarmonyPatch(typeof(Peg), "RefreshSplash")]
	[HarmonyPostfix]
	private static void RefreshSplashPre() {
		RefreshingPunch t = (RefreshingPunch)Tracker.trackers[Relics.RelicEffect.REFRESH_PEGS_SPLASH];
		t.count++;
	}
}

public class PegBag : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADDITIONAL_REFRESH2;
}

public class ThreeExtraRefresh : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADDITIONAL_REFRESH3;
}

public class EvadeChance : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.EVADE_CHANCE;
}

public class Apple : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.MAX_HEALTH_SMALL;
}

public class WallChicken : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.MAX_HEALTH_MEDIUM;
}

public class PowerGlove : OrbDamageCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.INCREASE_STRENGTH_SMALL;
}

public class Ambidexionary : SimpleCounter {
	public Ambidexionary() {
		BattleController.OnOrbDiscarded += this.OrbDiscarded;
	}
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADDITIONAL_DISCARD;
	public override void Used() {}
	public void OrbDiscarded() {
		if (!Tracker.HaveRelic(Relic))
			return;
		BattleController battleController = Utils.GetResource<BattleController>();
		if (battleController.NumShotsDiscarded == battleController.MaxDiscardedShots)
			count += 1;
	}
	public override string Tooltip => $"{count} extra discard{Utils.Plural(count)} used";
}

public class DecoyOrb : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.FREE_RELOAD;
	public override string Tooltip => $"{count} free reload{Utils.Plural(count)}";
}

public class KineticMeteorite : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BOMB_FORCE_ALWAYS;
	public override string Tooltip => $"{count} explosive force{Utils.Plural(count)}";
}

public class Pocketwatch : PegDamageCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.INFLIGHT_DAMAGE;
}

public class ImprovedCatalyst : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADDITIONAL_BOMB_DAMAGE;
	public override int Step => (int)(Utils.EnemyDamageCount() * Relics.RelicManager.ADDITIONAL_BOMB_DAMAGE);
	public override string Tooltip => $"{count} <style=damage>damage added</style>";
}

public class AmbiguousAmulet : PegDamageCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.WALL_BOUNCES_COUNT;
}

public class CursedMask : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.CONFUSION_RELIC;
}

public class SealedConviction : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.NO_DISCARD;
	// TODO: check impl for new Ballance based version, count stacks of ballance added?
}

public class Electropegnet : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.PEG_MAGNET;
}

public class SuperBoots : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.SUPER_BOOTS;
}

public class SpecialButton : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADDITIONAL_CRIT1;
}

public class FreshBandana : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADDITIONAL_REFRESH1;
}

public class MonsterTraining : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.LOW_HEALTH_INCREASED_DAMAGE;
}

public class Refreshiv : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.REFRESH_BOARD_ON_ENEMY_KILLED;
}

public class TacticalTreat : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.SHUFFLE_REFRESH_PEG;
}

public class UnicornHorn : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.LONGER_AIMER;
}

public class RallyingHeart : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADDITIONAL_END_BATTLE_HEAL;
}

public class SuffertheSling : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BASIC_STONE_BONUS_DAMAGE;
}

public class SandArrows : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ATTACKS_DEAL_BLIND;
}

public class Overwhammer : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.NORMAL_ATTACKS_OVERFLOW;
}

public class InconspicuousRing : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BOUNCERS_COUNT;
}

public class OldGardenerGloves : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.DAMAGE_BONUS_PLANT_FLAT;
	public override int Step => 10;
	public override string Tooltip => $"{count} <style=damage>damage added</style>";
}

public class SlimySalve : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.APPLIES_HEALING_SLIME;
}

public class InfernalIngot : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.LIFESTEAL_PEG_HITS;
}

public class MentalMantle : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.DAMAGE_TARGETED_PEG_HITS;
}

public class PoppingCorn : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.HEAL_ON_PEG_HITS;
}

public class WeaponizedEnvy : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.DAMAGE_TARGETED_ON_HEAL;
}

public class WandofSkulltimateWrath : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.DOUBLE_DAMAGE_HURT_ON_PEG;
}

public class RingofReuse : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ALL_ORBS_PERSIST;
}

public class EchoChamber : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ALL_ATTACKS_ECHO;
}

public class GrabbyHand : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.FLYING_HORIZONTAL_PIERCE;
}

public class KnifesEdge : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.LOW_HEALTH_GUARANTEED_CRIT;
}

public class BasicBlade : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.NON_CRIT_BONUS_DMG;
}

public class CritsomallosFleece : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.CRITS_STACK;
}

public class EyeofTurtle : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADDITIONAL_ORB_RELIC_OPTIONS;
}

public class GloriousSuffeRing : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ALL_ORBS_BUFF;
}

public class SapperSack : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ALL_BOMBS_RIGGED;
}

public class Bombulet : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.DOUBLE_BOMBS_ON_MAP;
}

public class BombBaton : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADDITIONAL_STARTING_BOMBS;
}

public class PowderCollector : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.SPAWN_BOMB_ON_PEG_HITS;
}

public class BadCheese : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.DAMAGE_ENEMIES_ON_RELOAD;
}

public class RingofPain : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.DAMAGE_RETURN;
}

public class PerfectedReactant : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADDITIONAL_BOMB_DAMAGE2;
}

public class DodgyShortcut : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.CRIT_PIT;
}

public class CrumpledCharacterSheet : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.RANDOM_ENEMY_HEALTH;
}

public class CurseofthePeglinKing : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ALTERNATE_SHOT_POWER;
}

public class AncientMeteorite : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.LEGACY_METEORITE;
}

public class DumbBell : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.STR_ON_RELOAD;
}

public class TheCake : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.MAX_HEALTH_LARGE;
}

public class Refreshield : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.REFRESH_BOARD_ON_RELOAD;
}

public class Puppet : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.PREVENT_FIRST_DAMAGE;
}

public class ComplexClaw : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.CRIT_BONUS_DMG;
}

public class IntentionalOboe : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.REDUCE_LOST_HEALTH;
}

public class SpiralSlayer : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.START_WITH_STR;
}

public class ShrewdScales : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BAL_ON_RELOAD;
}

public class ConsumingChalice : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.REDUCE_REFRESH;
}

public class UnpretentiousPendant : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.REDUCE_CRIT;
}

public class SmokeMod : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BOMBS_APPLY_BLIND;
}

public class PocketSand : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BLIND_WHEN_HIT;
}

public class BetsysHedge : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.HEDGE_BETS;
}

public class ShortStack : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ALL_IN_RELIC;
}

public class Pumpkinπ : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.SLOT_PORTAL;
}

public class HaglinsSatchel : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADD_ORBS_AND_UPGRADE;
}

public class SafetyNet : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.MINIMUM_PEGS;
}

public class AncientFleece : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ANCIENT_FLEECE;
}

public class RefresherCourse : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.REFRESH_BUFF;
}

public class HerosBackpack : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADJACENCY_BONUS;
}

public class AimLimiter : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.AIM_LIMITER;
}

public class AxeMeAnything : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.NO_DAMAGE_REDUCTION;
}

public class SeraphicShield : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.IMMORTAL;
}

public class Critikris : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.CRIT_DAMAGES_ENEMIES;
}

public class Peglintuition : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADDITIONAL_PEGLIN_CHOICES;
}

public class MoltenGold : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADDITIONAL_BATTLE_GOLD;
}

public class MoltenMantle : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.CONVERT_COIN_TO_DAMAGE;
}

public class Navigationflation : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.INCREASED_NAV_GOLD;
}

public class DuplicationPotion : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.DUPLICATE_SPECIAL_PEGS;
}

public class RefillosophersStone : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.CREATE_GOLD_ON_REFRESH;
}

public class PeglinerosPendant : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.GOLD_ADDS_TO_DAMAGE;
}

public class WandofSkulltimateGreed : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.DOUBLE_COINS_AND_PRICES;
}

public class DefreshPotion : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ALL_ORBS_MORBID;
}

public class SaltShaker : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.INCREASE_MAX_HP_GAIN;
}

public class GardenersGloves : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.REDUCE_ORB_SELF_DAMAGE;
}

public class GrindingMonstera : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.GAIN_MAX_HP_ON_ENEMY_DEFEAT;
}

public class SashofFocus : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.PREVENT_LETHAL_DAMAGE;
}

public class PrimeRodofFrost : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ALL_ATTACKS_PIERCE;
}

public class BasaltToadem : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.INC_MAX_HP_IF_FULL_HP;
}

public class DungeonDie : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.RANDOMLY_ROLL_DAMAGE;
}

public class PerfectForger : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.UPGRADE_ADDED_ORBS;
}

public class BranchofEmber : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BLIND_BRAMBLE_COMBO;
}

public class RipostalService : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BALLWARK_COUNTER;
}

public class EssentialOil : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ARMOUR_PLUS_ONE;
}

public class ArmourOnPegs : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ARMOUR_ON_PEGS_HIT;
}

public class StartWithBallwark : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.START_WITH_BALLWARK;
}

public class OrbertsStory : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.MORE_TREASURE_NODES;
}

public class KnightCap : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.HEAL_WITH_BALLWARK;
}

public class BallpeenHammer : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADD_BALLWARK_WHEN_SHIELD_PEG_CREATED;
}

public class FieryFurnace : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADD_BALLWARK_WHEN_SHIELD_PEG_BROKEN;
}

public class TrainingWeight : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.STRENGTH_PLUS_ONE;
}

public class BrassicaceaeKnuckles : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.STRENGTH_ON_REFRESH;
}

public class Roundreloquence : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.RANDOM_STATUS_EFFECT_ON_HIT;
}

public class MaskofSorrow : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.INCREASE_NEGATIVE_STATUS;
}

public class MaskofJoy : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.INCREASE_POSITIVE_STATUS;
}

public class GrubbyGloves : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ATTACKS_DEAL_POISON;
}

public class ChokeMod : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BOMBS_APPLY_POISON;
}

public class AuAuger : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.CRITS_PROVIDE_PIERCE;
}

public class BeckoningCrit : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.COINS_TO_CRITS;
}

public class AGoodSlime : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.SLIME_BUFFS_PEGS;
}

public class Adventurine : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BUFF_FIRST_PEG_HIT;
}

public class AliensRock : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.SPLASH_EFFECT_ON_TARGETED_ATTACKS;
}

public class Spinventoriginality : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.UNIQUE_ORBS_BUFF;
}

public class SpheridaesFate : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.CREATE_SQUIRRELS_ON_PEGS_HIT;
}

public class DuplicationStation : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.MULTIBALL_EVERY_X_SHOTS;
}

public class PrimeSlime : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.RANDOM_SLIME_ON_PEGS_HIT;
}

public class HouseofSlime : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BOUNCY_WALLS_AND_BOUNCERS;
}

public class LeafTheRestForLater : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ONLY_REFRESH_X_PEGS;
}

public class VitaminC : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BALLUSION_ON_CRIT;
}

public class IOU : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.GAMBLIN_IOU;
}

public class ReduceReFuseRecycle : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BOMB_NAV_GOLD;
}

public class BallwarkToMuscircle : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BALLWARK_TO_MUSCIRCLE;
}

public class BeleagueredBoots : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BALLUSION_DOUBLE_MAX;
}

public class DoubleBallusion : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BALLUSION_DOUBLE_GAIN;
}

public class DodgyDagger : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.SPINESSE_WHEN_DODGING;
}

public class StackedOrbacus : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.START_WITH_EXPLOITABALL;
}

public class FastReakaton : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADD_BALLUSION_WITH_SPAWNS;
}

public class PieceOfMind : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.LOSE_BALLWARK_GAIN_BALLANCE;
}

public class DistractionReaction : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.GAIN_BALLUSION_FROM_ENEMY_DMG;
}

public class Redoublet : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BALLUSION_DODGE_CREATE_CRIT;
}

public class ConstrictingChains : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.AIM_LIMITER_MULTIBALL;
}

public class EndlessDevouRing : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ALL_ORBS_DEBUFF;
}

public class BastionReaction : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.LOSE_HP_GAIN_BALLWARK;
}

public class IsDisYourCard : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.DISCARD_GAIN_CRIT_BALLUSION;
}

public class RefreshPerspective : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.REFRESH_UPGRADES_PEGS;
}

public class Mauliflower : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.CRITS_FOR_SPINESSE;
}

public class ModestMallet : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.DISCARD_TO_UPGRADE;
}

public class MasHPEachFloor : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.INCREASE_MAX_HP_EACH_FLOOR;
}

public class Aliensrock : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.TARGETED_ATTACKS_HIT_ALL;
}

public class HaglinsHat : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.COINS_PROVIDE_HEALING;
}

public class FlauntyGauntlets : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.COINS_PROVIDE_BALLUSION;
}

public class SteadyScope : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BALLUSION_GUARANTEED_CRIT;
}

public class SpiffyCrit : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ATTACKS_APPLY_EXPLOITABALL;
}

public class SubtractionReaction : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.DAMAGE_CREATES_DAMAGE_REDUCTION_SLIME;
}

public class CounterfeitCrits : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.HITTING_CRIT_ADDS_TEMP_CRITS;
}

public class ClearTheWay : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ATTACKS_APPLY_TRANSPHERENCY;
}

public class Balladroit : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.START_WITH_BALLWARK_FOR_ORBS_IN_DECK;
}

public class TrainingTabard : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.GAINING_BALLWARK_GIVES_MUSCIRCLE;
}

public class AddedAdvantarge : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.GAINING_MUSCIRCLE_GIVES_BALLWARK;
}

public class ShieldBasher : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.SHIELD_PEGS_ADD_DAMAGE;
}

public class MinibossMeetup : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ONLY_MINIBOSSES;
}

public class Spinsepsion : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.SPINFECTION_INC_OVER_TIME;
}

public class DebuffDistractor : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.STATUS_EFFECTS_ADD_DODGE;
}

public class TornSash : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.HALVE_INCOMING_DAMAGE;
}

public class CalloftheVoid : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ALL_ORBS_DELETE_PEGS;
}

public class ReverseProjectile : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADDITIONAL_REVERSE_PROJECTILE_ATTACK;
}

public class BombToDull : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BOMB_TO_DULL;
}

public class CrystalCatalyst : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.MORE_SPINFECTION_DAMAGE_PER_ACT;
}

public class SproutingSpinvestment : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.GAIN_PERCENTAGE_OF_GOLD_EACH_EVENT;
}

public class EffectiveCriticism : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.RANDOM_STATUS_EFFECT_ON_CRIT;
}

public class HeavyHand : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ORBS_MULTIHIT_BUT_LESS_AIM;
}
