using System.Collections.Generic;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// Load blog/blog.xml from MGCB.
    /// </summary>
    public static class BlogData
    {
        public static readonly int Month = 5;
        public static readonly int Day = 14;
        public static readonly int Year = 2025;
        public static readonly int Tier = 1;
        public static readonly string PostId = "532098578832687584";
        public static List<string> Subtitle { get; set; } = new List<string>()
        {
            "v%version%",
            "%year%-%day%-%month%",
            "%tier%"
        };
        public static List<string> Description { get; set; } = new List<string>()
        {
            // "WWWWWWWWWWWWWWWWWWWWWWWWWWWW"
            "- Fixed boot movies playback duration.",
            "- Fixed music not playing immediately.",
            "- Fixed long pause in KiwifruitDev logo.",
            "- Fixed enable/disable all w/boot movies.",
            "- Restored Workshop uploading statuses.",
            "- Changed Addons tab entry buttons.",
            "- Updated the localization files.",
            "%footer%"
        };
        public static string Url { get; set; } = "steam://openurl/https://store.steampowered.com/news/app/2516360/view/%postid%?l=%locale%";
    }
}
