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
    static CustomRelic heartshield;
    static CustomRelic evokemod;

    internal static void Register() {
        heartshield = new CustomRelic("Heartshield");
        heartshield.Name["en"] = "Heartshield";
        heartshield.Description["en"] = "<style=heal>Healing</style> above your max HP will be converted into <style=ballwark>half as much Ballwark</style>.";
        heartshield.Rarity = RelicRarity.RARE;
        heartshield.Sprite = AssetHelper.MakeSprite("heartshield.png");

        evokemod = new CustomRelic("Evoke Mod");
        evokemod.Name["en"] = "Evoke Mod";
        evokemod.Description["en"] = "Every second <sprite name=\"BOMB\"> thrown will add one <sprite name=\"COIN_ONLY\"> to the board.";
        evokemod.Rarity = RelicRarity.COMMON;
        evokemod.Sprite = AssetHelper.MakeSprite("evokemod.png");
        evokemod.Countdown = 2;
    }

    [HarmonyPatch(typeof(PlayerHealthController), "Heal")]
    [HarmonyPostfix]
    private static void OnHeal(float amount, float __result) {
        // The PlayerHealthController.Heal method takes a parameter for the amount
        // of healing to do, and returns the amount of healing actually done, so
        // the difference between the two is the amount of overhealing done

        int overhealing = Mathf.RoundToInt(amount - __result);
        int ballwark = overhealing / 2;
        if (ballwark > 0 && heartshield.AttemptUseRelic()) {
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
        // for us, since we set "Countdown = 2" above.
        if (evokemod.AttemptUseRelic()) {
            PegManager.AddGoldToPegsRequest(1, bomb.gameObject.transform.position);
        }
    }
}
