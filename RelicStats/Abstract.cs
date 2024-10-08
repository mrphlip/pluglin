using HarmonyLib;
using System;
using System.Collections.Generic;
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
	public virtual void Checked() {}


	public static readonly Dictionary<Relics.RelicEffect, Tracker> trackers = new Dictionary<Relics.RelicEffect, Tracker>();
	public static readonly HashSet<Relics.RelicEffect> relics = new HashSet<Relics.RelicEffect>();
	public static void PopulateTrackers() {
		foreach (Type t in typeof(Tracker).Assembly.GetTypes()) {
			if (t.IsSubclassOf(typeof(Tracker)) && !t.IsAbstract) {
				AddTracker((Tracker)t.GetConstructor(Type.EmptyTypes).Invoke(null));
			}
		}
		ResetAll();

		foreach (Relics.RelicEffect i in Enum.GetValues(typeof(Relics.RelicEffect))) {
			if (!trackers.ContainsKey(i)) {
				Plugin.Logger.LogWarning($"Missing a tracker for {i}");
			}
		}
	}
	private static void AddTracker(Tracker tracker) {
		trackers.Add(tracker.Relic, tracker);
	}
	public static void ResetAll(bool clearOwned = true) {
		foreach (var tracker in trackers) {
			tracker.Value.Reset();
		}
		if (clearOwned)
			relics.Clear();
	}
	// Maintain our own cache of which relics are collected, to avoid
	// having to go digging for a RelicManager every time we need to check
	public static void AddRelic(Relics.RelicEffect relic, bool isNew) {
		if (!relics.Contains(relic)) {
			Tracker tracker;
			if (isNew && trackers.TryGetValue(relic, out tracker))
				tracker.Reset();
			relics.Add(relic);
		}
	}
	public static bool HaveRelic(Relics.RelicEffect relic) => relics.Contains(relic);

	public static void LoadData() {
		ResetAll(false);
		RelicStatsSaveData data = (RelicStatsSaveData)ToolBox.Serialization.DataSerializer.Load<SaveObjectData>(RelicStatsSaveData.KEY, ToolBox.Serialization.DataSerializer.SaveType.RUN);
		if (data != null) {
			foreach (var item in data.relicStates) {
				Tracker tracker;
				if (trackers.TryGetValue((Relics.RelicEffect)item.Key, out tracker)) {
					tracker.State = item.Value;
				}
			}
		}
	}
	public static void SaveData() {
		RelicStatsSaveData data = new RelicStatsSaveData();
		foreach (var tracker in trackers.Values) {
			if (HaveRelic(tracker.Relic)) {
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
	public override void Reset() { count = 0; }
	public override void Used() { count += Step; }
	public override object State {
		get => count;
		set {
			count = (int)value;
		}
	}
}

public abstract class HealingCounter : SimpleCounter {
	protected bool _active = false;
	public override void Used() {}
	public virtual void Heal(float amount) {
		if (_active)
			count += (int)amount;
		_active = false;
	}
	public override string Tooltip => $"{count} <style=heal>healed</style>";
}

public abstract class SelfDamageCounter : SimpleCounter {
	protected bool _active = false;
	public override void Used() {}
	public virtual void SelfDamage(float amount) {
		if (_active)
			count += (int)amount;
		_active = false;
	}
	public override string Tooltip => $"{count} <style=damage>self-damage</style>";
}

public abstract class DamageCounter : Tracker {
	protected int goodCount = 0, badCount = 0;

	public override void Reset() { goodCount = badCount = 0; }
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
			tooltip = $"{tooltip}; {badCount} <style=dmg_negative>damage removed</style>";
		return tooltip;
	}}

	public virtual void HandleFire(Battle.Attacks.Attack attack, Battle.Attacks.AttackManager attackManager, float[] dmgValues, float dmgMult, int dmgBonus, int critCount) {
		if (!Tracker.HaveRelic(Relic))
			return;
		float fullDamage = attack.GetDamage(attackManager, dmgValues, dmgMult, dmgBonus, critCount, false);
		float baseDamage = GetBaseDamage(attack, attackManager, dmgValues, dmgMult, dmgBonus, critCount);
		if (fullDamage > baseDamage)
			goodCount += (int)(fullDamage - baseDamage);
		else if (fullDamage < baseDamage)
			badCount += (int)(baseDamage - fullDamage);
	}
	public abstract float GetBaseDamage(Battle.Attacks.Attack attack, Battle.Attacks.AttackManager attackManager, float[] dmgValues, float dmgMult, int dmgBonus, int critCount);
}
[HarmonyPatch]
public abstract class PegDamageCounter : DamageCounter {
	public virtual int Step => 1;
	protected bool _active = false;
	private float peg_count = 0;
	private int bonus_count = 0;
	public override void Reset() {
		base.Reset();
		_active = false;
		peg_count = bonus_count = 0;
	}
	public override void Used() {
		_active = true;
	}
	public virtual void StartAddPeg() {}
	public virtual void AddPeg(float multiplier, int bonus) {
		if (_active) {
			peg_count += multiplier;
			bonus_count += bonus;
		}
		_active = false;
	}
	public override float GetBaseDamage(Battle.Attacks.Attack attack, Battle.Attacks.AttackManager attackManager, float[] dmgValues, float dmgMult, int dmgBonus, int critCount) {
		float[] newDmgValues = new float[dmgValues.Length + 1];
		newDmgValues = dmgValues.Append(-peg_count).ToArray();
		float baseDamage = attack.GetDamage(attackManager, newDmgValues, dmgMult, dmgBonus - bonus_count, critCount, false);
		peg_count = bonus_count = 0;
		return baseDamage;
	}
}
[HarmonyPatch]
public abstract class OrbDamageCounter : DamageCounter {
	public override void Used() {}
	public override float GetBaseDamage(Battle.Attacks.Attack attack, Battle.Attacks.AttackManager attackManager, float[] dmgValues, float dmgMult, int dmgBonus, int critCount) {
		Relics.RelicManager relicManager = Utils.GetResource<Relics.RelicManager>();
		var owned = Refl<Dictionary<Relics.RelicEffect, Relics.Relic>>.GetAttr(relicManager, "_ownedRelics");
		Relics.Relic r = owned[Relic];
		owned.Remove(Relic);
		float baseDamage = attack.GetDamage(attackManager, dmgValues, dmgMult, dmgBonus, critCount, false);
		owned.Add(Relic, r);
		return baseDamage;
	}
}
[HarmonyPatch]
public abstract class MultDamageCounter : DamageCounter {
	protected bool _active = false;
	protected float multiplier = 1f;
	public override void Reset() {
		base.Reset();
		multiplier = 1f;
	}
	public override void Used() {
		_active = true;
	}
	public virtual void AddDamageMultiplier(float mult) {
		if (_active)
			multiplier *= mult;
		_active = false;
	}
	public override float GetBaseDamage(Battle.Attacks.Attack attack, Battle.Attacks.AttackManager attackManager, float[] dmgValues, float dmgMult, int dmgBonus, int critCount) {
		dmgMult /= multiplier;
		multiplier = 1f;
		return attack.GetDamage(attackManager, dmgValues, dmgMult, dmgBonus, critCount, false);
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
	public virtual void Damage(float amount) {
		if (_active)
			count += (int)amount;
		_active = false;
	}
	public override string Tooltip => $"{count} <style=damage>damage dealt</style>";
}

public abstract class DamageAllCounter : SimpleCounter {
	public DamageAllCounter() {
		Battle.Enemies.Enemy.OnAllEnemiesDamaged += new Battle.Enemies.Enemy.DamageAllEnemies(this.DamageAllEnemies);
	}

