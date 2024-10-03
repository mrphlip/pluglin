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


	public static readonly Dictionary<Relics.RelicEffect, Tracker> trackers = new Dictionary<Relics.RelicEffect, Tracker>();
	public static readonly HashSet<Relics.RelicEffect> relics = new HashSet<Relics.RelicEffect>();
	public static void PopulateTrackers() {
		AddTracker(new ConsolationPrize());
		AddTracker(new EnhancedGunpowder());
		AddTracker(new AlchemistCookbook());
		AddTracker(new Cookie());
		AddTracker(new WellDoneSteak());
		AddTracker(new BagOfOrangePegs());
		AddTracker(new LightShaftPotion());
		AddTracker(new HeavyShaftPotion());
		AddTracker(new WeightedChip());
		AddTracker(new OldSaltShaker());
		AddTracker(new GiftThatKeepsGiving());
		AddTracker(new RoundGuard());
		AddTracker(new Refillibuster());
		AddTracker(new MatryoshkaDoll());
		AddTracker(new Recombombulator());
		AddTracker(new ShortFuse());
		AddTracker(new StrangeBrew());
		AddTracker(new LuckyPenny());
		AddTracker(new ThreeExtraCrits());
		AddTracker(new RefreshingPunch());
		AddTracker(new PegBag());
		AddTracker(new ThreeExtraRefresh());
		AddTracker(new EvadeChance());
		AddTracker(new Apple());
		AddTracker(new WallChicken());
		AddTracker(new PowerGlove());

		AddTracker(new OldGardenerGloves());
		ResetAll();

		foreach (Relics.RelicEffect i in Enum.GetValues(typeof(Relics.RelicEffect))) {
			if (!trackers.ContainsKey(i) && (int)i <= 25) {
				Plugin.Logger.LogWarning($"Missing a tracker for {i}");
			}
		}
	}
	private static void AddTracker(Tracker tracker) {
		trackers.Add(tracker.Relic, tracker);
	}
	public static void ResetAll() {
		foreach (var tracker in trackers) {
			tracker.Value.Reset();
		}
		relics.Clear();
	}
	// Maintain our own cache of which relics are collected, to avoid
	// having to go digging for a RelicManager every time we need to check
	public static void AddRelic(Relics.RelicEffect relic) {
		if (!relics.Contains(relic)) {
			Tracker tracker;
			if (trackers.TryGetValue(relic, out tracker))
				tracker.Reset();
			relics.Add(relic);
		}
	}
	public static bool HaveRelic(Relics.RelicEffect relic) => relics.Contains(relic);
}

public abstract class NoopTracker : Tracker {
	public override void Reset() {}
	public override void Used() {}
	public override string Tooltip => null;
	public override object State { get => null; set {} }
}

public abstract class SimpleCounter : Tracker {
	protected int count = 0;

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
	public abstract int? HealAmount { get; }
	public override int Step { get {
		Battle.PlayerHealthController controller = Utils.GetResource<Battle.PlayerHealthController>();
		FloatVariable health = Utils.GetAttr<Battle.PlayerHealthController, FloatVariable>(controller, "_playerHealth");
		FloatVariable maxhealth = Utils.GetAttr<Battle.PlayerHealthController, FloatVariable>(controller, "_maxPlayerHealth");
		float amount = Mathf.Min(HealAmount ?? 0, maxhealth.Value - health.Value);
		if (amount > 0f)
			return (int)amount;
		else
			return 0;
	}}
	public override string Tooltip => $"{count} <style=heal>healed</style>";
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
	private int activate_count = 0;
	public override void Reset() {
		base.Reset();
		activate_count = 0;
	}
	public override void Used() {
		activate_count += Step;
	}
	public override float GetBaseDamage(Battle.Attacks.Attack attack, Battle.Attacks.AttackManager attackManager, float[] dmgValues, float dmgMult, int dmgBonus, int critCount) {
		float[] newDmgValues = new float[dmgValues.Length + 1];
		newDmgValues = dmgValues.Append(-activate_count).ToArray();
		activate_count = 0;
		return attack.GetDamage(attackManager, newDmgValues, dmgMult, dmgBonus, critCount, false);
	}
}
[HarmonyPatch]
public abstract class OrbDamageCounter : DamageCounter {
	public override void Used() {}
	public override float GetBaseDamage(Battle.Attacks.Attack attack, Battle.Attacks.AttackManager attackManager, float[] dmgValues, float dmgMult, int dmgBonus, int critCount) {
		Relics.RelicManager relicManager = Utils.GetResource<Relics.RelicManager>();
		var owned = Utils.GetAttr<Relics.RelicManager, Dictionary<Relics.RelicEffect, Relics.Relic>>(relicManager, "_ownedRelics");
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
		Plugin.Logger.LogInfo($"Used {Relic}");
		_active = true;
	}
	public virtual void AddDamageMultiplier(float mult) {
		Plugin.Logger.LogInfo($"Activated multiplier {mult}");
		if (_active)
			multiplier *= mult;
		Plugin.Logger.LogInfo($"Accumulated multiplier {multiplier}");
		_active = false;
	}
	public override float GetBaseDamage(Battle.Attacks.Attack attack, Battle.Attacks.AttackManager attackManager, float[] dmgValues, float dmgMult, int dmgBonus, int critCount) {
		Plugin.Logger.LogInfo($"Calculating damage: {dmgMult}");
		dmgMult /= multiplier;
		Plugin.Logger.LogInfo($"Previous multiplier: {dmgMult}");
		multiplier = 1f;
		return attack.GetDamage(attackManager, dmgValues, dmgMult, dmgBonus, critCount, false);
	}
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

	static public string Plural(int n, string ifplural = "s", string ifsingle = "") {
		return (n == 1) ? ifsingle : ifplural;
	}
}
