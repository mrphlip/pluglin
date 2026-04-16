// Start(DataExtract.Plugin.inst.Go());
// Start(DataExtract.Plugin.inst.LoadMap(1));

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace DataExtract;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Peglin.exe")]
[HarmonyPatch]
public class Plugin : BaseUnityPlugin {
	public readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
	internal static new ManualLogSource Logger;
	public static Plugin inst;

	private void Awake() {
		Plugin.inst = this;
		harmony.PatchAll();
		Logger = base.Logger;
		Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
	}

	private static bool firstTime = true;
	[HarmonyPatch(typeof(PeglinUI.MainMenu.MainMenuRandomOrbDrop), "Awake")] // here'll do
	[HarmonyPostfix]
	private static void OnLoadMainMenu() {
		if (firstTime) {
			firstTime = false;
			UniverseLib.RuntimeHelper.StartCoroutine(inst.Go());
		}
	}

	private String targetDir;

	private Loading.PeglinSceneLoader.Scene[] scenes = new Loading.PeglinSceneLoader.Scene[] {
		Loading.PeglinSceneLoader.Scene.MAIN_MENU,
		Loading.PeglinSceneLoader.Scene.FOREST_MAP,
		Loading.PeglinSceneLoader.Scene.CASTLE_MAP,
		Loading.PeglinSceneLoader.Scene.MINES_MAP,
		Loading.PeglinSceneLoader.Scene.CORE_MAP,
	};
	private const int ACT_COUNT = 4;

	public IEnumerator Go() {
		yield return null;
		using (var patcher = new Patcher()) {
			yield return UniverseLib.RuntimeHelper.StartCoroutine(LoadScene(1));

			Init();

			ExtractEnums();
			ExtractClasses();
			ExtractRelicData();
			ExtractChallengeData();
			ExtractOrbData();
			ExtractSeedData();
			yield return UniverseLib.RuntimeHelper.StartCoroutine(ExtractMapData());
		}

		ExtractOrbImages();
		ExtractRelicImages();
		ExtractChallengeImages();

		yield return UniverseLib.RuntimeHelper.StartCoroutine(LoadScene(0));
	}

	public IEnumerator LoadMap(int act) {
		using (var patcher = new Patcher()) {
			yield return UniverseLib.RuntimeHelper.StartCoroutine(LoadScene(act));
		}
	}

	public void Init() {
		String ver = UnityEngine.Application.version;
		targetDir = $"/home/phlip/pluglin/extract/v{ver.Replace('.', '_')}";
		try {
			Directory.Delete(targetDir, true);
		} catch (DirectoryNotFoundException) {}
		Directory.CreateDirectory(targetDir);

		using (StreamWriter fp = new StreamWriter($"{targetDir}/__init__.py")) {
			fp.WriteLine("from .enums import *");
			fp.WriteLine("from .classes import *");
			fp.WriteLine("from .relics import *");
			fp.WriteLine("from .challenges import *");
			fp.WriteLine("from .orbs import *");
			fp.WriteLine("from .seeds import *");
			fp.WriteLine("from .maps import *");
		}
	}

	public static string b(bool x) {
		return x ? "True" : "False";
	}

	public static string enm(object o) {
		if (o.ToString() == "None")
			return "_None";
		else
			return o.ToString();
	}

	public void ExtractEnums() {
		using (StreamWriter fp = new StreamWriter($"{targetDir}/enums.py")) {
			fp.WriteLine("from enum import Enum");
			fp.WriteLine("__all__ = [\"RelicRarity\", \"OrbRarity\", \"Class\", \"RoomType\", \"ScenarioPreReq\"]");
			fp.WriteLine("");

			ExtractEnum(fp, typeof(Relics.RelicRarity), true);
			fp.WriteLine("  _SHOP_ONLY = -1");
			fp.WriteLine("");
			ExtractEnum(fp, typeof(PachinkoBall.OrbRarity));
			ExtractEnum(fp, typeof(Peglin.ClassSystem.Class));
			ExtractEnum(fp, typeof(Worldmap.RoomType));
			ExtractEnum(fp, typeof(Data.Scenarios.ScenarioPreReq));

			fp.WriteLine("class OrbSize(Enum):");
			fp.WriteLine("  DEFAULT = 0");
			fp.WriteLine("  LARGE = 1");
			fp.WriteLine("  SMALL = 2");
			fp.WriteLine("");
		}
	}
	public void ExtractEnum(StreamWriter fp, Type e, bool extra=false) {
		fp.WriteLine($"class {e.Name}(Enum):");
		foreach (var i in Enum.GetValues(e)) {
			fp.WriteLine($"  {enm(i)} = {(int)i}");
		}
		if (!extra)
			fp.WriteLine("");
	}

