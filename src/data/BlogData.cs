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
        public static string Title { get; set; } = "Milestone 5";
        public static string LastVersion { get; set; } = "1.0.1.0";
        public static List<string> Subtitle { get; set; } = new List<string>()
        {
            "v1.0.1.0",
            "May 22 2024",
        };
        public static List<string> Description { get; set; } = new List<string>()
        {
            "The largest Nonsensical Video",
            "Generator update is here!",
            " ",
            "Quality of life improvements,",
            "localization, new features,",
            "and more!",
        };
        public static string Url { get; set; } = "https://steamcommunity.com/games/2516360/announcements/detail/6215608421537406607";
    }
}
