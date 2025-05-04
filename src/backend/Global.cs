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
        public static string productName = "Nonsensical Video Generator";
        public static string productNameShort = "NVG";
        public static string productVersion = "0.0.0.0DX";
#if DESKTOPGL
        public static string productSku = "GL";
#else
        public static string productSku = "DX";
#endif
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
        public static bool tooltipIsCycler = false;
        public static bool imageLibraryAvailable = false;
        public static bool imageLibraryAvailableInternal = true;
        public static int randomSeed = 0;
        public static int waitReady = int.MaxValue;
        public static bool selectLanguage = false;
        public static bool disableConsents = true;
        public static readonly int currentYear = DateTime.UtcNow.Year;
        
        // Aspect ratio functions
        public static (int, int) ConvertToFraction(double aspectRatio, double tolerance = 0.01)
        {
            int numerator = 1;
            int denominator = 1;

            while (Math.Abs((double)numerator / denominator - aspectRatio) > tolerance)
            {
                if ((double)numerator / denominator < aspectRatio)
                    numerator++;
                else
                    denominator++;
            }

            // Simplify the fraction
            int gcd = GCD(numerator, denominator);
            numerator /= gcd;
            denominator /= gcd;

            return (numerator, denominator);
        }
        public static int GCD(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }
    }
}
