/// <summary>
/// Version of the app, sent as version_app in every document it uploads.
/// Raise it for each published build: the build menu copies it to the Android version code, the Firestore rules
/// reject uploads below their minimum, and config/app.min_version blocks older apps with a "download the latest" panel.
/// The APKs published in 2024 are version 1 and don't send the field at all.
/// </summary>
public static class AppVersion
{
    public const int Number = 2;

    public static string Label => "1." + Number;

    /// <summary>True when the minimum published in config/app is above this build.</summary>
    public static bool IsOutdated(long minVersion) => Number < minVersion;
}
