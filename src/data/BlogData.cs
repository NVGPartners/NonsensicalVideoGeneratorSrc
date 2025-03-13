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
        public static string Title { get; set; } = "Milestone 7";
        public static List<string> Subtitle { get; set; } = new List<string>()
        {
            "v%s",
            "2025-03-13",
            "Major Update"
        };
        public static List<string> Description { get; set; } = new List<string>()
        {
            "- Videos now play more smoothly.",
            "- New KiwifruitDev startup video.",
            "- Updated default outro.",
            "- Added close and fullscreen buttons.",
            "- Steam rich presence support.",
            "- Updated credits and licenses.",
            "- Updated the localization files.",
            "View more information on Steam:"
        };
        public static string Url { get; set; } = "https://store.steampowered.com/news/app/2516360/view/587262054401835572";
    }
}
