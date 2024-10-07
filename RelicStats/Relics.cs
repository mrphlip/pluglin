using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

[HarmonyPatch]
public class Cookie : HealingCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.HEAL_ON_REFRESH_POTION;
	[HarmonyPatch(typeof(Battle.PlayerHealthController), "CheckForRelicOnRefreshPotion")]
	[HarmonyPrefix]
	private static void Enable() {
		Cookie t = (Cookie)Tracker.trackers[Relics.RelicEffect.HEAL_ON_REFRESH_POTION];
		t._active = true;
	}
	[HarmonyPatch(typeof(Battle.PlayerHealthController), "CheckForRelicOnRefreshPotion")]
	[HarmonyPostfix]
	private static void Disable() {
		Cookie t = (Cookie)Tracker.trackers[Relics.RelicEffect.HEAL_ON_REFRESH_POTION];
		t._active = false;
	}
}

[HarmonyPatch]
public class WellDoneSteak : HealingCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.HEAL_ON_RELOAD;
	[HarmonyPatch(typeof(Battle.PlayerHealthController), "CheckForRelicOnReload")]
	[HarmonyPrefix]
	private static void Enable() {
		WellDoneSteak t = (WellDoneSteak)Tracker.trackers[Relics.RelicEffect.HEAL_ON_RELOAD];
		t._active = true;
	}
	[HarmonyPatch(typeof(Battle.PlayerHealthController), "CheckForRelicOnReload")]
	[HarmonyPostfix]
	private static void Disable() {
		WellDoneSteak t = (WellDoneSteak)Tracker.trackers[Relics.RelicEffect.HEAL_ON_RELOAD];
		t._active = false;
	}
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
	private bool _active = false;
	public void Disable() {
		_active = false;
	}
	public override void Used() {
		_active = true;
	}
	public void DamageAvoided(float damage) {
		if (_active)
			count += (int)damage;
		_active = false;
	}
	public override string Tooltip => $"{count} <style=damage>damage avoided</style>";
}

[HarmonyPatch]
public class Refillibuster : DamageAllCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.REFRESH_DAMAGES_PEG_COUNT;
	[HarmonyPatch(typeof(Battle.PegManager), "ResetPegs")]
	[HarmonyPostfix]
	private static void AfterResetPegs() {
		((Refillibuster)Tracker.trackers[Relics.RelicEffect.REFRESH_DAMAGES_PEG_COUNT])._active = false;
	}
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
	[HarmonyPatch(typeof(BattleController), "HandlePegActivated")]
	[HarmonyPrefix]
	private static void Disable() {
		StrangeBrew t = (StrangeBrew)Tracker.trackers[Relics.RelicEffect.POTION_PEGS_COUNT];
		t._active = false;
	}
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

[HarmonyPatch]
public class SealedConviction : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.NO_DISCARD;
	public override int Step => Relics.RelicManager.SEALED_CONVICTION_BALLANCE;
	private bool _active = false;
	public override void Used() {
		if (_active)
			count += Step;
	}
	[HarmonyPatch(typeof(Battle.StatusEffects.PlayerStatusEffectController), "ApplyStartingBonuses")]
	[HarmonyPrefix]
	private static void BattleStartPre() {
		SealedConviction t = (SealedConviction)Tracker.trackers[Relics.RelicEffect.NO_DISCARD];
		t._active = true;
	}
	[HarmonyPatch(typeof(Battle.StatusEffects.PlayerStatusEffectController), "ApplyStartingBonuses")]
	[HarmonyPostfix]
	private static void BattleStartPost() {
		SealedConviction t = (SealedConviction)Tracker.trackers[Relics.RelicEffect.NO_DISCARD];
		t._active = false;
	}
	[HarmonyPatch(typeof(BattleController), "ShuffleDeck")]
	[HarmonyPrefix]
	private static void ReloadPre() {
		SealedConviction t = (SealedConviction)Tracker.trackers[Relics.RelicEffect.NO_DISCARD];
		t._active = true;
	}
	[HarmonyPatch(typeof(BattleController), "ShuffleDeck")]
	[HarmonyPostfix]
	private static void ReloadPost() {
		SealedConviction t = (SealedConviction)Tracker.trackers[Relics.RelicEffect.NO_DISCARD];
		t._active = false;
	}
	public override string Tooltip => $"{count} <style=balance>ballance added</style>";
}

public class Electropegnet : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.PEG_MAGNET;
}

[HarmonyPatch]
public class SuperBoots : HealingCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.SUPER_BOOTS;
	[HarmonyPatch(typeof(Battle.PostBattleController), "TriggerVictory")]
	[HarmonyPrefix]
	private static void Enable() {
		SuperBoots t = (SuperBoots)Tracker.trackers[Relics.RelicEffect.SUPER_BOOTS];
		t._active = true;
	}
	[HarmonyPatch(typeof(Battle.PostBattleController), "TriggerVictory")]
	[HarmonyPostfix]
	private static void Disable() {
		SuperBoots t = (SuperBoots)Tracker.trackers[Relics.RelicEffect.SUPER_BOOTS];
		t._active = false;
	}
}

public class SpecialButton : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADDITIONAL_CRIT1;
}

public class FreshBandana : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADDITIONAL_REFRESH1;
}

public class MonsterTraining : PegDamageCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.LOW_HEALTH_INCREASED_DAMAGE;
	public override void StartAddPeg() {
		_active = false;
	}
	public override void Used() {
		_active = true;
	}
	public override void AddPeg(float multiplier, int bonus) {
		// Only half the multiplier, and none of the bonus, can be attributed to this relic
		base.AddPeg(multiplier / 2f, 0);
	}
}

public class Refreshiv : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.REFRESH_BOARD_ON_ENEMY_KILLED;
	public override string Tooltip => $"{count} refresh{Utils.Plural(count, "es")}";
}

public class TacticalTreat : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.SHUFFLE_REFRESH_PEG;
}

public class UnicornHorn : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.LONGER_AIMER;
}

public class RallyingHeart : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADDITIONAL_END_BATTLE_HEAL;
	// TODO: A bit tricky, saving this one for later
}

public class SufferTheSling : OrbDamageCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BASIC_STONE_BONUS_DAMAGE;
}

public class SandArrows : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ATTACKS_DEAL_BLIND;
	public override void Used() {}
	public override string Tooltip => $"{count} <style=blind>Blind applied</style>";
}

public class Overwhammer : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.NORMAL_ATTACKS_OVERFLOW;
	// TODO: the relics that affect projectiles are going to be a pain
}

public class InconspicuousRing : PegDamageCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BOUNCERS_COUNT;
}

public class OldGardenerGloves : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.DAMAGE_BONUS_PLANT_FLAT;
	public override int Step => 10;
	public override string Tooltip => $"{count} <style=damage>damage added</style>";
}