	protected bool _active = false;
	public override void Used() {
		_active = true;
	}
	private void DamageAllEnemies(float damageAmount) {
		if (_active) {
			count += Utils.EnemyDamageCount() * (int)damageAmount;
			_active = false;
		}
	}

	public override string Tooltip => $"{count} <style=damage>damage dealt</style>";
}

public abstract class StatusEffectCounter : SimpleCounter {
	public abstract Battle.StatusEffects.StatusEffectType type { get; }
	protected bool _active = false;
	public override void Used() {
		_active = true;
	}
	public void ApplyStatusEffect(Battle.StatusEffects.StatusEffect statusEffect) {
		if (_active && statusEffect.EffectType == type)
			count += statusEffect.Intensity;
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

	static public TFld GetAttr<TObj, TFld>(TObj obj, string field) {
		var fld = typeof(TObj).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
		return (TFld)fld.GetValue(obj);
	}

	static public void SetAttr<TObj, TFld>(TObj obj, string field, TFld val) {
		var fld = typeof(TObj).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
		fld.SetValue(obj, val);
	}

	static public TFld GetStaticAttr<TObj, TFld>(string field) {
		var fld = typeof(TObj).GetField(field, BindingFlags.NonPublic | BindingFlags.Static);
		return (TFld)fld.GetValue(null);
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

public class Refl<TFld> {
	static public TFld GetAttr<TObj>(TObj obj, string field) {
		return Utils.GetAttr<TObj, TFld>(obj, field);
	}

	static public void SetAttr<TObj>(TObj obj, string field, TFld val) {
		Utils.SetAttr<TObj, TFld>(obj, field, val);
	}

	static public TFld GetStaticAttr<TObj>(string field) {
		return Utils.GetStaticAttr<TObj, TFld>(field);
	}
}
