using System;
using System.Collections.Generic;
using UnityEngine;
using Relics;

namespace Forbge;

public class RelicBuilder {
    public readonly Translation name = new Translation();
    public readonly Translation description = new Translation();
    public readonly Translation lockedDescription = new Translation();
    public PeglinUI.Tooltip.LockedDescriptionPresets lockedDescriptionPreset = PeglinUI.Tooltip.LockedDescriptionPresets.RELIC_DEFAULT;
    public RelicRarity rarity = RelicRarity.COMMON;
    public Sprite sprite = AssetHelper.QUESTIONMARK;
    public AudioClip useSfx = null;
    public bool intro = false;
    public int? countdown = null;
    public int? usesPerBattle = null;
    public int? usesPerRun = null;
    public int? usesPerShot = null;

    public Relic Register() {
        Registry.AssertInRegistration();
        Validate();

        RelicEffect effect = GenRelicEffect();
        string locKey = $"_forbge_custom_relic_{(int)effect}";

        name.Register($"Relics/{locKey}_name");
        description.Register($"Relics/{locKey}_desc");
        lockedDescription.Register($"Relics/{locKey}_desc_locked");

        var relic = ScriptableObject.CreateInstance<Relic>();
        relic.name = name.Default;
        relic.effect = effect;
        relic.locKey = locKey;
        relic.lockedDescriptionPreset = lockedDescriptionPreset;
        relic.globalRarity = rarity;
        relic.sprite = sprite;
        relic.useSfx = useSfx;

        Managers.relicManager.globalRelics.relics.Add(relic);
        if (intro)
            Managers.relicManager.limitedIntroRelics.relics.Add(relic);
        if (countdown != null)
            RelicManager.relicCountdownValues[relic.effect] = (int)countdown;
        if (usesPerBattle != null)
            RelicManager.relicUsesPerBattleCounts[relic.effect] = (int)usesPerBattle;
        if (usesPerRun != null)
            RelicManager.relicUsesPerRunCounts[relic.effect] = (int)usesPerRun;
        if (usesPerShot != null)
            RelicManager.relicUsesPerShotCounts[relic.effect] = (int)usesPerShot;

        return relic;
    }

    // Claim space in the ID range that's far from where the actual game is generating ids
    private const int EFFECT_PREFIX = ((int)'F') << 24;
    private const int EFFECT_HASH_MASK = 0x00FFFFFF;
    private readonly HashSet<int> usedEffects = new HashSet<int>();


    private RelicEffect GenRelicEffect() {
        // At least try to generate a stable effect ID for each relic so that
        // eg unlocks under Custom Start will be reliable even if mods are
        // added/removed/reordered (which would break a simple counter)

        // But, ultimately, if there are collisions, we just have to make do

        int hash = ($"{Registry.currentRegistrar.GetName().Name}__{name.Default}").GetHashCode();
        int candidate;
        while (usedEffects.Contains(candidate = EFFECT_PREFIX | (hash & EFFECT_HASH_MASK))) {
            hash++;
        }
        usedEffects.Add(candidate);
        return (RelicEffect)candidate;
    }

    public void Validate() {
        if (String.IsNullOrEmpty(name.Default))
            throw new ArgumentNullException("name");
        if (String.IsNullOrEmpty(description.Default))
            throw new ArgumentNullException("descrpition");
    }
}
