using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

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
			WandOfSkulltimateWrath wand = tracker as WandOfSkulltimateWrath;
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

[HarmonyPatch]
public class Transpilers {
	public static int? IsLoadArg(CodeInstruction op) {
		if (op.opcode == OpCodes.Ldarg_0) return 0;
		if (op.opcode == OpCodes.Ldarg_1) return 1;
		if (op.opcode == OpCodes.Ldarg_2) return 2;
		if (op.opcode == OpCodes.Ldarg_3) return 3;
		if (op.opcode == OpCodes.Ldarg) return (int?)op.operand;
		if (op.opcode == OpCodes.Ldarg_S) return (int?)(byte?)op.operand;
		return null;
	}

	public static int? IsLoadLoc(CodeInstruction op) {
		if (op.opcode == OpCodes.Ldloc_0) return 0;
		if (op.opcode == OpCodes.Ldloc_1) return 1;
		if (op.opcode == OpCodes.Ldloc_2) return 2;
		if (op.opcode == OpCodes.Ldloc_3) return 3;
		if (op.opcode == OpCodes.Ldloc) return (int?)op.operand;
		if (op.opcode == OpCodes.Ldloc_S) return (int?)(byte?)op.operand;
		return null;
	}

	public static int? IsStoreArg(CodeInstruction op) {
		if (op.opcode == OpCodes.Starg) return (int?)op.operand;
		if (op.opcode == OpCodes.Starg_S) return (int?)(byte?)op.operand;
		return null;
	}

	public static int? IsStoreLoc(CodeInstruction op) {
		if (op.opcode == OpCodes.Stloc_0) return 0;
		if (op.opcode == OpCodes.Stloc_1) return 1;
		if (op.opcode == OpCodes.Stloc_2) return 2;
		if (op.opcode == OpCodes.Stloc_3) return 3;
		if (op.opcode == OpCodes.Stloc) return (int?)op.operand;
		if (op.opcode == OpCodes.Stloc_S) return (int?)(byte?)op.operand;
		return null;
	}

	public static bool IsStore(CodeInstruction op) {
		return IsStoreArg(op) != null || IsStoreLoc(op) != null;
	}

	public static CodeInstruction MakeLoadArg(int arg) {
		if (arg == 0) return new CodeInstruction(OpCodes.Ldarg_0, null);
		if (arg == 1) return new CodeInstruction(OpCodes.Ldarg_1, null);
		if (arg == 2) return new CodeInstruction(OpCodes.Ldarg_2, null);
		if (arg == 3) return new CodeInstruction(OpCodes.Ldarg_3, null);
		if (arg >= 0 && arg < 256) return new CodeInstruction(OpCodes.Ldarg_S, (byte?)arg);
		return new CodeInstruction(OpCodes.Ldarg, (int?)arg);
	}

	public static CodeInstruction MakeLoadLoc(int loc) {
		if (loc == 0) return new CodeInstruction(OpCodes.Ldloc_0, null);
		if (loc == 1) return new CodeInstruction(OpCodes.Ldloc_1, null);
		if (loc == 2) return new CodeInstruction(OpCodes.Ldloc_2, null);
		if (loc == 3) return new CodeInstruction(OpCodes.Ldloc_3, null);
		if (loc >= 0 && loc < 256) return new CodeInstruction(OpCodes.Ldloc_S, (byte?)loc);
		return new CodeInstruction(OpCodes.Ldloc, (int?)loc);
	}

	public static CodeInstruction LoadFromStore(CodeInstruction op) {
		int? arg = IsStoreArg(op);
		if (arg != null) return MakeLoadArg((int)arg);
		arg = IsStoreLoc(op);
		if (arg != null) return MakeLoadLoc((int)arg);
		return null;
	}

	public static FieldInfo IsLoadField(CodeInstruction op) {
		if (op.opcode == OpCodes.Ldfld) return (FieldInfo)op.operand;
		return null;
	}

	public static MethodInfo IsCallMethod(CodeInstruction op) {
		if (op.opcode == OpCodes.Call) return (MethodInfo)op.operand;
		if (op.opcode == OpCodes.Callvirt) return (MethodInfo)op.operand;
		return null;
	}

