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
        public static string Title { get; set; } = "Milestone 6 Hotfix 9";
        public static List<string> Subtitle { get; set; } = new List<string>()
        {
            "v%s",
            "2024-12-24",
            "Small Update"
        };
        public static List<string> Description { get; set; } = new List<string>()
        {
            "- New window toggles in settings.",
            "- New custom cursor by default.",
            "- Added ziahorizon to the credits.",
            "- Addons reload if the locale changes.",
            "- Attempt to fix Proton-related issues.",
            "- Changed date format in the blog tab.",
            "- Fixed addon \"View Workshop\" button.",
            "- Fixed various cosmetic issues.",
        };
        public static string Url { get; set; } = "https://store.steampowered.com/news/app/2516360/view/499434332763457024";
    }
}
