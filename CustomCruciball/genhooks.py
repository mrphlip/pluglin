#!/usr/bin/python

def gen(method, level, name=None, cls="Cruciball.CruciballManager", extparam=""):
	if name is None:
		name = level

	print(f"""\
	[HarmonyPatch(typeof({cls}), "{method}")]
	[HarmonyPrefix]
	private static void Enable{name}(ref int __state{extparam}) {{ __state = currContext; currContext = {level}; }}
	[HarmonyPatch(typeof({cls}), "{method}")]
	[HarmonyPostfix]
	private static void Disable{name}(int __state) {{ currContext = __state; }}
""")


gen("AdditionalStarterStones", 0)
gen("WeakStones", 0, name="0Weak")
gen("GetUnknownMapEliteChance", 1)
gen("FewerCritPegs", 2)
gen("GetModifiedMisnavigationDamage", 3)
gen("FewerRefreshPegs", 4)
gen("EnemyShouldUseImprovedHealth", "boss ? 9 : 5", name=5, extparam=", bool boss")
gen("GetModifiedPostBattleHeal", 6)
gen("IsLessGoldAvailable", 7)
gen("GetStartingGold", 7, name="7Start")
gen("GetAvailableBattleGold", 7, name="7Battle")
gen("GetModifiedExternalGoldAdd", 7, name="7Ext")
gen("IncreasedRiggedDamage", 8)
gen("ShouldAddCursedOrb", 10)
gen("DecreasedBombDamage", 11)
gen("LessHealingFromBosses", 12)
gen("ShouldReduceMaxHP", 13)
gen("EnemiesGetExtraReloadTurn", 14)
gen("ShouldAddUnremovableCursedOrb", 15)
gen("GetMinibossDamageModifier", 16)
gen("ShouldIncreaseShopPrices", 17)
gen("CanSpawnBonusEnemies", 18)
gen("ShouldUpgradeBosses", 19)
