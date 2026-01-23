using HarmonyLib;
using System;

namespace CustomCruciball;

[HarmonyPatch]
public class State : SaveObjectData {
	public static readonly State inst = new State();

	public bool isCustom = false;
	public bool[] levels = new bool[Constants.NUM_LEVELS];

	// Hooks to reset when custom start data is cleared
	[HarmonyPatch(typeof(PeglinUI.LoadoutManager.LoadoutManager), "Cancel")]
	[HarmonyPostfix]
	private static void Cancel() {
		inst.isCustom = false;
	}
	[HarmonyPatch(typeof(PeglinUI.LoadoutManager.LoadoutManager), "DefaultLoadout")]
	[HarmonyPostfix]
	private static void Reset() {
		inst.isCustom = false;
		CustomUI.UpdateCruxDisplay();
	}
	[HarmonyPatch(typeof(PeglinUI.LoadoutManager.LoadoutManager), "OnEnable")]
	[HarmonyPostfix]
	private static void Init() {
		inst.isCustom = false;
	}

	// Hooks to save/load
	public readonly static string KEY = $"{MyPluginInfo.PLUGIN_GUID}_CustomCruxData";

	public override string Name => KEY;

	State() : base(true, ToolBox.Serialization.DataSerializer.SaveType.RUN) {
		SaveManager.OnLoadRequested += new SaveManager.LoadRequested(Load);
		SaveManager.OnSaveRequested += new SaveManager.SaveRequested(Save);
	}

	void Load() {
		State data = (State)ToolBox.Serialization.DataSerializer.Load<SaveObjectData>(State.KEY, ToolBox.Serialization.DataSerializer.SaveType.RUN);
		if (data != null) {
			isCustom = data.isCustom;
			Array.Copy(data.levels, levels, Constants.NUM_LEVELS);
		} else {
			// if there's no saved data, we are likely resuming a save from before the mod was installed
			// so assume this means no customisation
			isCustom = false;
		}
	}
}