[HarmonyPatch]
public class SlimySalve : HealingCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.APPLIES_HEALING_SLIME;
	private bool _sliming = false;
	private HashSet<int> slimedPegs = new HashSet<int>();
	public override void Reset() {
		base.Reset();
		_sliming = false;
		slimedPegs.Clear();
	}
	public override void Used() {
		_sliming = true;
	}
	[HarmonyPatch(typeof(RegularPeg), "CheckAndApplySlime")]
	[HarmonyPostfix]
	private static void CheckApplySlimeRegularPost() {
		SlimySalve t = (SlimySalve)Tracker.trackers[Relics.RelicEffect.APPLIES_HEALING_SLIME];
		t._sliming = false;
	}
	[HarmonyPatch(typeof(LongPeg), "CheckAndApplySlime")]
	[HarmonyPostfix]
	private static void CheckApplySlimeLongPost() {
		SlimySalve t = (SlimySalve)Tracker.trackers[Relics.RelicEffect.APPLIES_HEALING_SLIME];
		t._sliming = false;
	}
	[HarmonyPatch(typeof(RegularPeg), "ApplySlimeToPeg")]
	[HarmonyPostfix]
	private static void ApplySlimeRegular(RegularPeg __instance, Peg.SlimeType sType) {
		if (sType != Peg.SlimeType.HealSlime)
			return;
		SlimySalve t = (SlimySalve)Tracker.trackers[Relics.RelicEffect.APPLIES_HEALING_SLIME];
		if (!t._sliming)
			return;
		t.slimedPegs.Add(__instance.gameObject.GetInstanceID());
		t._sliming = false;
	}
	[HarmonyPatch(typeof(LongPeg), "ApplySlimeToPeg")]
	[HarmonyPostfix]
	private static void ApplySlimeLong(LongPeg __instance, Peg.SlimeType sType) {
		if (sType != Peg.SlimeType.HealSlime)
			return;
		SlimySalve t = (SlimySalve)Tracker.trackers[Relics.RelicEffect.APPLIES_HEALING_SLIME];
		if (!t._sliming)
			return;
		t.slimedPegs.Add(__instance.gameObject.GetInstanceID());
		t._sliming = false;
	}
	[HarmonyPatch(typeof(Battle.PegManager), "InitializePegs")]
	[HarmonyPostfix]
	private static void NewBattle() {
		SlimySalve t = (SlimySalve)Tracker.trackers[Relics.RelicEffect.APPLIES_HEALING_SLIME];
		t.slimedPegs.Clear();
		t._sliming = false;
	}
	[HarmonyPatch(typeof(RegularPeg), "PegActivated")]
	[HarmonyPrefix]
	private static void EnableRegular(RegularPeg __instance) {
		SlimySalve t = (SlimySalve)Tracker.trackers[Relics.RelicEffect.APPLIES_HEALING_SLIME];
		if (!t.slimedPegs.Contains(__instance.gameObject.GetInstanceID()))
			return;
		if (__instance.slimeType != Peg.SlimeType.HealSlime) {
			t.slimedPegs.Remove(__instance.gameObject.GetInstanceID());
			return;
		}
		t._active = true;
	}
	[HarmonyPatch(typeof(RegularPeg), "PegActivated")]
	[HarmonyPostfix]
	private static void DisableRegular() {
		SlimySalve t = (SlimySalve)Tracker.trackers[Relics.RelicEffect.APPLIES_HEALING_SLIME];
		t._active = false;
	}
	[HarmonyPatch(typeof(LongPeg), "PegActivated")]
	[HarmonyPrefix]
	private static void EnableLong(LongPeg __instance) {
		SlimySalve t = (SlimySalve)Tracker.trackers[Relics.RelicEffect.APPLIES_HEALING_SLIME];
		if (!t.slimedPegs.Contains(__instance.gameObject.GetInstanceID()))
			return;
		if (__instance.slimeType != Peg.SlimeType.HealSlime) {
			t.slimedPegs.Remove(__instance.gameObject.GetInstanceID());
			return;
		}
		t._active = true;
	}
	[HarmonyPatch(typeof(LongPeg), "PegActivated")]
	[HarmonyPostfix]
	private static void DisableLong() {
		SlimySalve t = (SlimySalve)Tracker.trackers[Relics.RelicEffect.APPLIES_HEALING_SLIME];
		t._active = false;
	}
}

[HarmonyPatch]
public class InfernalIngot : HealingCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.LIFESTEAL_PEG_HITS;
	private bool _damageActive = false;
	private int damageCount = 0;
	public override void Reset() {
		base.Reset();
		_damageActive = false;
		damageCount = 0;
	}
	public override void Used() {
		_active = true;
		_damageActive = true;
	}
	[HarmonyPatch(typeof(Battle.TargetingManager), "HandlePegActivated")]
	[HarmonyPostfix]
	private static void Disable() {
		InfernalIngot t = (InfernalIngot)Tracker.trackers[Relics.RelicEffect.LIFESTEAL_PEG_HITS];
		t._active = false;
		t._damageActive = false;
	}
	public void Damage(float amount) {
		if (_damageActive)
			damageCount += (int)amount;
		_damageActive = false;
	}
	public override string Tooltip => $"{damageCount} <style=damage>damage dealt</style>; {base.Tooltip}";
}

[HarmonyPatch]
public class MentalMantle : DamageTargetedCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.DAMAGE_TARGETED_PEG_HITS;
	[HarmonyPatch(typeof(Battle.TargetingManager), "HandlePegActivated")]
	[HarmonyPostfix]
	private static void Disable() {
		MentalMantle t = (MentalMantle)Tracker.trackers[Relics.RelicEffect.DAMAGE_TARGETED_PEG_HITS];
		t._active = false;
	}
}

[HarmonyPatch]
public class PoppingCorn : HealingCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.HEAL_ON_PEG_HITS;
	[HarmonyPatch(typeof(Battle.PlayerHealthController), "HandlePegActivated")]
	[HarmonyPrefix]
	private static void Enable() {
		PoppingCorn t = (PoppingCorn)Tracker.trackers[Relics.RelicEffect.HEAL_ON_PEG_HITS];
		t._active = true;
	}
	[HarmonyPatch(typeof(Battle.PlayerHealthController), "HandlePegActivated")]
	[HarmonyPostfix]
	private static void Disable() {
		PoppingCorn t = (PoppingCorn)Tracker.trackers[Relics.RelicEffect.HEAL_ON_PEG_HITS];
		t._active = false;
	}
}

[HarmonyPatch]
public class WeaponizedEnvy : DamageTargetedCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.DAMAGE_TARGETED_ON_HEAL;
	[HarmonyPatch(typeof(Battle.TargetingManager), "HandlePlayerHealed")]
	[HarmonyPostfix]
	private static void Disable() {
		WeaponizedEnvy t = (WeaponizedEnvy)Tracker.trackers[Relics.RelicEffect.DAMAGE_TARGETED_ON_HEAL];
		t._active = false;
	}
}

public class WandOfSkulltimateWrath : PegDamageCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.DOUBLE_DAMAGE_HURT_ON_PEG;
	public override void StartAddPeg() {
		_active = false;
	}
	public override void Used() {}
	public override void Checked() {
		_active = true;
	}
	public override void AddPeg(float multiplier, int bonus) {
		base.AddPeg(multiplier / 2f, 0);
	}

	private bool _selfDamageActive = false;
	private int selfDamageCount = 0;
	public override void Reset() {
		base.Reset();
		_selfDamageActive = false;
		selfDamageCount = 0;
	}
	[HarmonyPatch(typeof(Battle.PlayerHealthController), "HandlePegActivated")]
	[HarmonyPrefix]
	private static void Enable() {
		WandOfSkulltimateWrath t = (WandOfSkulltimateWrath)Tracker.trackers[Relics.RelicEffect.DOUBLE_DAMAGE_HURT_ON_PEG];
		t._selfDamageActive = true;
	}
	[HarmonyPatch(typeof(Battle.PlayerHealthController), "HandlePegActivated")]
	[HarmonyPostfix]
	private static void Disable() {
		WandOfSkulltimateWrath t = (WandOfSkulltimateWrath)Tracker.trackers[Relics.RelicEffect.DOUBLE_DAMAGE_HURT_ON_PEG];
		t._selfDamageActive = false;
	}
	public virtual void SelfDamage(float amount) {
		if (_selfDamageActive)
			selfDamageCount += (int)amount;
		_selfDamageActive = false;
	}
	public override string Tooltip => $"{base.Tooltip}; {selfDamageCount} <style=damage>self-damage</style>";
}

