using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// This class stores useful global variables and functions.
    /// </summary>
    public static class Global
    {
        public static Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
        public static FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
        public static string? productName = fileVersionInfo.ProductName;
        public static string? productVersion = fileVersionInfo.ProductVersion;
        public static Mask mask = new();
        public static bool justCompletedRender = true;
        public static bool exiting = false;
        public static bool shuffled = false;
        public static bool pluginsLoaded = true;
        public static bool dragDrop = false;
        public static bool ready = false;
        public static double readyTime = 0;
        public static bool canRender = false;
        public static List<string> dragDropFiles = new();
        public static bool useSystemFFmpeg = false;
        public static bool useSystemFFprobe = false;
        public static bool useSystemYtDlp = false;
        public static string editing = "";
        public static string appId = "2516360";
        public static GeneratorFactory generatorFactory = new GeneratorFactory();
    }
}