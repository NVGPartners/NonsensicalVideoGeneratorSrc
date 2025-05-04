#if MONOGAME
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Steamworks;
using System.Globalization;
using System.IO;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// This screen displays a warning about photosensitive epilepsy.
    /// </summary>
    public class PhotosensitiveWarningScreen : IScreen
    {
        /// <summary>
        /// The title of the screen. This is displayed on the header bar.
        /// </summary>
        public string title { get; } = "Intro";
        public int layer { get; set; } = 97;
        public ScreenType screenType { get; set; } = ScreenType.Hidden;
        public int currentPlacement { get; set; } = -1;
        private int overlayOpacity = 255;
        private int lastTextOpacity = 255;
        private bool accepted = false;
        private bool fadingIn = false;
        private bool textFadedIn = false;
        private double timeText = 0;
        private bool askAccessibility = false;
        private bool dontAskAgain = false;
        private static KeyboardState oldKeyboardState;
        private static KeyboardState newKeyboardState;
        // shamelessly copied from tutorial screen
        private BackgroundWorker? updateWorker;
        private readonly InteractableController controller = new();
        public static void ErrorOut()
        {
        }
        private void UpdateCheckThread(object? sender, DoWorkEventArgs e)
        {
            // Defer until Global.ready is true.
            while(!Global.ready)
            {
                System.Threading.Thread.Sleep(100);
            }
            try
            {
                if(SteamManager.initialized)
                    PluginHandler.LoadWorkshop();
                else
                    PluginHandler.LoadPluginsThreaded();
            }
            catch
            {
                ConsoleOutput.WriteLine("Failed to load Workshop addons.");
                PluginHandler.LoadPluginsThreaded();
            }
        }
        private List<string> warningText = new List<string>()
        {
            " ",
            "Intro:AccessibilityHelp",
            "Intro:AccessibilityF1",
            "Intro:AccessibilityF2",
            " ",
            " ",
            " ",
            " ",
            " ",
            " ",
            "Intro:Continue",
            " "
        };
        private List<string> accesibilityText = new List<string>()
        {
            " ",
            "Intro:PhotosensitiveWarning1",
            " ",
            "Intro:PhotosensitiveWarning2",
            "Intro:PhotosensitiveWarning3",
            "Intro:PhotosensitiveWarning4",
            " ",
            "Intro:PhotosensitiveWarning5",
            "Intro:PhotosensitiveWarning6",
            " ",
            "Intro:Continue",
            " "
        };
        private List<string> tipoftheday = new List<string>()
        {
            " ",
            "Intro:TipHeader",
            " ",
            " ",
            " ",
            "Intro:StatsHeader",
            " ",
            " ",
            " ",
            " ",
            "Intro:Continue",
            " "
        };
        private int tips = 15;
        public void Show()
        {
        }
        public void Hide()
        {
        }
        public bool Toggle(bool useBool = false, bool toggleTo = false)
        {
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            SpriteFont fontMunro = L.FontLarge();
            if(!fadingIn)
            {
                // Draw black background.
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(0, 0, GlobalGraphics.scaledWidth, GlobalGraphics.scaledHeight), new Color(0, 0, 0, 255));
                // Draw text center aligned.
                for (int i = 0; i < warningText.Count; i++)
                {
                    string text = warningText[i];
                    Vector2 textSize = fontMunro.MeasureString(text);
                    GlobalContent.DrawString(spriteBatch, fontMunro, text, new Vector2(GlobalGraphics.scaledWidth / 2 - textSize.X / 2, GlobalGraphics.Scale(24 + i * 16)), Color.White);
                }
                if(!askAccessibility && !accepted)
                {
                    // Interactable
                    controller.Draw(gameTime, spriteBatch);
                }
            }
            // Draw overlay over last text.
            spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(0, GlobalGraphics.Scale(24 + (warningText.Count - 1) * 16), GlobalGraphics.scaledWidth, GlobalGraphics.Scale(16)), new Color(0, 0, 0, lastTextOpacity));
            // Draw black overlay.
            spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(0, 0, GlobalGraphics.scaledWidth, GlobalGraphics.scaledHeight), new Color(0, 0, 0, overlayOpacity));
        }
        public bool Update(GameTime gameTime, bool handleInput)
        {
            if(screenType == ScreenType.Hidden || Global.fakeExit)
                return false;
            if(!accepted)
            {
                overlayOpacity -= 16;
                if(overlayOpacity <= 0 || askAccessibility)
                {
                    overlayOpacity = 0;
                    // Flash text.
                    if(!textFadedIn)
                    {
                        lastTextOpacity -= 16;
                        if(lastTextOpacity <= 0)
                        {
                            lastTextOpacity = 0;
                            textFadedIn = true;
                            timeText = gameTime.TotalGameTime.TotalMilliseconds;
                        }
                    }
                }
            }
            else if(!askAccessibility)
            {
                lastTextOpacity -= 16;
                overlayOpacity += 16;
                if (overlayOpacity >= 255)
                {
                    overlayOpacity = 255;
                    lastTextOpacity = 0;
                }
            }
            if (fadingIn || askAccessibility)
            {
                overlayOpacity -= 16;
                if (overlayOpacity <= 0)
                {
                    overlayOpacity = 0;
                    if(!askAccessibility)
                    {
                        screenType = ScreenType.Hidden;
                        return false;
                    }
                }
            }
            if(handleInput)
            {
                if(!askAccessibility && !accepted)
                {
                    // Interactable
                    if(controller.Update(gameTime, handleInput))
                        return true;
                }
                if(!accepted)
                {
                    // Keyboard input.
                    newKeyboardState = Keyboard.GetState();
                    oldKeyboardState = newKeyboardState;
                    Accessibility.CompatAccessibility(new Rectangle(GlobalGraphics.Scale(4), GlobalGraphics.Scale(24 + (warningText.Count - 2) * 16), GlobalGraphics.scaledWidth - GlobalGraphics.Scale(8), GlobalGraphics.Scale(16)), "Click anywhere to continue.");
                    if (MouseInput.MouseState.LeftButton == ButtonState.Pressed && MouseInput.LastMouseState.LeftButton == ButtonState.Released
                        || newKeyboardState.GetPressedKeys().Length > 0 && oldKeyboardState.GetPressedKeys().Length == 0)
                    {
                        if(!dontAskAgain || askAccessibility)
                        {
                            dontAskAgain = true;
                            if(askAccessibility)
                            {
                                SaveData.saveValues["FirstBoot"] = "false";
                                SaveData.Save();
                                askAccessibility = false;
                                dontAskAgain = false;
                            }
                            else
                            {
                                ConsoleOutput.WriteLine("User acknowledged photosensitive warning.", Color.LightGreen);
                            }
                            accepted = true;
                            GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        }
                    }
                }
                else if(!askAccessibility)
                {
                    if (overlayOpacity >= 255)
                    {
                        if(bool.Parse(SaveData.saveValues["FirstBoot"]))
                        {
                            askAccessibility = true;
                            warningText = new List<string>(accesibilityText);
                            for (int i = 0; i < warningText.Count; i++)
                            {
                                if(warningText[i] == " ")
                                    continue;
                                warningText[i] = L.T(0, warningText[i]);
                            }
                        }
                        else
                        {
                            try
                            {
                                if(SteamManager.initialized)
                                {
                                    ConsoleOutput.WriteLine("Steam initialized.", Color.LightGreen);
                                }
                                else
                                {
                                    ConsoleOutput.WriteLine("Steam is not running, skipping workshop initialization.", Color.LightGreen);
                                }
                            }
                            catch
                            {
                                ConsoleOutput.WriteLine("Steam failed, skipping workshop initialization.", Color.LightGreen);
                            }
                            updateWorker = new BackgroundWorker();
                            updateWorker.DoWork += UpdateCheckThread;
                            updateWorker.RunWorkerAsync();
                            ScreenManager.PushNavigation("Menu");
                            ScreenManager.PushNavigation("Content");
                            ScreenManager.PushNavigation("Background");
                            ScreenManager.PushNavigation("Socials");
                            ScreenManager.GetScreen<ContentScreen>("Content")?.Show();
                            ScreenManager.GetScreen<MenuScreen>("Menu")?.Show();
                            if(FramePlayer.audio != null)
                            {
                                ScreenManager.PushNavigation("Video");
                                ScreenManager.GetScreen<VideoScreen>("Video")?.Show();
                            }
                            ScreenManager.GetScreen<BackgroundScreen>("Background")?.Show();
                            ScreenManager.GetScreen<HeaderScreen>("Header")?.Show();
                            ScreenManager.GetScreen<SocialScreen>("Socials")?.Show();
                            Global.ready = true;
                            Global.readyTime = gameTime.TotalGameTime.TotalMilliseconds;
                            if(Global.selectLanguage)
                                Pagination.SetPage(3); // show options
                            // Play startup sound.
                            if(SaveData.saveValues["ActiveTheme"] == "")
                            {
                                Global.waitReady = 2500;
                                GlobalContent.GetSound("Start").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            }
                            fadingIn = true;
                            Global.generator.CleanUp();
                            VideoCache.ClearCache();
                        }
                        accepted = false;
                    }
                }
            }
            return true;
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            if(!bool.Parse(SaveData.saveValues["FirstBoot"]) && !Global.parameters.Contains("-intro"))
            {
                if(bool.Parse(SaveData.saveValues["SkipPhotosensitiveWarningScreen"]))
                {
                    askAccessibility = false;
                    accepted = true;
                    overlayOpacity = 255;
                }
                else
                {
                    askAccessibility = false;
                    overlayOpacity = 255;
                    // tip of the day
                    warningText = new List<string>(tipoftheday);
                    // Get random tip.
                    int tip = Global.generator.RandomInt(0, tips);
                    warningText[2] = "Intro:Tip" + tip.ToString(CultureInfo.InvariantCulture) + "_0";
                    warningText[3] = "Intro:Tip" + tip.ToString(CultureInfo.InvariantCulture) + "_1";
                    // Translate current text
                    for (int i = 0; i < warningText.Count; i++)
                    {
                        if(warningText[i] == " ")
                            continue;
                        warningText[i] = L.T(0, warningText[i]);
                    }
                    // get stats
                    int[] stats = new int[3]
                    {
                        int.Parse(SaveData.saveValues["TotalVideosRendered"], CultureInfo.InvariantCulture),
                        int.Parse(SaveData.saveValues["TotalMediaImported"], CultureInfo.InvariantCulture),
                        int.Parse(SaveData.saveValues["TotalClipsTrimmed"], CultureInfo.InvariantCulture)
                    };
                    bool[] plural = new bool[3]
                    {
                        stats[0] != 1,
                        stats[1] != 1,
                        stats[2] != 1
                    };
                    warningText[6] = plural[0] ? L.T(0, "Intro:StatsVideosRendered", stats[0].ToString(CultureInfo.InvariantCulture)) : L.T(0, "Intro:StatsVideosRenderedNonPlural", stats[0].ToString(CultureInfo.InvariantCulture));
                    warningText[7] = plural[1] ? L.T(0, "Intro:StatsMediaImports", stats[1].ToString(CultureInfo.InvariantCulture)) : L.T(0, "Intro:StatsMediaImportsNonPlural", stats[1].ToString(CultureInfo.InvariantCulture));
                    warningText[8] = plural[2] ? L.T(0, "Intro:StatsClipsTrimmed", stats[2].ToString(CultureInfo.InvariantCulture)) : L.T(0, "Intro:StatsClipsTrimmedNonPlural", stats[2].ToString(CultureInfo.InvariantCulture));
                }
            }
            else
            {
                for (int i = 0; i < warningText.Count; i++)
                {
                    if(warningText[i] == " ")
                        continue;
                    warningText[i] = L.T(0, warningText[i]);
                }
                controller.Add("ViewLocalizationOptions", new Switch("Change language", "Visit the locale selection to change NVG's language.", new Vector2(60, 165), (int i, string n) => {
                    bool switchState = (i & 256) != 0;
                    if((i & 2) != 0)
                    {
                        Global.selectLanguage = switchState;
                    }
                    return switchState;
                }, false));
                controller.Add("UseColorblindTheme", new Switch("", "Use colorblind-friendly theme.", new Vector2(60, 140), (int i, string n) => {
                    bool switchState = (i & 256) != 0;
                    if((i & 2) != 0)
                    {
                        string oldValue = SaveData.saveValues["ActiveTheme"];
                        if(switchState)
                            SaveData.saveValues["ActiveTheme"] = "colorblind.lua";
                        else
                            SaveData.saveValues["ActiveTheme"] = "";
                        if(oldValue != SaveData.saveValues["ActiveTheme"])
                            SaveData.Save();
                    }
                    return switchState;
                }, SaveData.saveValues["ActiveTheme"] == "colorblind.lua"));
                controller.Add("MotionDisable", new Switch("", "Turns off screen tweening and other elements.", new Vector2(60, 115), (int i, string n) => {
                    bool switchState = (i & 256) != 0;
                    if((i & 2) != 0)
                    {
                        string oldValue = SaveData.saveValues["DisableMotion"];
                        SaveData.saveValues["DisableMotion"] = switchState.ToString().ToLower();
                        if(oldValue != SaveData.saveValues["DisableMotion"])
                            SaveData.Save();
                    }
                    return switchState;
                }, SaveData.saveValues["DisableMotion"] == "true"));
                controller.Add("Mute", new Switch("", "Mutes in-app background music.", new Vector2(60, 90), (int i, string n) => {
                    bool switchState = (i & 256) != 0;
                    if((i & 2) != 0)
                    {
                        string oldValue = SaveData.saveValues["MusicVolume"];
                        SaveData.saveValues["MusicVolume"] = switchState ? "0" : "25";
                        if(oldValue != SaveData.saveValues["MusicVolume"])
                            SaveData.Save();
                    }
                    return switchState;
                }, SaveData.saveValues["MusicVolume"] == "0"));
            }
            // Interactable
            controller.LoadContent(contentManager, graphicsDevice);
        }
    }
}
#endif
