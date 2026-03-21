using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace RelicStats;

public abstract class Tracker {
	public abstract Relics.RelicEffect Relic { get; }

	public abstract void Reset();
public abstract string Tooltip { get; }
	public abstract object State { get; set; }
	public abstract void Used();
	public virtual void Flash() {}
	public virtual void Checked() {}
	public void Updated() {
		Plugin.UpdateTooltip(Relic);
	}


	public static readonly Dictionary<Relics.RelicEffect, Tracker> trackers = new Dictionary<Relics.RelicEffect, Tracker>();
	public static void PopulateTrackers() {
		foreach (Type t in typeof(Tracker).Assembly.GetTypes()) {
			if (t.IsSubclassOf(typeof(Tracker)) && !t.IsAbstract) {
				AddTracker((Tracker)t.GetConstructor(Type.EmptyTypes).Invoke(null));
			}
		}
		ResetAll();

		ValidateTrackers();
	}

	[Conditional("DEBUG")]
	public static void ValidateTrackers() {
		foreach (Relics.RelicEffect i in Enum.GetValues(typeof(Relics.RelicEffect))) {
			if (!trackers.ContainsKey(i)) {
				Plugin.Logger.LogWarning($"Missing a tracker for {i}");
			}
		}

		foreach (Type t in typeof(Tracker).Assembly.GetTypes()) {
			if (t.IsSubclassOf(typeof(Tracker))) {
				bool hasFields = false, hasStateFields = false;
				foreach (FieldInfo f in t.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
					hasFields = true;
					if (f.Name[0] != '_')
						hasStateFields = true;
				}
				MethodInfo reset = t.GetMethod("Reset", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
				PropertyInfo state = t.GetProperty("State", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
				if (hasFields && reset == null)
					Plugin.Logger.LogWarning($"Tracker class {t} should override Reset method");
				if (hasStateFields && state == null)
					Plugin.Logger.LogWarning($"Tracker class {t} should override State property");

				if (t.IsSubclassOf(typeof(TodoTracker)) && !t.IsAbstract) {
					Plugin.Logger.LogWarning($"Tracker class {t} is still TODO");
				}
			}

			bool isPatch = t.GetCustomAttributes(typeof(HarmonyPatch), false).Length > 0;
			bool hasPatch = false;
			foreach (MethodInfo m in t.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)) {
				if (m.GetCustomAttributes(typeof(HarmonyPatch), false).Length > 0) {
					hasPatch = true;
					break;
				}
			}
			if (!isPatch && hasPatch)
				Plugin.Logger.LogWarning($"Class {t} should have [HarmonyPatch]");
			else if (isPatch && !hasPatch)
				Plugin.Logger.LogWarning($"Class {t} should not have [HarmonyPatch]");
		}
	}
	private static void AddTracker(Tracker tracker) {
		trackers.Add(tracker.Relic, tracker);
	}
	public static void ResetAll() {
		foreach (var tracker in trackers) {
			tracker.Value.Reset();
		}
	}
	public static void AddRelic(Relics.RelicEffect relic) {
		if (!HaveRelicIgnoreDisable(relic)) {
			Tracker tracker;
			if (trackers.TryGetValue(relic, out tracker))
				tracker.Reset();
		}
	}
	public static bool HaveRelic(Relics.RelicEffect relic) => Utils.relicManager.RelicEffectActive(relic);
	public static bool HaveRelicIgnoreDisable(Relics.RelicEffect relic) =>
		Utils.relicManager._ownedRelics != null && Utils.relicManager._ownedRelics.ContainsKey(relic);

	public static void LoadData() {
		ResetAll();
		RelicStatsSaveData data = (RelicStatsSaveData)ToolBox.Serialization.DataSerializer.Load<SaveObjectData>(RelicStatsSaveData.KEY, ToolBox.Serialization.DataSerializer.SaveType.RUN);
		if (data != null) {
			foreach (var item in data.relicStates) {
				Tracker tracker;
				if (trackers.TryGetValue((Relics.RelicEffect)item.Key, out tracker)) {
					try {
						tracker.State = item.Value;
					} catch (InvalidCastException) {
					} catch(NullReferenceException) {
					}
				}
			}
		}
	}
	public static void SaveData() {
		RelicStatsSaveData data = new RelicStatsSaveData();
		foreach (var tracker in trackers.Values) {
			if (HaveRelicIgnoreDisable(tracker.Relic)) {
				data.relicStates[(int)tracker.Relic] = tracker.State;
			}
		}
		data.Save();
	}
}

public class RelicStatsSaveData : SaveObjectData {
	public readonly static String KEY = $"{MyPluginInfo.PLUGIN_GUID}_RelicData";

