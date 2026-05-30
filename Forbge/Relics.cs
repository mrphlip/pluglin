using System;
using System.Collections.Generic;
using UnityEngine;
using Relics;

namespace Forbge;

public class CustomRelic {
    private Relic _relic;
    public Relic Relic => _relic;
    public RelicEffect Effect => _relic.effect;

    private Translation _name;
    private Translation _description;
    private Translation _lockedDescription;
    public Translation Name => _name;
    public Translation Description => _description;
    public Translation LockedDescription => _lockedDescription;
    public PeglinUI.Tooltip.LockedDescriptionPresets LockedDescriptionPreset {
        get => Relic.lockedDescriptionPreset;
        set { Relic.lockedDescriptionPreset = value; }
    }

    public RelicRarity Rarity { get => Relic.globalRarity; set { Relic.globalRarity = value; }}
    public Sprite Sprite { get => Relic.sprite; set { Relic.sprite = value; }}
    public AudioClip UseSfx { get => Relic.useSfx; set { Relic.useSfx = value; }}

    private bool _intro = false;
    public bool Intro {
        get => _intro;
        set {
            if (!_intro && value) {
                Managers.relicManager.limitedIntroRelics.relics.Add(Relic);
            } else if (_intro && !value) {
                Managers.relicManager.limitedIntroRelics.relics.Remove(Relic);
            }
            _intro = value;
        }
    }
    public int? Countdown {
        get {
            int val;
            if (RelicManager.relicCountdownValues.TryGetValue(Relic.effect, out val))
                return val;
            else
                return null;
        }
        set {
            if (value == null)
                RelicManager.relicCountdownValues.Remove(Relic.effect);
            else
                RelicManager.relicCountdownValues[Relic.effect] = (int)value;
        }
    }
    public int? UsesPerBattle {
        get {
            int val;
            if (RelicManager.relicUsesPerBattleCounts.TryGetValue(Relic.effect, out val))
                return val;
            else
                return null;
        }
        set {
            if (value == null)
                RelicManager.relicUsesPerBattleCounts.Remove(Relic.effect);
            else
                RelicManager.relicUsesPerBattleCounts[Relic.effect] = (int)value;
        }
    }
    public int? UsesPerRun {
        get {
            int val;
            if (RelicManager.relicUsesPerRunCounts.TryGetValue(Relic.effect, out val))
                return val;
            else
                return null;
        }
        set {
            if (value == null)
                RelicManager.relicUsesPerRunCounts.Remove(Relic.effect);
            else
                RelicManager.relicUsesPerRunCounts[Relic.effect] = (int)value;
        }
    }
    public int? UsesPerShot {
        get {
            int val;
            if (RelicManager.relicUsesPerShotCounts.TryGetValue(Relic.effect, out val))
                return val;
            else
                return null;
        }
        set {
            if (value == null)
                RelicManager.relicUsesPerShotCounts.Remove(Relic.effect);
            else
                RelicManager.relicUsesPerShotCounts[Relic.effect] = (int)value;
        }
    }

    public CustomRelic(string name) {
        Registry.AssertInRegistration();

        _relic = ScriptableObject.CreateInstance<Relic>();
        _relic.name = name;
        _relic.effect = GenRelicEffect(name);
        string locKey = $"_forbge_custom_relic_{(int)_relic.effect}";
        _relic.locKey = locKey;
        _relic.lockedDescriptionPreset = PeglinUI.Tooltip.LockedDescriptionPresets.RELIC_DEFAULT;
        _relic.globalRarity = RelicRarity.COMMON;
        _relic.sprite = AssetHelper.QUESTIONMARK;
        _relic.useSfx = null;

        _name = new Translation(name);
        _description = new Translation("");
        _lockedDescription = new Translation();
        _name.Register($"Relics/{locKey}_name");
        _description.Register($"Relics/{locKey}_desc");
        _lockedDescription.Register($"Relics/{locKey}_desc_locked");

        Managers.relicManager.globalRelics.relics.Add(_relic);
    }
    // convenience method so the API looks the same as CustomOrb
    public static CustomRelic Create(string name) {
        return new CustomRelic(name);
    }

    // Claim space in the ID range that's far from where the actual game is generating ids
    private const int EFFECT_PREFIX = ((int)'F') << 24;
    private const int EFFECT_HASH_MASK = 0x00FFFFFF;
    private readonly HashSet<int> usedEffects = new HashSet<int>();


    private RelicEffect GenRelicEffect(string name) {
        // At least try to generate a stable effect ID for each relic so that
        // eg unlocks under Custom Start will be reliable even if mods are
        // added/removed/reordered (which would break a simple counter)

        // But, ultimately, if there are collisions, we just have to make do

        int hash = ($"{Registry.currentRegistrar.GetName().Name}__{name}").GetHashCode();
        int candidate;
        while (usedEffects.Contains(candidate = EFFECT_PREFIX | (hash & EFFECT_HASH_MASK))) {
            hash++;
        }
        usedEffects.Add(candidate);
        return (RelicEffect)candidate;
    }

    public bool AttemptUseRelic() {
        return Managers.relicManager.AttemptUseRelic(Effect);
    }
    public bool RelicEffectActive() {
        return Managers.relicManager.RelicEffectActive(Effect);
    }
}
