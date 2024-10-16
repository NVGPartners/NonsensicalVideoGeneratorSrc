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
        public static string Title { get; set; } = "Milestone 6 Hotfix 4";
        public static List<string> Subtitle { get; set; } = new List<string>()
        {
            "v%s",
            "October 15",
            "Small Update"
        };
        public static List<string> Description { get; set; } = new List<string>()
        {
            "- Added a save print when rendering.",
            "- Updated yt-dlp to 2024.10.07.",
            "- Fixed bug: -seed option now persists.",
            "- Fixed bug: Videos w/o audio will play.",
            "- Fixed crash from Library thumbnails.",
            " ",
            "See the full changelog on Steam.",
        };
        public static string Url { get; set; } = "https://store.steampowered.com/news/app/2516360/view/4529024222444677670";
    }
}
