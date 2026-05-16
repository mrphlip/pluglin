using System;
using HarmonyLib;
using UnityEngine;
using Forbge;
using Relics;
using Battle;
using Battle.Attacks;
using Battle.StatusEffects;

namespace ForbgeSample;

[HarmonyPatch]
public class SampleRelics {
    static Relic heartshield;
    static Relic evokemod;

    internal static void Register() {
        RelicBuilder r = new RelicBuilder();
        r.name["en"] = "Heartshield";
        r.description["en"] = "<style=heal>Healing</style> above your max HP will be converted into <style=ballwark>half as much Ballwark</style>.";
        r.rarity = RelicRarity.RARE;
        r.sprite = AssetHelper.MakeSprite("heartshield.png");
        heartshield = r.Register();

        r = new RelicBuilder();
        r.name["en"] = "Evoke Mod";
        r.description["en"] = "Every second <sprite name=\"BOMB\"> thrown will add one <sprite name=\"COIN_ONLY\"> to the board.";
        r.rarity = RelicRarity.COMMON;
        r.sprite = AssetHelper.MakeSprite("evokemod.png");
        r.countdown = 2;
        evokemod = r.Register();
    }

    [HarmonyPatch(typeof(PlayerHealthController), "Heal")]
    [HarmonyPostfix]
    private static void OnHeal(float amount, float __result) {
        // The PlayerHealthController.Heal method takes a parameter for the amount
        // of healing to do, and returns the amount of healing actually done, so
        // the difference between the two is the amount of overhealing done

        int overhealing = Mathf.RoundToInt(amount - __result);
        int ballwark = overhealing / 2;
        if (ballwark > 0 && Managers.relicManager.AttemptUseRelic(heartshield.effect)) {
            Managers.playerStatusEffectController.ApplyStatusEffect(
                new StatusEffect(StatusEffectType.Ballwark, ballwark),
                StatusEffectSource.PLAYER
            );
        }
    }

    [HarmonyPatch(typeof(BattleController), "OnBombDeath")]
    [HarmonyPrefix]
    private static void OnBombExplode(BombLob bomb) {
        // This call will automatically handle the "every second bomb" condition
        // for us, since we set "countdown = 2" above.
        if (Managers.relicManager.AttemptUseRelic(evokemod.effect)) {
            PegManager.AddGoldToPegsRequest(1, bomb.gameObject.transform.position);
        }
    }
}
