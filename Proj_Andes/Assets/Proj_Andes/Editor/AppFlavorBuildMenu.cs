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
        ApplyAndroidIcons(flavor);
        //AppVersion.Number is the single source of the version, so the APK never disagrees with the version_app it uploads
        PlayerSettings.Android.bundleVersionCode = AppVersion.Number;
        PlayerSettings.bundleVersion = AppVersion.Label;

        AssetDatabase.SaveAssets();
        AppFlavor.ClearCache();
        Debug.Log($"App flavor: {flavor.productName} ({flavor.applicationIdentifier}), narrative app: {flavor.isNarrativeApp}");
    }

    //Legacy and round icons use the full icon; the adaptive one (Android 8+) uses the two layers, background first
    static void ApplyAndroidIcons(AppFlavor flavor)
    {
        var android = NamedBuildTarget.Android;
        void SetAll(PlatformIconKind kind, params Texture2D[] layers)
        {
            var icons = PlayerSettings.GetPlatformIcons(android, kind);
            foreach (var icon in icons) icon.SetTextures(layers);
            PlayerSettings.SetPlatformIcons(android, kind, icons);
        }
        if (flavor.icon != null)
        {
            SetAll(UnityEditor.Android.AndroidPlatformIconKind.Legacy, flavor.icon);
            SetAll(UnityEditor.Android.AndroidPlatformIconKind.Round, flavor.icon);
        }
        if (flavor.adaptiveIconBackground != null && flavor.adaptiveIconForeground != null)
            SetAll(UnityEditor.Android.AndroidPlatformIconKind.Adaptive, flavor.adaptiveIconBackground, flavor.adaptiveIconForeground);
    }

    static void Build(AppFlavor flavor, bool development)
    {
        //Burst only finds the embedded Android NDK when its editor code was compiled with Android as the active platform,
        //so switching inside the build fails ("Burst requires the Android NDK"). Switch first and build again afterwards.
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            if (EditorUtility.DisplayDialog("Cambiar a Android primero",
                "La plataforma activa es " + EditorUserBuildSettings.activeBuildTarget + ". Hay que cambiar a Android y esperar a que Unity recompile antes de construir.\n\nCuando termine, vuelve a usar este menú de build.",
                "Cambiar a Android", "Cancelar"))
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            return;
        }
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
        StopGradleDaemons();
    }

    //The Gradle daemon a build leaves running keeps up to 4 GB of RAM until Unity quits; the next build starts a new one
    static void StopGradleDaemons()
    {
        try
        {
            var jdk = UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath;
            var launcher = Directory.GetFiles(Path.Combine(UnityEditor.Android.AndroidExternalToolsSettings.gradlePath, "lib"), "gradle-launcher-*.jar").FirstOrDefault();
            if (string.IsNullOrEmpty(jdk) || launcher == null) return;
            var info = new System.Diagnostics.ProcessStartInfo(Path.Combine(jdk, "bin", "java"), $"-classpath \"{launcher}\" org.gradle.launcher.GradleMain --stop")
            {
                UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true,
            };
            info.EnvironmentVariables["JAVA_HOME"] = jdk;
            using var process = System.Diagnostics.Process.Start(info);
            process.WaitForExit(30000);
            Debug.Log("Gradle daemons stopped: " + process.StandardOutput.ReadToEnd().Trim());
        }
        catch (System.Exception e) { Debug.LogWarning("Could not stop the Gradle daemons: " + e.Message); }
    }

    //Release builds must not carry editor tooling, and every new build needs a higher version code
    static bool Validate(bool development)
    {
        var problems = new System.Collections.Generic.List<string>();
        var androidDefines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Android);
        if (!development && androidDefines.Contains("UNITY_MCP"))
            problems.Add("El paquete Unity-MCP está instalado (define UNITY_MCP_*): sus DLLs de Assets/Plugins/NuGet entrarían al APK de release.");
        if (AppVersion.Number <= 1)
            problems.Add($"AppVersion.Number es {AppVersion.Number}, igual al de los APK publicados (1). Súbelo en Scripts/AppVersion.cs para distinguir este build de los publicados.");

        if (problems.Count == 0) return true;
        return EditorUtility.DisplayDialog("Revisar antes del build", string.Join("\n\n", problems), "Construir de todas formas", "Cancelar");
    }
}
