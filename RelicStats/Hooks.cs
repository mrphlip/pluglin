using HarmonyLib;
using System.Collections.Generic;

namespace RelicStats;

[HarmonyPatch]
public class Hooks {
	[HarmonyPatch(typeof(GameInit), "Start")]
	[HarmonyPrefix]
	static private void StartGame(GameInit __instance) {
		LoadMapData loadData = Utils.GetAttr<GameInit, LoadMapData>(__instance, "LoadData");
		if (loadData.NewGame) {
			Plugin.Logger.LogInfo("New game, resetting all counters");
			Tracker.ResetAll();
		}
		else {
			Plugin.Logger.LogError("No handling for loading a game yet!");
			Tracker.ResetAll();
		}
	}

	[HarmonyPatch(typeof(Relics.RelicManager), "AddRelic")]
	[HarmonyPostfix]
	static private void AddRelic(Relics.Relic relic) {
		Tracker.AddRelic(relic.effect);
	}

	[HarmonyPatch(typeof(Relics.RelicManager), "AttemptUseRelic")]
	[HarmonyPostfix]
	static private void RelicUsed(Relics.RelicEffect re, bool __result) {
		if (__result) {
			Tracker tracker = null;
			if (Tracker.trackers.TryGetValue(re, out tracker)) {
				tracker.Used();
			}
		}
	}
	// todo: AttemptUseCountdownRelicManyTimes for COINS_PROVIDE_HEALING

	[HarmonyPatch(typeof(Relics.RelicManager), "RelicEffectActive")]
	[HarmonyPostfix]
	static private void RelicChecked(Relics.RelicEffect re, bool __result) {
		if (__result) {
			Tracker tracker = null;
			if (Tracker.trackers.TryGetValue(re, out tracker)) {
				tracker.Checked();
			}
		}
	}

	public static float? damageBeingDealt = null;

	[HarmonyPatch(typeof(Battle.PlayerHealthController), "Damage")]
	[HarmonyPrefix]
	static private void PlayerDamagePre(float damage) {
		damageBeingDealt = damage;
	}

	[HarmonyPatch(typeof(Battle.PlayerHealthController), "Damage")]
	[HarmonyPostfix]
	static private void PlayerDamagePost() {
		damageBeingDealt = null;
	}

	[HarmonyPatch(typeof(TargetedAttack), "Fire")]
	[HarmonyPrefix]
	private static void FireTargeted(TargetedAttack __instance, Battle.Attacks.AttackManager attackManager, float[] dmgValues, float dmgMult, int dmgBonus, int critCount)
		=> HandleFire(__instance, attackManager, dmgValues, dmgMult, dmgBonus, critCount);
	[HarmonyPatch(typeof(Battle.Attacks.ProjectileAttack), "Fire")]
	[HarmonyPrefix]
	private static void FireProjectile(Battle.Attacks.ProjectileAttack __instance, Battle.Attacks.AttackManager attackManager, float[] dmgValues, float dmgMult, int dmgBonus, int critCount)
		=> HandleFire(__instance, attackManager, dmgValues, dmgMult, dmgBonus, critCount);
	private static void HandleFire(Battle.Attacks.Attack __instance, Battle.Attacks.AttackManager attackManager, float[] dmgValues, float dmgMult, int dmgBonus, int critCount) {
		foreach (var tracker in Tracker.trackers.Values) {
			DamageCounter dmgtracker = tracker as DamageCounter;
			if (dmgtracker != null)
				dmgtracker.HandleFire(__instance, attackManager, dmgValues, dmgMult, dmgBonus, critCount);
		}
	}

	[HarmonyPatch(typeof(BattleController), "AddDamageMultiplier")]
	[HarmonyPostfix]
	private static void AddDamageMultiplier(float mult) {
		foreach (var tracker in Tracker.trackers.Values) {
			MultDamageCounter dmgtracker = tracker as MultDamageCounter;
			if (dmgtracker != null)
				dmgtracker.AddDamageMultiplier(mult);
		}
	}

