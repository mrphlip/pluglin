using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace CustomCruciball;

[HarmonyPatch]
public class CustomUI {
	// Unity objects I'm creating
	private static GameObject cruxButtonPanel, cruxButtonLabel, cruxDisplay, cruxInputPopup;
	private static GameObject[] cruxDisplayImg, cruxCheckbox;
	
	// Unity objects in Peglin vanilla
	private static PeglinUI.LoadoutManager.LoadoutManager loadoutManager;
	private static GameObject buttonRow, seedButtonPanel, seedText, seedButton, seedButtonLabel;
	private static GameObject loadoutBasePanel, seedInputPopup, popupContainer, seedApplyButton;


	[HarmonyPatch(typeof(PeglinUI.LoadoutManager.LoadoutManager), "MoveToLoadoutSelection")]
	[HarmonyPostfix]
	private static void InitialiseUI() {
		if (loadoutManager == null) {
			loadoutManager = Utils.GetResource<PeglinUI.LoadoutManager.LoadoutManager>();
			if (loadoutManager == null) {
				Plugin.Logger.LogError("Could not find LoadoutManager!");
				return;
			}
		}

		InitialiseButton();
		InitialisePage();
	}

	/*
		I'm sure that all this UI wrangling would be a million times easier if I actually
		had the Unity editor available and built it all in the visual editor.
		But no, I'm doing it all manually in code instead...
		w/e if it works it works
	*/

	private static void InitialiseButton() {
		if (cruxButtonPanel != null)
			return;

		// Find the "custom seed" UI elements
		seedButtonPanel = loadoutManager.seedDisplay.transform.parent.gameObject;
		buttonRow = seedButtonPanel.transform.parent.gameObject;
		seedText = seedButtonPanel.GetComponentInChildren<TextMeshProUGUI>().gameObject;
		seedButton = seedButtonPanel.GetComponentInChildren<Button>().gameObject;
		seedButtonLabel = seedButton.GetComponentInChildren<PeglinUI.ImageReactToButtonPressDepress>().gameObject;

		// Shrink the "custom seed" UI to make room for the new elements
		((RectTransform)seedButtonPanel.transform).sizeDelta = new Vector2(180f, 34.2343f);

		// create our new UI
		cruxButtonPanel = MakePanel(buttonRow, "CruxButtonPanel", 180f, 32.2343f);

		CopyImage(seedButtonPanel, cruxButtonPanel);
		cruxButtonLabel = MakeText(cruxButtonPanel, "CruxButtonLabel", "Cruciball", 80f, 29.5f, TextAlignmentOptions.MidlineLeft, 14, 20);

		cruxDisplay = MakeObject(cruxButtonPanel, "CruxDisplay", 120f, 24f);
		cruxDisplayImg = new GameObject[20];
		for (int i = 0; i < 20; i++) {
			var img = MakeImage(cruxDisplay, $"CruxDisplay{i}", Assets.Unchecked);
			img.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
			int x = i % 10, y = i / 10;
			img.transform.localPosition = new Vector3(12f * x - 54f, -(12f * y - 6f), 1f);
			cruxDisplayImg[i] = img;
		}
		UpdateCruxDisplay();

		var cruxButton = MakeButton(cruxButtonPanel, "CruxButton", 40f, 40f, 0);
		var cruxButtonBtn = cruxButton.GetComponentInChildren<Button>();
		if (cruxButtonBtn.onClick == null) cruxButtonBtn.onClick = new Button.ButtonClickedEvent();
		cruxButtonBtn.onClick.AddListener(CustomUI.MoveToCruxEditor);
	}

