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
public class Plugin : BaseUnityPlugin {
	private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
	internal static new ManualLogSource Logger;

	private static readonly ColorBlock COMMON = MakeColorBlock(0.5f, 0.25f, 0.25f);
	private static readonly ColorBlock COMMON_LV2 = MakeColorBlock(0.5f, 0.5f, 0.5f);
	private static readonly ColorBlock COMMON_LV3 = MakeColorBlock(0.9f, 0.55f, 0.3f);
	private static readonly ColorBlock UNCOMMON = MakeColorBlock(0.25f, 0.75f, 0.25f);
	private static readonly ColorBlock RARE = MakeColorBlock(0.25f, 0.25f, 0.75f);
	private static readonly ColorBlock BOSS = MakeColorBlock(0.75f, 0.25f, 0.75f);
	private static readonly ColorBlock SPECIAL = MakeColorBlock(0.75f, 0.5f, 0.25f);

	private static readonly Sprite ItemBackground1 = MakeSprite(Assets.PNGItemBackground1);
	private static readonly Sprite ItemBackground2 = MakeSprite(Assets.PNGItemBackground2);
	private static readonly Sprite ItemBackground3 = MakeSprite(Assets.PNGItemBackground3);
	private static readonly Sprite ItemBackgroundSold = MakeSprite(Assets.PNGItemBackgroundSold);
	private static readonly Sprite ShopBackground = MakeSprite(Assets.PNGShopBackground);

