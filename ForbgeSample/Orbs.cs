using System;
using HarmonyLib;
using UnityEngine;
using Forbge;

namespace ForbgeSample;

[HarmonyPatch]
public class SampleOrbs {
    static CustomOrb test;

    internal static void Register() {
        test = CustomOrb.Create("test");
        test.Name.Default = "Question Orb";
        var t = test.AddDescription();
        t.Default = "Does nothing?";

        test.Damage = 2; test.Crit = 4;
        var lvl2 = test.CreateLevel();
        lvl2.Damage = 3; lvl2.Crit = 6;
        lvl2.GetDescription(0).Default = "Does nothing??";
        var lvl3 = lvl2.CreateLevel();
        lvl3.Damage = 4; lvl3.Crit = 8;
        lvl3.GetDescription(0).Default = "Does nothing???";

        // TODO: Better sample once more of this is implemented in the framework
    }
}
