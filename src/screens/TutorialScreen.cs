using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended.Tweening;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// This is the about screen.
    /// </summary>
    public class TutorialScreen : IScreen
    {
        /// <summary>
        /// The title of the screen. This is displayed on the header bar.
        /// </summary>
        public string title { get; } = "Initial Setup";
        public int layer { get; } = 7;
        public ScreenType screenType { get; set; } = ScreenType.Hidden;
        public int currentPlacement { get; set; } = -1;
        private bool hiding = false;
        private bool showing = false;
        private bool toggle = false;
        private bool check = false;
        public Vector2 offset = new(0, 0);
        private readonly Tweener tween = new();
        private readonly InteractableController controller = new();
        public void Show()
        {
            toggle = true;
            offset = new(0, GlobalGraphics.Scale(240)); // from bottom to top
            tween.TweenTo(this, t => t.offset, new Vector2(0, 0), 0.5f)
                .Easing(EasingFunctions.ExponentialOut);
            showing = true;
        }
        public void Hide()
        {
            toggle = false;
            offset = new(0, 0); // from top to bottom
            tween.TweenTo(this, t => t.offset, new Vector2(0, GlobalGraphics.Scale(240)), 0.5f)
                .Easing(EasingFunctions.ExponentialOut);
            hiding = true;
        }
        public bool Toggle(bool useBool = false, bool toggleTo = false)
        {
            if (useBool)
            {
                if (toggleTo)
                {
                    Show();
                    return true;
                }
                else
                {
                    Hide();
                    return false;
                }
            }
            else
            {
                if (toggle)
                {
                    Hide();
                    return false;
                }
                else
                {
                    Show();
                    return true;
                }
            }
        }
        public bool Update(GameTime gameTime, bool handleInput)
        {
            // When animation is done, set screen type
            if (hiding && offset.Y == GlobalGraphics.Scale(240))
            {
                screenType = ScreenType.Hidden;
                hiding = false;
            }
            else if (showing)
            {
                screenType = ScreenType.Drawn;
                showing = false;
                hiding = false;
            }
            // Tween
            tween.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            if(hiding || screenType == ScreenType.Hidden)
                return false;
            // Store mouse state
            MouseState mouseState = MouseInput.MouseState;
            // Repeat for last mouse state
            MouseState lastMouseState = MouseInput.LastMouseState;
            if(Accessibility.selectedDisambiguationOption == -1)
            {
                // Offset mouse position
                MouseState offsetMouseState = new MouseState(mouseState.X - (int)offset.X, mouseState.Y - (int)offset.Y, mouseState.ScrollWheelValue, mouseState.LeftButton, mouseState.MiddleButton, mouseState.RightButton, mouseState.XButton1, mouseState.XButton2);
                // Set new mouse state
                MouseInput._mouseState = offsetMouseState;
                MouseState offsetLastMouseState = new MouseState(lastMouseState.X - (int)offset.X, lastMouseState.Y - (int)offset.Y, lastMouseState.ScrollWheelValue, lastMouseState.LeftButton, lastMouseState.MiddleButton, lastMouseState.RightButton, lastMouseState.XButton1, lastMouseState.XButton2);
                MouseInput.LastMouseState = offsetLastMouseState;
            }
            // Update controller
            if(controller.Update(gameTime, handleInput))
                return true;
            // Revert
            if(Accessibility.selectedDisambiguationOption == -1)
            {
                MouseInput._mouseState = mouseState;
                MouseInput.LastMouseState = lastMouseState;
            }
            return false;
        }
        public static List<string>[] tutorialText = new List<string>[3]
        {
            new List<string>(),
            new List<string>(),
            new List<string>()
        };
        public static List<string>[] baseTutorialText = new List<string>[3]
        {
            new List<string>()
            { // PAGE 1
                "Welcome to Nonsensical Video Generator!",
                "",
                "This initial setup will help you get started.",
                "On the next page, we will check these prerequisites.",
                "",
                "These programs are required, but not included:",
                " - ffmpeg",
                " - ffprobe",
                "",
                "You will need to place them in the same folder as this software.",
                "",
                "(Advanced: Add a folder containing them to your PATH variable.)",
                "",
                "These programs are optional:",
                " - magick (only uses PATH)",
                " - yt-dlp (bundled)",
                "",
                "Click \"Next Page\" to continue."
            },
            new List<string>()
            { // PAGE 2
                "The status of each program is shown below.",
                "",
                "Required software:",
                " - ffmpeg: %FFMPEG%",
                " - ffprobe: %FFPROBE%",
                "",
                "Optional software:",
                " - magick: %IMAGEMAGICK%",
                " - yt-dlp: %YTDLP%",
                "",
                "A restart may be required if you install any of these programs.",
                "",
                "Need help? Create an issue or post on the GitHub issue tracker.",
                "",
                "The \"Next Page\" button will be disabled if there are issues."
            },
            new List<string>()
            { // PAGE 3
                "All required software is installed!",
                "",
                "Don't hesitate to ask for help on the GitHub issue tracker!",
                "Be sure to join the Discord server community from the menu!",
                "",
                "Enjoy using Nonsensical Video Generator!",
                "",
                "If there are still issues, the continue button will be disabled.",
                "In that case, broken plugins would have been detected.",
                "Check console with ~ to see which plugins are broken, if so.",
                "",
                "Click \"Continue\" to scan plugins and continue if successful."
            }
        };
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // End existing spritebatch
            spriteBatch.End();
            // Use offset
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(offset.X, offset.Y, 0));
            // Tutorial window
            Texture2D tutorialWindow = GlobalContent.GetTexture("TutorialWindow");
            spriteBatch.Draw(tutorialWindow, new Rectangle(GlobalGraphics.Scale(8), GlobalGraphics.Scale(36), GlobalGraphics.Scale(tutorialWindow.Width), GlobalGraphics.Scale(tutorialWindow.Height)), Color.White);
            spriteBatch.Draw(tutorialWindow, new Rectangle(GlobalGraphics.Scale(8+320), GlobalGraphics.Scale(36), GlobalGraphics.Scale(tutorialWindow.Width), GlobalGraphics.Scale(tutorialWindow.Height)), Color.White);
            spriteBatch.Draw(tutorialWindow, new Rectangle(GlobalGraphics.Scale(8+640), GlobalGraphics.Scale(36), GlobalGraphics.Scale(tutorialWindow.Width), GlobalGraphics.Scale(tutorialWindow.Height)), Color.White);
            controller.Draw(gameTime, spriteBatch);
            // Draw the center title bar text
            Vector2 titleSize1 = GlobalGraphics.fontMunroSmall.MeasureString(title + ": Page 1/3");
            spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, title + ": Page 1/3", new Vector2(GlobalGraphics.scaledWidth / 2 - titleSize1.X / 2, (6 * GlobalGraphics.scale) - GlobalGraphics.Scale(1-32)), Color.White);
            Vector2 titleSize2 = GlobalGraphics.fontMunroSmall.MeasureString(title + ": Page 2/3");
            spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, title + ": Page 2/3", new Vector2(GlobalGraphics.scaledWidth / 2 - titleSize2.X / 2 + GlobalGraphics.Scale(320), (6 * GlobalGraphics.scale) - GlobalGraphics.Scale(1-32)), Color.White);
            Vector2 titleSize3 = GlobalGraphics.fontMunroSmall.MeasureString(title + ": Page 3/3");
            spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, title + ": Page 3/3", new Vector2(GlobalGraphics.scaledWidth / 2 - titleSize3.X / 2 + GlobalGraphics.Scale(640), (6 * GlobalGraphics.scale) - GlobalGraphics.Scale(1-32)), Color.White);
            // Draw tutorial text
            for(int i = 0; i < 3; i++)
            {
                int offsetText = 0;
                for(int j = 0; j < tutorialText[i].Count; j++)
                {
                    string dummyText = tutorialText[i][j];
                    dummyText = dummyText.Replace("%FFMPEG%", "Checking...");
                    dummyText = dummyText.Replace("%FFPROBE%", "Checking...");
                    dummyText = dummyText.Replace("%IMAGEMAGICK%", "Checking...");
                    dummyText = dummyText.Replace("%YTDLP%", "Checking...");
                    Vector2 textSize = GlobalGraphics.fontMunroSmall.MeasureString(dummyText);
                    spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, dummyText, new Vector2(GlobalGraphics.Scale(8+16+1+320*i), GlobalGraphics.Scale(60+offsetText+1)), Color.Black);
                    spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, dummyText, new Vector2(GlobalGraphics.Scale(8+16+320*i), GlobalGraphics.Scale(60+offsetText)), Color.White);
                    // Draw red overlay if prerequisite is not met
                    if(GlobalGraphics.scale == 2)
                    {
                        if(tutorialText[i][j].Contains("Not found"))
                        {
                            int offset = 0;
                            switch(j)
                            {
                                case 3:
                                    offset = 43;
                                    break;
                                case 4:
                                    offset = 47;
                                    break;
                                case 7:
                                    offset = 42;
                                    break;
                                case 8:
                                    offset = 43;
                                    break;
                            }
                            spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, "Not found", new Vector2(GlobalGraphics.Scale(offset+8+16+320*i), GlobalGraphics.Scale(60+offsetText)), Color.OrangeRed);
                        }
                        if(tutorialText[i][j].Contains("Using system PATH"))
                        {
                            int offset = 0;
                            switch(j)
                            {
                                case 3:
                                    offset = 43;
                                    break;
                                case 4:
                                    offset = 47;
                                    break;
                                case 7:
                                    offset = 42;
                                    break;
                                case 8:
                                    offset = 43;
                                    break;
                            }
                            spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, "Using system PATH", new Vector2(GlobalGraphics.Scale(offset+8+16+320*i), GlobalGraphics.Scale(60+offsetText)), Color.SeaGreen);
                        }
                        // Draw green overlay if prerequisite is met
                        if(tutorialText[i][j].Contains("Installed"))
                        {
                            int offset = 0;
                            switch(j)
                            {
                                case 3:
                                    offset = 43;
                                    break;
                                case 4:
                                    offset = 47;
                                    break;
                                case 7:
                                    offset = 42;
                                    break;
                                case 8:
                                    offset = 43;
                                    break;
                            }
                            spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, "Installed", new Vector2(GlobalGraphics.Scale(offset+8+16+320*i), GlobalGraphics.Scale(60+offsetText)), Color.LimeGreen);
                        }
                        // Draw red overlay if update check failed
                        if(tutorialText[i][j].Contains("Failed"))
                        {
                            int offset = 68;
                            spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, "Failed", new Vector2(GlobalGraphics.Scale(offset+8+16+320*i), GlobalGraphics.Scale(60+offsetText)), Color.OrangeRed);
                        }
                        // Draw yellow overlay if checking
                        if(dummyText.Contains("Checking..."))
                        {
                            int offset = 68;
                            switch(j)
                            {
                                case 3:
                                    offset = 43;
                                    break;
                                case 4:
                                    offset = 47;
                                    break;
                                case 7:
                                    offset = 42;
                                    break;
                                case 8:
                                    offset = 43;
                                    break;
                            }
                            spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, "Checking...", new Vector2(GlobalGraphics.Scale(offset+8+16+320*i), GlobalGraphics.Scale(60+offsetText)), Color.Yellow);
                        }
                    }
                    offsetText += 8;
                }
            }
            // End offset spritebatch
            spriteBatch.End();
            // Remake spritebatch
            spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                null, null, null, null);
        }
        private BackgroundWorker dependencyWorker;
        private void DependencyCheckThread(object? sender, DoWorkEventArgs e)
        {
            // Get dependencies.
            UpdateManager.GetDependencyStatus();
            for (int h = 0; h < tutorialText.Length; h++)
            {
                for (int j = 0; j < tutorialText[h].Count; j++)
                {
                    string ffmpeg = UpdateManager.ffmpegInstalled ? "Installed" : "Not found";
                    string ffprobe = UpdateManager.ffprobeInstalled ? "Installed" : "Not found";
                    string ytDlp = UpdateManager.ytDlpInstalled ? "Installed" : "Not found";
                    if(Global.useSystemFFmpeg)
                    {
                        if(!UpdateManager.ffmpegInstalled)
                            ffmpeg = "Not found";
                        else
                            ffmpeg = "Using system PATH";
                    }
                    if(Global.useSystemFFprobe)
                    {
                        if(!UpdateManager.ffprobeInstalled)
                            ffprobe = "Not found";
                        else
                            ffprobe = "Using system PATH";
                    }
                    if(Global.useSystemYtDlp)
                    {
                        if(!UpdateManager.ytDlpInstalled)
                            ytDlp = "Not found";
                        else
                            ytDlp = "Using system PATH";
                    }
                    tutorialText[h][j] = tutorialText[h][j].Replace("%FFMPEG%", ffmpeg);
                    tutorialText[h][j] = tutorialText[h][j].Replace("%FFPROBE%", ffprobe);
                    tutorialText[h][j] = tutorialText[h][j].Replace("%IMAGEMAGICK%", UpdateManager.imagemagickInstalled ? "Using system PATH" : "Not found");
                    tutorialText[h][j] = tutorialText[h][j].Replace("%YTDLP%", ytDlp);
                }
            }
            check = false;
            // Dispose of worker
            dependencyWorker.Dispose();
            dependencyWorker = null;
        }
        private bool check3 = false;
        private BackgroundWorker pluginWorker;
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            // Tutorial window
            GlobalContent.AddTexture("TutorialWindow", contentManager.Load<Texture2D>("graphics/tutorialwindow"));
            // PAGE 1
            controller.Add("Button1", new Button("Next Page", "", new Vector2(237+32+2, 217+12-6), (int i) => {
                switch(i)
                {
                    case 2: // left click
                        // Get dependencies.
                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                        if(!check)
                        {
                            for(int h = 0; h < baseTutorialText.Length; h++)
                            {
                                tutorialText[h] = new List<string>();
                                for(int j = 0; j < baseTutorialText[h].Count; j++)
                                {
                                    tutorialText[h].Add(baseTutorialText[h][j]);
                                }
                            }
                            check = true;
                            dependencyWorker = new BackgroundWorker();
                            dependencyWorker.DoWork += DependencyCheckThread;
                            dependencyWorker.RunWorkerAsync();
                        }
                        tween.TweenTo(this, t => t.offset, new Vector2(GlobalGraphics.Scale(-320), 0), 0.5f)
                            .Easing(EasingFunctions.ExponentialOut);
                        return true;
                }
                return false;
            }));
            // PAGE 2
            controller.Add("Button2", new Button("Next Page", "", new Vector2(237+32+320+2, 217+12-6), (int i) => {
                switch(i)
                {
                    case 2: // left click
                        if(UpdateManager.ffmpegInstalled && UpdateManager.ffprobeInstalled)
                        {
                            GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                            tween.TweenTo(this, t => t.offset, new Vector2(GlobalGraphics.Scale(-640), 0), 0.5f)
                                .Easing(EasingFunctions.ExponentialOut);
                        }
                        else
                        {
                            if(!check)
                            {
                                for(int h = 0; h < baseTutorialText.Length; h++)
                                {
                                    tutorialText[h] = new List<string>();
                                    for(int j = 0; j < baseTutorialText[h].Count; j++)
                                    {
                                        tutorialText[h].Add(baseTutorialText[h][j]);
                                    }
                                }
                                check = true;
                                dependencyWorker = new BackgroundWorker();
                                dependencyWorker.DoWork += DependencyCheckThread;
                                dependencyWorker.RunWorkerAsync();
                            }
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                        }
                        return true;
                }
                return false;
            }));
            controller.Add("Button3", new Button("Previous Page", "", new Vector2(28+32+320-4, 217+12-6), (int i) => {
                switch(i)
                {
                    case 2: // left click
                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                        tween.TweenTo(this, t => t.offset, new Vector2(GlobalGraphics.Scale(0), 0), 0.5f)
                            .Easing(EasingFunctions.ExponentialOut);
                        return true;
                }
                return false;
            }));
            // PAGE 3
            controller.Add("Button4", new Button("Previous Page", "", new Vector2(28+32+640-4, 217+12-6), (int i) => {
                switch(i)
                {
                    case 2: // left click
                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                        tween.TweenTo(this, t => t.offset, new Vector2(GlobalGraphics.Scale(-320), 0), 0.5f)
                            .Easing(EasingFunctions.ExponentialOut);
                        return true;
                }
                return false;
            }));
            controller.Add("Button5", new Button("Continue", "", new Vector2(238+32+640+5, 217+12-6), (int i) => {
                switch(i)
                {
                    case 2: // left click
                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                        if(!check3)
                        {
                            check3 = true;
                            SaveData.Save();
                            GlobalContent.GetSound("Back").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                            toggle = false;
                            tween.TweenTo(this, t => t.offset, new Vector2(GlobalGraphics.Scale(-640), GlobalGraphics.Scale(240)), 0.5f)
                                .Easing(EasingFunctions.ExponentialOut);
                            hiding = true;
                            ScreenManager.PushNavigation("Main Menu");
                            ScreenManager.GetScreen<MenuScreen>("Main Menu")?.Show();
                            ScreenManager.PushNavigation("Video");
                            ScreenManager.GetScreen<VideoScreen>("Video")?.Show();
                            ScreenManager.PushNavigation("Content");
                            ScreenManager.GetScreen<ContentScreen>("Content")?.Show();
                            ScreenManager.PushNavigation("Socials");
                            ScreenManager.GetScreen<SocialScreen>("Socials")?.Show();
                        }
                        return true;
                }
                return false;
            }));
            /*
            controller.Add("Button6", new Button("Download Update", "", new Vector2(228+32+640-4, 78+12-6), (int i) => {
                switch(i)
                {
                    case 2: // left click
                        if(UpdateManager.updateUrl != "")
                        {
                            GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                            UpdateManager.DownloadUpdate();
                        }
                        else
                        {
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                        }
                        return true;
                }
                return false;
            }));
            */
            for(int i = 0; i < baseTutorialText.Length; i++)
            {
                tutorialText[i] = new List<string>();
                for(int j = 0; j < baseTutorialText[i].Count; j++)
                {
                    tutorialText[i].Add(baseTutorialText[i][j]);
                }
            }
            controller.LoadContent(contentManager, graphicsDevice);
        }
    }
}
