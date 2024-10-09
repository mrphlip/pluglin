using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using UnityEngine;

namespace RelicStats;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Peglin.exe")]
[HarmonyPatch]
public class Plugin : BaseUnityPlugin {
	private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
	internal static new ManualLogSource Logger;

	private void Awake() {
		Logger = base.Logger;
		Tracker.PopulateTrackers();

		harmony.PatchAll();
		Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
	}

	[HarmonyPatch(typeof(PeglinUI.Tooltip), "Initialize", new Type[] {typeof(Relics.Relic)})]
	[HarmonyPostfix]
	static private void PatchTooltipInitPost(PeglinUI.Tooltip __instance, Relics.Relic relic) {
		if (!Tracker.HaveRelic(relic.effect))
			return;

		Tracker tracker = null;
		if (!Tracker.trackers.TryGetValue(relic.effect, out tracker))
			return;

		string tooltip = tracker.Tooltip;
		if (String.IsNullOrEmpty(tooltip))
			return;

		string oldTooltip = __instance.descriptionText.text;
		I2.Loc.Localize loc = __instance.descriptionText.GetComponent<I2.Loc.Localize>();
		loc.mTerm = null;
		loc.mTermSecondary = null;
		loc.FinalTerm = null;
		loc.FinalSecondaryTerm = null;
		__instance.descriptionText.text = $"{oldTooltip}\n<sprite name=\"BULLET\"><indent=8%>{tooltip}</indent>";
	}
}
