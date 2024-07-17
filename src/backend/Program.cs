using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
#if !MONOGAME
using System.Windows.Forms;
#endif
using Steamworks;

namespace NonsensicalVideoGenerator
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Launch options
            for(int i = 0; i < args.Length; i++)
            {
                Global.parameters.Add(args[i]);
            }
            if(Global.parameters.Contains("-console"))
            {
                string cwd = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".");
                string consoleTxt = Path.Combine(cwd, "console.txt");
                if(File.Exists(consoleTxt))
                {
                    ProcessStartInfo startInfo = new()
                    {
                        FileName = consoleTxt,
                        UseShellExecute = true
                    };
                    Process.Start(startInfo);
                    Environment.Exit(0);
                }
                else
                {
                    ConsoleOutput.Clear();
                }
            }
            else if(!Global.parameters.Contains("-keeplog"))
            {
                ConsoleOutput.Clear();
            }
            Global.randomSeed = Global.generator.globalRandom.Next();
            if(Global.parameters.Contains("-seed"))
            {
                int index = Global.parameters.IndexOf("-seed");
                if(index + 1 < Global.parameters.Count)
                {
                    if(int.TryParse(Global.parameters[index + 1], out int seed))
                    {
                        Global.randomSeed = seed;
                    }
                }
            }
            Global.generator.globalRandom = new Random(Global.randomSeed);
            SaveData.Load();
            if(Global.parameters.Contains("-v"))
            {
                SaveData.saveValues["HiddenVerbose"] = "true";
                SaveData.Save();
            }
            DisabledMedia.Load();
            if(Global.parameters.Count > 0)
                ConsoleOutput.WriteLine("Using command line parameters: " + String.Join(" ", Global.parameters.ToArray()));
#if DEBUG
            Debug.debugBuild = true;
            Debug.SetDebugMode(true);
#endif
            // Initialize Steam
            try
            {
                SteamManager.Initialize();
            }
            catch(Exception ex)
            {
                ConsoleOutput.WriteLine("SteamManager failed to initialize: " + ex.Message, Color.Red);
            }
            if(Global.parameters.Contains("-debug"))
            {
                Debug.SetDebugMode(true);
            }
            if(Global.parameters.Contains("-locale"))
            {
                int index = Global.parameters.IndexOf("-locale");
                if(index + 1 < Global.parameters.Count)
                {
                    SaveData.saveValues["Locale"] = Global.parameters[index + 1];
                }
            }
            // Fix locales that had their names changed
            if(SaveData.saveValues["Locale"] == "en_us")
                SaveData.saveValues["Locale"] = "english";
            else if(SaveData.saveValues["Locale"] == "es_mx")
                SaveData.saveValues["Locale"] = "latam";
            else if(SaveData.saveValues["Locale"] == "de_de")
                SaveData.saveValues["Locale"] = "german";
            // Get language that Steam reports
            bool languageSet = false;
            if(SaveData.saveValues["Locale"] != "fixme")
                languageSet = true;
            if(SteamManager.initialized && !languageSet)
            {
                string steamLang = SteamApps.GetCurrentGameLanguage();
                if(steamLang != null)
                {
                    SaveData.saveValues["Locale"] = steamLang;
                    languageSet = true;
                }
            }
            // Default to English
            if(!languageSet)
                SaveData.saveValues["Locale"] = "english";
            L.ReloadLocales();
            if(Global.parameters.Contains("-intro"))
                SaveData.saveValues["FirstBoot"] = "true";
            HolidayManager.CheckHolidays();
            if(Global.parameters.Contains("-holiday"))
            {
                int index = Global.parameters.IndexOf("-holiday");
                if(index + 1 < Global.parameters.Count)
                {
                    string holiday = Global.parameters[index + 1];
                    foreach(Holiday h in HolidayManager.Holidays)
                    {
                        if(h.InternalName == holiday.ToLower())
                        {
                            HolidayManager.SetHoliday(h);
                            break;
                        }
                    }
                }
            }
#if MONOGAME
            using (var game = new UserInterface())
                game.Run();
#else
            // windows forms
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
#endif
        }
    }
}
