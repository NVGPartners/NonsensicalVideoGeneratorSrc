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
        public static string Title { get; set; } = "Milestone 6 Hotfix 3";
        public static List<string> Subtitle { get; set; } = new List<string>()
        {
            "v%s",
            "October 13",
            "Small Update"
        };
        public static List<string> Description { get; set; } = new List<string>()
        {
            "- Added a printout when NVG exits.",
            "- Time zones now apply to holidays.",
            "- Workshop button moved to Addons tab.",
            "- Fixed tooltips in addon settings.",
            "- Skip Intro is now shown in Options.",
            "- Updated the localization files.",
            "See the full changelog on Steam.",
        };
        public static string Url { get; set; } = "https://store.steampowered.com/news/app/2516360/view/4536905158758868306";
    }
}