	private static void InitialisePage() {
		if (cruxInputPopup != null)
			return;

		// find the "custom seed" popup UI controls
		loadoutBasePanel = loadoutManager.loadoutBasePanel;
		seedInputPopup = loadoutManager.seedInputPopup.gameObject;
		popupContainer = seedInputPopup.transform.parent.gameObject;
		var seedBg1 = seedInputPopup.transform.Find("FadeBackground").gameObject;
		var seedBg2 = seedBg1.transform.Find("SeedInputBackground").gameObject;
		seedApplyButton = seedBg2.transform.Find("ButtonPanel").GetChild(0).gameObject;

		// create our own UI
		cruxInputPopup = MakeObject(popupContainer, "CruxInputPopup", 543f, 200f);
		cruxInputPopup.SetActive(false);
		CopyImage(seedBg1, cruxInputPopup);
		var bg2 = MakeObject(cruxInputPopup, "CruxInputPopupBg", 300f, 175f);
		CopyImage(seedBg2, bg2);

		var buttonRow = MakePanel(bg2, "CruxButtonRow", 300f, 15f, false, -15f);
		buttonRow.transform.localPosition = new Vector3(0f, -84f, 0f);
		var resetButton = MakeButton(buttonRow, "CruxReset", 100f, 30f, 2, "Reset");
		var applyButton = MakeButton(buttonRow, "CruxApply", 100f, 30f, 1, "Apply");

		cruxCheckbox = new GameObject[20];
		for (int i = 0; i < Constants.NUM_LEVELS; i++) {
			MakeCheckbox(cruxInputPopup, i);
		}
	}

	private static void DbgObject(string label, GameObject obj) {
		Plugin.Logger.LogInfo($"{label} {obj}");
		foreach (var i in obj.GetComponents<Component>()) {
			Plugin.Logger.LogInfo($" * {i.GetType()}");
		}
	}

	private static GameObject MakeObject(GameObject parent, string name, float width, float height) {
		var obj = new GameObject(name, typeof(RectTransform));
		var transform = obj.GetComponent<RectTransform>();
		transform.SetParent(parent.transform, false);
		transform.sizeDelta = new Vector2(width, height);
		transform.localScale = new Vector3(1f, 1f, 1f);
		return obj;
	}

	private static GameObject MakePanel(GameObject parent, string name, float width, float height, bool vertical=false, float spacing=3f) {
		var obj = MakeObject(parent, name, width, height);
		HorizontalOrVerticalLayoutGroup layout;
		if (vertical)
			layout = obj.AddComponent<VerticalLayoutGroup>();
		else
			layout = obj.AddComponent<HorizontalLayoutGroup>();
		layout.childControlWidth = false;
		layout.childControlHeight = false;
		layout.childForceExpandWidth = false;
		layout.childForceExpandHeight = false;
		layout.spacing = spacing;
		layout.childAlignment = TextAnchor.MiddleCenter;
		return obj;
	}

	private static GameObject MakeText(GameObject parent, string name, string label, float width, float height, TextAlignmentOptions align, float fontSizeMin, float fontSizeMax) {
		var obj = MakeObject(parent, name, width, height);
		var text = obj.AddComponent<TextMeshProUGUI>();
		text.text = label;
		text.alignment = align;
		text.color = new Color(0f, 0f, 0f, 1f);
		text.enableAutoSizing = true;
		text.enableKerning = true;
		text.enableWordWrapping = false;
		text.fontSize = fontSizeMax;
		text.fontSizeMax = fontSizeMax;
		text.fontSizeMin = fontSizeMin;
		return obj;
	}

	private static GameObject MakeButton(GameObject parent, string name, float width, float height, int style, string text="") {
		var obj = MakeObject(parent, name, width, height);
		obj.AddComponent<UI.ButtonHandleHover>();
		var button = obj.AddComponent<Button>();
		var btnHandler = obj.AddComponent<PeglinUI.UIUtils.ButtonDownUpHandler>();

		switch(style) {
			case 0: {
				obj.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
				CopyImage(seedButton, obj);
				var srcButton = seedButton.GetComponent<Button>();
				button.transition = srcButton.transition;
				button.spriteState = srcButton.spriteState;

				var label = MakeObject(obj, name + "_lbl", width, height);
				CopyImage(seedButtonLabel, label);
				var lblHandler = label.AddComponent<PeglinUI.ImageReactToButtonPressDepress>();
				if (btnHandler.onPointerDown == null) btnHandler.onPointerDown = new UnityEvent();
				if (btnHandler.onPointerUp == null) btnHandler.onPointerUp = new UnityEvent();
				btnHandler.onPointerDown.AddListener(lblHandler.Press);
				btnHandler.onPointerUp.AddListener(lblHandler.Depress);

				break;
			}
			case 1:
			case 2: {
				obj.transform.localScale = new Vector3(0.54f, 0.54f, 1f);
				CopyImage(seedApplyButton, obj);
				if (style == 1)
					obj.GetComponent<Image>().color = new Color(0.2078f, 0.5686f, 0.3373f, 1f);
				else
					obj.GetComponent<Image>().color = new Color(0.783f, 0.3049f, 0.2992f, 1f);
				var srcButton = seedApplyButton.GetComponent<Button>();
				button.transition = srcButton.transition;
				button.spriteState = srcButton.spriteState;

				var label = MakeText(obj, name + "_lbl", text, width, height, TextAlignmentOptions.Midline, 20, 20);
				var lblHandler = label.AddComponent<PeglinUI.TextReactToButtonPressDepress>();
				if (btnHandler.onPointerDown == null) btnHandler.onPointerDown = new UnityEvent();
				if (btnHandler.onPointerUp == null) btnHandler.onPointerUp = new UnityEvent();
				btnHandler.onPointerDown.AddListener(lblHandler.Press);
				btnHandler.onPointerUp.AddListener(lblHandler.Depress);

				break;
			}
		}

		return obj;
	}

