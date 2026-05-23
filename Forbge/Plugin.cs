using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.SceneManagement;

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

    private static bool firstTime = true;
    [HarmonyPatch(typeof(Loading.PeglinSceneLoader), "HideLoadingScreen")]
    [HarmonyPostfix]
    private static void HookSceneChange() {
        // Try to hook into the first time we switch to the Main Menu scene
        // This is after all the builtin content has been loaded, and the important
        // managers should be available. (eg LoadoutManager doesn't exist until
        // we switch to the main menu scene.)
        if (firstTime && SceneManager.GetActiveScene().name == "MainMenu") {
            firstTime = false;
            PerformRegistration();
        }
    }


    internal static bool inRegistration = false;
    internal static Assembly currentRegistrar = null;

    private static void PerformRegistration() {
        if (onRegister == null) {
            Logger.LogWarning("No plugins registered");
            return;
        }

        CustomOrb.Init();

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
