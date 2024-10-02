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
        public static string Title { get; set; } = "Milestone 6 Hotfix 1";
        public static List<string> Subtitle { get; set; } = new List<string>()
        {
            "v%s",
            "October 1",
            "Major Update"
        };
        public static List<string> Description { get; set; } = new List<string>()
        {
            " ",
            " ",
            " ",
            " ",
            " ",
            " ",
            "See full changelog on Steam.",
        };
        public static string Url { get; set; } = "https://store.steampowered.com/news/app/2516360/view/4695656140431567186";
    }
}
