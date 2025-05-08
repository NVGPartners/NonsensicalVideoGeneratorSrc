using System;
using Steamworks;

namespace NonsensicalVideoGenerator
{
    public static class SteamRichPresence
    {
        public static Tab prevtab = Tab.Generate;
        public static Tab curtab = Tab.Generate;
        public static Tab ToTab(string name)
        {
            switch(name)
            {
                case "PageGenerate":
                    return Tab.Generate;
                case "PageLibrary":
                    return Tab.Library;
                case "PageAddons":
                    return Tab.Addons;
                case "PageOptions":
                    return Tab.Options;
                case "PageLocales":
                    return Tab.Locales;
                case "PageGame":
                    return Tab.Game;
                case "PageBlog":
                    return Tab.Blog;
                default:
                    return Tab.Generate;
            }
        }
        public static void Update()
        {
            if(!SteamManager.initialized)
                return;
            try
            {
                if(curtab != prevtab)
                {
                    prevtab = curtab;
                    UpdatePresence();
                }
            }
            catch(Exception e)
            {
                ConsoleOutput.WriteLine("Steam rich presence error: " + e.Message, Microsoft.Xna.Framework.Color.Red);
            }
        }
        public static void UpdatePresence()
        {
            if(!SteamManager.initialized)
                return;
            SteamFriends.SetRichPresence("render", Global.videoTitle + ".mp4");
            SteamFriends.SetRichPresence("tab", curtab.ToString());
            SteamFriends.SetRichPresence("steam_display", "#Status");
        }
    }
}
