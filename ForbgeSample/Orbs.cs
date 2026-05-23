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
    }
}
