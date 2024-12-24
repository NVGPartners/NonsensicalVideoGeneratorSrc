using System;
using System.IO;

public static class Architecture
{
    public static string translatorVersion = "";
    public static bool detectedTranslator = false;
    public static bool IsRunningThroughProton()
    {
        string steamCompatDataPath = Environment.GetEnvironmentVariable("STEAM_COMPAT_DATA_PATH") ?? string.Empty;
        bool detected = !string.IsNullOrEmpty(steamCompatDataPath);
        if(!detectedTranslator && detected)
        {
            detectedTranslator = true;
            string versionFilePath = Path.Combine(steamCompatDataPath, "version");
            if(File.Exists(versionFilePath))
            {
                translatorVersion = File.ReadAllText(versionFilePath).Trim();
            }
        }
        return detected;
    }
    public static bool IsRunningThroughWine()
    {
        string wineVersion = Environment.GetEnvironmentVariable("WINE_VERSION") ?? string.Empty;
        string winePrefix = Environment.GetEnvironmentVariable("WINEPREFIX") ?? string.Empty;
        bool detected = !string.IsNullOrEmpty(wineVersion) || !string.IsNullOrEmpty(winePrefix);
        if(!detectedTranslator && detected)
        {
            detectedTranslator = true;
            translatorVersion = wineVersion;
        }
        return !string.IsNullOrEmpty(wineVersion) || !string.IsNullOrEmpty(winePrefix);
    }
}
