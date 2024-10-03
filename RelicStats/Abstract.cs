using HarmonyLib;
using System;
using System.Collections.Generic;
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

[HarmonyPatch]
public abstract class PegDamageCounter : SimpleCounter {
	private int activate_count = 0;
	public override void Used() {
		activate_count += Step;
	}
	public virtual void HandleFire(int damagePerPeg) {
		if (damagePerPeg > 0 && activate_count > 0)
			count += activate_count * damagePerPeg;
		activate_count = 0;
	}
	public override string Tooltip => $"{count} <style=damage>damage dealt</style>";
}
[HarmonyPatch]
public abstract class OrbDamageCounter : SimpleCounter {
	public virtual int BonusDamage => 0;
	public virtual int BonusCrit => 0;
	public override void Used() {}
	public virtual void HandleFire(Battle.Attacks.Attack attack, float[] dmgValues, int critCount) {
		if (!Tracker.HaveRelic(Relic))
			return;
		var rules = attack.GetComponent<Battle.Pachinko.BallBehaviours.AttackDamageModifiableRules>();
		if (critCount <= 0 && rules.baseDamageNonMod)
			return;
		else if (critCount > 0 && rules.critDamageNonMod)
			return;
		int extraDamage = (critCount > 0) ? BonusCrit : BonusDamage;
		if (critCount <= 0 && rules != null && rules.critBoostAppliesToRegular)
			extraDamage += BonusCrit;
		else if (critCount > 0 && rules != null && rules.regularBoostAppliesToCrit)
			extraDamage += BonusDamage;
		if (extraDamage > 0)
			foreach (float dmg in dmgValues)
				if (dmg > 0)
					count += extraDamage * (int)dmg;
	}
	public override string Tooltip => $"{count} <style=damage>damage dealt</style>";
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