	public IEnumerator LoadScene(int act) {
		if (Map.MapController.instance != null) {
			if (act > 0 && Map.MapController.instance.Act == act)
				yield break;

			UnityEngine.Object.Destroy(Map.MapController.instance.gameObject);
			Map.MapController.instance = null;
		}
		Loading.PeglinSceneLoader.Instance.LoadScene(scenes[act]);
		if (act > 0)
			while (Map.MapController.instance == null)
				yield return null;
	}

	public void ExtractClasses() {
		using (StreamWriter fp = new StreamWriter($"{targetDir}/classes.py")) {
			var relicManager = Map.MapController.instance._relicManager;
			var loadouts = relicManager._classLoadouts;
			var deckManager = Map.MapController.instance._deckManager;

			fp.WriteLine("from collections import namedtuple");
			fp.WriteLine("from .enums import Class, RelicRarity, OrbRarity");
			fp.WriteLine("__all__ = [\"class_data\"]");
			fp.WriteLine("");

			fp.WriteLine("ClassData = namedtuple(\"ClassData\", [\"orbs\", \"relics\", \"orb_overrides\", \"relic_overrides\", \"shop_only_chance\"])\n");
			fp.WriteLine("class_data = {\n");
			foreach	(Peglin.ClassSystem.Class cls in Enum.GetValues(typeof(Peglin.ClassSystem.Class))) {
				Peglin.ClassSystem.ClassLoadoutData loadout = null;
				foreach (var i in loadouts) {
					if (i.Class == cls) {
						loadout = i.Loadout;
					}
				}

				fp.WriteLine($"  Class.{cls}: ClassData(\n");
				fp.Write("    [");
				foreach (var orb in loadout.StartingOrbs) {
					var atk = orb.GetComponent<Battle.Attacks.Attack>();
					fp.Write($"(\"{atk.locNameString}\", {atk.Level}), ");
				}
				fp.WriteLine("],");
				fp.Write("    [");
				foreach (var relic in loadout.StartingRelics) {
					fp.Write($"\"{relic.name}\", ");
				}
				fp.WriteLine("],");
				fp.WriteLine("    {");
				foreach (var orb in deckManager.OrbPool.AvailableOrbs) {
					var atk = orb.GetComponent<Battle.Attacks.Attack>();
					foreach (var i in orb.GetComponents<ClassSystem.ClassOrbRarityOverride>()) {
						if (i._class == cls) {
							fp.WriteLine($"      \"{atk.locNameString}\": OrbRarity.{i.orbRarityOverride},");
						}
					}
				}
				fp.WriteLine("    },");
				fp.WriteLine("    {");
				if (loadout.CommonRelicOverrides != null) {
					foreach (var relic in loadout.CommonRelicOverrides.relics) {
						fp.WriteLine($"      \"{relic.name}\": RelicRarity.COMMON,");
					}
				}
				if (loadout.RareRelicOverrides != null) {
					foreach (var relic in loadout.RareRelicOverrides.relics) {
						fp.WriteLine($"      \"{relic.name}\": RelicRarity.RARE,");
					}
				}
				if (loadout.BossRelicOverrides != null) {
					foreach (var relic in loadout.BossRelicOverrides.relics) {
						fp.WriteLine($"      \"{relic.name}\": RelicRarity.BOSS,");
					}
				}
				if (loadout.RareScenarioRelicOverrides != null) {
					foreach (var relic in loadout.RareScenarioRelicOverrides.relics) {
						fp.WriteLine($"      \"{relic.name}\": RelicRarity.NONE,");
					}
				}
				if (loadout.UnavailableRelicOverrides != null) {
					foreach (var relic in loadout.UnavailableRelicOverrides.relics) {
						fp.WriteLine($"      \"{relic.name}\": RelicRarity.UNAVAILABLE,");
					}
				}
				if (loadout.ShopRelicOverrides != null) {
					foreach (var relic in loadout.ShopRelicOverrides.relics) {
						fp.WriteLine($"      \"{relic.name}\": RelicRarity._SHOP_ONLY,");
					}
				}
				fp.WriteLine("    },");
				fp.WriteLine($"    {loadout.chanceForGuaranteedShopRelicInShops},");
				fp.WriteLine("  ),");
			}
			fp.WriteLine("}");
		}
	}


