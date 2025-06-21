using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// This class handles data saving and loading.
    /// All data stored here are default values.
    /// </summary>
    public static class SaveData
    {
        public static Dictionary<string, string> saveValues = new Dictionary<string, string>()
        {
            {"ScreenScale", "2"}, // 1 - 4 ONLY!
            {"MinStreamDuration", "0.2"},
            {"MaxStreamDuration", "0.4"},
            {"MaxClipCount", "20"},
            {"VideoWidth", "640"},
            {"VideoHeight", "480"},
            {"IntrosEnabled", "false"},
            {"OutrosEnabled", "true"},
            {"PlayAutomatically", "true"},
            {"GameHighScore", "0"},
            {"MusicVolume", "50"},
            {"SoundEffectVolume", "100"},
            {"VideoVolume", "100"},
            {"TransitionChance", "20"},
            {"OverlayChance", "20"},
            {"EffectChance", "60"},
            {"TransitionEffects", "true"},
            {"TransitionEffectChance", "30"},
            {"HiddenKeepTemporaryJobFolders", "false"},
            {"HiddenVerbose", "false"},
            {"DisableMotion", "false"},
            {"FirstBoot", "true"},
            {"DisabledMedia", "[]"},
            {"TotalVideosRendered", "0"},
            {"TotalMediaImported", "0"},
            {"TotalClipsTrimmed", "0"},
            {"PlayOverlayInFull", "false"},
            {"MaxUniqueClips", "0"},
            {"DeleteClipsAfterMaxUniqueClips", "false"},
            {"DisableClipsAfterMaxUniqueClips", "false"},
            {"EnableDiscordRPC", "true"},
            {"ConstrainAspectRatio", "true"},
            {"MuteMusicWhileTabbedOut", "true"},
            {"VideoPlaybackScale", "2"},
            {"ActiveTheme", ""},
            {"TimeOut", "60"},
            {"EnableTimeOut", "true"},
            {"SkipPhotosensitiveWarningScreen", "false"},
            {"PluginListFilterFlags", "15"},
            {"LastVersion", ""},
            {"Locale", "fixme"},
            {"DisableHolidays", "false"},
            {"UseExternalVideoPlayer", "false"},
            {"Fullscreen", "false"},
            {"AlwaysOnTop", "false"},
            {"MatchAspectRatio", "false"},
            {"UseNativeCursor", "false"},
            {"VideoFPS", "30"},
            {"LastMusicTrack", "-1"}
        };
        public static string saveFileName = "Options.json";
        public static bool Save()
        {
            try
            {
                string json = JsonConvert.SerializeObject(saveValues, Formatting.Indented);
                File.WriteAllText(saveFileName, json);
                return true;
            }
            catch(Exception e)
            {
                ConsoleOutput.WriteLine(e.Message, Color.Red);
                return false;
            }
        }
        public static bool Load()
        {
            try
            {
                bool go = true;
                if (!File.Exists(saveFileName))
                {
                    ConsoleOutput.WriteLine("Save file not found. Creating new one.", Color.Yellow);
                    go = Save();
                }
                if (go)
                {
                    string json = File.ReadAllText(saveFileName);
                    Dictionary<string, string>? loadedValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    if (loadedValues == null)
                    {
                        ConsoleOutput.WriteLine("Save file is corrupted.", Color.Red);
                        loadedValues = new Dictionary<string, string>();
                    }
                    // Merge loaded values into save values.
                    foreach (KeyValuePair<string, string> pair in saveValues)
                    {
                        if (loadedValues.ContainsKey(pair.Key))
                        {
                            if (loadedValues[pair.Key] != pair.Value)
                            {
                                saveValues[pair.Key] = loadedValues[pair.Key];
                            }
                        }
                        else
                        {
                            saveValues[pair.Key] = pair.Value;
                        }
                    }
                    // Save the new values.
                    Save();
                }
                else
                {
                    ConsoleOutput.WriteLine("Failed to load save file.", Color.Red);
                }
                if (UserInterface.instance != null)
                {
                    if (UserInterface.instance.videoPlayer != null)
                        UserInterface.instance.videoPlayer.Volume = int.Parse(SaveData.saveValues["VideoVolume"], CultureInfo.InvariantCulture) / 100f;
                }
                return true;
            }
            catch(Exception e)
            {
                ConsoleOutput.WriteLine(e.Message);
                return false;
            }
        }
    }
}
