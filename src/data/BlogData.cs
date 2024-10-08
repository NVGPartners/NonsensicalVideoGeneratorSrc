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
        public static string Title { get; set; } = "Milestone 6 Hotfix 2";
        public static List<string> Subtitle { get; set; } = new List<string>()
        {
            "v%s",
            "October 7",
            "Small Update"
        };
        public static List<string> Description { get; set; } = new List<string>()
        {
            "- Added external media player option.",
            "- Various holiday changes.",
            "- Updated the default addon templates.",
            "- Fixed frei0r not being detected.",
            "- Removed glow from vocoder effect.",
            "- Updated the localization files.",
            "See the full changelog on Steam.",
        };
        public static string Url { get; set; } = "https://store.steampowered.com/news/app/2516360/view/4695656140453239013";
    }
}
