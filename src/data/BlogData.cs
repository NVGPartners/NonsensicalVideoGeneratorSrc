using System.Collections.Generic;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// Load blog/blog.xml from MGCB.
    /// </summary>
    public static class BlogData
    {
        public static readonly int Month = 5;
        public static readonly int Day = 8;
        public static readonly int Year = 2025;
        public static readonly int Tier = 2;
        public static readonly string PostId = "499446847117461011";
        public static List<string> Subtitle { get; set; } = new List<string>()
        {
            "v%version%",
            "%day%-%month%-%year%",
            "%tier%"
        };
        public static List<string> Description { get; set; } = new List<string>()
        {
            // "WWWWWWWWWWWWWWWWWWWWWWWWWWWW"
            "- Introduced Boot Movies addon type,",
            "- Added KiwifruitDev logo stock addon.",
            "- Added PNG, GIF Workshop icon support.",
            "- Steam URLs now open directly in client.",
            "- Fixed outros being replaced by default.",
            "- Various bug fixes and improvements.",
            "- Updated the localization files.",
            "%footer%"
        };
        public static string Url { get; set; } = "steam://openurl/https://store.steampowered.com/news/app/2516360/view/%postid%?l=%locale%";
    }
}