public class RingOfReuse : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ALL_ORBS_PERSIST;
}

public class EchoChamber : OrbDamageCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ALL_ATTACKS_ECHO;
}

public class GrabbyHand : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.FLYING_HORIZONTAL_PIERCE;
	// TODO: the relics that affect projectiles are going to be a pain
}

public class KnifesEdge : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.LOW_HEALTH_GUARANTEED_CRIT;
	public override string Tooltip => $"{count} crit{Utils.Plural(count)}";
}

public class BasicBlade : OrbDamageCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.NON_CRIT_BONUS_DMG;
}

public class CritsomallosFleece : OrbDamageCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.CRITS_STACK;
}

[HarmonyPatch]
public class EyeOfTurtle : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADDITIONAL_ORB_RELIC_OPTIONS;
	private bool orbActive = false, relicActive = false;
	private int orbCount = 0, relicCount = 0;
	public override void Reset() {
		orbActive = relicActive = false;
		orbCount = relicCount = 0;
	}
	public override void Used() {
		if (orbActive)
			orbCount++;
		if (relicActive)
			relicCount++;
		orbActive = relicActive = false;
	}
	[HarmonyPatch(typeof(PopulateSuggestionOrbs), "GenerateAddableOrbs")]
	[HarmonyPrefix]
	private static void EnableOrb() {
		EyeOfTurtle t = (EyeOfTurtle)Tracker.trackers[Relics.RelicEffect.ADDITIONAL_ORB_RELIC_OPTIONS];
		t.orbActive = true;
		Peglintuition t2 = (Peglintuition)Tracker.trackers[Relics.RelicEffect.ADDITIONAL_PEGLIN_CHOICES];
		t2.orbActive = true;
	}
	[HarmonyPatch(typeof(PopulateSuggestionOrbs), "GenerateAddableOrbs")]
	[HarmonyPostfix]
	private static void DisableOrb() {
		EyeOfTurtle t = (EyeOfTurtle)Tracker.trackers[Relics.RelicEffect.ADDITIONAL_ORB_RELIC_OPTIONS];
		t.orbActive = false;
		Peglintuition t2 = (Peglintuition)Tracker.trackers[Relics.RelicEffect.ADDITIONAL_PEGLIN_CHOICES];
		t2.orbActive = false;
	}
	[HarmonyPatch(typeof(PeglinUI.PostBattle.BattleUpgradeCanvas), "SetupRelicGrant")]
	[HarmonyPrefix]
	private static void EnableRelic(bool isTreasure) {
		EyeOfTurtle t = (EyeOfTurtle)Tracker.trackers[Relics.RelicEffect.ADDITIONAL_ORB_RELIC_OPTIONS];
		t.relicActive = !isTreasure;
		Peglintuition t2 = (Peglintuition)Tracker.trackers[Relics.RelicEffect.ADDITIONAL_PEGLIN_CHOICES];
		t2.relicActive = true;
	}
	[HarmonyPatch(typeof(PeglinUI.PostBattle.BattleUpgradeCanvas), "SetupRelicGrant")]
	[HarmonyPostfix]
	private static void DisableRelic() {
		EyeOfTurtle t = (EyeOfTurtle)Tracker.trackers[Relics.RelicEffect.ADDITIONAL_ORB_RELIC_OPTIONS];
		t.relicActive = false;
		Peglintuition t2 = (Peglintuition)Tracker.trackers[Relics.RelicEffect.ADDITIONAL_PEGLIN_CHOICES];
		t2.relicActive = false;
	}
	public override string Tooltip => $"{orbCount} extra orb{Utils.Plural(orbCount)}; {relicCount} extra relic{Utils.Plural(relicCount)}";
}

public class GloriousSuffeRing : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ALL_ORBS_BUFF;
	// TODO: Tracking of relics that buff/debuff pegs
}

public class SapperSack : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ALL_BOMBS_RIGGED;
}

public class Bombulet : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.DOUBLE_BOMBS_ON_MAP;
}

public class BombBaton : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADDITIONAL_STARTING_BOMBS;
}

public class PowderCollector : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.SPAWN_BOMB_ON_PEG_HITS;
	public override string Tooltip => $"{count} <sprite name=\"BOMB_REGULAR\"> created";
}

[HarmonyPatch]
public class BadCheese : DamageAllCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.DAMAGE_ENEMIES_ON_RELOAD;
	[HarmonyPatch(typeof(BattleController), "DealCheeseDamage")]
	[HarmonyPrefix]
	private static void Enable() {
		((BadCheese)Tracker.trackers[Relics.RelicEffect.DAMAGE_ENEMIES_ON_RELOAD])._active = true;
	}
	[HarmonyPatch(typeof(BattleController), "DealCheeseDamage")]
	[HarmonyPostfix]
	private static void Disable() {
		((BadCheese)Tracker.trackers[Relics.RelicEffect.DAMAGE_ENEMIES_ON_RELOAD])._active = false;
	}
}

[HarmonyPatch]
public class RingOfPain : DamageAllCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.DAMAGE_RETURN;
	[HarmonyPatch(typeof(Battle.PlayerHealthController), "AttemptDamageReturn")]
	[HarmonyPrefix]
	private static void Enable() {
		((RingOfPain)Tracker.trackers[Relics.RelicEffect.DAMAGE_RETURN])._active = true;
	}
	[HarmonyPatch(typeof(Battle.PlayerHealthController), "AttemptDamageReturn")]
	[HarmonyPostfix]
	private static void Disable() {
		((RingOfPain)Tracker.trackers[Relics.RelicEffect.DAMAGE_RETURN])._active = false;
	}
}

public class PerfectedReactant : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADDITIONAL_BOMB_DAMAGE2;
	public override int Step => (int)(Utils.EnemyDamageCount() * Relics.RelicManager.ADDITIONAL_BOMB_DAMAGE2);
	public override string Tooltip => $"{count} <style=damage>damage added</style>";
}

public class DodgyShortcut : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.CRIT_PIT;
}

public class CrumpledCharacterSheet : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.RANDOM_ENEMY_HEALTH;
}

public class CurseOfThePeglinKing : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ALTERNATE_SHOT_POWER;
}

public class AncientMeteorite : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.LEGACY_METEORITE;
	public override string Tooltip => $"{count} explosive force{Utils.Plural(count)}";
}

public class DumbBell : StatusEffectCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.STR_ON_RELOAD;
	public override Battle.StatusEffects.StatusEffectType type => Battle.StatusEffects.StatusEffectType.Strength;
}

public class TheCake : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.MAX_HEALTH_LARGE;
}

public class Refreshield : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.REFRESH_BOARD_ON_RELOAD;
	public override string Tooltip => $"{count} refresh{Utils.Plural(count, "es")}";
}

public class Puppet : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.PREVENT_FIRST_DAMAGE;
	public override void Used() {}
	public void DamageAvoided(float damage) {
		count += (int)damage;
	}
	public override string Tooltip => $"{count} <style=damage>damage avoided</style>";
}

