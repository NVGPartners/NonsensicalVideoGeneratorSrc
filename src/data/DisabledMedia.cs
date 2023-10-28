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
    /// This class is a JSON object that stores a list of disabled media.
    /// </summary>
    public static class DisabledMedia
    {
        public static List<string> DisabledMediaList { get; set; } = new List<string>();
        public static string saveFileName = "DisabledMedia.json";
        public static bool Save()
        {
            try
            {
                File.WriteAllText(saveFileName, JsonConvert.SerializeObject(DisabledMediaList));
                return true;
            }
            catch (Exception e)
            {
                ConsoleOutput.WriteLine($"Failed to save disabled media list: {e.Message}", Color.Red);
                return false;
            }
        }
        public static void Load()
        {
            // Create the file if it doesn't exist.
            if (!File.Exists(saveFileName))
            {
                // Import SaveData.saveValues["DisabledMedia"] parsed as a list.
                if (SaveData.saveValues["DisabledMedia"] != null && SaveData.saveValues["DisabledMedia"] != "[]")
                {
                    try
                    {
                        DisabledMediaList = JsonConvert.DeserializeObject<List<string>>(SaveData.saveValues["DisabledMedia"]);
                    }
                    catch (Exception e)
                    {
                        ConsoleOutput.WriteLine($"Failed to import disabled media list: {e.Message}", Color.Red);
                    }
                    SaveData.saveValues["DisabledMedia"] = "[]";
                    SaveData.Save();
                }
            }
            // Load the file.
            try
            {
                if (File.Exists(saveFileName))
                {
                    DisabledMediaList = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(saveFileName));
                }
            }
            catch (Exception e)
            {
                ConsoleOutput.WriteLine($"Failed to load disabled media list: {e.Message}", Color.Red);
            }
        }
        public static bool Contains(string mediaName)
        {
            return DisabledMediaList.Contains(mediaName);
        }
        public static void Add(string mediaName)
        {
            if (!DisabledMediaList.Contains(mediaName))
            {
                DisabledMediaList.Add(mediaName);
            }
        }
        public static void Remove(string mediaName)
        {
            if (DisabledMediaList.Contains(mediaName))
            {
                DisabledMediaList.Remove(mediaName);
            }
        }
    }
}
