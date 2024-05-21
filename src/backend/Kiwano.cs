using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.Xna.Framework;
using Steamworks;

namespace NonsensicalVideoGenerator
{
    // Kiwano is a rudimentary anti-piracy system that just calls out pirates in logs.
    public static class Kiwano
    {
        public static bool connected = false;
        public static string kiwanoUrl = "https://reporting.kiwifruitdev.sentry.io/708845c4-9d70-4b1f-8c60-130d43cc4d0b"; // not a real domain
        public static string sentryId = "releasebuild";
        // This just masquerades as the Sentry error reporting system.
        // All it does it call out pirates in console.txt.
        public static void Check()
        {
            string cwd = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".");
            ConsoleOutput.WriteLine("Kiwano: Preparing for crash reporting...", Color.Transparent);
            if(SteamManager.initialized)
            {
                // Add user's steam id to potentially blacklist them
                // Reverse their steam id and split it into two parts (harder for end users to know what it is)
                string steamId = SteamUser.GetSteamID().ToString();
                string reversedSteamId = new string(steamId.Reverse().ToArray());
                string[] splitSteamId = new string[2];
                splitSteamId[0] = reversedSteamId.Substring(0, reversedSteamId.Length / 2);
                splitSteamId[1] = reversedSteamId.Substring(reversedSteamId.Length / 2);
                sentryId = splitSteamId[0] +" (" + splitSteamId[1] + ")";
                ConsoleOutput.WriteLine("Kiwano: Connected", Color.Transparent);
            }
            else
            {
                ConsoleOutput.WriteLine("Kiwano: Not connected", Color.Transparent);
            }
            // Real debug builds do not print out these bogus messages.
            // It is potentially possible to edit the binary or use cheat engine to "fake" a debug build.
            if(Debug.debugBuild)
            {
                ConsoleOutput.WriteLine("Kiwano: Sentry bypassed (Debug build)", Color.Transparent);
#if !DEBUG
                if(sentryId == "releasebuild")
                    sentryId = "debugbuild";
                // Check for a file that should never exist, just to defer reverse engineering.
                // Debug builds will never print these messages.
                if(File.Exists(Path.Combine(cwd, "sentry_api_key.txt")))
                {
                    ConsoleOutput.WriteLine("Kiwano: Sentry API key: "+File.ReadAllText(Path.Combine(cwd, "sentry_api_key.txt")), Color.Transparent);
                }
                else
                {
                    ConsoleOutput.WriteLine("Kiwano: Sentry API key not found", Color.Transparent);
                }
#else
                // Proves that this is a real debug build.
                ConsoleOutput.WriteLine("Kiwano: Sentry API key not checked (Debug build)", Color.Transparent);
#endif
            }
            if(File.Exists(Path.Combine(cwd, "steam_appid.txt")))
            {
                // Release builds should NEVER have this file.
                // This is how pirates could potentially bypass the Steam DRM.
                ConsoleOutput.WriteLine("Kiwano: Steam app ID: "+File.ReadAllText(Path.Combine(cwd, "steam_appid.txt")), Color.Transparent);
            }
#if DEBUG
            ConsoleOutput.WriteLine("Kiwano: Steam app ID set up!", Color.Transparent);
#endif
            // How to check for pirates:
            // 1. "Kiwano: Not connected"
            //    Pirates may not be able to connect to Steam.
            // 2. "Kiwano: Sentry API key:" or "Kiwano: Sentry API key not found"
            //    Pirates may fake a debug build, but real debug builds will never print this message.
            // 3. "Kiwano: Steam app ID:" or "Kiwano: Steam app ID not found"
            //    NVG will successfully run if this file is present in order to aid development.
            //    However, this file should never be utilized by end users.
            //    If a real debug build is being used (developers), "Kiwano: Steam app ID set up!" will be printed.
            //    Otherwise, it is likely a pirate.
            // 4. "Kiwano: Set up Sentry reporting! (steamid)"
            //    This should always have a Steam ID attached to it.
            //    If it does not, it is likely a pirate.
            ConsoleOutput.WriteLine("Kiwano: Set up Sentry reporting! Session: "+sentryId, Color.Transparent);
        }
    }
}