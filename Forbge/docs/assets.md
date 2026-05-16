# Asset management
Forbge provides some utility methods you can use to load image assets from your mod to use as sprites.

Images should be stored in an `assets` subfolder in your project, and then included in your mod by an `<EmbeddedResource>` directive in the `.csproj` file.

# Loading raw assets
A raw asset file can be loaded by either:
* `byte[] data = Forbge.AssetHelper.GetAsset(typeof(Plugin).Assembly, "filename.png");`
* `byte[] data = Forbge.AssetHelper.GetAsset("filename.png");`

The second, shorter form, can only be used when you're working inside your plugin's `Register` callback, as Forbge knows which mod is currently being registered, and will load assets from that mod.

If you want to load an asset at a different time (eg, as an initialiser on a static variable) then you will need to pass in the explicit Assembly reference.

# Loading images as sprites
You can load an asset as a Unity Sprite, or open image data you have already loaded as a Sprite:
* `UnityEngine.Sprite sprite = Forbge.AssetHelper.MakeSprite(typeof(Plugin).Assembly, "filename.png");`
* `UnityEngine.Sprite sprite = Forbge.AssetHelper.MakeSprite("filename.png");`
* `UnityEngine.Sprite sprite = Forbge.AssetHelper.MakeSprite(data);`

This takes the image and turns into a single sprite.

# Loading images as a spritesheet
You can also include several sprites in a single image, and then cut individual pieces out of it to serve as separate sprites. For example, this loads a single 48x16 image and cuts it into three 16x16 sprites:
```cs
UnityEngine.Texture2D texture = Forbge.AssetHelper.MakeTexture(typeof(Plugin).Assembly, "filename.png");
UnityEngine.Sprite sprite1 = Forbge.AssetHelper.MakeSprite(texture, new Rect(0, 0, 16, 16));
UnityEngine.Sprite sprite2 = Forbge.AssetHelper.MakeSprite(texture, new Rect(16, 0, 16, 16));
UnityEngine.Sprite sprite3 = Forbge.AssetHelper.MakeSprite(texture, new Rect(32, 0, 16, 16));
```
