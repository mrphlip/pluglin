using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace CustomCruciball;

public class Utils {
	static public T GetResource<T>() where T : UnityEngine.Object {
		T[] objs = Resources.FindObjectsOfTypeAll<T>();
		if (objs.Length > 0)
			return objs[0];
		else
			return null;
	}
}
