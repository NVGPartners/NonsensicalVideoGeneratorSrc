#if MONOGAME
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
                    if(MouseInput.LastMouseState.LeftButton == ButtonState.Released && MouseInput.MouseState.LeftButton == ButtonState.Pressed)
                    {
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(33) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(33+6))
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
                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(33+8) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(33+8+6))
                        {
                            // Set random seed to 0.
                            Global.randomSeed = 0;
                            Global.generator.globalRandom = new Random(0);
                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(33+(8*2)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(33+(8*2)+6))
                        {
                            // Toggle user resizing.
                            UserInterface.instance.Window.AllowUserResizing = !UserInterface.instance.Window.AllowUserResizing;
                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(33+(8*3)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(33+(8*3)+6))
                        {
                            // Toggle game cheat.
                            Debug.gameCheat = !Debug.gameCheat;
                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(33+(8*4)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(33+(8*4)+6))
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
                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(33+(8*5)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(33+(8*5)+6))
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
                                    UserInterface.instance.Resize(GlobalGraphics.scaledWidth*2, GlobalGraphics.scaledHeight);
                                }
                                else
                                {
                                    consoleScreen.screenType = ScreenType.Hidden;
                                    consoleScreen.offset = new Vector2(0, GlobalGraphics.scaledHeight);
                                    UserInterface.instance.Resize(GlobalGraphics.scaledWidth, GlobalGraphics.scaledHeight);
                                }
                                GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            }
                            else
                            {
                                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            }
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(33+(8*6)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(33+(8*6)+6))
                        {
                            // Toggle speed boost.
                            Debug.debugSpeedBoost++;
                            if(Debug.debugSpeedBoost > 10)
                                Debug.debugSpeedBoost = 2;
                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(33+(8*7)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(33+(8*7)+6))
                        {
                            // Toggle draw offset.
                            AspectRatio current = GlobalGraphics.GetAspectRatio();
                            // Get the index of the current AspectRatio
                            int index = AspectRatio.All.IndexOf(current);
                            // Get the next AspectRatio
                            AspectRatio next = AspectRatio.All[(index + 1) % AspectRatio.All.Count];
                            // Set the next AspectRatio
                            GlobalGraphics.SetAspectRatio(next);
                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(33+(8*8)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(33+(8*8)+6))
                        {
                            // Toggle export params.
                            if(Generator.exportParams.StartsWith("-vcodec"))
                                Generator.exportParams = Generator.oldExportParams;
                            else
                                Generator.exportParams = Generator.betterExportParams;
                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(33+(8*9)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(33+(8*9)+6))
                        {
                            // Cycle music.
                            UserInterface.instance.FindMusic();
                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(33+(8*10)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(33+(8*10)+6))
                        {
                            // Show tutorial window.
                            Pagination.SetPage(0);
                            PhotosensitiveWarningScreen.ErrorOut();
                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(33+(8*11)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(33+(8*11)+6))
                        {
                            // Cycle theme.
                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
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
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(33+(8*12)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(33+(8*12)+6))
                        {
                            // Save.
                            SaveData.Save();
                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(33+(8*13)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(33+(8*13)+6))
                        {
                            // Cycle holiday.
                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
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
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(33+(8*14)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(33+(8*14)+6))
                        {
                            // Toggle addon consents.
                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            Global.disableConsents = !Global.disableConsents;
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(33+(8*15)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(33+(8*15)+6))
                        {
                            // Toggle fullscreen.
                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            UserInterface.instance.ToggleFullscreen();
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(33+(8*16)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(33+(8*16)+6))
                        {
                            // Toggle hidden keep temporary job folders.
                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            SaveData.saveValues["HiddenKeepTemporaryJobFolders"] = (!bool.Parse(SaveData.saveValues["HiddenKeepTemporaryJobFolders"])).ToString(CultureInfo.InvariantCulture);
                            SaveData.Save();
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(33+(8*17)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(33+(8*17)+6))
                        {
                            // Toggle hidden verbose.
                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            SaveData.saveValues["HiddenVerbose"] = (!bool.Parse(SaveData.saveValues["HiddenVerbose"])).ToString(CultureInfo.InvariantCulture);
                            SaveData.Save();
                            return true;
                        }
                        if(MouseInput.MouseState.Y >= GlobalGraphics.Scale(33+(8*18)) && MouseInput.MouseState.Y <= GlobalGraphics.Scale(33+(8*18)+6))
                        {
                            // Toggle audio sync.
                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            Global.generator.audioSync = !Global.generator.audioSync;
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
                DrawButton(spriteBatch, 6, 33, "Locale: " + L.GetLocale().name + " " + L.GetLocale().localizedName);
                DrawButton(spriteBatch, 6, 33+8, "Random Seed: " + Global.randomSeed.ToString(CultureInfo.InvariantCulture));
                DrawButton(spriteBatch, 6, 33+(8*2), "User Resizing: " + (UserInterface.instance.Window.AllowUserResizing ? "Enabled" : "Disabled"));
                DrawButton(spriteBatch, 6, 33+(8*3), "Game Cheat: " + (Debug.gameCheat ? "Enabled" : "Disabled"));
                DrawButton(spriteBatch, 6, 33+(8*4), "Screen Scale: " + SaveData.saveValues["ScreenScale"]);
                DrawButton(spriteBatch, 6, 33+(8*5), "Toggle Console On Right");
                DrawButton(spriteBatch, 6, 33+(8*6), "Speed Boost: x" + Debug.debugSpeedBoost);
                DrawButton(spriteBatch, 6, 33+(8*7), "Draw Offset: " + GlobalGraphics.drawOffset.X.ToString(CultureInfo.InvariantCulture) + ", " + GlobalGraphics.drawOffset.Y.ToString(CultureInfo.InvariantCulture));
                DrawButton(spriteBatch, 6, 33+(8*8), "Export Params: " + (Generator.exportParams.StartsWith("-vcodec") ? "better" : (Generator.exportParams.StartsWith("-af") ? "better (audio sync)" : "old")));
                DrawButton(spriteBatch, 6, 33+(8*9), "Music: #" + (UserInterface.instance.music+1));
                DrawButton(spriteBatch, 6, 33+(8*10), "Show Tutorial Window");
                DrawButton(spriteBatch, 6, 33+(8*11), "Theme: " + ThemeManager.activeTheme.name);
                DrawButton(spriteBatch, 6, 33+(8*12), "Save");
                DrawButton(spriteBatch, 6, 33+(8*13), "Holiday: " + (HolidayManager.CurrentHoliday != null ? HolidayManager.CurrentHoliday.Name : "None"));
                DrawButton(spriteBatch, 6, 33+(8*14), (Global.disableConsents ? "Enable" : "Disable") + " Addon Consents");
                DrawButton(spriteBatch, 6, 33+(8*15), (GlobalGraphics.fullScreen ? "Disable" : "Enable") + " Fullscreen");
                DrawButton(spriteBatch, 6, 33+(8*16), (bool.Parse(SaveData.saveValues["HiddenKeepTemporaryJobFolders"]) ? "Delete" : "Keep") + " Temporary Job Folders");
                DrawButton(spriteBatch, 6, 33+(8*17), (bool.Parse(SaveData.saveValues["HiddenVerbose"]) ? "Disable" : "Enable") + " Verbose");
                DrawButton(spriteBatch, 6, 33+(8*18), (Global.generator.audioSync ? "Disable" : "Enable") + " Audio Sync");
                GlobalContent.DrawString(spriteBatch, L.FontSmall(), Debug.debugBuild ? "Debug Build" : (Debug.GetDebugMode() ? "Release Build; Debug Mode" : "Release Build"), new Vector2(GlobalGraphics.Scale(6), GlobalGraphics.scaledHeight - GlobalGraphics.Scale(6*9) - GlobalGraphics.Scale(9)), Color.White);
                GlobalContent.DrawString(spriteBatch, L.FontSmall(), "Parameters: "+String.Join(" ", Global.parameters.ToArray()), new Vector2(GlobalGraphics.Scale(6), GlobalGraphics.scaledHeight - GlobalGraphics.Scale(6*8) - GlobalGraphics.Scale(9)), Color.White);
                GlobalContent.DrawString(spriteBatch, L.FontSmall(), "CTRL + F3: Toggle Debug Mode", new Vector2(GlobalGraphics.Scale(6), GlobalGraphics.scaledHeight - GlobalGraphics.Scale(6*7) - GlobalGraphics.Scale(9)), Color.White);
                GlobalContent.DrawString(spriteBatch, L.FontSmall(), "F3: Toggle Debug Menu", new Vector2(GlobalGraphics.Scale(6), GlobalGraphics.scaledHeight - GlobalGraphics.Scale(6*6) - GlobalGraphics.Scale(9)), Color.White);
                GlobalContent.DrawString(spriteBatch, L.FontSmall(), "F4: Toggle Main Window Tween", new Vector2(GlobalGraphics.Scale(6), GlobalGraphics.scaledHeight - GlobalGraphics.Scale(6*5) - GlobalGraphics.Scale(9)), Color.White);
                GlobalContent.DrawString(spriteBatch, L.FontSmall(), "F6: Pause", new Vector2(GlobalGraphics.Scale(6), GlobalGraphics.scaledHeight - GlobalGraphics.Scale(6*4) - GlobalGraphics.Scale(9)), Color.White);
                GlobalContent.DrawString(spriteBatch, L.FontSmall(), "F7: Advance Frame", new Vector2(GlobalGraphics.Scale(6), GlobalGraphics.scaledHeight - GlobalGraphics.Scale(6*3) - GlobalGraphics.Scale(9)), Color.White);
                GlobalContent.DrawString(spriteBatch, L.FontSmall(), "F8: Speed Boost", new Vector2(GlobalGraphics.Scale(6), GlobalGraphics.scaledHeight - GlobalGraphics.Scale(6*2) - GlobalGraphics.Scale(9)), Color.White);
                GlobalContent.DrawString(spriteBatch, L.FontSmall(), "F9: Reload Locales", new Vector2(GlobalGraphics.Scale(6), GlobalGraphics.scaledHeight - GlobalGraphics.Scale(6) - GlobalGraphics.Scale(9)), Color.White);
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
#endif
