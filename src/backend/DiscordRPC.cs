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
        private static string curstate = "";
        public static string state = "";
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
            if(client == null)
                return;
            try
            {
                if(state != curstate)
                {
                    curstate = state;
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
            if(client == null)
                return;
            try
            {
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
                        if(Global.generator.generatorActive)
                        {
                            presence.Assets.SmallImageKey = "rendering";
                            presence.Assets.SmallImageText = L.T(0, "DiscordRPC:Tab", L.T(0, "Rendering:Title"));
                            break;
                        }
                        presence.Assets.SmallImageKey = "generate";
                        presence.Assets.SmallImageText = L.T(0, "DiscordRPC:Tab", L.T(0, "Generate:Title"));
                        break;
                    case Tab.Library:
                        presence.Assets.SmallImageKey = "library";
                        presence.Assets.SmallImageText = L.T(0, "DiscordRPC:Tab", L.T(0, "Library:Title"));
                        break;
                    case Tab.Addons:
                        presence.Assets.SmallImageKey = "effects";
                        presence.Assets.SmallImageText = L.T(0, "DiscordRPC:Tab", L.T(0, "Addons:Title"));
                        break;
                    case Tab.Options:
                        presence.Assets.SmallImageKey = "options";
                        presence.Assets.SmallImageText = L.T(0, "DiscordRPC:Tab", L.T(0, "Options:Title"));
                        break;
                    case Tab.Locales:
                        presence.Assets.SmallImageKey = "locales";
                        presence.Assets.SmallImageText = L.T(0, "DiscordRPC:Tab", L.T(0, "Locales:Title"));
                        break;
                    case Tab.Blog:
                        presence.Assets.SmallImageKey = "blog";
                        presence.Assets.SmallImageText = L.T(0, "DiscordRPC:Tab", L.T(0, "Blog:Title"));
                        break;
                    case Tab.Game:
                        presence.Assets.SmallImageKey = "game";
                        presence.Assets.SmallImageText = L.T(0, "DiscordRPC:Tab", L.T(0, "Game:Title"));
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
            if(client == null)
                return;
            try
            {
                client.Dispose();
            }
            catch(Exception e)
            {
                ConsoleOutput.WriteLine("Discord RPC error: " + e.Message, Microsoft.Xna.Framework.Color.Red);
            }
        }
    }
}