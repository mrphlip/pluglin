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

	private static PeglinUI.Tooltip _activeTooltip = null;
	private static string _baseTooltip = null;
	private static Relics.RelicEffect? _activeRelic = null;

	[HarmonyPatch(typeof(PeglinUI.Tooltip), "Initialize", new Type[] {typeof(Relics.Relic)})]
	[HarmonyPostfix]
	static private void PatchTooltipInitPost(PeglinUI.Tooltip __instance, Relics.Relic relic) {
		if (!Tracker.HaveRelic(relic.effect))
			return;

		_activeTooltip = __instance;
		_activeRelic = relic.effect;

		_baseTooltip = __instance.descriptionText.text;
		I2.Loc.Localize loc = __instance.descriptionText.GetComponent<I2.Loc.Localize>();
		loc.mTerm = null;
		loc.mTermSecondary = null;
		loc.FinalTerm = null;
		loc.FinalSecondaryTerm = null;

		UpdateTooltip(relic.effect);
	}

	[HarmonyPatch(typeof(TooltipManager), "HideTooltip")]
	[HarmonyPrefix]
	static private void HideTooltip() {
		_activeTooltip = null;
		_activeRelic = null;
		_baseTooltip = null;
	}

	static public void UpdateTooltip(Relics.RelicEffect relic)
	{
		if (relic != _activeRelic)
			return;
		Tracker tracker = null;
		if (!Tracker.trackers.TryGetValue(relic, out tracker))
			return;

		string tooltip = tracker.Tooltip;
		if (String.IsNullOrEmpty(tooltip)) {
			_activeTooltip.descriptionText.text = _baseTooltip;
		} else {
			string[] lines = tooltip.Split('\n');
			for (int i = 0; i < lines.Length; i++)
				lines[i] = $"<sprite name=\"BULLET\"><indent=8%>{lines[i]}</indent>";
			tooltip = string.Join('\n', lines);
			_activeTooltip.descriptionText.text = $"{_baseTooltip}\n{tooltip}";
		}
	}
}