public class ComplexClaw : OrbDamageCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.CRIT_BONUS_DMG;
}

public class IntentionalOboe : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.REDUCE_LOST_HEALTH;
	public override string Tooltip => $"{count} <style=damage>damage avoided</style>";
}

public class SpiralSlayer : StatusEffectCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.START_WITH_STR;
	public override Battle.StatusEffects.StatusEffectType type => Battle.StatusEffects.StatusEffectType.Strength;
}

public class ShrewdScales : StatusEffectCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BAL_ON_RELOAD;
	public override Battle.StatusEffects.StatusEffectType type => Battle.StatusEffects.StatusEffectType.Balance;
}

public class ConsumingChalice : OrbDamageCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.REDUCE_REFRESH;
}

public class UnpretentiousPendant : OrbDamageCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.REDUCE_CRIT;
}

public class SmokeMod : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BOMBS_APPLY_BLIND;
	public override string Tooltip => $"{count} <style=balance>Blind applied</style>";
}

public class PocketSand : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BLIND_WHEN_HIT;
	public override int Step => 15;
	public override string Tooltip => $"{count} <style=balance>Blind applied</style>";
}

public class BetsysHedge : OrbDamageCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.HEDGE_BETS;
}

public class ShortStack : OrbDamageCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ALL_IN_RELIC;
}

public class PumpkinPi : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.SLOT_PORTAL;
	// TODO: No easy hook here, would have to do more code injection
}

public class HaglinsSatchel : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADD_ORBS_AND_UPGRADE;
}

public class SafetyNet : PegDamageCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.MINIMUM_PEGS;
	private int counter = 0;
	public override void Used() {
		_active = true;
		counter = 5;
	}
	public override void AddPeg(float multiplier, int bonus) {
		base.AddPeg(multiplier, bonus);
		if (--counter > 0)
			_active = true;
	}
}

public class AncientFleece : OrbDamageCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ANCIENT_FLEECE;
}

public class RefresherCourse : Tracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.REFRESH_BUFF;
	private int musCount = 0, spinCount = 0;
	private bool _active = false;

	public override void Reset() {
		musCount = spinCount = 0;
	}
	public override void Used() {
		_active = true;
	}
	public void ApplyStatusEffect(Battle.StatusEffects.StatusEffect statusEffect) {
		if (!_active)
			return;
		if (statusEffect.EffectType == Battle.StatusEffects.StatusEffectType.Strength)
			musCount += statusEffect.Intensity;
		else if (statusEffect.EffectType == Battle.StatusEffects.StatusEffectType.Finesse)
			spinCount += statusEffect.Intensity;
		_active = false;
	}
	public override object State {
		get => (musCount, spinCount);
		set {
			(musCount, spinCount) = ((int, int))value;
		}
	}
	public override string Tooltip => $"{musCount} <style=strength>Muscircle added</style>; {spinCount} <style=finesse>Spinesse added</style>";
}

public class HerosBackpack : OrbDamageCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADJACENCY_BONUS;
}

public class AimLimiter : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.AIM_LIMITER;
}

public class AxeMeAnything : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.NO_DAMAGE_REDUCTION;
	// TODO: Doing this for orbs seems simple enough, but for bombs have to dig deep
}

public class SeraphicShield : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.IMMORTAL;
	private bool _active = false;
	public void Disable() {
		_active = false;
	}
	public override void Used() {
		_active = true;
	}
	public void DamageAvoided(float damage) {
		if (_active)
			count += (int)damage;
		_active = false;
	}
	public override string Tooltip => $"{count} <style=damage>damage avoided</style>";
}

[HarmonyPatch]
public class Critikris : DamageAllCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.CRIT_DAMAGES_ENEMIES;
	[HarmonyPatch(typeof(BattleController), "ActivateCrit")]
	[HarmonyPostfix]
	private static void AfterActivateCrit() {
		((Critikris)Tracker.trackers[Relics.RelicEffect.CRIT_DAMAGES_ENEMIES])._active = false;
	}
}

public class Peglintuition : EyeOfTurtle {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADDITIONAL_PEGLIN_CHOICES;
}

public class MoltenGold : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADDITIONAL_BATTLE_GOLD;
}

[HarmonyPatch]
public class MoltenMantle : DamageTargetedCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.CONVERT_COIN_TO_DAMAGE;
	[HarmonyPatch(typeof(BattleController), "HandleCoinCollected")]
	[HarmonyPostfix]
	private static void Disable() {
		MoltenMantle t = (MoltenMantle)Tracker.trackers[Relics.RelicEffect.CONVERT_COIN_TO_DAMAGE];
		t._active = false;
	}
}

[HarmonyPatch]
public class Navigationflation : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.INCREASED_NAV_GOLD;
	private bool doingBomb = false;
	public override int Step { get {
		if (Tracker.HaveRelic(Relics.RelicEffect.CONVERT_COIN_TO_DAMAGE))
			return 0;
		if (doingBomb) {
			if (Tracker.HaveRelic(Relics.RelicEffect.BOMB_NAV_GOLD))
				return 15;
			else
				return 0;
		} else {
			return 3;
		}
	}}
	[HarmonyPatch(typeof(Bomb), "PegActivated")]
	[HarmonyPrefix]
	private static void StartBomb() {
		Navigationflation t = (Navigationflation)Tracker.trackers[Relics.RelicEffect.INCREASED_NAV_GOLD];
		t.doingBomb = true;
	}
	[HarmonyPatch(typeof(Bomb), "PegActivated")]
	[HarmonyPostfix]
	private static void EndBomb() {
		Navigationflation t = (Navigationflation)Tracker.trackers[Relics.RelicEffect.INCREASED_NAV_GOLD];
		t.doingBomb = false;
	}
	public override string Tooltip => $"{count} <sprite name=\"GOLD\"> added";
}

public class DuplicationPotion : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.DUPLICATE_SPECIAL_PEGS;
}

public class RefillosophersStone : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.CREATE_GOLD_ON_REFRESH;
}

[HarmonyPatch]
public class PeglinerosPendant : PegDamageCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.GOLD_ADDS_TO_DAMAGE;
	[HarmonyPatch(typeof(Battle.PegBehaviour.PegCoinOverlay), "TriggerCoinCollected")]
	[HarmonyPostfix]
	private static void Disable() {
		PeglinerosPendant t = (PeglinerosPendant)Tracker.trackers[Relics.RelicEffect.GOLD_ADDS_TO_DAMAGE];
		t._active = false;
	}
}

public class WandOfSkulltimateGreed : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.DOUBLE_COINS_AND_PRICES;
}

public class DefreshPotion : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ALL_ORBS_MORBID;
}

public class SaltShaker : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.INCREASE_MAX_HP_GAIN;
	public override string Tooltip => $"{count} max HP added";
}

[HarmonyPatch]
public class GardenersGloves : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.REDUCE_ORB_SELF_DAMAGE;
	private bool _active = false;
	private float _currentDamage = 0;
	public override void Used() {
		if (_active)
			count += (int)Mathf.Ceil(_currentDamage / 2);
		_active = false;
	}
	[HarmonyPatch(typeof(Battle.PlayerHealthController), "DealSelfDamage")]
	[HarmonyPrefix]
	private static void Enable(float damage) {
		GardenersGloves t = (GardenersGloves)Tracker.trackers[Relics.RelicEffect.REDUCE_ORB_SELF_DAMAGE];
		t._currentDamage = damage;
		t._active = true;
	}
	[HarmonyPatch(typeof(Battle.PlayerHealthController), "DealSelfDamage")]
	[HarmonyPostfix]
	private static void Disable(float damage) {
		GardenersGloves t = (GardenersGloves)Tracker.trackers[Relics.RelicEffect.REDUCE_ORB_SELF_DAMAGE];
		t._active = false;
	}
	public override string Tooltip => $"{count} <style=damage>damage avoided</style>";
}

