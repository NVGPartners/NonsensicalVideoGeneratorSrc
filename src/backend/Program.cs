using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
#if !MONOGAME
using System.Windows.Forms;
#endif

namespace NonsensicalVideoGenerator
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Global.randomSeed = Global.generator.globalRandom.Next();
            Global.generator.globalRandom = new Random(Global.randomSeed);
            ConsoleOutput.Clear();
            SaveData.Load();
            DisabledMedia.Load();
            for(int i = 0; i < args.Length; i++)
            {
                Global.parameters.Add(args[i]);
            }
            if(Global.parameters.Count > 0)
                ConsoleOutput.WriteLine("Using command line parameters: " + String.Join(" ", Global.parameters.ToArray()));
#if DEBUG
            Debug.debugBuild = true;
            Debug.SetDebugMode(true);
#endif
            string locale = SaveData.saveValues["Locale"];
            if(Global.parameters.Contains("-lang"))
            {
                int index = Global.parameters.IndexOf("-lang");
                if(index + 1 < Global.parameters.Count)
                {
                    locale = Global.parameters[index + 1];
                }
            }
            L.LoadLocale(locale);
            if(Global.parameters.Contains("-intro"))
                SaveData.saveValues["FirstBoot"] = "true";
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
