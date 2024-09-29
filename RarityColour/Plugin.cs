using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace RarityColour;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Peglin.exe")]
[HarmonyPatch]
public class Plugin : BaseUnityPlugin
{
	private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
	internal static new ManualLogSource Logger;

	// Normal background is red tinted - #77403C
	// These colours are tinted cyan to compensate
	private static readonly ColorBlock COMMON = MakeColorBlock(1f, 1f, 1f);
	private static readonly ColorBlock UNCOMMON = MakeColorBlock(0.25f, 1f, 0.5f);
	private static readonly ColorBlock RARE = MakeColorBlock(0.25f, 0.75f, 1f);
	private static readonly ColorBlock BOSS = MakeColorBlock(0.75f, 0.25f, 1f);
	private static readonly ColorBlock SPECIAL = MakeColorBlock(0.75f, 1f, 0.25f);

	private void Awake()
	{
		harmony.PatchAll();
		Logger = base.Logger;
		Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
	}

	private static ColorBlock MakeColorBlock(float r, float g, float b) {
		ColorBlock cb = new ColorBlock();
		cb.normalColor = new Color(r * 0.7f, g * 0.7f, b * 0.7f, 1f);
		cb.highlightedColor = new Color(r, g, b, 1f);
		cb.pressedColor = new Color(r, g, b, 1f);
		cb.selectedColor = new Color(r * 0.95f, g * 0.95f, b * 0.95f, 1f);
		cb.disabledColor = new Color(r * 0.8f, g * 0.8f, b * 0.8f, 1f);
		cb.colorMultiplier = 1f;
		cb.fadeDuration = 0.1f;
		return cb;
	}


	[HarmonyPatch(typeof(PeglinUI.PostBattle.UpgradeOption), "SpecifiedOrb", MethodType.Setter)]
	[HarmonyPostfix]
	static private void PatchOfferOrb(PeglinUI.PostBattle.UpgradeOption __instance, UnityEngine.GameObject value) {
		//Logger.LogInfo($"HOOK: Set orb offer {__instance.name} to {value.name}");

		Button btn = __instance.gameObject.GetComponent<Button>();
		if (!btn) {
			Logger.LogError($"Couldn't find UnityEngine.UI.Button on {__instance.name}!");
			return;
		}

		DeckManager[] deckManagers = Resources.FindObjectsOfTypeAll<DeckManager>();
		if (deckManagers.Length <= 0) {
			Logger.LogError($"Couldn't find DeckManager!");
			return;
		}
		DeckManager deckManager = deckManagers[0];

		Peglin.ClassSystem.Class cls = (Peglin.ClassSystem.Class)typeof(DeckManager).GetField("_selectedClass", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(deckManager);
		PachinkoBall.OrbRarity rarity = (PachinkoBall.OrbRarity)typeof(DeckManager).GetMethod("GetFinalRarityForOrb", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(deckManager, new object[]{value, cls});
		// Some orbs, like Pebball, are "Not Present" on some classes, but can still be in your deck
		// Or maybe you're using Custom Start
		// In this case, try to get the orb's base rarity
		if (rarity == PachinkoBall.OrbRarity.NOT_PRESENT)
			rarity = value.GetComponent<PachinkoBall>().orbRarity;

		//Logger.LogInfo($"Final rarity is {rarity}");
		switch (rarity) {
			case PachinkoBall.OrbRarity.NOT_PRESENT:
			case PachinkoBall.OrbRarity.COMMON:
				btn.colors = COMMON;
				break;
			case PachinkoBall.OrbRarity.UNCOMMON:
				btn.colors = UNCOMMON;
				break;
			case PachinkoBall.OrbRarity.RARE:
				btn.colors = RARE;
				break;
			case PachinkoBall.OrbRarity.SPECIAL:
				btn.colors = SPECIAL;
				break;
		}
	}

	[HarmonyPatch(typeof(RelicIcon), "SetRelic")]
	[HarmonyPostfix]
	static private void PatchOfferRelic(RelicIcon __instance, Relics.Relic r) {
		//Logger.LogInfo($"HOOK: Set relic offer {__instance.name} to {r.name}");

		Button btn = __instance.gameObject.GetComponentInParent<UnityEngine.UI.Button>();
		if (!btn) {
			// This is not an error, as this method is also called for the icons in the left status area
			// which are not buttons
			//Logger.LogError($"Couldn't find UnityEngine.UI.Button on {__instance.name}!");
			return;
		}

		Relics.RelicManager[] relicManagers = Resources.FindObjectsOfTypeAll<Relics.RelicManager>();
		if (relicManagers.Length <= 0) {
			Logger.LogError($"Couldn't find RelicManager!");
			return;
		}
		Relics.RelicManager relicManager = relicManagers[0];

		Peglin.ClassSystem.Class cls = (Peglin.ClassSystem.Class)typeof(Relics.RelicManager).GetField("_selectedClass", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(relicManager);

		PeglinUI.LoadoutManager.ClassLoadoutPairs[] loadouts = (PeglinUI.LoadoutManager.ClassLoadoutPairs[])typeof(Relics.RelicManager).GetField("_classLoadouts", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(relicManager);
		Peglin.ClassSystem.ClassLoadoutData loadout = null;
		for (int i = 0; i < loadouts.Length; i++) {
			if (loadouts[i].Class == cls) {
				loadout = loadouts[i].Loadout;
				break;
			}
		}
		Relics.RelicRarity rarity;
		if (loadout != null) {
			rarity = (Relics.RelicRarity)typeof(Relics.RelicManager).GetMethod("GetFinalRarityForRelic", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(relicManager, new object[]{r, loadout});
			if (rarity == Relics.RelicRarity.UNAVAILABLE)
				rarity = r.globalRarity;
		} else {
			rarity = r.globalRarity;
		}

		//Logger.LogInfo($"Final rarity is {rarity}");
		switch (rarity) {
			case Relics.RelicRarity.COMMON:
			case Relics.RelicRarity.UNAVAILABLE:
				btn.colors = COMMON;
				break;
			case Relics.RelicRarity.RARE:
				btn.colors = RARE;
				break;
			case Relics.RelicRarity.BOSS:
				btn.colors = BOSS;
				break;
			case Relics.RelicRarity.NONE:
				btn.colors = SPECIAL;
				break;
		}
	}
}