public class GrindingMonstera : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.GAIN_MAX_HP_ON_ENEMY_DEFEAT;
	public override string Tooltip => $"{count} max HP added";
}

public class SashOfFocus : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.PREVENT_LETHAL_DAMAGE;
	public override void Used() {}
	public void DamageAvoided(float damage) {
		count += (int)damage;
	}
	public override string Tooltip => $"{count} <style=damage>damage avoided</style>";
}

public class PrimeRodOfFrost : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ALL_ATTACKS_PIERCE;
	// TODO: the relics that affect projectiles are going to be a pain
}

public class BasaltToadem : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.INC_MAX_HP_IF_FULL_HP;
	public override int Step => 4;
	public override string Tooltip => $"{count} max HP added";
}

public class DungeonDie : OrbDamageCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.RANDOMLY_ROLL_DAMAGE;
}

public class PerfectForger : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.UPGRADE_ADDED_ORBS;
	public override string Tooltip => $"{count} orb{Utils.Plural(count)} upgraded";
}

[HarmonyPatch]
public class BranchOfEmber : Tracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BLIND_BRAMBLE_COMBO;
	private bool _active = false;
	private int blindCount = 0, brambleCount = 0;
	public override void Reset() {
		_active = false;
		blindCount = brambleCount = 0;
	}
	public override void Used() {
		_active = true;
	}
	[HarmonyPatch(typeof(Battle.Enemies.Enemy), "CheckForKnockOnEffects")]
	[HarmonyPostfix]
	private static void Disable() {
		BranchOfEmber t = (BranchOfEmber)Tracker.trackers[Relics.RelicEffect.BLIND_BRAMBLE_COMBO];
		t._active = false;
	}
	[HarmonyPatch(typeof(Battle.Enemies.Enemy), "ApplyStatusEffect")]
	[HarmonyPrefix]
	private static void Apply(Battle.StatusEffects.StatusEffect statusEffect) {
		BranchOfEmber t = (BranchOfEmber)Tracker.trackers[Relics.RelicEffect.BLIND_BRAMBLE_COMBO];
		if (!t._active)
			return;
		if (statusEffect.EffectType == Battle.StatusEffects.StatusEffectType.Blind)
			t.blindCount += statusEffect.Intensity;
		else if (statusEffect.EffectType == Battle.StatusEffects.StatusEffectType.Thorned)
			t.brambleCount += statusEffect.Intensity;
		t._active = false;
	}
	public override object State {
		get => (blindCount, brambleCount);
		set {
			(blindCount, brambleCount) = ((int, int))value;
		}
	}
	public override string Tooltip => $"{blindCount} <style=blind>Blind applied</style>; {brambleCount} <style=bramble>Bramble applied</style>";
}

public class RipostalService : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BALLWARK_COUNTER;
	public override void Used() {}
	public void Damage(float damage) {
		count += (int)damage;
	}
	public override string Tooltip => $"{count} <style=damage>damage dealt</style>";
}

public class EssentialOil : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ARMOUR_PLUS_ONE;
	public override int Step => 2;
	public override string Tooltip => $"{count} <style=ballwark>Ballwark added</style>";
}

public class ArmourOnPegs : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ARMOUR_ON_PEGS_HIT;
}

public class StartWithBallwark : StatusEffectCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.START_WITH_BALLWARK;
	public override Battle.StatusEffects.StatusEffectType type => Battle.StatusEffects.StatusEffectType.Ballwark;
}

[HarmonyPatch]
public class OrbertsStory : Tracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.MORE_TREASURE_NODES;
	private int treasureCount = 0, rareCount = 0;
	public override void Reset() { treasureCount = rareCount = 0; }
	public override void Used() {}
	[HarmonyPatch(typeof(Map.MapController), "GetRandomScenario")]
	[HarmonyPrefix]
	private static void GetScenario(Map.SeededUnknownNodeData seededUnknownNodeData) {
		if (!Tracker.HaveRelic(Relics.RelicEffect.MORE_TREASURE_NODES))
			return;
		if (seededUnknownNodeData.typeRoll >= 0.05f && seededUnknownNodeData.typeRoll < 0.12f) {
			OrbertsStory t = (OrbertsStory)Tracker.trackers[Relics.RelicEffect.MORE_TREASURE_NODES];
			t.treasureCount++;
		}
	}
	[HarmonyPatch(typeof(Scenarios.ChestScenarioController), "OpenChest")]
	[HarmonyPrefix]
	private static void OpenChest(Scenarios.ChestScenarioController __instance) {
		if (!Tracker.HaveRelic(Relics.RelicEffect.MORE_TREASURE_NODES))
			return;
		float rarechance = Relics.RelicManager.CHEST_RARE_CHANCE;
		if (StaticGameData.dataToLoad is MapDataTreasure mapdata)
			rarechance = mapdata.rareChance;
		var nodedata = Refl<Map.SeededTreasureNodeData>.GetAttr(__instance, "_seededTreasureNodeData");
		if (nodedata != null && nodedata.rareRelicChanceRoll > rarechance && nodedata.rareRelicChanceRoll <= rarechance + 0.07f)
		{
			OrbertsStory t = (OrbertsStory)Tracker.trackers[Relics.RelicEffect.MORE_TREASURE_NODES];
			t.rareCount++;
		}
	}
	public override object State {
		get => (treasureCount, rareCount);
		set {
			(treasureCount, rareCount) = ((int, int))value;
		}
	}
	public override string Tooltip => $"{treasureCount} extra treasure{Utils.Plural(treasureCount)}; {rareCount} extra rare relic{Utils.Plural(rareCount)}";
}

[HarmonyPatch]
public class KnightCap : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.HEAL_WITH_BALLWARK;
	private bool _active = false;
	public override void Used() {
		_active = true;
	}
	[HarmonyPatch(typeof(PeglinUI.PostBattle.BattleUpgradeCanvas), "SetUpPostBattleOptions")]
	[HarmonyPostfix]
	private static void Disable() {
		KnightCap t = (KnightCap)Tracker.trackers[Relics.RelicEffect.HEAL_WITH_BALLWARK];
		t._active = false;
	}
	[HarmonyPatch(typeof(Battle.PlayerHealthController), "AdjustMaxHealth")]
	[HarmonyPrefix]
	private static void AddMaxHP(float amount) {
		KnightCap t = (KnightCap)Tracker.trackers[Relics.RelicEffect.HEAL_WITH_BALLWARK];
		if (t._active)
			t.count += (int)amount;
		t._active = false;
	}
	public override string Tooltip => $"{count} max HP added";
}

public class BallpeenHammer : StatusEffectCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADD_BALLWARK_WHEN_SHIELD_PEG_CREATED;
	public override Battle.StatusEffects.StatusEffectType type => Battle.StatusEffects.StatusEffectType.Ballwark;
}