	public void ExtractRelicData() {
		using (StreamWriter fp = new StreamWriter($"{targetDir}/relics.py")) {
			fp.WriteLine("from collections import namedtuple");
			fp.WriteLine("from .enums import RelicRarity");
			fp.WriteLine("__all__ = [\"relics\"]");
			fp.WriteLine("");

			var relicManager = Map.MapController.instance._relicManager;

			fp.WriteLine("Relic = namedtuple(\"Relic\", [\"key\", \"name\", \"effect\", \"rarity\"])");
			fp.WriteLine("relics = {");
			foreach	(var relic in relicManager.globalRelics.relics) {
				ExtractRelic(fp, relic);
			}
			ExtractRelic(fp, relicManager.consolationPrize);
			fp.WriteLine("}\n\n");
		}
	}

	public void ExtractRelic(StreamWriter fp, Relics.Relic relic) {
		fp.Write($"  \"{relic.name}\": Relic(");
		fp.Write($"\"{relic.name}\", \"{relic.englishDisplayName}\", \"{relic.effect}\", ");
		fp.WriteLine($"RelicRarity.{relic.globalRarity}),");
	}

	public void ExtractChallengeData() {
		using (StreamWriter fp = new StreamWriter($"{targetDir}/challenges.py")) {
			fp.WriteLine("from collections import namedtuple");
			fp.WriteLine("__all__ = [\"challenges\"]");
			fp.WriteLine("");

			var challengeSet = Map.MapController.instance._globalChallenges;

			fp.WriteLine("Challenge = namedtuple(\"Challenge\", [\"key\", \"name\", \"effect\"])");
			fp.WriteLine("challenges = {");
			foreach	(var challenge in challengeSet.challenges) {
				string name = I2.Loc.LocalizationManager.GetTranslation(challenge.nameKey);
				fp.WriteLine($"  \"{challenge.name}\": Challenge(\"{challenge.name}\", \"{name}\", \"{challenge.effect}\"),");
			}
			fp.WriteLine("}\n\n");
		}
	}

	public void ExtractOrbData() {
		using (StreamWriter fp = new StreamWriter($"{targetDir}/orbs.py")) {
			fp.WriteLine("from collections import namedtuple");
			fp.WriteLine("from .enums import OrbRarity, OrbSize");
			fp.WriteLine("__all__ = [\"orbs\"]");
			fp.WriteLine("");

			var deckManager = Map.MapController.instance._deckManager;

			fp.WriteLine("Orb = namedtuple(\"Orb\", [\"key\", \"name\", \"rarity\", \"size\"])");
			fp.WriteLine("orbs = {\n");
			foreach (var orb in deckManager.OrbPool.AvailableOrbs) {
				var atk = orb.GetComponent<Battle.Attacks.Attack>();
				var ball = orb.GetComponent<PachinkoBall>();
				var size = ball.IsOrbLargerThanDefault() ? "LARGE" : ball.IsOrbSmallerThanDefault() ? "SMALL" : "DEFAULT";
				fp.WriteLine($"  \"{atk.locNameString}\": Orb(\"{atk.locNameString}\", \"{atk.Name}\", OrbRarity.{ball.orbRarity}, OrbSize.{size}),");
			}
			fp.WriteLine("}\n\n");
		}
	}

