using HarmonyLib;

namespace CustomCruciball;

[HarmonyPatch]
public class Hooks {
	// Track what context we're currently calling the "get cruciball level" property from
	private static int currContext = -1;

	[HarmonyPatch(typeof(Cruciball.CruciballManager), "currentCruciballLevel", MethodType.Getter)]
	[HarmonyPrefix]
	private static bool GetCruciballLevel(ref int __result) {
		if (!State.inst.isCustom)
			return true;
		if (currContext >= 0) {
			__result = State.inst.levels[currContext] ? Constants.NUM_LEVELS : 0;
			return false;
		}
		// as a fallback, just count how many levels are ticked
		__result = 0;
		for (int i = 0; i < Constants.NUM_LEVELS; i++)
			if (State.inst.levels[i])
				__result++;
		return false;
	}

	// other specialty patches
	[HarmonyPatch(typeof(Battle.BattleController), "Awake")]
	[HarmonyPostfix]
	private static void HandleLeshy(Battle.BattleController __instance) {
		if (State.inst.isCustom)
			__instance._pegManager.isC20Leshy = State.inst.levels[19];
	}
	[HarmonyPatch(typeof(PeglinUI.PlayerInfoUI), "ChangeFloorText")]
	[HarmonyPostfix]
	private static void CustomDisplay(PeglinUI.PlayerInfoUI __instance) {
		if (State.inst.isCustom && __instance.cruciballText != null) {
			__instance.cruciballText.transform.parent.gameObject.SetActive(true);
			__instance.cruciballText.text = "Cruciball Custom";
		}
	}

	// And now all the context patches
	// cf genhooks.py
	[HarmonyPatch(typeof(Cruciball.CruciballManager), "AdditionalStarterStones")]
	[HarmonyPrefix]
	private static void Enable0(ref int __state) { __state = currContext; currContext = 0; }
	[HarmonyPatch(typeof(Cruciball.CruciballManager), "AdditionalStarterStones")]
	[HarmonyPostfix]
	private static void Disable0(int __state) { currContext = __state; }

	[HarmonyPatch(typeof(Cruciball.CruciballManager), "WeakStones")]
	[HarmonyPrefix]
	private static void Enable0Weak(ref int __state) { __state = currContext; currContext = 0; }
	[HarmonyPatch(typeof(Cruciball.CruciballManager), "WeakStones")]
	[HarmonyPostfix]
	private static void Disable0Weak(int __state) { currContext = __state; }

	[HarmonyPatch(typeof(Cruciball.CruciballManager), "GetUnknownMapEliteChance")]
	[HarmonyPrefix]
	private static void Enable1(ref int __state) { __state = currContext; currContext = 1; }
	[HarmonyPatch(typeof(Cruciball.CruciballManager), "GetUnknownMapEliteChance")]
	[HarmonyPostfix]
	private static void Disable1(int __state) { currContext = __state; }

	[HarmonyPatch(typeof(Cruciball.CruciballManager), "FewerCritPegs")]
	[HarmonyPrefix]
	private static void Enable2(ref int __state) { __state = currContext; currContext = 2; }
	[HarmonyPatch(typeof(Cruciball.CruciballManager), "FewerCritPegs")]
	[HarmonyPostfix]
	private static void Disable2(int __state) { currContext = __state; }

	[HarmonyPatch(typeof(Cruciball.CruciballManager), "GetModifiedMisnavigationDamage")]
	[HarmonyPrefix]
	private static void Enable3(ref int __state) { __state = currContext; currContext = 3; }
	[HarmonyPatch(typeof(Cruciball.CruciballManager), "GetModifiedMisnavigationDamage")]
	[HarmonyPostfix]
	private static void Disable3(int __state) { currContext = __state; }

	[HarmonyPatch(typeof(Cruciball.CruciballManager), "FewerRefreshPegs")]
	[HarmonyPrefix]
	private static void Enable4(ref int __state) { __state = currContext; currContext = 4; }
	[HarmonyPatch(typeof(Cruciball.CruciballManager), "FewerRefreshPegs")]
	[HarmonyPostfix]
	private static void Disable4(int __state) { currContext = __state; }

	[HarmonyPatch(typeof(Cruciball.CruciballManager), "EnemyShouldUseImprovedHealth")]
	[HarmonyPrefix]
	private static void Enable5(ref int __state, bool boss) { __state = currContext; currContext = boss ? 9 : 5; }
	[HarmonyPatch(typeof(Cruciball.CruciballManager), "EnemyShouldUseImprovedHealth")]
	[HarmonyPostfix]
	private static void Disable5(int __state) { currContext = __state; }

	[HarmonyPatch(typeof(Cruciball.CruciballManager), "GetModifiedPostBattleHeal")]
	[HarmonyPrefix]
	private static void Enable6(ref int __state) { __state = currContext; currContext = 6; }
	[HarmonyPatch(typeof(Cruciball.CruciballManager), "GetModifiedPostBattleHeal")]
	[HarmonyPostfix]
	private static void Disable6(int __state) { currContext = __state; }

	[HarmonyPatch(typeof(Cruciball.CruciballManager), "IsLessGoldAvailable")]
	[HarmonyPrefix]
	private static void Enable7(ref int __state) { __state = currContext; currContext = 7; }
	[HarmonyPatch(typeof(Cruciball.CruciballManager), "IsLessGoldAvailable")]
	[HarmonyPostfix]
	private static void Disable7(int __state) { currContext = __state; }

	[HarmonyPatch(typeof(Cruciball.CruciballManager), "GetStartingGold")]
	[HarmonyPrefix]
	private static void Enable7Start(ref int __state) { __state = currContext; currContext = 7; }
	[HarmonyPatch(typeof(Cruciball.CruciballManager), "GetStartingGold")]
	[HarmonyPostfix]
	private static void Disable7Start(int __state) { currContext = __state; }

