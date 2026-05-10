using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace ForbgeSample;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[HarmonyPatch]
public class Plugin : BaseUnityPlugin {
    private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
    internal static new ManualLogSource Logger;

    private void Awake() {
        Logger = base.Logger;

        harmony.PatchAll();
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        Forbge.Registry.onRegister += Register;
    }

    private void Register() {
        Logger.LogInfo("Time to register stuff!");
    }
}
