using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Switches the project between the apps built from it and builds them.
/// Replaces hand-editing GameSequencesList.isTheNarrativeSequence / AppSkipSceneButton.ActiveDebugGlobalUI.
/// </summary>
public static class AppFlavorBuildMenu
{
    const string MinijuegosPath = "Assets/Proj_Andes/Menu/AppFlowSystem/Scriptables/AppFlavors/AppFlavor_Minijuegos.asset";
    const string HistoriasPath = "Assets/Proj_Andes/Menu/AppFlowSystem/Scriptables/AppFlavors/AppFlavor_Historias.asset";
    const string ActiveFlavorPath = "Assets/Proj_Andes/Menu/AppFlowSystem/Scriptables/Resources/ActiveAppFlavor.asset";
    const string BuildsFolder = "Builds";

    [MenuItem("Hi Hat Games/App Flavor/Usar Minijuegos", priority = 0)]
    static void UseMinijuegos() => Apply(Load(MinijuegosPath));

    [MenuItem("Hi Hat Games/App Flavor/Usar Historias", priority = 1)]
    static void UseHistorias() => Apply(Load(HistoriasPath));

    [MenuItem("Hi Hat Games/App Flavor/Usar Minijuegos", true)]
    static bool UseMinijuegosCheck() { Menu.SetChecked("Hi Hat Games/App Flavor/Usar Minijuegos", IsActive(MinijuegosPath)); return !EditorApplication.isPlaying; }

    [MenuItem("Hi Hat Games/App Flavor/Usar Historias", true)]
    static bool UseHistoriasCheck() { Menu.SetChecked("Hi Hat Games/App Flavor/Usar Historias", IsActive(HistoriasPath)); return !EditorApplication.isPlaying; }

    [MenuItem("Hi Hat Games/Build/Minijuegos - Android (release)", priority = 20)]
    static void BuildMinijuegosRelease() => Build(Load(MinijuegosPath), development: false);

    [MenuItem("Hi Hat Games/Build/Minijuegos - Android (desarrollo)", priority = 21)]
    static void BuildMinijuegosDev() => Build(Load(MinijuegosPath), development: true);

    [MenuItem("Hi Hat Games/Build/Historias - Android (release)", priority = 40)]
    static void BuildHistoriasRelease() => Build(Load(HistoriasPath), development: false);

    [MenuItem("Hi Hat Games/Build/Historias - Android (desarrollo)", priority = 41)]
    static void BuildHistoriasDev() => Build(Load(HistoriasPath), development: true);

    static AppFlavor Load(string path)
    {
        var flavor = AssetDatabase.LoadAssetAtPath<AppFlavor>(path);
        if (flavor == null) throw new FileNotFoundException("Missing app flavor asset", path);
        return flavor;
    }

    static bool IsActive(string path)
    {
        var selection = AssetDatabase.LoadAssetAtPath<ActiveAppFlavor>(ActiveFlavorPath);
        return selection != null && selection.flavor != null && AssetDatabase.GetAssetPath(selection.flavor) == path;
    }

    /// <summary>Makes the flavor the active one and applies its identity to the Player Settings.</summary>
    public static void Apply(AppFlavor flavor)
    {
        var selection = AssetDatabase.LoadAssetAtPath<ActiveAppFlavor>(ActiveFlavorPath);
        if (selection == null)
        {
            selection = ScriptableObject.CreateInstance<ActiveAppFlavor>();
            AssetDatabase.CreateAsset(selection, ActiveFlavorPath);
        }
        selection.flavor = flavor;
        EditorUtility.SetDirty(selection);

        PlayerSettings.productName = flavor.productName;
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, flavor.applicationIdentifier);
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, flavor.applicationIdentifier);
        if (flavor.icon != null) PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { flavor.icon }, IconKind.Any);

        AssetDatabase.SaveAssets();
        AppFlavor.ClearCache();
        Debug.Log($"App flavor: {flavor.productName} ({flavor.applicationIdentifier}), narrative app: {flavor.isNarrativeApp}");
    }

    static void Build(AppFlavor flavor, bool development)
    {
        if (!Validate(development)) return;
        Apply(flavor);

        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        var fileName = $"{flavor.productName}_{PlayerSettings.bundleVersion}_{PlayerSettings.Android.bundleVersionCode}{(development ? "_dev" : "")}.apk";
        var outputPath = Path.Combine(BuildsFolder, flavor.name, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
            options = development ? BuildOptions.Development : BuildOptions.None,
        };
        var report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;
        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"Build OK: {outputPath} ({summary.totalSize / (1024 * 1024)} MB, {summary.totalTime})");
            EditorUtility.RevealInFinder(outputPath);
        }
        else Debug.LogError($"Build {summary.result}: {summary.totalErrors} errors, see the console");
    }

    //Release builds must not carry editor tooling, and every new build needs a higher version code
    static bool Validate(bool development)
    {
        var problems = new System.Collections.Generic.List<string>();
        var androidDefines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Android);
        if (!development && androidDefines.Contains("UNITY_MCP"))
            problems.Add("El paquete Unity-MCP está instalado (define UNITY_MCP_*): sus DLLs de Assets/Plugins/NuGet entrarían al APK de release.");
        if (PlayerSettings.Android.bundleVersionCode <= 1)
            problems.Add($"Version code es {PlayerSettings.Android.bundleVersionCode}, igual al de los APK publicados (1). Súbelo para distinguir este build de los publicados.");

        if (problems.Count == 0) return true;
        return EditorUtility.DisplayDialog("Revisar antes del build", string.Join("\n\n", problems), "Construir de todas formas", "Cancelar");
    }
}
