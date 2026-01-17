using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;

namespace CustomCruciball;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Peglin.exe")]
public class Plugin : BaseUnityPlugin {
	private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
	internal static new ManualLogSource Logger;

	private void Awake() {
		harmony.PatchAll();
		Logger = base.Logger;
	}
}
