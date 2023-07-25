using System;
using DiscordRPC;

namespace NonsensicalVideoGenerator
{
    public enum Tab
    {
        Generate,
        Library,
        Effects,
        Options,
        Game,
    }
    public static class DiscordRPC
    {
        public static DiscordRpcClient client;
        public static DateTime timestamp;
        public static string curstate;
        public static Tab prevtab;
        public static Tab curtab;
        public static Tab ToTab(string name)
        {
            switch(name)
            {
                case "Generate":
                    return Tab.Generate;
                case "Library":
                    return Tab.Library;
                case "Effects":
                    return Tab.Effects;
                case "Options":
                    return Tab.Options;
                case "Game":
                    return Tab.Game;
                default:
                    return Tab.Generate;
            }
        }
        public static void Initialize()
        {
            timestamp = DateTime.UtcNow;
            client = new DiscordRpcClient("1133301113219727460");
            client.Initialize();
        }
        public static void Update()
        {
            if(Global.generatorFactory.progressText != curstate)
            {
                curstate = Global.generatorFactory.progressText;
                UpdatePresence();
            }
            if(curtab != prevtab)
            {
                prevtab = curtab;
                UpdatePresence();
            }
            client.Invoke();
        }
        public static void UpdatePresence()
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
                    presence.Assets.SmallImageKey = "generate";
                    presence.Assets.SmallImageText = "Generate Tab";
                    break;
                case Tab.Library:
                    presence.Assets.SmallImageKey = "library";
                    presence.Assets.SmallImageText = "Library Tab";
                    break;
                case Tab.Effects:
                    presence.Assets.SmallImageKey = "effects";
                    presence.Assets.SmallImageText = "Effects Tab";
                    break;
                case Tab.Options:
                    presence.Assets.SmallImageKey = "options";
                    presence.Assets.SmallImageText = "Options Tab";
                    break;
                case Tab.Game:
                    presence.Assets.SmallImageKey = "game";
                    presence.Assets.SmallImageText = "Game Tab";
                    break;
            }
            client.SetPresence(presence);
        }
        public static void Shutdown()
        {
            client.Dispose();
        }
    }
}