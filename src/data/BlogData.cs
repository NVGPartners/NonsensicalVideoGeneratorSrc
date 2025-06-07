using System.Collections.Generic;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// Load blog/blog.xml from MGCB.
    /// </summary>
    public static class BlogData
    {
        public static readonly int Month = 6;
        public static readonly int Day = 1;
        public static readonly int Year = 2025;
        public static readonly int Tier = 1;
        public static readonly string PostId = "654821763669034264";
        public static List<string> Subtitle { get; set; } = new List<string>()
        {
            "v%version%",
            "%year%-%month%-%day%",
            "%tier%"
        };
        public static List<string> Description { get; set; } = new List<string>()
        {
            // "WWWWWWWWWWWWWWWWWWWWWWWWWWWW"
            "- Blog date format uses ISO 8601 format.",
            "%footer%"
        };
        public static string Url { get; set; } = "steam://openurl/https://store.steampowered.com/news/app/2516360/view/%postid%?l=%locale%";
    }
}