	public void ExtractSeedData() {
		using (StreamWriter fp = new StreamWriter($"{targetDir}/seeds.py")) {
			fp.WriteLine("from collections import namedtuple");
			fp.WriteLine("from .enums import RoomType");
			fp.WriteLine("__all__ = [\"secret_seeds\"]");
			fp.WriteLine("");

			var seedManager = Map.MapController.instance._secretSeedManager;

			fp.WriteLine("SecretSeed = namedtuple(\"SecretSeed\", [\"name\", \"seed\", \"relics\", \"pools\", \"map\"])");
			fp.WriteLine("SecretSeedPools = namedtuple(\"SecretSeedPools\", [\"orbs\", \"common\", \"uncommon\", \"rare\", \"other\", \"relics\", \"relic_fallback\"])");
			fp.WriteLine("SecretSeedMap = namedtuple(\"SecretSeedMap\", [\"unknown\", \"scenario\", \"miniboss\", \"battle\", \"treasure\", \"store\", \"minigame\", \"midline\", \"any_miniboss\"])");
			fp.WriteLine("secret_seeds = {");
			foreach (var seed in seedManager.secretSeeds) {
				fp.Write($"  \"{seed.seed}\": SecretSeed(\"{seed.name}\", \"{seed.seed}\", [");
				foreach (var relic in seed.unlockAndAddRelics) {
					fp.Write($"\"{relic.name}\", ");
				}
				fp.WriteLine("],");
				var seed_as_pool = seed as Seeding.CustomPoolsSecretSeed;
				if (seed_as_pool) {
					fp.WriteLine("    SecretSeedPools(");
					fp.WriteLine("      [");
					foreach (var orb in seed_as_pool.customOrbPool) {
						var atk = orb.GetComponent<Battle.Attacks.Attack>();
						fp.WriteLine($"        \"{atk.locNameString}\",");
					}
					fp.WriteLine($"      ], {seed_as_pool.commonOrbChance}, {seed_as_pool.uncommonOrbChance}, {seed_as_pool.rareOrbChance}, {seed_as_pool.otherOrbOfferChance},");
					fp.WriteLine("      [");
					foreach (var relic in seed_as_pool.customRelicPool) {
						fp.WriteLine($"        \"{relic.name}\",");
					}
					fp.WriteLine($"      ], {b(seed_as_pool.relicPoolFallback)},");
					fp.WriteLine("    ),");
				} else {
					fp.WriteLine("    None,");
				}
				var seed_as_map = seed as Seeding.ChangeMapSecretSeed;
				if (seed_as_map) {
					fp.WriteLine($"    SecretSeedMap(");
					fp.WriteLine($"      {seed_as_map.unknownChance}, {seed_as_map.scenrioChance}, {seed_as_map.minibossChance}, {seed_as_map.battleChance}, {seed_as_map.treasureChance}, {seed_as_map.storeChance}, {seed_as_map.pegMinigameChance},");
					fp.WriteLine($"      RoomType.{seed_as_map.setFixedChestMidLineTo}, {b(seed_as_map.overrideCantBeMiniboss)},");
					fp.WriteLine("    ),");
				} else {
					fp.WriteLine("    None,");
				}
				fp.WriteLine("  ),");
			}
			fp.WriteLine("}");
		}
	}

	public IEnumerator ExtractMapData() {
		using (StreamWriter fp = new StreamWriter($"{targetDir}/maps.py")) {
			fp.WriteLine("from collections import namedtuple");
			fp.WriteLine("from .enums import RelicRarity, RoomType, ScenarioPreReq");
			fp.WriteLine("__all__ = [\"map\"]");
			fp.WriteLine("");

			fp.WriteLine("Map = namedtuple(\"Map\", [\"name\", \"easy_battles\", \"battles\", \"minibosses\", \"mimic\", \"scenarios\", \"minigames\", \"nodes\", \"treasure_rare_chance\", \"min_miniboss\", \"min_shop\", \"min_treasure\"])");
			fp.WriteLine("MapNode = namedtuple(\"MapNode\", [\"key\", \"children\", \"type\", \"fixed\", \"final\", \"miniboss\", \"battles\"])");
			fp.WriteLine("MapBattle = namedtuple(\"MapBattle\", [\"name\", \"dont_remove\", \"is_mimic\", \"relic_rarity\"])");
			fp.WriteLine("MapScenario = namedtuple(\"MapScenario\", [\"name\", \"id\", \"prereqs\"])");
			fp.WriteLine("MapMinigame = namedtuple(\"MapMinigame\", [\"name\", \"is_orbs\", \"is_relics\", \"orb_count\", \"relic_count\", \"relic_rarity\"])");
			fp.WriteLine("maps = [");
			for (var i = 1; i <= ACT_COUNT; i++) {
				yield return UniverseLib.RuntimeHelper.StartCoroutine(LoadMap(i));
				var map = Map.MapController.instance;
				fp.WriteLine("  Map(");
				fp.WriteLine($"    \"{map._mapName}\",");
				fp.WriteLine("    [");
				foreach (var battle in map._potentialEasyBattles)
					ExtractBattle(fp, battle, "  ", false);
				fp.WriteLine("    ],");
				fp.WriteLine("    [");
				foreach (var battle in map._potentialRandomBattles)
					ExtractBattle(fp, battle, "  ", false);
				fp.WriteLine("    ],");
				fp.WriteLine("    [");
				foreach (var battle in map._potentialEliteBattles)
					ExtractBattle(fp, battle, "  ", false);
				fp.WriteLine("    ],");
				ExtractBattle(fp, map._mimicBatteData, "", true);
				fp.WriteLine("    [");
				foreach (var scenario in map._potentialRandomScenarios)
					ExtractScenario(fp, scenario, "");
				fp.WriteLine("    ],");
				fp.WriteLine("    [");
				foreach (var minigame in map._potentialPegMinigameScenarios)
					ExtractMinigame(fp, minigame);
				fp.WriteLine("    ],");
				fp.WriteLine("    {");
				foreach (var node in map._nodes)
					ExtractNode(fp, node);
				fp.WriteLine("    },");
				fp.WriteLine($"    {map._treasureMapData.rareChance},");
				fp.WriteLine($"    {map.minEliteNodes},");
				fp.WriteLine($"    {map.minTreasureNodes},");
				fp.WriteLine($"    {map.minShopNodes},");

				fp.WriteLine("  ),");
			}
			fp.WriteLine("]");
		}
	}

