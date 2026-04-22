using System.Collections.Generic;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// Load blog/blog.xml from MGCB.
    /// </summary>
    public static class BlogData
    {
        public static readonly int MinorUpcoming = 8;
        public static readonly int MajorUpcoming = 3;
        public static readonly int Month = 4;
        public static readonly int Day = 21;
        public static readonly int Year = 2026;
        public static readonly int Tier = 1; // 0: Upcoming, 1: Minor Update, 2: Major Update
        public static readonly string PostId = "685251502317503745"; // Fill in with update post ID before publishing the build
        public static List<string> Subtitle { get; set; } = new List<string>()
        {
            "v%version%",
            "%year%-%month%-%day%",
            "%tier%"
        };
        public static List<string> Description { get; set; } = new List<string>()
        {
            // "WWWWWWWWWWWWWWWWWWWWWWWWWWWW"
            "- Updated dependencies",
            "- Add Deno to fix silent yt-dlp",
            "- Fixed yt-dlp directory bug",
            "- Added addon ability for",
            "several random library files",
            " ",
            " ",
            "%footer%"
        };
        public static string Url { get; set; } = "steam://openurl/https://store.steampowered.com/news/app/2516360/view/%postid%?l=%locale%";
    }
}
