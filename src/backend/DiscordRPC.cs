using System;
using System.Drawing;
using DiscordRPC;

namespace NonsensicalVideoGenerator
{
    public enum Tab
    {
        Generate,
        Library,
        Addons,
        Options,
        Locales,
        Blog,
        Game,
    }
    public static class DiscordRPC
    {
        private static DiscordRpcClient? client;
        public static DateTime timestamp;
        private static string? curstate;
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
        public static void Initialize()
        {
            try
            {
                timestamp = DateTime.UtcNow;
                client = new DiscordRpcClient("1133301113219727460");
                client.Initialize();
            }
            catch(Exception e)
            {
                ConsoleOutput.WriteLine("Discord RPC error: " + e.Message, Microsoft.Xna.Framework.Color.Red);
            }
        }
        public static void Update()
        {
            try
            {
                if(client == null)
                    return;
                if(Global.generator.progressText != curstate)
                {
                    curstate = Global.generator.progressText;
                    UpdatePresence();
                }
                if(curtab != prevtab)
                {
                    prevtab = curtab;
                    UpdatePresence();
                }
                client.Invoke();
            }
            catch(Exception e)
            {
                ConsoleOutput.WriteLine("Discord RPC error: " + e.Message, Microsoft.Xna.Framework.Color.Red);
            }
        }
        public static void UpdatePresence()
        {
            try
            {
                if(client == null)
                    return;
                RichPresence presence = new RichPresence()
                {
                    Details = Global.videoTitle + ".mp4",
                    State = curstate,
                    Assets = new Assets()
                    {
                        LargeImageKey = "icon",
                        LargeImageText = "v" + Global.productVersion,
                    },
                    Timestamps = new Timestamps(timestamp),
                };
                switch(curtab)
                {
                    case Tab.Generate:
                        presence.Assets.SmallImageKey = "generate";
                        presence.Assets.SmallImageText = "Generate Tab";
                        break;
                    case Tab.Library:
                        presence.Assets.SmallImageKey = "library";
                        presence.Assets.SmallImageText = "Library Tab";
                        break;
                    case Tab.Addons:
                        presence.Assets.SmallImageKey = "effects";
                        presence.Assets.SmallImageText = "Addons Tab";
                        break;
                    case Tab.Options:
                        presence.Assets.SmallImageKey = "options";
                        presence.Assets.SmallImageText = "Options Tab";
                        break;
                    case Tab.Locales:
                        presence.Assets.SmallImageKey = "locales";
                        presence.Assets.SmallImageText = "Locales Tab";
                        break;
                    case Tab.Blog:
                        presence.Assets.SmallImageKey = "blog";
                        presence.Assets.SmallImageText = "Blog Tab";
                        break;
                    case Tab.Game:
                        presence.Assets.SmallImageKey = "game";
                        presence.Assets.SmallImageText = "Game Tab";
                        break;
                }
                client.SetPresence(presence);
            }
            catch(Exception e)
            {
                ConsoleOutput.WriteLine("Discord RPC error: " + e.Message, Microsoft.Xna.Framework.Color.Red);
            }
        }
        public static void Shutdown()
        {
            try
            {
                if(client == null)
                    return;
                client.Dispose();
            }
            catch(Exception e)
            {
                ConsoleOutput.WriteLine("Discord RPC error: " + e.Message, Microsoft.Xna.Framework.Color.Red);
            }
        }
    }
}