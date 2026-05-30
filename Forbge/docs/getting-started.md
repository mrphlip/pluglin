# Part 1: Creating a basic BepInEx plugin for Peglin
1. Install [.NET SDK](https://dotnet.microsoft.com/en-us/download)
2. Install BepInEx templates:
```sh
dotnet new install BepInEx.Templates::2.0.0-be.4 --nuget-source https://nuget.bepinex.dev/v3/index.json
```
3. In a new folder for your project:
```sh
dotnet new bepinex5plugin -n <PluginName> -T netstandard2.1 -U 2021.3.28
```
4. Edit your `PluginName.csproj` file, to add a reference to the Peglin codebase:
```xml
<ItemGroup>
	<PackageReference Include="BepInEx.AssemblyPublicizer.MSBuild" Version="0.4.2" PrivateAssets="all" />
	<Reference Include="path/to/Peglin/Peglin_Data/Managed/Assembly-CSharp.dll" Publicize="true" />
</ItemGroup>
```
5. If needed you can also add references to other modules that you need access to:
```xml
<ItemGroup>
	<Reference Include="UnityEngine.CoreModule">
		<HintPath>path/to/Peglin/Peglin_Data/Managed/UnityEngine.CoreModule.dll</HintPath>
	</Reference>
	<Reference Include="UnityEngine.UI">
		<HintPath>path/to/Peglin/Peglin_Data/Managed/UnityEngine.UI.dll</HintPath>
	</Reference>
</ItemGroup>
```
6. Build the plugin:
```sh
dotnet build --configuration Debug
```
7. Copy `bin/Debug/netstandard2.1/PluginName.dll` to your Peglin `BepInEx/plugins` folder, to test it.
8. Personally I like to use a `Makefile` to organise the build and installation commands, but this is not necessary if you would prefer to do something else.

# Part 2: Adding the Forbge framework to the plugin
1. Edit your `PluginName.csproj` file, to add a reference to the Forbge plugin:
```xml
<Reference Include="Forbge">
	<HintPath>path/to/BepInEx/plugins/mrphlip-Forbge/Forbge.dll</HintPath>
</Reference>
```
2. In your plugin's `Awake` method, register a callback which will create all your custom items:
```cs
private void Awake() {
	Forbge.Registry.onRegister += Register;
}

private void Register() {
	// ...
}
```
3. In your project folder, make a subfolder called `assets`, where images can be stored, and add it to your `.csproj`:
```xml
<ItemGroup>
  <EmbeddedResource Include="assets/*.png" />
</ItemGroup>
```

# References
* The docs for [dotnet](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet), the build system
* The docs for [BepInEx](https://docs.bepinex.dev/articles/dev_guide/plugin_tutorial/index.html), the mod loader
* The docs for [Harmony](https://harmony.pardeike.net/articles/intro.html) and [HarmonyX](https://github.com/BepInEx/HarmonyX/wiki/Basic-usage), the tools for making actual changes to the game code
* The docs for [Thunderstore](https://new.thunderstore.io/package/create/docs/), the mod distribution platform