	public void ExtractBattle(StreamWriter fp, Data.MapDataBattle battle, string extraindent, bool is_mimic) {
		// Do we want to extract any other info, for display? Like a list of enemies, or the pegboard layout?
		fp.WriteLine($"{extraindent}    MapBattle(\"{battle.name}\", {b(battle.ignoreRemove)}, {b(is_mimic)}, RelicRarity.{battle.grantedRelicRarity}),");
	}
	public void ExtractScenario(StreamWriter fp, Data.Scenarios.MapDataScenario scenario, string extraindent) {
		fp.Write($"{extraindent}      MapScenario(\"{scenario.name}\", \"{scenario.scenarioName}\", ");
		if (scenario.scenarioPreReqs == null || scenario.scenarioPreReqs.Length == 0) {
			fp.Write("[]");
		} else {
			fp.Write("[\n");
			foreach (var i in scenario.scenarioPreReqs)
				fp.WriteLine($"{extraindent}        ScenarioPreReq.{enm(i)},");
			fp.Write($"{extraindent}      ]");
		}
		fp.WriteLine("),");
	}
	public void ExtractMinigame(StreamWriter fp, MapDataPegMinigame minigame) {
		var minigame_orb = minigame as Peglin.PegMinigame.MapDataPegMinigameOrbs;
		var minigame_relic = minigame as Peglin.PegMinigame.MapDataPegMinigameRelics;
		fp.Write($"      MapMinigame(\"{minigame.name}\", {b(minigame_orb)}, {b(minigame_relic)}, ");
		if (minigame_orb)
			fp.Write($"{minigame_orb.numberOfRewards}, ");
		else
			fp.Write($"None, ");
		if (minigame_relic)
			fp.Write($"{minigame_relic.numberOfRewards}, RelicRarity.{minigame_relic.rarity}");
		else
			fp.Write($"None, None");
		fp.WriteLine("),");
	}

	public void ExtractNode(StreamWriter fp, Worldmap.MapNode node) {
		fp.Write($"      \"{node.name}\": MapNode(\"{node.name}\", [");
		foreach (var i in node.ChildNodes)
			fp.Write($"\"{i.name}\", ");
		fp.Write($"], RoomType.{node.RoomType}, {b(node.isFixedNode)}, {b(node.isFinalNode)}, {b(node.canBeMiniboss)}, ");
		if (node.potentialMapData == null) {
			fp.Write("None");
		} else if (node.potentialMapData.Length == 0) {
			fp.Write("[]");
		} else {
			fp.WriteLine("[");
			foreach (var data in node.potentialMapData) {
				if (data as Data.MapDataBattle)
					ExtractBattle(fp, data as Data.MapDataBattle, "    ", false);
				else if (data as Data.Scenarios.MapDataScenario)
					ExtractScenario(fp, data as Data.Scenarios.MapDataScenario, "  ");
				else
					fp.WriteLine("        ...,");
			}
			fp.Write("      ]");
		}
		fp.WriteLine("),");
		// Maybe: secretTunnelConnection?
	}

