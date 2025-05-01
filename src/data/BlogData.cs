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
        public static string Title { get; set; } = "April 2025: Update v7.1";
        public static List<string> Subtitle { get; set; } = new List<string>()
        {
            "v%s",
            "2025-04-30",
            "Minor Update"
        };
        public static List<string> Description { get; set; } = new List<string>()
        {
            // "WWWWWWWWWWWWWWWWWWWWWWWWWWWW"
            "- Updated the credits",
            "- Changed update version identifiers",
            "- Fixed cache video clearing bug",
            "- Fixed media volume ignored bug",
            "- LanaPixel font restored for locales",
            "- Added French language (darkyuuu_)",
            "- Added Slovak language (peto444)",
            "View more information on Steam:"
        };
        public static string Url { get; set; } = "https://store.steampowered.com/news/app/2516360/view/499446213320376534";
    }
}
