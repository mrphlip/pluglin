using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Forbge;

namespace ForbgeSample;

[BepInPlugin(ForbgeSample.MyPluginInfo.PLUGIN_GUID, ForbgeSample.MyPluginInfo.PLUGIN_NAME, ForbgeSample.MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin {
    private readonly Harmony harmony = new Harmony(ForbgeSample.MyPluginInfo.PLUGIN_GUID);
    internal static new ManualLogSource Logger;

    private void Awake() {
        Logger = base.Logger;

        harmony.PatchAll();
        Logger.LogInfo($"Plugin {ForbgeSample.MyPluginInfo.PLUGIN_GUID} is loaded!");

        Forbge.Registry.onRegister += Register;
    }

    private void Register() {
        SampleRelics.Register();
        SampleOrbs.Register();
    }
}