public class FieryFurnace : StatusEffectCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADD_BALLWARK_WHEN_SHIELD_PEG_BROKEN;
	public override Battle.StatusEffects.StatusEffectType type => Battle.StatusEffects.StatusEffectType.Ballwark;
}

public class TrainingWeight : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.STRENGTH_PLUS_ONE;
	public override string Tooltip => $"{count} <style=strength>Muscircle added</style>";
}

public class BrassicaceaeKnuckles : StatusEffectCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.STRENGTH_ON_REFRESH;
	public override Battle.StatusEffects.StatusEffectType type => Battle.StatusEffects.StatusEffectType.Strength;
}

[HarmonyPatch]
public class Roundreloquence : Tracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.RANDOM_STATUS_EFFECT_ON_HIT;
	protected bool _active = false;
	private int brambleCount = 0, poisonCount = 0, blindCount = 0, exploitCount = 0, transpCount = 0;
	public override void Reset() {
		_active = false;
		brambleCount = poisonCount = blindCount = exploitCount = transpCount = 0;
	}
	public override void Used() {}
	[HarmonyPatch(typeof(Battle.Attacks.AttackBehaviours.AddRandomStatusEffectOnHit), "AffectEnemy")]
	[HarmonyPrefix]
	private static void Enable() {
		Roundreloquence t = (Roundreloquence)Tracker.trackers[Relics.RelicEffect.RANDOM_STATUS_EFFECT_ON_HIT];
		t._active = true;
	}
	[HarmonyPatch(typeof(Battle.Attacks.AttackBehaviours.AddRandomStatusEffectOnHit), "AffectEnemy")]
	[HarmonyPostfix]
	private static void Disable() {
		Roundreloquence t = (Roundreloquence)Tracker.trackers[Relics.RelicEffect.RANDOM_STATUS_EFFECT_ON_HIT];
		t._active = false;
	}
	[HarmonyPatch(typeof(Battle.Enemies.Enemy), "ApplyStatusEffect")]
	[HarmonyPrefix]
	private static void ApplyStatusHook(Battle.StatusEffects.StatusEffect statusEffect) {
		Roundreloquence t = (Roundreloquence)Tracker.trackers[Relics.RelicEffect.RANDOM_STATUS_EFFECT_ON_HIT];
		t.ApplyStatus(statusEffect);
		EffectiveCriticism t2 = (EffectiveCriticism)Tracker.trackers[Relics.RelicEffect.RANDOM_STATUS_EFFECT_ON_CRIT];
		t2.ApplyStatus(statusEffect);
	}
	protected void ApplyStatus(Battle.StatusEffects.StatusEffect statusEffect) {
		if (!_active)
			return;
		if (statusEffect.EffectType == Battle.StatusEffects.StatusEffectType.Thorned)
			brambleCount += statusEffect.Intensity;
		else if (statusEffect.EffectType == Battle.StatusEffects.StatusEffectType.Poison)
			poisonCount += statusEffect.Intensity;
		else if (statusEffect.EffectType == Battle.StatusEffects.StatusEffectType.Blind)
			blindCount += statusEffect.Intensity;
		else if (statusEffect.EffectType == Battle.StatusEffects.StatusEffectType.Exploitaball)
			exploitCount += statusEffect.Intensity;
		else if (statusEffect.EffectType == Battle.StatusEffects.StatusEffectType.Transpherency)
			transpCount += statusEffect.Intensity;
		else
			Plugin.Logger.LogWarning($"Unexpected debuff from Roundreloquence: {statusEffect.EffectType}");
		_active = false;
	}
	public override object State {
		get => (brambleCount, poisonCount, blindCount, exploitCount, transpCount);
		set {
			(brambleCount, poisonCount, blindCount, exploitCount, transpCount) = ((int, int, int, int, int))value;
		}
	}
	public override string Tooltip => $"{brambleCount} <style=bramble>Bramble</style>; {poisonCount} <style=poison>Spinfection</style>; {blindCount} <style=blind>Blind</style>; {exploitCount} <style=exploitaball>Exploitaball</style>; {transpCount} <style=transpherency>Transpherency</style>";
}

public class MaskOfSorrow : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.INCREASE_NEGATIVE_STATUS;
	public override string Tooltip => $"{count} debuff{Utils.Plural(count)} increased";
}

[HarmonyPatch]
public class MaskOfJoy : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.INCREASE_POSITIVE_STATUS;
	private bool _active = true;
	public override void Used() {
		if (_active)
			base.Used();
	}
	[HarmonyPatch(typeof(Battle.StatusEffects.PlayerStatusEffectController), "CheckRelicIntensityEffects")]
	[HarmonyPrefix]
	private static void Disable(Battle.StatusEffects.PlayerStatusEffectController __instance, Battle.StatusEffects.StatusEffect statusEffect) {
		// incorrect ordering of conditions here makes Used() get called incorrectly
		Battle.StatusEffects.StatusEffectType[] positives = Refl<Battle.StatusEffects.StatusEffectType[]>.GetAttr(__instance, "_positiveStatusEffects");
		if (!Enumerable.Contains(positives, statusEffect.EffectType) || statusEffect.EffectType == Battle.StatusEffects.StatusEffectType.Ballwark || statusEffect.Intensity <= 0) {
			MaskOfJoy t = (MaskOfJoy)Tracker.trackers[Relics.RelicEffect.INCREASE_POSITIVE_STATUS];
			t._active = false;
		}
	}
	[HarmonyPatch(typeof(Battle.StatusEffects.PlayerStatusEffectController), "CheckRelicIntensityEffects")]
	[HarmonyPostfix]
	private static void Enable() {
		MaskOfJoy t = (MaskOfJoy)Tracker.trackers[Relics.RelicEffect.INCREASE_POSITIVE_STATUS];
		t._active = true;
	}
	public override string Tooltip => $"{count} buff{Utils.Plural(count)} increased";
}

public class GrubbyGloves : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ATTACKS_DEAL_POISON;
	public override string Tooltip => $"{count} <style=poison>Spinfection applied</style>";
}

public class ChokeMod : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BOMBS_APPLY_POISON;
	public override string Tooltip => $"{count} <style=poison>Spinfection applied</style>";
}

public class AuAuger : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.CRITS_PROVIDE_PIERCE;
	// TODO: the relics that affect projectiles are going to be a pain
}

public class BeckoningCrit : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.COINS_TO_CRITS;
	public override string Tooltip => $"{count} crit{Utils.Plural(count)}";
}

public class AGoodSlime : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.SLIME_BUFFS_PEGS;
	// TODO: Tracking of relics that buff/debuff pegs
}

public class Adventurine : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BUFF_FIRST_PEG_HIT;
	// TODO: Tracking of relics that buff/debuff pegs
}