	[HarmonyPatch(typeof(Cruciball.CruciballManager), "GetAvailableBattleGold")]
	[HarmonyPrefix]
	private static void Enable7Battle(ref int __state) { __state = currContext; currContext = 7; }
	[HarmonyPatch(typeof(Cruciball.CruciballManager), "GetAvailableBattleGold")]
	[HarmonyPostfix]
	private static void Disable7Battle(int __state) { currContext = __state; }

	[HarmonyPatch(typeof(Cruciball.CruciballManager), "GetModifiedExternalGoldAdd")]
	[HarmonyPrefix]
	private static void Enable7Ext(ref int __state) { __state = currContext; currContext = 7; }
	[HarmonyPatch(typeof(Cruciball.CruciballManager), "GetModifiedExternalGoldAdd")]
	[HarmonyPostfix]
	private static void Disable7Ext(int __state) { currContext = __state; }

	[HarmonyPatch(typeof(Cruciball.CruciballManager), "IncreasedRiggedDamage")]
	[HarmonyPrefix]
	private static void Enable8(ref int __state) { __state = currContext; currContext = 8; }
	[HarmonyPatch(typeof(Cruciball.CruciballManager), "IncreasedRiggedDamage")]
	[HarmonyPostfix]
	private static void Disable8(int __state) { currContext = __state; }

	[HarmonyPatch(typeof(Cruciball.CruciballManager), "ShouldAddCursedOrb")]
	[HarmonyPrefix]
	private static void Enable10(ref int __state) { __state = currContext; currContext = 10; }
	[HarmonyPatch(typeof(Cruciball.CruciballManager), "ShouldAddCursedOrb")]
	[HarmonyPostfix]
	private static void Disable10(int __state) { currContext = __state; }

	[HarmonyPatch(typeof(Cruciball.CruciballManager), "DecreasedBombDamage")]
	[HarmonyPrefix]
	private static void Enable11(ref int __state) { __state = currContext; currContext = 11; }
	[HarmonyPatch(typeof(Cruciball.CruciballManager), "DecreasedBombDamage")]
	[HarmonyPostfix]
	private static void Disable11(int __state) { currContext = __state; }

	[HarmonyPatch(typeof(Cruciball.CruciballManager), "LessHealingFromBosses")]
	[HarmonyPrefix]
	private static void Enable12(ref int __state) { __state = currContext; currContext = 12; }
	[HarmonyPatch(typeof(Cruciball.CruciballManager), "LessHealingFromBosses")]
	[HarmonyPostfix]
	private static void Disable12(int __state) { currContext = __state; }

	[HarmonyPatch(typeof(Cruciball.CruciballManager), "ShouldReduceMaxHP")]
	[HarmonyPrefix]
	private static void Enable13(ref int __state) { __state = currContext; currContext = 13; }
	[HarmonyPatch(typeof(Cruciball.CruciballManager), "ShouldReduceMaxHP")]
	[HarmonyPostfix]
	private static void Disable13(int __state) { currContext = __state; }

	[HarmonyPatch(typeof(Cruciball.CruciballManager), "EnemiesGetExtraReloadTurn")]
	[HarmonyPrefix]
	private static void Enable14(ref int __state) { __state = currContext; currContext = 14; }
	[HarmonyPatch(typeof(Cruciball.CruciballManager), "EnemiesGetExtraReloadTurn")]
	[HarmonyPostfix]
	private static void Disable14(int __state) { currContext = __state; }

	[HarmonyPatch(typeof(Cruciball.CruciballManager), "ShouldAddUnremovableCursedOrb")]
	[HarmonyPrefix]
	private static void Enable15(ref int __state) { __state = currContext; currContext = 15; }
	[HarmonyPatch(typeof(Cruciball.CruciballManager), "ShouldAddUnremovableCursedOrb")]
	[HarmonyPostfix]
	private static void Disable15(int __state) { currContext = __state; }

	[HarmonyPatch(typeof(Cruciball.CruciballManager), "GetMinibossDamageModifier")]
	[HarmonyPrefix]
	private static void Enable16(ref int __state) { __state = currContext; currContext = 16; }
	[HarmonyPatch(typeof(Cruciball.CruciballManager), "GetMinibossDamageModifier")]
	[HarmonyPostfix]
	private static void Disable16(int __state) { currContext = __state; }

	[HarmonyPatch(typeof(Cruciball.CruciballManager), "ShouldIncreaseShopPrices")]
	[HarmonyPrefix]
	private static void Enable17(ref int __state) { __state = currContext; currContext = 17; }
	[HarmonyPatch(typeof(Cruciball.CruciballManager), "ShouldIncreaseShopPrices")]
	[HarmonyPostfix]
	private static void Disable17(int __state) { currContext = __state; }

	[HarmonyPatch(typeof(Cruciball.CruciballManager), "CanSpawnBonusEnemies")]
	[HarmonyPrefix]
	private static void Enable18(ref int __state) { __state = currContext; currContext = 18; }
	[HarmonyPatch(typeof(Cruciball.CruciballManager), "CanSpawnBonusEnemies")]
	[HarmonyPostfix]
	private static void Disable18(int __state) { currContext = __state; }

	[HarmonyPatch(typeof(Cruciball.CruciballManager), "ShouldUpgradeBosses")]
	[HarmonyPrefix]
	private static void Enable19(ref int __state) { __state = currContext; currContext = 19; }
	[HarmonyPatch(typeof(Cruciball.CruciballManager), "ShouldUpgradeBosses")]
	[HarmonyPostfix]
	private static void Disable19(int __state) { currContext = __state; }
}
