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
        public static string Title { get; set; } = "Milestone 6 Hotfix 8";
        public static List<string> Subtitle { get; set; } = new List<string>()
        {
            "v%s",
            "December 19",
            "Small Update"
        };
        public static List<string> Description { get; set; } = new List<string>()
        {
            "- Updated bundled yt-dlp to 2024.12.13.",
            "- Updated FFmpeg requirement to 7.1.",
            "- Added a frei0r log to console.txt.",
            "- Added a launch option to open console.txt.",
            "- Updated the Steam Workshop page.",
            "- Updated the Steam Store page.",
            "See the full changelog on Steam.",
        };
        public static string Url { get; set; } = "https://store.steampowered.com/news/app/2516360/view/499434332763455513";
    }
}
