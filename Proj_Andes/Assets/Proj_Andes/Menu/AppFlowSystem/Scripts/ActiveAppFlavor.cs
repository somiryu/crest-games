using UnityEngine;

/// <summary>
/// Lives in Resources and points to the AppFlavor being built, so only that flavor is included in the build.
/// </summary>
public class ActiveAppFlavor : ScriptableObject
{
    public AppFlavor flavor;
}