	public override String Name => KEY;

	public Dictionary<int, object> relicStates;

	public RelicStatsSaveData() : base(true, ToolBox.Serialization.DataSerializer.SaveType.RUN) {
		this.relicStates = new Dictionary<int, object>();
	}
}

public abstract class NoopTracker : Tracker {
	public override void Reset() {}
	public override void Used() {}
	public override string Tooltip => null;
	public override object State { get => null; set {} }
}

public abstract class TodoTracker : NoopTracker {}

public abstract class SimpleCounter : Tracker {
	public int count = 0;

	public virtual int Step => 1;
	public override void Reset() { count = 0; Updated(); }
	public override void Used() { count += Step; Updated(); }
	public override object State {
		get => count;
		set {
			count = (int)value;
		}
	}
}

public abstract class HealingCounter : SimpleCounter {
	protected bool _active = false;
	public override void Reset() {
		base.Reset();
		_active = false;
	}
	public override void Used() {}
	public virtual void Heal(float amount) {
		if (_active) {
			count += (int)amount;
			Updated();
		}
		_active = false;
	}
	public override string Tooltip => $"{count} <style=heal>healed</style>";
}

public abstract class SelfDamageCounter : SimpleCounter {
	protected bool _active = false;
	public override void Reset() {
		base.Reset();
		_active = false;
	}
	public override void Used() {}
	public virtual void SelfDamage(float amount) {
		if (_active) {
			count += (int)amount;
			Updated();
		}
		_active = false;
	}
	public override string Tooltip => $"{count} <style=damage>self-damage</style>";
}

public abstract class DamageCounter : Tracker {
	protected int goodCount = 0, badCount = 0;

	public override void Reset() { goodCount = badCount = 0; Updated(); }
	public override void Used() {}
	public override object State {
		get => (goodCount, badCount);
		set {
			(goodCount, badCount) = ((int, int))value;
		}
	}
	public override string Tooltip { get {
		string tooltip = $"{goodCount} <style=damage>damage added</style>";
		if (badCount > 0)
			tooltip = $"{tooltip}\n{badCount} <style=dmg_negative>damage removed</style>";
		return tooltip;
	}}

