using Microsoft.Xna.Framework;

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
        public static int lastPage = 0;
        public static void SetDebugMode(bool enable)
        {
            if(!debugModePermanent)
            {
                if(debugBuild)
                {
                    ConsoleOutput.WriteLine("Debug Build (!!!)", debugColor);
                    Global.productVersion = Global.productVersion+" (DEBUG)";
                }
                else
                {
                    ConsoleOutput.WriteLine("Release Debug Mode (!!!)", debugColor);
                    Global.productVersion = Global.productVersion+" (RELEASE; DEBUG MODE)";
                }
                if(UserInterface.instance != null)
                    UserInterface.instance.Window.Title = Global.productName+" v"+Global.productVersion;
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
        public static void ShowDebugMenu()
        {
            if(debugMode)
            {
                if(Pagination.SelectedPage != Pagination.TopPageCount)
                    lastPage = Pagination.SelectedPage;
                GlobalContent.PlaySound("Select");
                Pagination.SetPage(Pagination.TopPageCount);
            }
        }
        public static void HideDebugMenu()
        {
            if(debugMode)
            {
                GlobalContent.PlaySound("Back");
                Pagination.SetPage(lastPage);
            }
        }
        public static void ToggleDebugMenu()
        {
            if(debugMode)
            {
                if(Pagination.SelectedPage != Pagination.TopPageCount)
                    ShowDebugMenu();
                else
                    HideDebugMenu();
            }
        }
    }
}
