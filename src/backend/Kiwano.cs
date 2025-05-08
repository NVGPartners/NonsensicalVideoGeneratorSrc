using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Steamworks;

namespace NonsensicalVideoGenerator
{
    public static class Kiwano
    {
        // Just reports user SteamIDs in a manner that is less likely to be removed in the console log when asking for support.
        // It also checks for the presence of the steam_appid.txt file, which should not be present in release builds.
        public static void Check()
        {
            string cwd = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".");
            ConsoleOutput.WriteLine("Kiwano: Preparing for crash reporting...", Color.Transparent);
            if(SteamManager.initialized)
            {
                // Reverse their steam id and split it into two parts (harder to identify)
                string steamId = SteamUser.GetSteamID().ToString();
                string reversedSteamId = new string(steamId.Reverse().ToArray());
                string[] splitSteamId =
                {
                    reversedSteamId.Substring(0, reversedSteamId.Length / 2),
                    reversedSteamId.Substring(reversedSteamId.Length / 2),
                };
                ConsoleOutput.WriteLine("Kiwano: Connected! Session: " + splitSteamId[0] + " (" + splitSteamId[1] + ")", Color.Transparent);
            }
            else
            {
                // Not connected to Steam, can't get their SteamID.
                ConsoleOutput.WriteLine("Kiwano: Not connected", Color.Transparent);
            }
            if(File.Exists(Path.Combine(cwd, "steam_appid.txt")))
            {
                // Release builds should NEVER have this file.
                ConsoleOutput.WriteLine("Kiwano: Steam app ID: "+File.ReadAllText(Path.Combine(cwd, "steam_appid.txt")), Color.Transparent);
            }
        }
    }
}