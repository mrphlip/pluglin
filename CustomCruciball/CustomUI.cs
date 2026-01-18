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
	private static GameObject cruxButtonPanel, cruxButtonLabel, cruxDisplay;
	private static GameObject[] cruxDisplayImg;
	
	// Unity objects in Peglin vanilla
	private static PeglinUI.LoadoutManager.LoadoutManager loadoutManager;
	private static GameObject buttonRow, seedButtonPanel, seedText, seedButton, seedButtonLabel;


	[HarmonyPatch(typeof(PeglinUI.LoadoutManager.LoadoutManager), "MoveToLoadoutSelection")]
	[HarmonyPostfix]
	private static void InitialiseUI() {
		InitialiseMainButton();
		InitialisePage();
	}

	/*
		I'm sure that all this UI wrangling would be a million times easier if I actually
		had the Unity editor available and built it all in the visual editor.
		But no, I'm doing it all manually in code instead...
		w/e if it works it works
	*/

	private static void InitialiseMainButton() {
		if (cruxButtonPanel != null)
			return;

		loadoutManager = Utils.GetResource<PeglinUI.LoadoutManager.LoadoutManager>();
		if (loadoutManager == null) {
			Plugin.Logger.LogError("Could not find LoadoutManager!");
			return;
		}

		// Find the "custom seed" UI elements
		seedButtonPanel = loadoutManager.seedDisplay.transform.parent.gameObject;
		buttonRow = seedButtonPanel.transform.parent.gameObject;
		seedText = seedButtonPanel.transform.Find("seedText").gameObject;
		seedButton = seedButtonPanel.transform.Find("SeedEditButton").GetChild(0).gameObject;
		seedButtonLabel = seedButton.GetComponentInChildren<PeglinUI.ImageReactToButtonPressDepress>().gameObject;

		// Shrink the "custom seed" UI to make room for the new elements
		((RectTransform)seedButtonPanel.transform).sizeDelta = new Vector2(180f, 34.2343f);

		// create our new UI
		cruxButtonPanel = MakePanel(buttonRow, "CruxButtonPanel", 180f, 32.2343f);

		CopyImage(seedButtonPanel, cruxButtonPanel);
		cruxButtonLabel = MakeText(cruxButtonPanel, "CruxButtonLabel", "Cruciball", 51.27f, 29.5f);

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

	private static GameObject MakeText(GameObject parent, string name, string label, float width, float height) {
		var obj = MakeObject(parent, name, width, height);
		var text = obj.AddComponent<TextMeshProUGUI>();
		text.text = label;
		text.alignment = TextAlignmentOptions.MidlineLeft;
		text.color = new Color(0f, 0f, 0f, 1f);
		text.enableAutoSizing = true;
		text.enableKerning = true;
		text.enableWordWrapping = false;
		text.fontSize = 20;
		text.fontSizeMax = 20;
		text.fontSizeMin = 14;
		return obj;
	}

	private static GameObject MakeButton(GameObject parent, string name, float width, float height, int style) {
		var obj = MakeObject(parent, name, width, height);
		obj.AddComponent<UI.ButtonHandleHover>();
		var btn = MakeObject(obj, name + "_btn", width, height);
		var button = btn.AddComponent<Button>();
		var label = MakeObject(btn, name + "_lbl", width, height);

		switch(style) {
			case 0:
				obj.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
				CopyImage(seedButton, btn);
				var srcButton = seedButton.GetComponent<Button>();
				button.transition = srcButton.transition;
				button.spriteState = srcButton.spriteState;
				CopyImage(seedButtonLabel, label);
				break;
		}

		var btnHandler = btn.AddComponent<PeglinUI.UIUtils.ButtonDownUpHandler>();
		var lblHandler = label.AddComponent<PeglinUI.ImageReactToButtonPressDepress>();
		if (btnHandler.onPointerDown == null) btnHandler.onPointerDown = new UnityEvent();
		if (btnHandler.onPointerUp == null) btnHandler.onPointerUp = new UnityEvent();
		btnHandler.onPointerDown.AddListener(lblHandler.Press);
		btnHandler.onPointerUp.AddListener(lblHandler.Depress);

		return obj;
	}

	private static void CopyImage(GameObject src, GameObject dst) {
		var srcTransform = src.GetComponent<RectTransform>();
		var dstTransform = dst.GetComponent<RectTransform>();
		dstTransform.sizeDelta = srcTransform.sizeDelta;
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
		cruxButtonLabel.SetActive(!State.inst.isCustom);
		cruxDisplay.SetActive(State.inst.isCustom);
		for (int i = 0; i < 20; i++) {
			cruxDisplayImg[i].GetComponent<Image>().sprite = State.inst.levels[i] ? Assets.Checked : Assets.Unchecked;
		}
	}

	private static void MoveToCruxEditor() {
		Plugin.Logger.LogInfo("Click the button!");
	}
}