	private void Awake() {
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

	private static Sprite MakeSprite(byte[] pngdata) {
		Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
		tex.LoadImage(pngdata);
		tex.filterMode = FilterMode.Point;
		return UnityEngine.Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
	}

	static private (PachinkoBall.OrbRarity, int) GetOrbProps(GameObject orb) {
		DeckManager[] deckManagers = Resources.FindObjectsOfTypeAll<DeckManager>();
		if (deckManagers.Length <= 0) {
			Logger.LogError($"Couldn't find DeckManager!");
			return (PachinkoBall.OrbRarity.COMMON, 1);
		}
		DeckManager deckManager = deckManagers[0];

		Peglin.ClassSystem.Class cls = (Peglin.ClassSystem.Class)typeof(DeckManager).GetField("_selectedClass", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(deckManager);
		PachinkoBall.OrbRarity rarity = (PachinkoBall.OrbRarity)typeof(DeckManager).GetMethod("GetFinalRarityForOrb", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(deckManager, new object[]{orb, cls});
		// Some orbs, like Pebball, are "Not Present" on some classes, but can still be in your deck
		// Or maybe you're using Custom Start
		// In this case, try to get the orb's base rarity
		if (rarity == PachinkoBall.OrbRarity.NOT_PRESENT)
			rarity = orb.GetComponent<PachinkoBall>().orbRarity;

		Battle.Attacks.Attack attack = orb.GetComponent<Battle.Attacks.Attack>();
		int level= attack.Level;
		if (attack is Battle.Attacks.AssemballAttack)
			level = (attack as Battle.Attacks.AssemballAttack).GetAssemballDisplayLevel();

		return (rarity, level);
	}

	static private ColorBlock GetOrbColour(PachinkoBall.OrbRarity rarity, int level) {
		switch (rarity) {
			case PachinkoBall.OrbRarity.NOT_PRESENT:
			case PachinkoBall.OrbRarity.COMMON:
			default:
				if (level <= 1)
					return COMMON;
				else if (level == 2)
					return COMMON_LV2;
				else
					return COMMON_LV3;
			case PachinkoBall.OrbRarity.UNCOMMON:
				return UNCOMMON;
			case PachinkoBall.OrbRarity.RARE:
				return RARE;
			case PachinkoBall.OrbRarity.SPECIAL:
				return SPECIAL;
		}
	}

	[HarmonyPatch(typeof(PeglinUI.PostBattle.UpgradeOption), "SpecifiedOrb", MethodType.Setter)]
	[HarmonyPostfix]
	static private void PatchOfferOrb(PeglinUI.PostBattle.UpgradeOption __instance, GameObject value) {
		Button btn = __instance.gameObject.GetComponent<Button>();
		if (!btn) {
			Logger.LogError($"Couldn't find UnityEngine.UI.Button on {__instance.name}!");
			return;
		}
		Image backgroundImage = btn.targetGraphic as Image;
		if (backgroundImage == null || backgroundImage.name != "OrbBackground")
		{
			// The UpgradeOption class seems to handle both the actual orb buttons we want to style
			// and the "OK" button on the confirmation prompt, which we definitely do not
			return;
		}

		(PachinkoBall.OrbRarity rarity, int level) = GetOrbProps(value);
		btn.colors = GetOrbColour(rarity, level);

		if (level <= 1)
			backgroundImage.sprite = ItemBackground1;
		else if (level == 2)
			backgroundImage.sprite = ItemBackground2;
		else if (level >= 3)
			backgroundImage.sprite = ItemBackground3;
	}

	[HarmonyPatch(typeof(PeglinUI.PostBattle.UpgradeOption), "SetOptionInactive")]
	[HarmonyPostfix]
	static private void PatchPurchaseOrb(PeglinUI.PostBattle.UpgradeOption __instance) {
		Button btn = __instance.gameObject.GetComponent<Button>();
		if (!btn) {
			Logger.LogError($"Couldn't find UnityEngine.UI.Button on {__instance.name}!");
			return;
		}
		Image backgroundImage = btn.targetGraphic as Image;
		if (backgroundImage == null || backgroundImage.name != "OrbBackground")
		{
			// The UpgradeOption class seems to handle both the actual orb buttons we want to style
			// and the "OK" button on the confirmation prompt, which we definitely do not
			return;
		}
		backgroundImage.sprite = ItemBackgroundSold;

		if (btn.colors == COMMON_LV2 || btn.colors == COMMON_LV3)
			btn.colors = COMMON;
	}

	static private Relics.RelicRarity GetRelicRarity(Relics.Relic relic) {
		Relics.RelicManager[] relicManagers = Resources.FindObjectsOfTypeAll<Relics.RelicManager>();
		if (relicManagers.Length <= 0) {
			Logger.LogError($"Couldn't find RelicManager!");
			return Relics.RelicRarity.COMMON;
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
			rarity = (Relics.RelicRarity)typeof(Relics.RelicManager).GetMethod("GetFinalRarityForRelic", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(relicManager, new object[]{relic, loadout});
			if (rarity == Relics.RelicRarity.UNAVAILABLE)
				rarity = relic.globalRarity;
		} else {
			rarity = relic.globalRarity;
		}

		return rarity;
	}

	static private ColorBlock GetRelicColor(Relics.RelicRarity rarity) {
		switch (rarity) {
			case Relics.RelicRarity.COMMON:
			case Relics.RelicRarity.UNAVAILABLE:
			default:
				return COMMON;
			case Relics.RelicRarity.RARE:
				return RARE;
			case Relics.RelicRarity.BOSS:
				return BOSS;
			case Relics.RelicRarity.NONE:
				return SPECIAL;
		}
	}

	[HarmonyPatch(typeof(RelicIcon), "SetRelic")]
	[HarmonyPostfix]
	static private void PatchOfferRelic(RelicIcon __instance, Relics.Relic r) {
		Button btn = __instance.gameObject.GetComponentInParent<UnityEngine.UI.Button>();
		if (!btn) {
			// This is not an error, as this method is also called for the icons in the left status area
			// which are not buttons
			//Logger.LogError($"Couldn't find UnityEngine.UI.Button on {__instance.name}!");
			return;
		}

		Relics.RelicRarity rarity = GetRelicRarity(r);
		btn.colors = GetRelicColor(rarity);
		Image backgroundImage = btn.targetGraphic as Image;
		if (backgroundImage != null) {
			backgroundImage.sprite = ItemBackgroundSold;
		}
	}

	[HarmonyPatch(typeof(Scenarios.Shop.ShopItem), "Initialize")]
	[HarmonyPostfix]
	static private void PatchShop(Scenarios.Shop.ShopItem __instance, Scenarios.Shop.IPurchasableItem item, Scenarios.Shop.ShopManager sm) {
		Scenarios.Shop.PurchasableOrb item_orb = item as Scenarios.Shop.PurchasableOrb;
		Scenarios.Shop.PurchasableRelic item_relic = item as Scenarios.Shop.PurchasableRelic;
		Image background = __instance.itemBackground.GetComponent<Image>();
		if (item_orb != null) {
			GameObject orb = (GameObject)typeof(Scenarios.Shop.PurchasableOrb).GetField("_orbPrefab", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(item_orb);

			(PachinkoBall.OrbRarity rarity, int level) = GetOrbProps(orb);
			background.color = GetOrbColour(rarity, 1).normalColor;
			background.sprite = ShopBackground;
		} else if (item_relic != null) {
			Relics.Relic relic = (Relics.Relic)typeof(Scenarios.Shop.PurchasableRelic).GetField("_relic", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(item_relic);

			Relics.RelicRarity rarity = GetRelicRarity(relic);
			background.color = GetRelicColor(rarity).normalColor;
			background.sprite = ShopBackground;
		}
	}
}
