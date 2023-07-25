using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if MONOGAME
using Microsoft.Xna.Framework;
#else
using System.Drawing;
#endif
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
            {"ScreenWidth", "320"},
            {"ScreenHeight", "240"},
            {"ScreenScale", "2"}, // 1 - 4 ONLY!
            {"BackgroundSaturation", "0"},
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
            {"SoundEffectVolume", "65"},
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
            {"PlayOverlayInFull", "false"}
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
                if(go)
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
