using HarmonyLib;

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
	static private void RelicUsed(Relics.RelicManager __instance, Relics.RelicEffect re, bool __result) {
		if (__result) {
			Tracker tracker = null;
			if (Tracker.trackers.TryGetValue(re, out tracker)) {
				tracker.Used();
			}
		}
	}
	// todo: AttemptUseCountdownRelicManyTimes for COINS_PROVIDE_HEALING

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
	private static void FireTargeted(TargetedAttack __instance, float[] dmgValues, int critCount)
		=> HandleFire(__instance, dmgValues, critCount);
	[HarmonyPatch(typeof(Battle.Attacks.ProjectileAttack), "Fire")]
	[HarmonyPrefix]
	private static void FireProjectile(Battle.Attacks.ProjectileAttack __instance, float[] dmgValues, int critCount)
		=> HandleFire(__instance, dmgValues, critCount);
	private static void HandleFire(Battle.Attacks.Attack __instance, float[] dmgValues, int critCount) {
		int damagePerPeg = (int)__instance.GetModifiedDamagePerPeg(critCount);
		foreach (var tracker in Tracker.trackers) {
			PegDamageCounter pegtracker = tracker.Value as PegDamageCounter;
			if (pegtracker != null)
				pegtracker.HandleFire(damagePerPeg);
			OrbDamageCounter orbtracker = tracker.Value as OrbDamageCounter;
			if (orbtracker != null)
				orbtracker.HandleFire(__instance, dmgValues, critCount);
		}
	}
}