	public static int? IsLoadConstInt(CodeInstruction op) {
		if (op.opcode == OpCodes.Ldc_I4_0) return 0;
		if (op.opcode == OpCodes.Ldc_I4_1) return 1;
		if (op.opcode == OpCodes.Ldc_I4_2) return 2;
		if (op.opcode == OpCodes.Ldc_I4_3) return 3;
		if (op.opcode == OpCodes.Ldc_I4_4) return 4;
		if (op.opcode == OpCodes.Ldc_I4_5) return 5;
		if (op.opcode == OpCodes.Ldc_I4_6) return 6;
		if (op.opcode == OpCodes.Ldc_I4_7) return 7;
		if (op.opcode == OpCodes.Ldc_I4_8) return 8;
		if (op.opcode == OpCodes.Ldc_I4_M1) return -1;
		if (op.opcode == OpCodes.Ldc_I4) return (int?)op.operand;
		if (op.opcode == OpCodes.Ldc_I4_S) return (int?)(sbyte?)op.operand;
		return null;
	}

	public static float? IsLoadConstFloat(CodeInstruction op) {
		if (op.opcode == OpCodes.Ldc_R4) return (float?)op.operand;
		return null;
	}

	public static bool IsBranchFalse(CodeInstruction op) {
		if (op.opcode == OpCodes.Brfalse) return true;
		if (op.opcode == OpCodes.Brfalse_S) return true;
		return false;
	}

	public static CodeInstruction MakeCall(MethodInfo func) {
		return new CodeInstruction(OpCodes.Call, func);
	}

