using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Forbge;

public class AssetHelper {
    public static readonly Sprite QUESTIONMARK = MakeSprite(typeof(AssetHelper).Assembly, "question.png");

    public static byte[] GetAsset(string assetname) {
        return GetAsset(Registry.currentRegistrar, assetname);
    }

    public static byte[] GetAsset(Assembly assembly, string assetname) {
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

    public static Texture2D MakeTexture(string assetname) {
        return MakeTexture(GetAsset(Registry.currentRegistrar, assetname));
    }

    public static Texture2D MakeTexture(Assembly assembly, string assetname) {
        return MakeTexture(GetAsset(assembly, assetname));
    }

    public static Texture2D MakeTexture(byte[] imagedata) {
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.LoadImage(imagedata);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.wrapModeU = TextureWrapMode.Clamp;
        tex.wrapModeV = TextureWrapMode.Clamp;
        tex.wrapModeW = TextureWrapMode.Clamp;
        return tex;
    }

    public static Sprite MakeSprite(string assetname) {
        return MakeSprite(MakeTexture(GetAsset(Registry.currentRegistrar, assetname)));
    }

    public static Sprite MakeSprite(Assembly assembly, string assetname) {
        return MakeSprite(MakeTexture(GetAsset(assembly, assetname)));
    }

    public static Sprite MakeSprite(byte[] imagedata) {
        return MakeSprite(MakeTexture(imagedata));
    }

    public static Sprite MakeSprite(Texture2D tex) {
        return MakeSprite(tex, new Rect(0.0f, 0.0f, tex.width, tex.height));
    }

    public static Sprite MakeSprite(Texture2D tex, Rect bounds) {
        return Sprite.Create(tex, bounds, new Vector2(0.5f, 0.5f));
    }
}
