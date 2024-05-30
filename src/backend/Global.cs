using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Steamworks;
using Microsoft.Xna.Framework;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// This class stores useful global variables and functions.
    /// </summary>
    public static class Global
    {
        public static Assembly assembly = Assembly.GetExecutingAssembly();
        public static FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
        public static string? productName = fileVersionInfo.FileDescription;
        public static string productNameShort = "NVG";
        public static string? productVersion = fileVersionInfo.ProductVersion;
#if MONOGAME
        public static Mask mask = new();
#endif
        public static bool justCompletedRender = true;
        public static bool exiting = false;
        public static bool fakeExit = false;
        public static float exitOpacityIncrease = 0.0075f;
        public static Func<bool> exitFunc = () => false;
        public static bool shuffled = false;
        public static bool pluginsLoaded = true;
        public static bool dragDrop = false;
        public static bool ready = false;
        public static bool canAchieve = false;
        public static double readyTime = 0;
        public static bool canRender = false;
        public static List<string> dragDropFiles = new();
        public static bool useSystemFFmpeg = false;
        public static bool useSystemFFprobe = false;
        public static bool useSystemMagick = false;
        public static bool useSystemYtDlp = false;
        public static bool useSystemFrei0r = false;
        public static string editing = "";
        public static AppId_t appId = new(2516360);
        public static string videoTitle = "Render1";
        public static bool rolledForOverlay = false;
        public static bool usedWorkshopPlugin = false;
        public static bool usedAllEffectChance = false;
        public static bool usedDifferentOutro = false;
        public static bool highScore50 = false;
        public static Generator generator = new Generator();
        public static List<string> parameters = new List<string>();
        public static string tooltip = "";
        public static bool imageLibraryAvailable = false;
        public static bool imageLibraryAvailableInternal = true;
        public static int randomSeed = 0;
        public static Vector2 drawOffset = new(0, 0);
        public static int waitReady = int.MaxValue;
    }
}