	public virtual void HandleFire(Battle.Attacks.Attack attack, Battle.Attacks.AttackManager attackManager, int pegMultipliersTally, float dmgMult, int dmgBonus, int critCount) {
		if (!Tracker.HaveRelic(Relic))
			return;
		float fullDamage = attack.GetDamage(attackManager, pegMultipliersTally, dmgMult, dmgBonus, critCount, false);
		float baseDamage = GetBaseDamage(attack, attackManager, pegMultipliersTally, dmgMult, dmgBonus, critCount);
		if (fullDamage > baseDamage) {
			goodCount += (int)(fullDamage - baseDamage);
			Updated();
		} else if (fullDamage < baseDamage) {
			badCount += (int)(baseDamage - fullDamage);
			Updated();
		}
	}
	public abstract float GetBaseDamage(Battle.Attacks.Attack attack, Battle.Attacks.AttackManager attackManager, int pegMultipliersTally, float dmgMult, int dmgBonus, int critCount);
}
public abstract class PegDamageCounter : DamageCounter {
	public virtual int Step => 1;
	protected bool _active = false;
	protected int _peg_count = 0;
	protected int _bonus_count = 0;
	public override void Reset() {
		base.Reset();
		_active = false;
		_peg_count = _bonus_count = 0;
	}
	public override void Used() {
		_active = true;
	}
	public virtual void StartAddPeg() {}
	public virtual void AddPeg(int multiplier, int bonus) {
		if (_active) {
			_peg_count += multiplier;
			_bonus_count += bonus;
		}
		_active = false;
	}
	public override float GetBaseDamage(Battle.Attacks.Attack attack, Battle.Attacks.AttackManager attackManager, int pegMultipliersTally, float dmgMult, int dmgBonus, int critCount) {
		float baseDamage = attack.GetDamage(attackManager, pegMultipliersTally - _peg_count, dmgMult, dmgBonus - _bonus_count, critCount, false);
		_peg_count = _bonus_count = 0;
		return baseDamage;
	}
}
public abstract class PegMultiDamageCounter : PegDamageCounter {
	public override void AddPeg(int multiplier, int bonus) {}
	public virtual void MultiPeg(int multiplier, int bonus, int hitCount) {
		if (_active) {
			_peg_count += multiplier * hitCount;
			_bonus_count += bonus * hitCount;
		}
		_active = false;
	}
}
public abstract class OrbDamageCounter : DamageCounter {
	public override void Used() {}
	public override float GetBaseDamage(Battle.Attacks.Attack attack, Battle.Attacks.AttackManager attackManager, int pegMultipliersTally, float dmgMult, int dmgBonus, int critCount) {
		var owned = Utils.relicManager._ownedRelics;
		Relics.Relic r = owned[Relic];
		Battle.Attacks.Attack.StaticDamageModifierData olddata = attack._currentShotStaticDamageModifierData;
		owned.Remove(Relic);
		attack.CalculateStaticDamageBuffs(true);
		float baseDamage = attack.GetDamage(attackManager, pegMultipliersTally, dmgMult, dmgBonus, critCount, false);
		owned.Add(Relic, r);
		attack._currentShotStaticDamageModifierData = olddata;
		return baseDamage;
	}
}
public abstract class MultDamageCounter : DamageCounter {
	protected bool _active = false;
	protected float _multiplier = 1f;
	public override void Reset() {
		base.Reset();
		_active = false;
		_multiplier = 1f;
	}
	public override void Used() {
		_active = true;
	}
	public virtual void AddDamageMultiplier(float mult) {
		if (_active)
			_multiplier *= mult;
		_active = false;
	}
	public override float GetBaseDamage(Battle.Attacks.Attack attack, Battle.Attacks.AttackManager attackManager, int pegMultipliersTally, float dmgMult, int dmgBonus, int critCount) {
		dmgMult /= _multiplier;
		_multiplier = 1f;
		return attack.GetDamage(attackManager, pegMultipliersTally, dmgMult, dmgBonus, critCount, false);
	}
}
public abstract class PegBuffDamageCounter : DamageCounter {
	protected Dictionary<int, int> _pegBuffs = new Dictionary<int, int>();
	protected int _bonus = 0;
	public override void Reset() {
		base.Reset();
		NewBattle();
	}
	public void NewBattle() {
		_pegBuffs.Clear();
		_bonus = 0;
	}
	public void HandleHitPeg(Peg peg) {
		if (Tracker.HaveRelic(Relic)) {
			if (_pegBuffs.ContainsKey(peg.gameObject.GetInstanceID())) {
				_bonus += (int)(_pegBuffs[peg.gameObject.GetInstanceID()] * peg.buffDamageMultiplier);
			}
		}
	}
	public override float GetBaseDamage(Battle.Attacks.Attack attack, Battle.Attacks.AttackManager attackManager, int pegMultipliersTally, float dmgMult, int dmgBonus, int critCount) {
		dmgBonus -= _bonus;
		_bonus = 0;
		return attack.GetDamage(attackManager, pegMultipliersTally, dmgMult, dmgBonus, critCount, false);
	}
}

public abstract class DamageTargetedCounter : SimpleCounter {
	protected bool _active = false;
	public override void Reset() {
		base.Reset();
		_active = false;
	}
	public override void Used() {
		_active = true;
	}
	public virtual void Damage(int amount) {
		if (_active) {
			count += amount;
			Updated();
		}
		_active = false;
	}
	public override string Tooltip => $"{count} <style=damage>damage dealt</style>";
}

public abstract class DamageAllCounter : SimpleCounter {
	public DamageAllCounter() {
		Battle.Enemies.Enemy.OnAllEnemiesDamaged += new Battle.Enemies.Enemy.DamageAllEnemies(this.DamageAllEnemies);
	}