	private static void CopyImage(GameObject src, GameObject dst) {
		var srcTransform = src.GetComponent<RectTransform>();
		var dstTransform = dst.GetComponent<RectTransform>();
		dstTransform.sizeDelta = srcTransform.sizeDelta;
		dstTransform.localScale = srcTransform.localScale;
		var srcImg = src.GetComponent<Image>();
		var dstImg = dst.AddComponent<Image>();
		dstImg.sprite = srcImg.sprite;
		dstImg.pixelsPerUnitMultiplier = srcImg.pixelsPerUnitMultiplier;
		dstImg.type = srcImg.type;
		dstImg.color = srcImg.color;
	}

	private static GameObject MakeImage(GameObject parent, string name, Sprite sprite) {
		var obj = MakeObject(parent, name, sprite.rect.width, sprite.rect.height);
		var img = obj.AddComponent<Image>();
		img.sprite = sprite;
		return obj;
	}

	private static GameObject MakeCheckbox(GameObject parent, int ix) {
		int x = ix / Constants.NUM_ROWS, y = ix % Constants.NUM_ROWS;
		var obj = MakeObject(parent, $"CruxCheckboxPanel{ix+1}", Constants.CELL_WIDTH, Constants.CELL_HEIGHT);
		obj.transform.localPosition = new Vector3(Constants.POPUP_XMIN + Constants.CELL_WIDTH * (x + 0.5f), Constants.POPUP_YMAX - Constants.CELL_HEIGHT * (y + 0.5f), 0);
		cruxCheckbox[ix] = MakeImage(obj, $"CruxCheckbox{ix+1}", Assets.Unchecked);
		cruxCheckbox[ix].transform.localPosition = new Vector3((Constants.CELL_HEIGHT - Constants.CELL_WIDTH)/2f, 0f, 0f);
		var label = MakeText(obj, $"CruxCheckboxLabel{ix+1}", $"[{ix+1}] {Constants.LABELS[ix]}", Constants.CELL_WIDTH - Constants.CELL_HEIGHT, Constants.CELL_HEIGHT, TextAlignmentOptions.MidlineLeft, 10, 10);
		label.transform.localPosition = new Vector3(Constants.CELL_HEIGHT/2f, 0f, 0f);
		return obj;
	}

	// Hide the "Seed" text when a custom seed is entered - to make the field smaller
	[HarmonyPatch(typeof(SeedInputPopup), "ReturnToCustomStartMenu")]
	[HarmonyPostfix]
	private static void CloseCustomSeed(SeedInputPopup __instance) {
		seedText.SetActive(!__instance.hasCustomSeed);
	}
	[HarmonyPatch(typeof(PeglinUI.LoadoutManager.LoadoutManager), "DefaultLoadout")]
	[HarmonyPostfix]
	private static void ResetCustomSeed() {
		seedText.SetActive(true);
	}

	private static void UpdateCruxDisplay() {
		// This method is called from external hooks so double-check everything is still OK
		if (cruxButtonLabel != null) cruxButtonLabel.SetActive(!State.inst.isCustom);
		if (cruxDisplay != null) cruxDisplay.SetActive(State.inst.isCustom);
		for (int i = 0; i < 20; i++) {
			if (cruxDisplayImg[i] != null)
				cruxDisplayImg[i].GetComponent<Image>().sprite = State.inst.levels[i] ? Assets.Checked : Assets.Unchecked;
		}
	}

	private static void MoveToCruxEditor() {
		loadoutBasePanel.SetActive(false);
		cruxInputPopup.SetActive(true);
	}
}
