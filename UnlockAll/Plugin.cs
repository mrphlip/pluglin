using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Relics;
using Saving;

namespace UnlockAll;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[HarmonyPatch]
public class Plugin : BaseUnityPlugin {
    private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
    internal static new ManualLogSource Logger;

    private void Awake() {
        Logger = base.Logger;

        harmony.PatchAll();
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    [HarmonyPatch(typeof(PeglinUI.LoadoutManager.LoadoutIcon), "InitializeRelic")]
    [HarmonyPrefix]
    private static void UnlockRelic(Relic r, ref bool __state) {
        __state = PersistentPlayerData.Instance.UnlockedRelics.Contains(r.effect);
        if (!__state)
            PersistentPlayerData.Instance.UnlockedRelics.Add(r.effect);
    }
    [HarmonyPatch(typeof(PeglinUI.LoadoutManager.LoadoutIcon), "InitializeRelic")]
    [HarmonyPostfix]
    private static void UndoRelic(Relic r, bool __state) {
        if (!__state)
            PersistentPlayerData.Instance.UnlockedRelics.Remove(r.effect);
    }

    [HarmonyPatch(typeof(PeglinUI.LoadoutManager.LoadoutIcon), "InitializeOrb")]
    [HarmonyPrefix]
    private static void UnlockOrb(GameObject orbPrefab, ref bool __state) {
        string name = orbPrefab.GetComponent<Battle.Attacks.Attack>().locNameString;
        __state = PersistentPlayerData.Instance.UnlockedOrbs.Contains(name);
        if (!__state)
            PersistentPlayerData.Instance.UnlockedOrbs.Add(name);
    }
    [HarmonyPatch(typeof(PeglinUI.LoadoutManager.LoadoutIcon), "InitializeOrb")]
    [HarmonyPostfix]
    private static void UndoOrb(GameObject orbPrefab, bool __state) {
        string name = orbPrefab.GetComponent<Battle.Attacks.Attack>().locNameString;
        if (!__state)
            PersistentPlayerData.Instance.UnlockedOrbs.Remove(name);
    }
}