	protected bool _active = false;
	public override void Reset() {
		base.Reset();
		_active = false;
	}
	public override void Used() {
		_active = true;
	}
	private void DamageAllEnemies(long damageAmount, Battle.Enemies.Enemy.EnemyDamageSource source = 0) {
		if (_active) {
			count += Utils.EnemyDamageCount() * (int)damageAmount;
			Updated();
		}
		_active = false;
	}

	public override string Tooltip => $"{count} <style=damage>damage dealt</style>";
}

public abstract class StatusEffectCounter : SimpleCounter {
	public abstract Battle.StatusEffects.StatusEffectType type { get; }
	protected bool _active = false;
	public override void Reset() {
		base.Reset();
		_active = false;
	}
	public override void Used() {
		_active = true;
	}
	public void ApplyStatusEffect(Battle.StatusEffects.StatusEffect statusEffect) {
		if (_active && statusEffect.EffectType == type) {
			count += statusEffect.Intensity;
			Updated();
		}
		_active = false;
	}
	public override string Tooltip => $"{count} {Utils.TypeDesc(type)} added</style>";
}

public class Utils {
	static public T GetResource<T>() where T : UnityEngine.Object {
		T[] objs = Resources.FindObjectsOfTypeAll<T>();
		if (objs.Length > 0)
			return objs[0];
		else
			return null;
	}

	static private Relics.RelicManager _relicManager = null;
	static public Relics.RelicManager relicManager {
		get {
			// checking "== null" implicitly also checks if this refers to a unity object
			// that has been deleted
			if (_relicManager == null)
				_relicManager = GetResource<Relics.RelicManager>();
			return _relicManager;
		}
	}

	static public string Plural(int n, string ifplural = "s", string ifsingle = "") {
		return (n == 1) ? ifsingle : ifplural;
	}

	static public int EnemyDamageCount() {
		int enemyCount = 0;
		foreach (var dlg in Battle.Enemies.Enemy.OnAllEnemiesDamaged.GetInvocationList()) {
			// Is this an actual enemy that will be damaged, or one of my injected hooks?
			if (dlg.Target is Battle.Enemies.Enemy) {
				enemyCount += 1;
			}
		}
		return enemyCount;
	}

	static public string TypeDesc(Battle.StatusEffects.StatusEffectType type) => type switch {
		Battle.StatusEffects.StatusEffectType.Thorned => "<style=bramble>Bramble",
		Battle.StatusEffects.StatusEffectType.Stunned => "<style=stunned>Stunned",
		Battle.StatusEffects.StatusEffectType.Blind => "<style=blind>Blind",
		Battle.StatusEffects.StatusEffectType.Confusion => "<style=confuse>Confused",
		Battle.StatusEffects.StatusEffectType.Reflect => "<style=damage>Reflect",
		Battle.StatusEffects.StatusEffectType.Strength => "<style=strength>Muscircle",
		Battle.StatusEffects.StatusEffectType.Finesse => "<style=finesse>Spinesse",
		Battle.StatusEffects.StatusEffectType.Balance => "<style=balance>Ballance",
		Battle.StatusEffects.StatusEffectType.Dexterity => "<style=dexterity>Dexspherity",
		Battle.StatusEffects.StatusEffectType.Ballwark => "<style=shield>Ballwark",
		Battle.StatusEffects.StatusEffectType.Poison => "<style=poison>Spinfection",
		Battle.StatusEffects.StatusEffectType.Ballusion => "<style=dodge>Ballusion",
		Battle.StatusEffects.StatusEffectType.Intangiball => "<style=dmg_limit>Intangiball",
		Battle.StatusEffects.StatusEffectType.Exploitaball => "<style=exploitaball>Exploitaball",
		Battle.StatusEffects.StatusEffectType.Transpherency => "<style=transpherency>Transpherency",
		_ => null,
	};
}
