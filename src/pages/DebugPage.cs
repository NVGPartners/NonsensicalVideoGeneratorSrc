using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Globalization;
using System;
using System.Linq;
using System.Collections.Generic;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// Debug page.
    /// </summary>
    public class DebugPage : IPage
    {
        public string Name { get; set; } = "PageDebug";
        public string Tooltip { get; } = "";
        private readonly InteractableController controller = new();
        public bool Update(GameTime gameTime, bool handleInput)
        {
            if(Debug.GetDebugMode())
            {
                // Interactable
                if(controller.Update(gameTime, handleInput))
                    return true;
                if(handleInput)
                {
                    if(MouseInput.LastMouseState.LeftButton == ButtonState.Released && MouseInput.MouseState.LeftButton == ButtonState.Pressed
                        && MouseInput.MouseState.X >= GlobalGraphics.Scale(137) && MouseInput.MouseState.X <= GlobalGraphics.Scale(303))
                    {
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(58) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(58+6))
                        {
                            // Search for the next logical entry in locales/*.json
                            string currentLocale = L.GetLocale().name;
                            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", L.localeFolder);
                            if(!File.Exists(Path.Combine(path, currentLocale + ".json")))
                                currentLocale = L.defaultLocale;
                            string[] files = Directory.GetFiles(path, "*.json");
                            string newLocale = currentLocale;
                            for(int i = 0; i < files.Length; i++)
                            {
                                if(files[i].EndsWith(currentLocale + ".json"))
                                {
                                    // Get the next logical entry.
                                    if(i + 1 < files.Length)
                                    {
                                        newLocale = Path.GetFileNameWithoutExtension(files[i + 1]);
                                        break;
                                    }
                                    // Wrap around to the first entry.
                                    newLocale = Path.GetFileNameWithoutExtension(files[0]);
                                    break;
                                }
                            }
                            newLocale = newLocale.Replace(".json", "");
                            L.LoadLocale(newLocale);
                            GlobalContent.PlaySound("Select");
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(58+9) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(58+9+6))
                        {
                            // Set random seed to 0.
                            Global.randomSeed = 0;
                            Global.generator.globalRandom = new Random(0);
                            GlobalContent.PlaySound("Select");
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(58+(9*2)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(58+(9*2)+6))
                        {
                            // Toggle user resizing.
                            if(UserInterface.instance != null)
                                UserInterface.instance.Window.AllowUserResizing = !UserInterface.instance.Window.AllowUserResizing;
                            GlobalContent.PlaySound("Select");
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(58+(9*3)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(58+(9*3)+6))
                        {
                            // Toggle game cheat.
                            Debug.gameCheat = !Debug.gameCheat;
                            GlobalContent.PlaySound("Select");
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(58+(9*4)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(58+(9*4)+6))
                        {
                            // Toggle screen scale.
                            if(SaveData.saveValues["ScreenScale"] == "1")
                                SaveData.saveValues["ScreenScale"] = "2";
                            else if(SaveData.saveValues["ScreenScale"] == "2")
                                SaveData.saveValues["ScreenScale"] = "3";
                            else if(SaveData.saveValues["ScreenScale"] == "3")
                                SaveData.saveValues["ScreenScale"] = "4";
                            else if(SaveData.saveValues["ScreenScale"] == "4")
                                SaveData.saveValues["ScreenScale"] = "1";
                            GlobalContent.PlaySound("Select");
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(58+(9*5)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(58+(9*5)+6))
                        {
                            // Toggle console.
                            ConsoleScreen? consoleScreen = ScreenManager.GetScreen<ConsoleScreen>("Console");
                            if(consoleScreen != null)
                            {
                                ConsoleOutput.ResetScroll();
                                if(consoleScreen.offset.X == 0)
                                {
                                    consoleScreen.screenType = ScreenType.Drawn;
                                    consoleScreen.offset = new Vector2(GlobalGraphics.scaledWidth, 0);
                                    if(UserInterface.instance != null)
                                        UserInterface.instance.Resize(GlobalGraphics.scaledWidth*2, GlobalGraphics.scaledHeight);
                                }
                                else
                                {
                                    consoleScreen.screenType = ScreenType.Hidden;
                                    consoleScreen.offset = new Vector2(0, GlobalGraphics.scaledHeight);
                                    if(UserInterface.instance != null)
                                        UserInterface.instance.Resize(GlobalGraphics.scaledWidth, GlobalGraphics.scaledHeight);
                                }
                                GlobalContent.PlaySound("Select");
                            }
                            else
                            {
                                GlobalContent.PlaySound("Error");
                            }
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(58+(9*6)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(58+(9*6)+6))
                        {
                            // Toggle speed boost.
                            Debug.debugSpeedBoost++;
                            if(Debug.debugSpeedBoost > 10)
                                Debug.debugSpeedBoost = 2;
                            GlobalContent.PlaySound("Select");
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(58+(9*7)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(58+(9*7)+6))
                        {
                            // Toggle draw offset.
                            AspectRatio current = GlobalGraphics.GetAspectRatio();
                            // Get the index of the current AspectRatio
                            int index = AspectRatio.All.IndexOf(current);
                            // Get the next AspectRatio
                            AspectRatio next = AspectRatio.All[(index + 1) % AspectRatio.All.Count];
                            // Set the next AspectRatio
                            GlobalGraphics.SetAspectRatio(next);
                            GlobalContent.PlaySound("Select");
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(58+(9*8)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(58+(9*8)+6))
                        {
                            // Toggle export params.
                            if(Generator.exportParams.StartsWith("-vcodec"))
                                Generator.exportParams = Generator.oldExportParams;
                            else
                                Generator.exportParams = Generator.betterExportParams;
                            GlobalContent.PlaySound("Select");
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(58+(9*9)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(58+(9*9)+6))
                        {
                            // Cycle music.
                            if(UserInterface.instance != null)
                                UserInterface.instance.FindMusic();
                            GlobalContent.PlaySound("Select");
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(58+(9*10)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(58+(9*10)+6))
                        {
                            // Cycle theme.
                            GlobalContent.PlaySound("Select");
                            string name = ThemeManager.activeTheme.name;
                            SaveData.saveValues["ActiveTheme"] = "";
                            // Check to see if it's an internal theme.
                            for(int i = 0; i < DefaultThemes.themes.Count(); i++)
                            {
                                if(name == DefaultThemes.themes[i].name)
                                {
                                    if(i + 1 < DefaultThemes.themes.Count())
                                    {
                                        ThemeManager.ApplyTheme(DefaultThemes.themes[i + 1]);
                                        return true;
                                    }
                                    name = "";
                                    break;
                                }
                            }
                            // Otherwise, check to see if it's a plugin theme.
                            List<Plugin> themes = PluginHandler.GetPluginsOfType(AddonType.Theme);
                            if(themes.Count == 0)
                            {
                                ThemeManager.ApplyTheme(DefaultThemes.themes[0]);
                                return true;
                            }
                            for(int i = 0; i < themes.Count; i++)
                            {
                                if(name == themes[i].GetDisplayName() || name == "")
                                {
                                    if(i + 1 < themes.Count || name == "")
                                    {
                                        SaveData.saveValues["ActiveTheme"] = Path.GetFileName(themes[name == "" ? i : i + 1].path);
                                        name = themes[name == "" ? i : i + 1].GetDisplayName();
                                        break;
                                    }
                                    SaveData.saveValues["ActiveTheme"] = "";
                                    name = DefaultThemes.themes[0].name;
                                    break;
                                }
                            }
                            for(int i = 0; i < PluginHandler.GetPluginCount(); i++)
                            {
                                if(name == PluginHandler.plugins[i].GetDisplayName())
                                {
                                    PluginHandler.plugins[i].enabled = true;
                                    break;
                                }
                            }
                            ThemeManager.LoadThemes();
                            ThemeManager.themes.ForEach((Theme theme) => {
                                if(theme.name == name)
                                {
                                    ThemeManager.ApplyTheme(theme);
                                }
                            });
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(58+(9*11)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(58+(9*11)+6))
                        {
                            // Save.
                            SaveData.Save();
                            GlobalContent.PlaySound("Select");
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(58+(9*12)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(58+(9*12)+6))
                        {
                            // Cycle holiday.
                            GlobalContent.PlaySound("Select");
                            if(HolidayManager.CurrentHoliday == null)
                            {
                                // set the first holiday
                                Holiday holiday = HolidayManager.Holidays[0];
                                ThemeManager.ApplyTheme(holiday.Theme);
                                HolidayManager.SetHoliday(holiday);
                                return true;
                            }
                            for(int i = 0; i < HolidayManager.Holidays.Count; i++)
                            {
                                if(HolidayManager.CurrentHoliday.InternalName == HolidayManager.Holidays[i].InternalName)
                                {
                                    if(i + 1 < HolidayManager.Holidays.Count)
                                    {
                                        if(HolidayManager.Holidays[i + 1].Theme != null)
                                            ThemeManager.ApplyTheme(HolidayManager.Holidays[i + 1].Theme);
                                        HolidayManager.SetHoliday(HolidayManager.Holidays[i + 1]);
                                        return true;
                                    }
                                    ThemeManager.ApplyTheme(DefaultThemes.Nonsensical);
                                    HolidayManager.SetHoliday(null);
                                    return true;
                                }
                            }
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(58+(9*13)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(58+(9*13)+6))
                        {
                            // Toggle fullscreen.
                            GlobalContent.PlaySound("Select");
                            if(UserInterface.instance != null)
                                UserInterface.instance.ToggleFullscreen();
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(58+(9*14)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(58+(9*14)+6))
                        {
                            // Toggle hidden keep temporary job folders.
                            GlobalContent.PlaySound("Select");
                            SaveData.saveValues["HiddenKeepTemporaryJobFolders"] = (!bool.Parse(SaveData.saveValues["HiddenKeepTemporaryJobFolders"])).ToString(CultureInfo.InvariantCulture);
                            SaveData.Save();
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(58+(9*15)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(58+(9*15)+6))
                        {
                            // Toggle hidden verbose.
                            GlobalContent.PlaySound("Select");
                            SaveData.saveValues["HiddenVerbose"] = (!bool.Parse(SaveData.saveValues["HiddenVerbose"])).ToString(CultureInfo.InvariantCulture);
                            SaveData.Save();
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(58+(9*16)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(58+(9*16)+6))
                        {
                            // Toggle audio sync.
                            GlobalContent.PlaySound("Select");
                            Global.generator.audioSync = !Global.generator.audioSync;
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(58+(9*17)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(58+(9*17)+6))
                        {
                            // Open console.txt.
                            GlobalContent.PlaySound("Select");
                            string consoleLogFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "console.txt");
                            if(File.Exists(consoleLogFile))
                                System.Diagnostics.Process.Start("notepad", consoleLogFile);
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(58+(9*18)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(58+(9*18)+6))
                        {
                            // Unlock FPS.
                            GlobalContent.PlaySound("Select");
                            if(UserInterface.instance != null)
                                UserInterface.instance.SetFPSUnlock(UserInterface.instance.IsFixedTimeStep);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Interactable
            if(Debug.GetDebugMode())
            {
                controller.Draw(gameTime, spriteBatch);
                DrawButton(spriteBatch, 137, 58, "Locale: " + L.GetLocale().name + " " + L.GetLocale().localizedName);
                DrawButton(spriteBatch, 137, 58+9, "Random Seed: " + Global.randomSeed.ToString(CultureInfo.InvariantCulture));
                DrawButton(spriteBatch, 137, 58+(9*2), "User Resizing: " + (UserInterface.instance != null && UserInterface.instance.Window.AllowUserResizing ? "Enabled" : "Disabled"));
                DrawButton(spriteBatch, 137, 58+(9*3), "Game Cheat: " + (Debug.gameCheat ? "Enabled" : "Disabled"));
                DrawButton(spriteBatch, 137, 58+(9*4), "Screen Scale: " + SaveData.saveValues["ScreenScale"]);
                DrawButton(spriteBatch, 137, 58+(9*5), "Toggle Console On Right");
                DrawButton(spriteBatch, 137, 58+(9*6), "Speed Boost: x" + Debug.debugSpeedBoost);
                DrawButton(spriteBatch, 137, 58+(9*7), "Draw Offset: " + GlobalGraphics.drawOffset.X.ToString(CultureInfo.InvariantCulture) + ", " + GlobalGraphics.drawOffset.Y.ToString(CultureInfo.InvariantCulture));
                DrawButton(spriteBatch, 137, 58+(9*8), "Export Params: " + (Generator.exportParams.StartsWith("-vcodec") ? "better" : (Generator.exportParams.StartsWith("-af") ? "better (audio sync)" : "old")));
                DrawButton(spriteBatch, 137, 58+(9*9), "Music: #" + (UserInterface.instance != null ? UserInterface.instance.music+1 : 0));
                DrawButton(spriteBatch, 137, 58+(9*10), "Theme: " + ThemeManager.activeTheme.name);
                DrawButton(spriteBatch, 137, 58+(9*11), "Save");
                DrawButton(spriteBatch, 137, 58+(9*12), "Holiday: " + (HolidayManager.CurrentHoliday != null ? HolidayManager.CurrentHoliday.Name : "None"));
                DrawButton(spriteBatch, 137, 58+(9*13), (GlobalGraphics.fullScreen ? "Disable" : "Enable") + " Fullscreen");
                DrawButton(spriteBatch, 137, 58+(9*14), (bool.Parse(SaveData.saveValues["HiddenKeepTemporaryJobFolders"]) ? "Delete" : "Keep") + " Temporary Job Folders");
                DrawButton(spriteBatch, 137, 58+(9*15), (bool.Parse(SaveData.saveValues["HiddenVerbose"]) ? "Disable" : "Enable") + " Verbose");
                DrawButton(spriteBatch, 137, 58+(9*16), (Global.generator.audioSync ? "Disable" : "Enable") + " Audio Sync");
                DrawButton(spriteBatch, 137, 58+(9*17), "Open console.txt");
                DrawButton(spriteBatch, 137, 58+(9*18), (UserInterface.instance != null && UserInterface.instance.IsFixedTimeStep ? "Unlock" : "Lock") + " FPS and VSync");
                GlobalContent.DrawString(spriteBatch, L.FontSmall(), "F3: Toggle Debug Menu", new Vector2(GlobalGraphics.Scale(6), GlobalGraphics.Scale(41)), Color.White);
                GlobalContent.DrawString(spriteBatch, L.FontSmall(), "F4: Toggle Main Window Tween", new Vector2(GlobalGraphics.Scale(6), GlobalGraphics.Scale(41) + GlobalGraphics.Scale(8)), Color.White);
                GlobalContent.DrawString(spriteBatch, L.FontSmall(), "F6: Pause", new Vector2(GlobalGraphics.Scale(6), GlobalGraphics.Scale(41) + GlobalGraphics.Scale(8*2)), Color.White);
                GlobalContent.DrawString(spriteBatch, L.FontSmall(), "F7: Advance Frame", new Vector2(GlobalGraphics.Scale(6), GlobalGraphics.Scale(41) + GlobalGraphics.Scale(8*3)), Color.White);
                GlobalContent.DrawString(spriteBatch, L.FontSmall(), "F8: Speed Boost", new Vector2(GlobalGraphics.Scale(6), GlobalGraphics.Scale(41) + GlobalGraphics.Scale(8*4)), Color.White);
                GlobalContent.DrawString(spriteBatch, L.FontSmall(), "F9: Reload Locales", new Vector2(GlobalGraphics.Scale(6), GlobalGraphics.Scale(41) + GlobalGraphics.Scale(8*5)), Color.White);
                GlobalContent.DrawString(spriteBatch, L.FontSmall(), "F10: Unload All Locales", new Vector2(GlobalGraphics.Scale(6), GlobalGraphics.Scale(41) + GlobalGraphics.Scale(8*6)), Color.White);
            }
        }
        public void DrawButton(SpriteBatch spriteBatch, int x, int y, string text)
        {
            GlobalGraphics.DrawButton(spriteBatch, GlobalGraphics.Scale(x), GlobalGraphics.Scale(y), Color.Transparent, text, Color.White, Color.Gray);
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            // Interactable
            controller.LoadContent(contentManager, graphicsDevice);
        }
    }
}
