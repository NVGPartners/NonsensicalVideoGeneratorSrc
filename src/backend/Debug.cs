using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace NonsensicalVideoGenerator
{
    public static class Debug
    {
        public static bool debugModePermanent = false;
        public static bool debugBuild = false;
        private static bool debugMode = false;
        private static Color debugColor = new Color(255, 128, 64, 255);
        public static bool gameCheat = false;
        public static bool paused = false;
        public static bool frame = false;
        public static bool debugSpeedDebounce = false;
        public static int debugSpeedBoost = 2;
        public static void SetDebugMode(bool enable)
        {
            if(!debugModePermanent)
            {
                if(debugBuild)
                {
                    ConsoleOutput.WriteLine("Debug Build (!!!)", debugColor);
                    Global.productVersion = Global.fileVersionInfo.ProductVersion+" (DEBUG)";
                }
                else
                {
                    ConsoleOutput.WriteLine("Release Debug Mode (!!!)", debugColor);
                    Global.productVersion = Global.fileVersionInfo.ProductVersion+" (RELEASE; DEBUG MODE)";
                }
            }
            debugModePermanent = true;
            debugMode = enable;
        }
        public static bool GetDebugMode()
        {
            return debugMode;
        }
        public static void Log(string message)
        {
            if(debugMode)
                ConsoleOutput.WriteLine(message, debugColor);
        }
    }
}