[HarmonyPatch]
public class AliensRock : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.SPLASH_EFFECT_ON_TARGETED_ATTACKS;
	private static bool _active = false;
	private static Battle.Enemies.Enemy _lastEnemy;
	private static int _lastRange;
	private static Battle.Attacks.AoeAttack.AoeType _lastType;
	private static EnemyManager.SlotType _lastSlot;
	private static Battle.Enemies.Enemy[] _lastResult;
	public override void Reset() {
		base.Reset();
		ResetTemps();
	}
	private static void ResetTemps() {
		_active = false;
		_lastEnemy = null;
		_lastResult = null;
	}
	public override void Used() {}
	[HarmonyPatch(typeof(EnemyManager), "GetSplashRangeEnemies")]
	[HarmonyPostfix]
	private static void GetEnemies(Battle.Enemies.Enemy enemy, int range, Battle.Attacks.AoeAttack.AoeType aoeType, EnemyManager.SlotType slotType, Battle.Enemies.Enemy[] __result) {
		if (_active) {
			_lastEnemy = enemy;
			_lastRange = range;
			_lastType = aoeType;
			_lastSlot = slotType;
			_lastResult = __result;
		}
	}
	[HarmonyPatch(typeof(TargetedAttack), "HandleSpellHit")]
	[HarmonyPrefix]
	private static void EnableTarget() {
		_active = true;
	}
	[HarmonyPatch(typeof(TargetedAttack), "HandleSpellHit")]
	[HarmonyPostfix]
	private static void ProcessTarget(TargetedAttack __instance) {
		_active = false;
		EnemyManager enemyManager = Refl<Battle.Attacks.AttackManager>.GetAttr(__instance, "_attackManager").enemyManager;
		float hitDamage = Refl<float>.GetAttr(__instance, "_hitDamage");
		if (Tracker.HaveRelic(Relics.RelicEffect.TARGETED_ATTACKS_HIT_ALL) || Tracker.HaveRelic(Relics.RelicEffect.SPLASH_EFFECT_ON_TARGETED_ATTACKS)) {
			Battle.Enemies.Enemy[] notSplash = enemyManager.GetSplashRangeEnemies(_lastEnemy, 0, _lastType, _lastSlot);
			if (Tracker.HaveRelic(Relics.RelicEffect.TARGETED_ATTACKS_HIT_ALL))
				((OldAliensrock)Tracker.trackers[Relics.RelicEffect.TARGETED_ATTACKS_HIT_ALL]).Handle(_lastResult, notSplash, hitDamage);
			else
				((AliensRock)Tracker.trackers[Relics.RelicEffect.SPLASH_EFFECT_ON_TARGETED_ATTACKS]).Handle(_lastResult, notSplash, hitDamage);
		}
		ResetTemps();
	}
	[HarmonyPatch(typeof(Battle.Attacks.AoeAttack), "HandleSpellHit")]
	[HarmonyPrefix]
	private static void EnableAoe() {
		_active = true;
	}
	[HarmonyPatch(typeof(Battle.Attacks.AoeAttack), "HandleSpellHit")]
	[HarmonyPostfix]
	private static void ProcessAoe(Battle.Attacks.AoeAttack __instance) {
		_active = false;
		EnemyManager enemyManager = Refl<Battle.Attacks.AttackManager>.GetAttr(__instance, "_attackManager").enemyManager;
		float hitDamage = Refl<float>.GetAttr(__instance, "_hitDamage");
		if (Tracker.HaveRelic(Relics.RelicEffect.SPLASH_EFFECT_ON_TARGETED_ATTACKS)) {
			Battle.Enemies.Enemy[] notSplash = enemyManager.GetSplashRangeEnemies(_lastEnemy, _lastRange - 1, _lastType, _lastSlot);
			((AliensRock)Tracker.trackers[Relics.RelicEffect.SPLASH_EFFECT_ON_TARGETED_ATTACKS]).Handle(_lastResult, notSplash, hitDamage);
		}
		ResetTemps();
	}
	private void Handle(Battle.Enemies.Enemy[] enemiesHit, Battle.Enemies.Enemy[] enemiesIgnore, float damage) {
		int enemyCount = 0;
		foreach (var enemy in enemiesHit)
			if (enemy != null && !Enumerable.Contains(enemiesIgnore, enemy))
				enemyCount++;
		count += enemyCount * (int)damage;
	}
	public override string Tooltip => $"{count} <style=damage>damage added</style>";
}

public class Spinventoriginality : OrbDamageCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.UNIQUE_ORBS_BUFF;
	public override float GetBaseDamage(Battle.Attacks.Attack attack, Battle.Attacks.AttackManager attackManager, float[] dmgValues, float dmgMult, int dmgBonus, int critCount) {
		bool oldApplyUniqueBuff = Refl<bool>.GetAttr(attack, "applyUniqueBuff");
		Utils.SetAttr(attack, "applyUniqueBuff", false);
		float res = base.GetBaseDamage(attack, attackManager, dmgValues, dmgMult, dmgBonus, critCount);
		Utils.SetAttr(attack, "applyUniqueBuff", oldApplyUniqueBuff);
		return res;
	}
}

[HarmonyPatch]
public class SpheridaesFate : PegDamageCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.CREATE_SQUIRRELS_ON_PEGS_HIT;
	public override void Used() {}
	[HarmonyPatch(typeof(RegularPeg), "DoPegCollision")]
	[HarmonyPrefix]
	private static void Enable(PachinkoBall pachinko) {
		if (pachinko.name.Contains("Squirrelball")) {
			SpheridaesFate t = (SpheridaesFate)Tracker.trackers[Relics.RelicEffect.CREATE_SQUIRRELS_ON_PEGS_HIT];
			t._active = true;
		}
	}
	[HarmonyPatch(typeof(RegularPeg), "DoPegCollision")]
	[HarmonyPostfix]
	private static void Disable() {
		SpheridaesFate t = (SpheridaesFate)Tracker.trackers[Relics.RelicEffect.CREATE_SQUIRRELS_ON_PEGS_HIT];
		t._active = false;
	}

	[HarmonyPatch(typeof(LongPeg), "DoPegCollision")]
	[HarmonyPrefix]
	private static void EnableLong(PachinkoBall pachinko) { Enable(pachinko); }
	[HarmonyPatch(typeof(LongPeg), "DoPegCollision")]
	[HarmonyPostfix]
	private static void DisableLong() { Disable(); }
	[HarmonyPatch(typeof(IndestructiblePeg), "DoPegCollision")]
	[HarmonyPrefix]
	private static void EnableIndestructible(PachinkoBall pachinko) { Enable(pachinko); }
	[HarmonyPatch(typeof(IndestructiblePeg), "DoPegCollision")]
	[HarmonyPostfix]
	private static void DisableIndestructible() { Disable(); }
	[HarmonyPatch(typeof(SlimeOnlyPeg), "DoPegCollision")]
	[HarmonyPrefix]
	private static void EnableSlimeOnly(PachinkoBall pachinko) { Enable(pachinko); }
	[HarmonyPatch(typeof(SlimeOnlyPeg), "DoPegCollision")]
	[HarmonyPostfix]
	private static void DisableSlimeOnly() { Disable(); }
	[HarmonyPatch(typeof(Battle.BouncerPeg), "DoPegCollision")]
	[HarmonyPrefix]
	private static void EnableBouncer(PachinkoBall pachinko) { Enable(pachinko); }
	[HarmonyPatch(typeof(Battle.BouncerPeg), "DoPegCollision")]
	[HarmonyPostfix]
	private static void DisableBouncer() { Disable(); }
}

public class DuplicationStation : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.MULTIBALL_EVERY_X_SHOTS;
	public override string Tooltip => $"{count} orb{Utils.Plural(count)} duplicated";
}

public class PrimeSlime : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.RANDOM_SLIME_ON_PEGS_HIT;
	public override string Tooltip => $"{count} <sprite name=\"PEG\"> slimed";
}

public class HouseOfSlime : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BOUNCY_WALLS_AND_BOUNCERS;
}

public class LeafTheRestForLater : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ONLY_REFRESH_X_PEGS;
}