	Texture2D CropTexture(Texture2D source, Rect rect, int scale=1) {
		// from: https://discussions.unity.com/t/easy-way-to-make-texture-isreadable-true-by-script/848617/2
		RenderTexture renderTex = RenderTexture.GetTemporary(
			source.width * scale,
			source.height * scale,
			0,
			RenderTextureFormat.Default,
			RenderTextureReadWrite.Linear);

		RenderTexture previous = RenderTexture.active;
		//Graphics.Blit(source, renderTex, new Vector2(1.0f/scale, scale), rect.position);
		//Graphics.Blit(source, renderTex, Vector2.one, rect.position);
		Graphics.Blit(source, renderTex);
		RenderTexture.active = renderTex;
		int width = Mathf.RoundToInt(rect.width) * scale;
		int height = Mathf.RoundToInt(rect.height) * scale;
		float x = rect.x * scale;
		// Sprite.textureRect measures y coordinates from the bottom up
		// but ReadPixels measures them from the top down
		// whyyyy
		// OR! MAYBE SOMETIMES IT DOESN'T I DON'T UNDERSTAND YOU UNITY
		bool UNITY_IS_BEING_A_SHIT_TODAY = false;
		float y;
		if (UNITY_IS_BEING_A_SHIT_TODAY)
			y = (source.height - rect.y - rect.height) * scale;
		else
			y = rect.y * scale;
		Texture2D readableTex = new Texture2D(width, height);
		readableTex.ReadPixels(new Rect(x, y, width, height), 0, 0);
		readableTex.Apply();
		RenderTexture.active = previous;
		RenderTexture.ReleaseTemporary(renderTex);
		return readableTex;
	}

	void WritePNG(string filename, Texture2D tex, Rect rect, int scale=1) {
		Texture2D readableTex = CropTexture(tex, rect, scale);
		File.WriteAllBytes(filename, readableTex.EncodeToPNG());
		UnityEngine.Object.Destroy(readableTex);
	}

	void WritePNG(string filename, Sprite sprite, int scale=1) {
		WritePNG(filename, sprite.texture, sprite.textureRect, scale);
	}

	// Mostly orbs that have been renamed since uploading them
	// I already did a pass once renaming all the images to the correct names,
	// not gonna do it again
	private readonly Dictionary<string, string> ORB_NAME = new Dictionary<string, string> {
		["Darcness_Eterball"] = "Darkness_Eterball",
		["Douball-Edged_Sworb"] = "Double-Edged_Sworb",
		["Douball_Trouball"] = "Double_Trouball",

		["Squirrelball-Lvl1"] = "Squirrel_Ball",
		["DemonSquirrel-Lvl1"] = "Demon_Squirrel",
	};
	private readonly HashSet<string> SKIP_ORBS = new HashSet<string> {
		"BaseOrb",
		"GoldPeg",
		"NavigationOrb",
		"VineMultiball-Lvl1",
		"VineMultiballSmall-Lvl1",
		"VineMultiballLarge-Lvl1",
	};

	void ExtractOrbImages() {
		Directory.CreateDirectory($"{targetDir}/orbs");
		Directory.CreateDirectory($"{targetDir}/orbs/anim");
		using (StreamWriter fp = new StreamWriter($"{targetDir}/orbs/anim/gen.sh")) {
			fp.NewLine = "\n";
			fp.WriteLine("#!/bin/bash");
			fp.WriteLine("set -euo pipefail");
			fp.WriteLine("rm -rf ../animout");
			fp.WriteLine("mkdir ../animout");

			ExtractOrbImages(fp);
		}
	}

	void ExtractOrbImages(StreamWriter fp) {
		var extracted = new HashSet<int>();
		var suffix = new Dictionary<string, int>();
		bool assemballSeen = false;
		foreach (var orb in Resources.FindObjectsOfTypeAll<PachinkoBall>()) {
			var atk = orb.GetComponent<Battle.Attacks.Attack>();
			if (!atk)
				continue;

			var sprite = orb.sprite;
			if (orb is Battle.Pachinko.BallBehaviours.AssemballParent assemball) {
				if (assemballSeen)
					continue;
				assemballSeen = true;
				sprite = assemball.BuildSprite(Battle.Pachinko.BallBehaviours.AssemballType.ALL);
			}	else {
				if (extracted.Contains(sprite.GetInstanceID()))
					continue;
				extracted.Add(sprite.GetInstanceID());
			}

			string name;
			if (atk.Name != null && atk.Name != "")
				name = atk.Name;
			else
				name = orb.name;
			name = name.Replace(" ", "_");
			if (ORB_NAME.ContainsKey(name))
				name = ORB_NAME[name];
			if (SKIP_ORBS.Contains(name))
				continue;
			if (suffix.ContainsKey(name)) {
				suffix[name]++;
				name = $"{name}__{suffix[name]}";
			} else {
				suffix[name] = 1;
			}

			WritePNG($"{targetDir}/orbs/{name}.png", sprite, 4);

			var anim = orb.GetComponentInChildren<Animator>();
			if (anim)
				ExtractOrbAnimation(fp, name, anim);
		}
	}

