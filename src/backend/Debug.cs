using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace NonsensicalVideoGenerator
{
    public static class Debug
    {
        private static bool debugMode = false;
        private static Color debugColor = new Color(255, 128, 64, 255);
        public static void SetDebugMode(bool enable)
        {
            debugMode = true;
            Log("Debug mode set to " + enable);
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