	private static int prevDamageAmountCount = 0;
	private static int prevDamageBonus = 0;
	[HarmonyPatch(typeof(BattleController), "AddPeg")]
	[HarmonyPrefix]
	private static void AddPegPre(BattleController __instance) {
		prevDamageAmountCount = __instance.damageAmounts.Count;
		prevDamageBonus = Utils.GetAttr<BattleController, int>(__instance, "_damageBonus");
		foreach (var tracker in Tracker.trackers.Values) {
			PegDamageCounter dmgtracker = tracker as PegDamageCounter;
			if (dmgtracker != null)
				dmgtracker.StartAddPeg();
		}
	}
	[HarmonyPatch(typeof(BattleController), "AddPeg")]
	[HarmonyPostfix]
	private static void AddPegPost(BattleController __instance) {
		float damageAdded = 0;
		for (int i = prevDamageAmountCount; i < __instance.damageAmounts.Count; i++)
			damageAdded += __instance.damageAmounts[i];
		int damageBonus = Utils.GetAttr<BattleController, int>(__instance, "_damageBonus");
		damageBonus -= prevDamageBonus;
		foreach (var tracker in Tracker.trackers.Values) {
			PegDamageCounter dmgtracker = tracker as PegDamageCounter;
			if (dmgtracker != null)
				dmgtracker.AddPeg(damageAdded, damageBonus);
		}
	}
	[HarmonyPatch(typeof(BattleController), "GrantAdditionalBasicPeg")]
	[HarmonyPrefix]
	private static void GrantAdditionalBasicPegPre(BattleController __instance) {
		AddPegPre(__instance);
	}
	[HarmonyPatch(typeof(BattleController), "GrantAdditionalBasicPeg")]
	[HarmonyPostfix]
	private static void GrantAdditionalBasicPegPost(BattleController __instance) {
		AddPegPost(__instance);
	}

	[HarmonyPatch(typeof(Battle.PlayerHealthController), "Heal")]
	[HarmonyPostfix]
	private static void Heal(float amount, float __result) {
		foreach (var tracker in Tracker.trackers.Values) {
			HealingCounter healtracker = tracker as HealingCounter;
			if (healtracker != null)
				healtracker.Heal(__result);
		}
	}

	[HarmonyPatch(typeof(Battle.PlayerHealthController), "DealSelfDamage")]
	[HarmonyPrefix]
	private static void SelfDamage(float damage) {
		foreach (var tracker in Tracker.trackers.Values) {
			SelfDamageCounter dmgtracker = tracker as SelfDamageCounter;
			if (dmgtracker != null)
				dmgtracker.SelfDamage(damage);
			// This relic can't be both PegDamageCounter and SelfDamageCounter...
			WandofSkulltimateWrath wand = tracker as WandofSkulltimateWrath;
			if (wand != null)
				wand.SelfDamage(damage);
		}
	}

	[HarmonyPatch(typeof(Battle.Attacks.Attack), "GetStatusEffects")]
	[HarmonyPostfix]
	public static void AttackEffects(List<Battle.StatusEffects.StatusEffect> __result) {
		foreach (Battle.StatusEffects.StatusEffect effect in __result) {
			Relics.RelicEffect? relic = effect.EffectType switch {
				Battle.StatusEffects.StatusEffectType.Blind => Relics.RelicEffect.ATTACKS_DEAL_BLIND,
				_ => null,
			};
			if (relic != null && Tracker.HaveRelic((Relics.RelicEffect)relic)) {
				SimpleCounter tracker = (SimpleCounter)Tracker.trackers[(Relics.RelicEffect)relic];
				tracker.count += effect.Intensity;
			}
		}
	}

	[HarmonyPatch(typeof(Battle.TargetingManager), "AttemptDamageTargetedEnemy")]
	[HarmonyPostfix]
	private static void Damage(float damage, bool __result) {
		if (!__result)
			return;
		foreach (var tracker in Tracker.trackers.Values) {
			DamageTargetedCounter dmgtracker = tracker as DamageTargetedCounter;
			if (dmgtracker != null)
				dmgtracker.Damage(damage);
			// This relic can't be both HealingConter and DamageTargetedCounter...
			InfernalIngot ingot = tracker as InfernalIngot;
			if (ingot != null)
				ingot.Damage(damage);
		}
	}
}