	[HarmonyPatch(typeof(Battle.PlayerHealthController), "Damage")]
	[HarmonyTranspiler]
	static private IEnumerable<CodeInstruction> PlayerDamagePatcher(IEnumerable<CodeInstruction> origCode) {
		/***
		We want to find instructions that look like this:
			IL_002a:  ldarg.0 
			IL_002b:  ldfld class Relics.RelicManager Battle.PlayerHealthController::_relicManager
			IL_0030:  ldc.i4.s 0x0b
			IL_0032:  callvirt instance bool class Relics.RelicManager::AttemptUseRelic(valuetype Relics.RelicEffect)
			IL_0037:  brfalse.s IL_0040
			IL_0039:  ldc.r4 0.
			IL_003e:  starg.s 1
		which corresponds to code like:
			if (... && this._relicManager.AttemptUseRelic(RelicEffect.NO_DAMAGE_ON_RELOAD)) {
				damage = 0f;
			}
		and we want to inject our hook just before the var gets set to 0
		***/
		var code = new List<CodeInstruction>(origCode);

		bool foundRoundGuard = false;
		bool foundPuppet = false;
		bool foundSash = false;
		bool foundCounter = false;

		for (int i = 0; i < code.Count; i++) {
			// Round Guard
			if (
				IsLoadArg(code[i]) == 0 &&
				IsLoadField(code[i+1])?.Name == "_relicManager" &&
				IsLoadConstInt(code[i+2]) == (int)Relics.RelicEffect.NO_DAMAGE_ON_RELOAD &&
				IsCallMethod(code[i+3])?.Name == "AttemptUseRelic" &&
				IsBranchFalse(code[i+4]) &&
				IsLoadConstFloat(code[i+5]) == 0f &&
				IsStore(code[i+6])
			) {
				CodeInstruction op1 = LoadFromStore(code[i+6]);
				CodeInstruction op2 = MakeCall(typeof(Transpilers).GetMethod("NoDamageOnReload"));
				if (code[i+5].labels.Count > 0) {
					op1.labels = code[i+5].labels;
					code[i+5].labels = [];
				}
				code.Insert(i+5, op2);
				code.Insert(i+5, op1);
				i+=6;
				foundRoundGuard = true;
				continue;
			}

			// Puppet
			if (
				IsLoadArg(code[i]) == 0 &&
				IsLoadField(code[i+1])?.Name == "_relicManager" &&
				IsLoadConstInt(code[i+2]) == (int)Relics.RelicEffect.PREVENT_FIRST_DAMAGE &&
				IsCallMethod(code[i+3])?.Name == "AttemptUseRelic" &&
				IsBranchFalse(code[i+4]) &&
				IsLoadConstFloat(code[i+5]) == 0f &&
				IsStore(code[i+6])
			) {
				CodeInstruction op1 = LoadFromStore(code[i+6]);
				CodeInstruction op2 = MakeCall(typeof(Transpilers).GetMethod("PreventFirstDamage"));
				if (code[i+5].labels.Count > 0) {
					op1.labels = code[i+5].labels;
					code[i+5].labels = [];
				}
				code.Insert(i+5, op2);
				code.Insert(i+5, op1);
				i+=6;
				foundPuppet = true;
				continue;
			}

			// Sash of Focus
			if (
				IsLoadArg(code[i]) == 0 &&
				IsLoadField(code[i+1])?.Name == "_relicManager" &&
				IsLoadConstInt(code[i+2]) == (int)Relics.RelicEffect.PREVENT_LETHAL_DAMAGE &&
				IsCallMethod(code[i+3])?.Name == "AttemptUseRelic" &&
				IsBranchFalse(code[i+4]) &&
				IsLoadConstFloat(code[i+5]) == 0f &&
				IsStore(code[i+6])
			) {
				CodeInstruction op1 = LoadFromStore(code[i+6]);
				CodeInstruction op2 = MakeCall(typeof(Transpilers).GetMethod("PreventLethalDamage"));
				if (code[i+5].labels.Count > 0) {
					op1.labels = code[i+5].labels;
					code[i+5].labels = [];
				}
				code.Insert(i+5, op2);
				code.Insert(i+5, op1);
				i+=6;
				foundSash = true;
				continue;
			}

			// Ripostal Service
			if (
				IsLoadArg(code[i]) == 0 &&
				IsLoadField(code[i+1])?.Name == "_relicManager" &&
				IsLoadConstInt(code[i+2]) == (int)Relics.RelicEffect.BALLWARK_COUNTER &&
				IsCallMethod(code[i+3])?.Name == "AttemptUseRelic" &&
				IsBranchFalse(code[i+4]) &&
				IsLoadArg(code[i+5]) != null &&
				IsLoadArg(code[i+6]) == 0 &&
				IsLoadField(code[i+7])?.Name == "_armour" &&
				IsLoadConstFloat(code[i+8]) != null &&
				code[i+9].opcode == OpCodes.Mul
			) {
				CodeInstruction op1 = new CodeInstruction(OpCodes.Dup, null);
				CodeInstruction op2 = MakeCall(typeof(Transpilers).GetMethod("BallwarkCounter"));
				code.Insert(i+10, op2);
				code.Insert(i+10, op1);
				i+=10;
				foundCounter = true;
				continue;
			}
		}

		if (!foundRoundGuard) {
			Plugin.Logger.LogError("Couldn't find Round Guard code in PlayerHealthController.Damage");
			return null;
		}
		if (!foundPuppet) {
			Plugin.Logger.LogError("Couldn't find Puppet code in PlayerHealthController.Damage");
			return null;
		}
		if (!foundSash) {
			Plugin.Logger.LogError("Couldn't find Sash of Focus code in PlayerHealthController.Damage");
			return null;
		}
		if (!foundCounter) {
			Plugin.Logger.LogError("Couldn't find Ripostal Service code in PlayerHealthController.Damage");
			return null;
		}

		code.Insert(0, MakeCall(typeof(Transpilers).GetMethod("DamagePre")));

		return code;
	}

	public static void DamagePre() {
		((RoundGuard)Tracker.trackers[Relics.RelicEffect.NO_DAMAGE_ON_RELOAD]).Disable();
		((SeraphicShield)Tracker.trackers[Relics.RelicEffect.IMMORTAL]).Disable();
	}

	public static void NoDamageOnReload(float amount) {
		((RoundGuard)Tracker.trackers[Relics.RelicEffect.NO_DAMAGE_ON_RELOAD]).DamageAvoided(amount);
		((SeraphicShield)Tracker.trackers[Relics.RelicEffect.IMMORTAL]).DamageAvoided(amount);
	}

	public static void PreventFirstDamage(float amount) {
		((Puppet)Tracker.trackers[Relics.RelicEffect.PREVENT_FIRST_DAMAGE]).DamageAvoided(amount);
	}

	public static void PreventLethalDamage(float amount) {
		((SashOfFocus)Tracker.trackers[Relics.RelicEffect.PREVENT_LETHAL_DAMAGE]).DamageAvoided(amount);
	}

	public static void BallwarkCounter(float amount) {
		((RipostalService)Tracker.trackers[Relics.RelicEffect.BALLWARK_COUNTER]).Damage(amount);
	}
}
