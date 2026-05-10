using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace Forbge;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin {
    private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
    internal static new ManualLogSource Logger;

    private void Awake() {
        Logger = base.Logger;

        harmony.PatchAll();
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }
}

[HarmonyPatch]
public class Registry {
    public static Action onRegister = null;

    [HarmonyPatch(typeof(Loading.AssetLoading), "LoadAllAssets")]
    [HarmonyPrefix]
    private static void HookLoadAssets(ref Action onLoadComplete) {
        // call our PerformRegistration method _before_ chaining to Peglin's callback
        // which will finalise everything and switch to the main menu
        onLoadComplete = PerformRegistration + onLoadComplete;
    }

    private static void PerformRegistration() {
        Plugin.Logger.LogInfo($"Starting registration for {onRegister.GetInvocationList().Length} plugins");
        onRegister();
        Plugin.Logger.LogInfo($"Registration complete!");
    }
}
