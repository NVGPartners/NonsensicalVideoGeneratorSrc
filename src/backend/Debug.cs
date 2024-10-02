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
        public static int lastPage = 0;
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
                if(UserInterface.instance != null)
                    UserInterface.instance.Window.Title = Global.productName+" "+Global.productVersion;
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
                GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                Pagination.SetPage(Pagination.TopPageCount);
                ScreenManager.PushNavigation("Content");
                ScreenManager.GetScreen<ContentScreen>("Content")?.Show();
                ScreenManager.GetScreen<ContentScreen>("Content").offset = new Vector2(0, 0);
                ScreenManager.GetScreen<VideoScreen>("Video")?.Hide();
                ScreenManager.GetScreen<VideoScreen>("Video").offset = new Vector2(GlobalGraphics.Scale(-124), 0);
                ScreenManager.GetScreen<SocialScreen>("Socials")?.Hide();
                ScreenManager.GetScreen<SocialScreen>("Socials").offset = new Vector2(0, GlobalGraphics.Scale(240));
                ScreenManager.GetScreen<MenuScreen>("Menu")?.Hide();
                ScreenManager.GetScreen<MenuScreen>("Menu").offset = new Vector2(GlobalGraphics.Scale(-124), 0);
            }
        }
        public static void HideDebugMenu()
        {
            if(debugMode)
            {
                GlobalContent.GetSound("Back").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                Pagination.SetPage(lastPage);
                ScreenManager.PushNavigation("Content");
                ScreenManager.GetScreen<ContentScreen>("Content").Show();
                ScreenManager.GetScreen<ContentScreen>("Content").offset = new Vector2(0, 0);
                ScreenManager.GetScreen<VideoScreen>("Video")?.Show();
                ScreenManager.GetScreen<VideoScreen>("Video").offset = new Vector2(0, 0);
                ScreenManager.PushNavigation("Socials");
                ScreenManager.GetScreen<SocialScreen>("Socials")?.Show();
                ScreenManager.GetScreen<SocialScreen>("Socials").offset = new Vector2(0, 0);
                ScreenManager.PushNavigation("Menu");
                ScreenManager.GetScreen<MenuScreen>("Menu")?.Show();
                ScreenManager.GetScreen<MenuScreen>("Menu").offset = new Vector2(0, 0);
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