public class VitaminC : StatusEffectCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BALLUSION_ON_CRIT;
	public override Battle.StatusEffects.StatusEffectType type => Battle.StatusEffects.StatusEffectType.Ballusion;
}

public class IOU : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.GAMBLIN_IOU;
}

public class ReduceReFuseRecycle : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BOMB_NAV_GOLD;
	public override int Step { get {
		if (Tracker.HaveRelic(Relics.RelicEffect.CONVERT_COIN_TO_DAMAGE))
			return 0;
		if (Tracker.HaveRelic(Relics.RelicEffect.INCREASED_NAV_GOLD))
			return 20;
		else
			return 5;
	}}
	public override string Tooltip => $"{count} <sprite name=\"GOLD\"> added";
}

public class BallwarkToMuscircle : StatusEffectCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BALLWARK_TO_MUSCIRCLE;
	public override Battle.StatusEffects.StatusEffectType type => Battle.StatusEffects.StatusEffectType.Strength;
}

public class BeleagueredBoots : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BALLUSION_DOUBLE_MAX;
}

public class DoubleBallusion : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BALLUSION_DOUBLE_GAIN;
	// Not entirely sure what this one is supposed to do?
	// Seems to double ballusion gained, but also you lose all ballusion on dodge instead of half
	// Luckily, it doesn't actually exist
}

public class DodgyDagger : StatusEffectCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.SPINESSE_WHEN_DODGING;
	public override Battle.StatusEffects.StatusEffectType type => Battle.StatusEffects.StatusEffectType.Finesse;
}

public class StackedOrbacus : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.START_WITH_EXPLOITABALL;
}

public class FastReakaton : StatusEffectCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ADD_BALLUSION_WITH_SPAWNS;
	public override Battle.StatusEffects.StatusEffectType type => Battle.StatusEffects.StatusEffectType.Ballusion;
}

public class PieceOfMind : StatusEffectCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.LOSE_BALLWARK_GAIN_BALLANCE;
	public override Battle.StatusEffects.StatusEffectType type => Battle.StatusEffects.StatusEffectType.Balance;
}

public class DistractionReaction : StatusEffectCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.GAIN_BALLUSION_FROM_ENEMY_DMG;
	public override Battle.StatusEffects.StatusEffectType type => Battle.StatusEffects.StatusEffectType.Ballusion;
}

public class Redoublet : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BALLUSION_DODGE_CREATE_CRIT;
}

public class ConstrictingChains : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.AIM_LIMITER_MULTIBALL;
}

public class EndlessDevouRing : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ALL_ORBS_DEBUFF;
	// TODO: Tracking of relics that buff/debuff pegs
}

[HarmonyPatch]
public class BastionReaction : StatusEffectCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.LOSE_HP_GAIN_BALLWARK;
	public override Battle.StatusEffects.StatusEffectType type => Battle.StatusEffects.StatusEffectType.Ballwark;
}

public class IsDisYourCard : StatusEffectCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.DISCARD_GAIN_CRIT_BALLUSION;
	public override Battle.StatusEffects.StatusEffectType type => Battle.StatusEffects.StatusEffectType.Ballusion;
}

public class RefreshPerspective : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.REFRESH_UPGRADES_PEGS;
}

public class Mauliflower : StatusEffectCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.CRITS_FOR_SPINESSE;
	public override Battle.StatusEffects.StatusEffectType type => Battle.StatusEffects.StatusEffectType.Finesse;
}

public class ModestMallet : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.DISCARD_TO_UPGRADE;
	public override string Tooltip => $"{count} orb{Utils.Plural(count)} upgraded";
}

public class MaxHPEachFloor : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.INCREASE_MAX_HP_EACH_FLOOR;
	public override string Tooltip => $"{count} max HP added";
}

public class OldAliensrock : AliensRock {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.TARGETED_ATTACKS_HIT_ALL;
}

[HarmonyPatch]
public class HaglinsHat : HealingCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.COINS_PROVIDE_HEALING;
	[HarmonyPatch(typeof(Relics.RelicManager), "CoinsProvideHealingRelicCallback")]
	[HarmonyPrefix]
	private static void Enable() {
		HaglinsHat t = (HaglinsHat)Tracker.trackers[Relics.RelicEffect.COINS_PROVIDE_HEALING];
		t._active = true;
	}
	[HarmonyPatch(typeof(Relics.RelicManager), "CoinsProvideHealingRelicCallback")]
	[HarmonyPostfix]
	private static void Disable() {
		HaglinsHat t = (HaglinsHat)Tracker.trackers[Relics.RelicEffect.COINS_PROVIDE_HEALING];
		t._active = false;
	}
}

public class FlauntyGauntlets : StatusEffectCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.COINS_PROVIDE_BALLUSION;
	public override Battle.StatusEffects.StatusEffectType type => Battle.StatusEffects.StatusEffectType.Ballusion;
}

public class SteadyScope : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.BALLUSION_GUARANTEED_CRIT;
	public override string Tooltip => $"{count} crit{Utils.Plural(count)}";
}

[HarmonyPatch]
public class SpiffyCrit : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ATTACKS_APPLY_EXPLOITABALL;
	private bool _active = false;
	public override void Used() {
		_active = true;
	}
	[HarmonyPatch(typeof(Battle.Attacks.Attack), "GetStatusEffects")]
	[HarmonyPostfix]
	private static void GetStatus(List<Battle.StatusEffects.StatusEffect> __result) {
		SpiffyCrit t = (SpiffyCrit)Tracker.trackers[Relics.RelicEffect.ATTACKS_APPLY_EXPLOITABALL];
		if (!t._active)
			return;
		foreach (var e in __result) {
			if (e.EffectType == Battle.StatusEffects.StatusEffectType.Exploitaball) {
				t.count += e.Intensity;
			}
		}
		t._active = false;
	}
	public override string Tooltip => $"{count} <style=exploitaball>Exploitaball applied</style>";
}

public class SubtractionReaction : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.DAMAGE_CREATES_DAMAGE_REDUCTION_SLIME;
}

public class CounterfeitCrits : NoopTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.HITTING_CRIT_ADDS_TEMP_CRITS;
}

public class ClearTheWay : SimpleCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ATTACKS_APPLY_TRANSPHERENCY;
	public override int Step => Relics.RelicManager.ATTACKS_APPLY_TRANSPHERENCY_AMOUNT;
	public override void Checked() { Used(); }
	public override string Tooltip => $"{count} <style=transpherency>Transpherency applied</style>";
}

public class Balladroit : StatusEffectCounter {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.START_WITH_BALLWARK_FOR_ORBS_IN_DECK;
	public override Battle.StatusEffects.StatusEffectType type => Battle.StatusEffects.StatusEffectType.Ballwark;
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

public class CallOfTheVoid : TodoTracker {
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

[HarmonyPatch]
public class EffectiveCriticism : Roundreloquence {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.RANDOM_STATUS_EFFECT_ON_CRIT;
	public override void Used() {
		_active = true;
	}
	[HarmonyPatch(typeof(Battle.TargetingManager), "OnCritActivated")]
	[HarmonyPostfix]
	private static void Disable() {
		EffectiveCriticism t = (EffectiveCriticism)Tracker.trackers[Relics.RelicEffect.RANDOM_STATUS_EFFECT_ON_CRIT];
		t._active = false;
	}
}

public class HeavyHand : TodoTracker {
	public override Relics.RelicEffect Relic => Relics.RelicEffect.ORBS_MULTIHIT_BUT_LESS_AIM;
}