	private void ExtractOrbAnimation(StreamWriter fp, string name, Animator anim) {
		if (!anim) return;
		if (!anim.enabled) return;
		if (!anim.runtimeAnimatorController) return;
		if (anim.runtimeAnimatorController.animationClips.Length <= 0) return;
		var render = anim.gameObject.GetComponent<SpriteRenderer>();
		foreach (var clip in anim.runtimeAnimatorController.animationClips) {
			if (clip.empty)
				continue;

			string suffix = "";
			if (anim.runtimeAnimatorController.animationClips.Length > 1)
				suffix = $"__{clip.name}";

			if (name == "Egg" && suffix.ToLower().Contains("crack")) {
				name = "Egg_Crack";
				suffix = "";
			}

			float frameRate = clip.frameRate * 2;
			var frames = new List<(Sprite sprite, int count)>();
			float f;
			for (int i = 0; (f = (i + 0.5f) / frameRate + clip.startTime) < clip.stopTime; i++) {
				clip.SampleAnimation(render.gameObject, f);
				if (frames.Count > 0 && SameSprite(frames[frames.Count - 1].sprite, render.sprite))
					frames[frames.Count - 1] = (render.sprite, frames[frames.Count - 1].count + 1);
				else
					frames.Add((render.sprite, 1));
			}
			if (frames.Count <= 1)
				continue;

			int ix = 0;
			using (StreamWriter fp2 = new StreamWriter($"{targetDir}/orbs/anim/{name}{suffix}.txt")) {
				fp2.WriteLine("ffconcat version 1.0");
				foreach (var (sprite, count) in frames) {
					WritePNG($"{targetDir}/orbs/anim/{name}{suffix}__{ix:00}.png", sprite, 4);
					fp2.WriteLine($"file '{name}{suffix}__{ix:00}.png'");
					fp2.WriteLine($"duration {count/frameRate}");
					ix++;
				}
			}
			fp.WriteLine($"ffmpeg -f concat -safe 0 -i '{name}{suffix}.txt' -f apng -plays 0 '../animout/{name}{suffix}_anim.png'");
		}
	}

	private bool SameSprite(Sprite a, Sprite b) {
		if (a == b)
			return true;
		if (a.texture == b.texture && a.textureRect == b.textureRect)
			return true;
		return false;
	}

