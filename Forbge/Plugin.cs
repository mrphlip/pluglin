using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace Forbge;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[HarmonyPatch]
public class Registry : BaseUnityPlugin {
    private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
    internal static new ManualLogSource Logger;

    public static Action onRegister;

    private void Awake() {
        Logger = base.Logger;

        harmony.PatchAll();
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    [HarmonyPatch(typeof(Loading.AssetLoading), "LoadAllAssets")]
    [HarmonyPrefix]
    private static void HookLoadAssets(ref Action onLoadComplete) {
        // call our PerformRegistration method _before_ chaining to Peglin's callback
        // which will finalise everything and switch to the main menu
        onLoadComplete = PerformRegistration + onLoadComplete;
    }


    internal static bool inRegistration = false;
    internal static Assembly currentRegistrar = null;

    private static void PerformRegistration() {
        if (onRegister == null) {
            Logger.LogWarning("No plugins registered");
            return;
        }

        Logger.LogInfo($"Starting registration for {onRegister.GetInvocationList().Length} plugin(s)");
        inRegistration = true;
        int count = 0;
        foreach (Action mod in onRegister.GetInvocationList()) {
            currentRegistrar = mod.Method.DeclaringType.Assembly;
            Logger.LogInfo($"[{++count}] Registering: {currentRegistrar.GetName().Name}");
            mod();
        }
        inRegistration = false;
        currentRegistrar = null;
        Logger.LogInfo($"Registration complete!");
    }

    internal static void AssertInRegistration() {
        if (!inRegistration)
            throw new InvalidOperationException("Not in registration state");
    }
}
