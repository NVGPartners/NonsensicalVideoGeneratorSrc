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
        public static readonly int Month = 5;
        public static readonly int Day = 3;
        public static readonly int Year = 2025;
        public static readonly int Tier = 1;
        public static readonly string PostId = "499446213320377116";
        public static List<string> Subtitle { get; set; } = new List<string>()
        {
            "v%version%",
            "%month%-%day%-%year%",
            "%tier%"
        };
        public static List<string> Description { get; set; } = new List<string>()
        {
            // "WWWWWWWWWWWWWWWWWWWWWWWWWWWW"
            "- ",
            "- ",
            "- ",
            "- ",
            "- ",
            "- ",
            "- ",
            "%footer%"
        };
        public static string Url { get; set; } = "https://store.steampowered.com/news/app/2516360/view/%postid%?l=%locale%";
    }
}
