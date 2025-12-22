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
	};

	public IEnumerator Go() {
		yield return null;
		using (var patcher = new Patcher()) {
			yield return UniverseLib.RuntimeHelper.StartCoroutine(LoadScene(1));

			Init();

			ExtractEnums();
			ExtractClasses();
			ExtractRelicData();
			ExtractOrbData();
			ExtractSeedData();
			yield return UniverseLib.RuntimeHelper.StartCoroutine(ExtractMapData());
		}

		ExtractOrbImages();
		ExtractRelicImages();

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

	public void ExtractOrbData() {
		using (StreamWriter fp = new StreamWriter($"{targetDir}/orbs.py")) {
			fp.WriteLine("from collections import namedtuple");
			fp.WriteLine("from .enums import OrbRarity");
			fp.WriteLine("__all__ = [\"orbs\"]");
			fp.WriteLine("");

			var deckManager = Map.MapController.instance._deckManager;

			fp.WriteLine("Orb = namedtuple(\"Orb\", [\"key\", \"name\", \"rarity\"])");
			fp.WriteLine("orbs = {\n");
			foreach (var orb in deckManager.OrbPool.AvailableOrbs) {
				var atk = orb.GetComponent<Battle.Attacks.Attack>();
				var ball = orb.GetComponent<PachinkoBall>();
				fp.WriteLine($"  \"{atk.locNameString}\": Orb(\"{atk.locNameString}\", \"{atk.Name}\", OrbRarity.{ball.orbRarity}),");
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
			for (var i = 1; i <= 3; i++) {
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
		float y = (source.height - rect.y - rect.height) * scale;
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

	void ExtractOrbImages() {
		Directory.CreateDirectory($"{targetDir}/orbs");
		var extracted = new HashSet<int>();
		var suffix = new Dictionary<string, int>();
		bool assemballSeen = false;
		foreach (var orb in Resources.FindObjectsOfTypeAll<PachinkoBall>()) {
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

			var atk = orb.GetComponent<Battle.Attacks.Attack>();
			string name;
			if (atk != null && atk.Name != null && atk.Name != "")
				name = atk.Name;
			else
				name = orb.name;
			name = name.Replace(" ", "_");
			if (suffix.ContainsKey(name)) {
				suffix[name]++;
				name = $"{name}__{suffix[name]}";
			} else {
				suffix[name] = 1;
			}

			WritePNG($"{targetDir}/orbs/{name}.png", sprite, 4);
		}
	}

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
		name = name.Replace("?", "_");
		name = name.Replace(" ", "_");
		WritePNG($"{targetDir}/relics/{name}.png", relic.sprite, 4);
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