	// Rename relics to match the filenames that already exist in the wiki for these
	// I'm not going through and renaming them all wikiside this time, there's too many
	// Mostly just names in all-lowercase but also some typos
	private readonly Dictionary<Relics.RelicEffect, string> RELIC_NAME = new Dictionary<Relics.RelicEffect, string> {
		[Relics.RelicEffect.NO_DAMAGE_REDUCTION] = "Axe",
		[Relics.RelicEffect.LOSE_HP_GAIN_BALLWARK] = "Bastion_reaction",
		[Relics.RelicEffect.RETAIN_DODGE_BETWEEN_BATTLES] = "Beleaguered_boots",
		[Relics.RelicEffect.LONGER_AIMER] = "Unicorn_Horn",
		[Relics.RelicEffect.BLIND_BRAMBLE_COMBO] = "Branch_of_Ember",
		[Relics.RelicEffect.NO_DAMAGE_ON_RELOAD] = "Buckler",
		[Relics.RelicEffect.MAX_HEALTH_LARGE] = "Cake",
		[Relics.RelicEffect.ATTACKS_APPLY_TRANSPHERENCY] = "Clear_the_way",
		[Relics.RelicEffect.HITTING_CRIT_ADDS_TEMP_CRITS] = "Countercrit",
		[Relics.RelicEffect.ALL_ORBS_MORBID] = "Defresh_potion",
		[Relics.RelicEffect.GAIN_BALLUSION_FROM_ENEMY_DMG] = "Distraction_reaction",
		[Relics.RelicEffect.SPINESSE_WHEN_DODGING] = "Dodgy_dagger",
		[Relics.RelicEffect.RANDOMLY_ROLL_DAMAGE] = "Dungeon_die",
		[Relics.RelicEffect.RANDOM_STATUS_EFFECT_ON_CRIT] = "Effective_Critisism",
		[Relics.RelicEffect.ADD_BALLUSION_WITH_SPAWNS] = "Fast_reakaton",
		[Relics.RelicEffect.COINS_PROVIDE_BALLUSION] = "Flaunty_gauntlets",
		[Relics.RelicEffect.COINS_PROVIDE_HEALING] = "Haglins_hat",
		[Relics.RelicEffect.DISCARD_GAIN_CRIT_BALLUSION] = "Is_dis_your_card",
		[Relics.RelicEffect.DISCARD_TO_UPGRADE] = "Modest_mallet",
		[Relics.RelicEffect.CONVERT_COIN_TO_DAMAGE] = "Molten_mantle",
		[Relics.RelicEffect.GOLD_ADDS_TO_DAMAGE] = "Peglinero_pendant",
		[Relics.RelicEffect.LOSE_BALLWARK_GAIN_BALLANCE] = "Piece_of_mind",
		[Relics.RelicEffect.SLOT_PORTAL] = "Pumpkin_Pi",
		[Relics.RelicEffect.BOMB_NAV_GOLD] = "Reduce_refuse_recycle",
		[Relics.RelicEffect.REFRESH_UPGRADES_PEGS] = "Refresh_perspective",
		[Relics.RelicEffect.DAMAGE_RETURN] = "Ring_of_Indignation",
		[Relics.RelicEffect.PREVENT_LETHAL_DAMAGE] = "Sash_of_focus",
		[Relics.RelicEffect.ATTACKS_APPLY_EXPLOITABALL] = "Spiffy_crit",
		[Relics.RelicEffect.START_WITH_EXPLOITABALL] = "Stacked_orbacus",
		[Relics.RelicEffect.BALLUSION_GUARANTEED_CRIT] = "Steady_scope",
		[Relics.RelicEffect.DAMAGE_CREATES_DAMAGE_REDUCTION_SLIME] = "Subtraction_reaction",
		[Relics.RelicEffect.BALLUSION_ON_CRIT] = "Vitamin_c",
		[Relics.RelicEffect.DOUBLE_DAMAGE_HURT_ON_PEG] = "Wand_of_Skulltimate_Power",
	};

	void ExtractRelicImages() {
		Directory.CreateDirectory($"{targetDir}/relics");
		var relicManager = Map.MapController.instance._relicManager;
		foreach	(var relic in relicManager.globalRelics.relics) {
			ExtractRelicImage(relic);
		}
		ExtractRelicImage(relicManager.consolationPrize);
	}

	void ExtractRelicImage(Relics.Relic relic) {
		string name = relic.englishDisplayName;
		//name = name.Replace("?", "_");
		name = name.Replace("'", "");
		name = name.Replace(" ", "_");
		if (RELIC_NAME.ContainsKey(relic.effect))
			name = RELIC_NAME[relic.effect];
		WritePNG($"{targetDir}/relics/{name}.png", relic.sprite, 4);
	}

	void ExtractChallengeImages() {
		Directory.CreateDirectory($"{targetDir}/challenges");
		var challengeSet = Map.MapController.instance._globalChallenges;

		foreach	(var challenge in challengeSet.challenges) {
			string name = I2.Loc.LocalizationManager.GetTranslation(challenge.nameKey);
			name = name.Replace("?", "_");
			name = name.Replace(" ", "_");
			WritePNG($"{targetDir}/challenges/{name}.png", challenge.sprite, 4);
		}
	}
}

public class Patcher : IDisposable {
	public Patcher() {
		MethodInfo patchfunc = typeof(Patcher).GetMethod("PatchDoNothing");
		MethodInfo target = typeof(Map.MapController).GetMethod("Start", BindingFlags.Instance|BindingFlags.NonPublic);
		Plugin.inst.harmony.Patch(target, new HarmonyLib.HarmonyMethod(patchfunc));
	}

	public void Dispose() {
		MethodInfo patchfunc = typeof(Patcher).GetMethod("PatchDoNothing");
		MethodInfo target = typeof(Map.MapController).GetMethod("Start", BindingFlags.Instance|BindingFlags.NonPublic);
		Plugin.inst.harmony.Unpatch(target, patchfunc);
	}

	public static bool PatchDoNothing() {
		return false;
	}
}
