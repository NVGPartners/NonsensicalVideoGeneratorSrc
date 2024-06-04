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
        public static string Title { get; set; } = "Milestone 5 Hotfix 1";
        public static string LastVersion { get; set; } = "1.0.1.1";
        public static List<string> Subtitle { get; set; } = new List<string>()
        {
            "v1.0.1.1",
            "June 4 2024",
            "Small Update"
        };
        public static List<string> Description { get; set; } = new List<string>()
        {
            "- Addon changes and fixes.",
            "- Added and moved action buttons.",
            "- Updated the localization files.",
            "- Disabled 2 music tracks for now.",
            "- Other minor changes.",
            " ",
            "See full changelog on Steam.",
        };
        public static string Url { get; set; } = "https://store.steampowered.com/news/app/2516360/view/4192367467637706465";
    }
}
