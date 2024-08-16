using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// Load blog/blog.xml from MGCB.
    /// </summary>
    public static class BlogData
    {
        public static string Title { get; set; } = "Milestone 5 Hotfix 3";
        public static string LastVersion { get; set; } = "1.0.1.4";
        public static List<string> Subtitle { get; set; } = new List<string>()
        {
            "v1.0.1.4",
            "August 16 2024",
            "Small Update"
        };
        public static List<string> Description { get; set; } = new List<string>()
        {
            "- Disabled addon consent forms.",
            "- Addons can download from YouTube.",
            "- Added new action buttons.",
            "- Updated yt-dlp.",
            "- Updated the localization files.",
            " ",
            "See full changelog on Steam.",
        };
        public static string Url { get; set; } = "https://store.steampowered.com/news/app/2516360/view/6692998230485958046";
    }
}
