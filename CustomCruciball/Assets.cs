using System.IO;
using System.Reflection;
using UnityEngine;

namespace CustomCruciball;

public class Assets {
	public static readonly Sprite Unchecked = MakeSprite("unchecked.png");
	public static readonly Sprite Checked = MakeSprite("checked.png");

	private static byte[] GetAsset(string assetname) {
		Assembly assembly = typeof(Plugin).Assembly;
		string fullname = $"{assembly.GetName().Name}.assets.{assetname}";
		Stream datastream = assembly.GetManifestResourceStream(fullname);
		int len = (int)datastream.Length, n = 0;
		byte[] data = new byte[len];
		while (n < len) {
			int i = datastream.Read(data, n, len - n);
			if (i == 0) break;
			n += i;
		}
		return data;
	}

	private static Sprite MakeSprite(string assetname) {
		Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
		tex.LoadImage(GetAsset(assetname));
		tex.filterMode = FilterMode.Point;
		tex.wrapMode = TextureWrapMode.Clamp;
		tex.wrapModeU = TextureWrapMode.Clamp;
		tex.wrapModeV = TextureWrapMode.Clamp;
		tex.wrapModeW = TextureWrapMode.Clamp;
		return UnityEngine.Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
	}
}
