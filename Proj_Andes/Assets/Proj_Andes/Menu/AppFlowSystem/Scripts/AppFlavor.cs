using UnityEngine;

/// <summary>
/// One of the apps built from this project (Minijuegos or Historias).
/// The flavor in use is the one referenced by Resources/ActiveAppFlavor, set by the
/// "Hi Hat Games/App Flavor" and "Hi Hat Games/Build" menus, never by hand-editing code.
/// </summary>
[CreateAssetMenu(fileName = "AppFlavor", menuName = "GameSequencesList/AppFlavor")]
public class AppFlavor : ScriptableObject
{
    public string productName;
    public string applicationIdentifier;
    [Tooltip("Historias plays the narrative sequence, Minijuegos the games sequence")]
    public bool isNarrativeApp;
    [Tooltip("Optional, if empty the icon in Player Settings is kept")]
    public Texture2D icon;

    public const string ActiveFlavorResourcePath = "ActiveAppFlavor";

    static AppFlavor current;

    public static AppFlavor Current
    {
        get
        {
            if (current == null) current = LoadActive();
            return current;
        }
    }

    //Loaded before the first scene so no one ends up calling Resources.Load from a Firebase task thread
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void LoadOnStart() => current = LoadActive();

    static AppFlavor LoadActive()
    {
        var selection = Resources.Load<ActiveAppFlavor>(ActiveFlavorResourcePath);
        if (selection != null && selection.flavor != null) return selection.flavor;

        Debug.LogError("No active app flavor found in Resources/" + ActiveFlavorResourcePath + ", running as Minijuegos");
        var fallback = CreateInstance<AppFlavor>();
        fallback.name = "Fallback Minijuegos";
        return fallback;
    }

#if UNITY_EDITOR
    //So switching flavor in the editor applies on the next play without a domain reload
    public static void ClearCache() => current = null;
#endif
}
