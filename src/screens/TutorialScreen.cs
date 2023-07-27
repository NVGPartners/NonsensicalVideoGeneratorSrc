#if MONOGAME
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended.Tweening;
using System.Diagnostics;

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
        public int layer { get; } = 9;
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
            if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
            {
                if(UserConsent.needsConsent)
                {
                    offset = new(GlobalGraphics.Scale(-960), GlobalGraphics.Scale(240)); // from bottom to top
                    tween.TweenTo(this, t => t.offset, new Vector2(GlobalGraphics.Scale(-960), 0), 0.5f)
                        .Easing(EasingFunctions.ExponentialOut);
                }
                else
                {
                    offset = new(0, GlobalGraphics.Scale(240)); // from bottom to top
                    tween.TweenTo(this, t => t.offset, new Vector2(0, 0), 0.5f)
                        .Easing(EasingFunctions.ExponentialOut);
                }
            }
            else
            {
                if(UserConsent.needsConsent)
                {
                    offset = new(GlobalGraphics.Scale(-960), 0);
                }
                else
                {
                    offset = new(0, 0);
                }
            }
            if(UserConsent.needsConsent)
            {
                for(int h = 0; h < baseTutorialText.Length; h++)
                {
                    tutorialText[h] = new List<string>();
                    for(int j = 0; j < baseTutorialText[h].Count; j++)
                    {
                        tutorialText[h].Add(baseTutorialText[h][j]);
                    }
                }
                for(int i = 0; i < tutorialText[3].Count; i++)
                {
                    bool isPerm1Text = tutorialText[3][i].Contains("%PERM1%");
                    bool isPerm2Text = tutorialText[3][i].Contains("%PERM2%");
                    bool isPerm3Text = tutorialText[3][i].Contains("%PERM3%");
                    // consents is a flag enum with 1 bit for each consent (3 bits)
                    tutorialText[3][i] = tutorialText[3][i].Replace("%PLUGIN%", UserConsent.consentForm?.name ?? "Unknown");
                    // first bit
                    tutorialText[3][i] = tutorialText[3][i].Replace("%PERM1%", UserConsent.consentForm.consents.HasFlag(Consents.DownloadFiles) ? "The ability to download these files:" : "");
                    // second bit
                    tutorialText[3][i] = tutorialText[3][i].Replace("%PERM2%", UserConsent.consentForm.consents.HasFlag(Consents.ExecutePrograms) ? "The ability to execute these programs:" : "");
                    // third bit
                    tutorialText[3][i] = tutorialText[3][i].Replace("%PERM3%", UserConsent.consentForm.consents.HasFlag(Consents.AddToLibrary) ? "The ability to add these files to the library:" : "");
                    // append to list right here for each param in consent form for flags
                    if(isPerm1Text)
                    {
                        if(UserConsent.consentForm.consentParams.ContainsKey(Consents.DownloadFiles))
                        {
                            foreach(string param in UserConsent.consentForm.consentParams[Consents.DownloadFiles])
                            {
                                tutorialText[3].Insert(i + 1, " - " + param);
                            }
                        }
                    }
                    if(isPerm2Text)
                    {
                        if(UserConsent.consentForm.consentParams.ContainsKey(Consents.ExecutePrograms))
                        {
                            foreach(string param in UserConsent.consentForm.consentParams[Consents.ExecutePrograms])
                            {
                                tutorialText[3].Insert(i + 1, " - " + param);
                            }
                        }
                    }
                    if(isPerm3Text)
                    {
                        if(UserConsent.consentForm.consentParams.ContainsKey(Consents.AddToLibrary))
                        {
                            foreach(string param in UserConsent.consentForm.consentParams[Consents.AddToLibrary])
                            {
                                tutorialText[3].Insert(i + 1, " - " + param);
                            }
                        }
                    }
                }
            }
            showing = true;
        }
        public void Hide()
        {
            toggle = false;
            if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
            {
                offset = new(0, 0); // from top to bottom
                tween.TweenTo(this, t => t.offset, new Vector2(0, GlobalGraphics.Scale(240)), 0.5f)
                    .Easing(EasingFunctions.ExponentialOut);
            }
            else
            {
                offset = new(0, GlobalGraphics.Scale(240));
            }
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
            bool returned = false;
            if(Accessibility.selectedDisambiguationOption != -1
                && Accessibility.disambiguationOptions.Count > Accessibility.selectedDisambiguationOption)
            {
                switch(Accessibility.selectedDisambiguationOption)
                {
                    case 0:
                        if(offset.X == 0)
                            NextPage1(2);
                        break;
                    case 1:
                        if(offset.X == -GlobalGraphics.scaledWidth)
                            NextPage2(2);
                        break;
                    case 2:
                        if(offset.X == -GlobalGraphics.scaledWidth)
                            PreviousPage1(2);
                        break;
                    case 3:
                        if(offset.X == -GlobalGraphics.scaledWidth * 2)
                            PreviousPage2(2);
                        break;
                    case 4:
                        if(offset.X == -GlobalGraphics.scaledWidth * 2)
                            ContinueButton(2);
                        break;
                    case 5:
                        if(offset.X == -GlobalGraphics.scaledWidth)
                            ButtonFFmpeg(2);
                        break;
                    case 6:
                        if(offset.X == -GlobalGraphics.scaledWidth*3)
                            ContinueButton2(2);
                        break;
                    case 7:
                        if(offset.X == -GlobalGraphics.scaledWidth*3)
                            ContinueButton3(2);
                        break;
                }
                returned = true;
            }
            else
            {
                // Offset mouse position
                MouseState offsetMouseState = new MouseState((int)mouseState.X - (int)offset.X, mouseState.Y - (int)offset.Y, mouseState.ScrollWheelValue, mouseState.LeftButton, mouseState.MiddleButton, mouseState.RightButton, mouseState.XButton1, mouseState.XButton2);
                MouseInput._mouseState = offsetMouseState;
                MouseState offsetLastMouseState = new MouseState((int)mouseState.X - (int)offset.X, lastMouseState.Y - (int)offset.Y, lastMouseState.ScrollWheelValue, lastMouseState.LeftButton, lastMouseState.MiddleButton, lastMouseState.RightButton, lastMouseState.XButton1, lastMouseState.XButton2);
                MouseInput.LastMouseState = offsetLastMouseState;
                // Update controller
                if(controller.Update(gameTime, handleInput))
                    returned = true;
                // Revert
                MouseInput._mouseState = mouseState;
                MouseInput.LastMouseState = lastMouseState;
            }
            return returned;
        }
        public static List<string>[] tutorialText = new List<string>[4]
        {
            new List<string>(),
            new List<string>(),
            new List<string>(),
            new List<string>()
        };
        public static List<string>[] baseTutorialText = new List<string>[4]
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
                "Click \"Next Page\" to re-check the status of each program.",
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
                "Check console with F5 to see which plugins are broken, if so.",
                "",
                "Click \"Continue\" to scan plugins and continue if successful."
            },
            new List<string>()
            { // PLUGIN CONSENT FORM
                "The effect %PLUGIN% requires additional permissions:",
                "",
                "%PERM1%",
                "",
                "%PERM2%",
                "",
                "%PERM3%",
                "",
                "If you do not agree, unsubscribe from the effect by visiting its",
                "workshop page from the button below.",
                "",
                "Click \"Continue\" to accept the permissions, or if unsubscribed,",
                "decline the permissions."
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
            spriteBatch.Draw(tutorialWindow, new Rectangle(GlobalGraphics.Scale(8+960), GlobalGraphics.Scale(36), GlobalGraphics.Scale(tutorialWindow.Width), GlobalGraphics.Scale(tutorialWindow.Height)), Color.White);
            controller.Draw(gameTime, spriteBatch);
            // Draw the center title bar text
            Vector2 titleSize1 = GlobalGraphics.fontMunroSmall.MeasureString(title + ": Page 1/3");
            spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, title + ": Page 1/3", new Vector2(GlobalGraphics.scaledWidth / 2 - titleSize1.X / 2, (6 * GlobalGraphics.scale) - GlobalGraphics.Scale(1-32)), Color.White);
            Vector2 titleSize2 = GlobalGraphics.fontMunroSmall.MeasureString(title + ": Page 2/3");
            spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, title + ": Page 2/3", new Vector2(GlobalGraphics.scaledWidth / 2 - titleSize2.X / 2 + GlobalGraphics.Scale(320), (6 * GlobalGraphics.scale) - GlobalGraphics.Scale(1-32)), Color.White);
            Vector2 titleSize3 = GlobalGraphics.fontMunroSmall.MeasureString(title + ": Page 3/3");
            spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, title + ": Page 3/3", new Vector2(GlobalGraphics.scaledWidth / 2 - titleSize3.X / 2 + GlobalGraphics.Scale(640), (6 * GlobalGraphics.scale) - GlobalGraphics.Scale(1-32)), Color.White);
            Vector2 titleSize4 = GlobalGraphics.fontMunroSmall.MeasureString("User Consent Form");
            spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, "User Consent Form", new Vector2(GlobalGraphics.scaledWidth / 2 - titleSize4.X / 2 + GlobalGraphics.Scale(960), (6 * GlobalGraphics.scale) - GlobalGraphics.Scale(1-32)), Color.White);
            // Draw tutorial text
            for(int i = 0; i < tutorialText.Length; i++)
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
                        if(tutorialText[i][j].Contains("Downloading..."))
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
                            }
                            spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, "Downloading...", new Vector2(GlobalGraphics.Scale(offset+8+16+320*i), GlobalGraphics.Scale(60+offsetText)), Color.BlueViolet);
                        }
                        if(tutorialText[i][j].Contains("Extracting..."))
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
                            }
                            spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, "Extracting...", new Vector2(GlobalGraphics.Scale(offset+8+16+320*i), GlobalGraphics.Scale(60+offsetText)), Color.BlueViolet);
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
                        {
                            ffmpeg = "Using system PATH";
                        }
                    }
                    if(Global.useSystemFFprobe)
                    {
                        if(!UpdateManager.ffprobeInstalled)
                            ffprobe = "Not found";
                        else
                            ffprobe = "Using system PATH";
                    }
                    if(Global.useSystemFFmpeg && Global.useSystemFFprobe
                        && UpdateManager.ffmpegInstalled && UpdateManager.ffprobeInstalled)
                    {
                        // Remove ffmpeg button
                        controller.Remove("ButtonFFmpeg");
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
        private BackgroundWorker ffmpegDownloadWorker;
        public bool PreviousPage1(int i)
        {
            switch(i)
            {
                case 2: // left click
                    if(!UserConsent.needsConsent)
                    {
                        if(!Global.generator.progressText.ToLower().Contains("extract")
                            && !Global.generator.progressText.ToLower().Contains("downloading"))
                        {
                            GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
                            {
                                tween.TweenTo(this, t => t.offset, new Vector2(GlobalGraphics.Scale(0), 0), 0.5f)
                                    .Easing(EasingFunctions.ExponentialOut);
                            }
                            else
                            {
                                offset = new Vector2(GlobalGraphics.Scale(0), 0);
                            }
                        }
                        else
                        {
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        }
                    }
                    else
                    {
                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    }
                    return true;
            }
            return false;
        }
        public bool PreviousPage2(int i)
        {
            switch(i)
            {
                case 2: // left click
                    if(!UserConsent.needsConsent)
                    {
                        // Get dependencies.
                        if(!Global.generator.progressText.ToLower().Contains("extract")
                            && !Global.generator.progressText.ToLower().Contains("downloading"))
                        {
                            GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
                            {
                                tween.TweenTo(this, t => t.offset, new Vector2(GlobalGraphics.Scale(-320), 0), 0.5f)
                                    .Easing(EasingFunctions.ExponentialOut);
                            }
                            else
                            {
                                offset = new Vector2(GlobalGraphics.Scale(-320), 0);
                            }
                        }
                    }
                    else
                    {
                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    }
                    return true;
            }
            return false;
        }
        public bool ButtonFFmpeg(int i)
        {
            switch(i)
            {
                case 2: // left click
                    if(!UserConsent.needsConsent)
                    {
                        Global.generator.progressText = "Starting download...";

                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        tutorialText[1][3] = " - ffmpeg: Downloading...";
                        tutorialText[1][4] = " - ffprobe: Downloading...";
                        controller.Remove("ButtonFFmpeg");

                        // Create temp folder
                        Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "temp"));

                        // Download ffmpeg essential release from gyan.dev
                        System.Uri url = new("https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip");
                        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "temp", "ffmpeg-release-essentials.zip");

                        // Delete file if it exists
                        if(File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "temp", "ffmpeg-release-essentials.zip")))
                        {
                            File.Delete(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "temp", "ffmpeg-release-essentials.zip"));
                        }
                        
                        // Download ffmpeg
                        try
                        {
                            using (WebClient client = new WebClient())
                            {
                                client.DownloadProgressChanged += (sender, e) => {
                                    Global.generator.progressText = "Downloading... (" + e.ProgressPercentage + "%)";
                                    tutorialText[1][3] = " - ffmpeg: Downloading... (" + e.ProgressPercentage + "%)";
                                    tutorialText[1][4] = " - ffprobe: Downloading... (" + e.ProgressPercentage + "%)";
                                };
                                client.DownloadFileCompleted += (sender, e) => {
                                    Global.generator.progressText = "Extracting...";
                                    tutorialText[1][3] = " - ffmpeg: Extracting...";
                                    tutorialText[1][4] = " - ffprobe: Extracting...";
                                    ffmpegDownloadWorker = new BackgroundWorker();
                                    ffmpegDownloadWorker.DoWork += (object sender, DoWorkEventArgs e) => {
                                        DownloadFFmpegThread();
                                    };
                                    ffmpegDownloadWorker.RunWorkerAsync();
                                };
                                client.DownloadFileAsync(url, path);
                            }
                        }
                        catch
                        {
                            ConsoleOutput.WriteLine("Failed to download ffmpeg-release-essentials.zip", Color.Red);
                            // Re-scan for ffmpeg
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
                            Global.generator.progressText = "Failed to download.";
                            // Play sound effect
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        }
                    }
                    else
                    {
                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    }
                    return true;
            }
            return false;
        }
        public bool ContinueButton(int i)
        {
            switch(i)
            {
                case 2: // left click
                    if(!UserConsent.needsConsent)
                    {
                        if(!Global.generator.progressText.ToLower().Contains("extract")
                            && !Global.generator.progressText.ToLower().Contains("downloading"))
                        {
                            GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            if(!check3)
                            {
                                check3 = true;
                                SaveData.Save();
                                GlobalContent.GetSound("Back").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                toggle = false;
                                if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
                                {
                                    tween.TweenTo(this, t => t.offset, new Vector2(GlobalGraphics.Scale(-640), GlobalGraphics.Scale(240)), 0.5f)
                                        .Easing(EasingFunctions.ExponentialOut);
                                }
                                else
                                {
                                    offset = new Vector2(GlobalGraphics.Scale(-640), GlobalGraphics.Scale(240));
                                }
                                hiding = true;
                                FramePlayer.canPlayBgMusic = true;
                                ScreenManager.PushNavigation("Main Menu");
                                ScreenManager.GetScreen<MenuScreen>("Main Menu")?.Show();
                                if(FramePlayer.audio != null)
                                {
                                    ScreenManager.PushNavigation("Video");
                                    ScreenManager.GetScreen<VideoScreen>("Video")?.Show();
                                }
                                ScreenManager.PushNavigation("Content");
                                ScreenManager.GetScreen<ContentScreen>("Content")?.Show();
                                ScreenManager.PushNavigation("Socials");
                                ScreenManager.GetScreen<SocialScreen>("Socials")?.Show();
                                if(SteamManager.initialized)
                                    PluginHandler.LoadWorkshop();
                                else
                                    PluginHandler.LoadPluginsThreaded();
                            }
                        }
                    }
                    else
                    {
                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    }
                    return true;
            }
            return false;
        }
        public bool ContinueButton3(int i)
        {
            switch(i)
            {
                case 2: // left click
                    if(UserConsent.needsConsent)
                    {
                        if(!Global.generator.progressText.ToLower().Contains("extract")
                            && !Global.generator.progressText.ToLower().Contains("downloading"))
                        {
                            GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            UserConsent.Accept(UserConsent.consentForm?.pluginName);
                            GlobalContent.GetSound("Back").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            toggle = false;
                            Global.generator.progressText = $"Plugin {UserConsent.consentForm?.pluginName} accepted.";
                            if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
                            {
                                tween.TweenTo(this, t => t.offset, new Vector2(GlobalGraphics.Scale(-640), GlobalGraphics.Scale(240)), 0.5f)
                                    .Easing(EasingFunctions.ExponentialOut);
                            }
                            else
                            {
                                offset = new Vector2(GlobalGraphics.Scale(-640), GlobalGraphics.Scale(240));
                            }
                            hiding = true;
                            FramePlayer.canPlayBgMusic = true;
                            ScreenManager.PushNavigation("Main Menu");
                            ScreenManager.GetScreen<MenuScreen>("Main Menu")?.Show();
                            if(FramePlayer.audio != null)
                            {
                                ScreenManager.PushNavigation("Video");
                                ScreenManager.GetScreen<VideoScreen>("Video")?.Show();
                            }
                            ScreenManager.PushNavigation("Content");
                            ScreenManager.GetScreen<ContentScreen>("Content")?.Show();
                            ScreenManager.PushNavigation("Socials");
                            ScreenManager.GetScreen<SocialScreen>("Socials")?.Show();
                            if(SteamManager.initialized)
                                PluginHandler.LoadWorkshop();
                            else
                                PluginHandler.LoadPluginsThreaded();
                            return true;
                        }
                    }
                    else
                    {
                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    }
                    break;
            }
            return false;
        }
        public bool ContinueButton2(int i)
        {
            switch(i)
            {
                case 2: // left click
                    if(UserConsent.needsConsent)
                    {
                        if(!Global.generator.progressText.ToLower().Contains("extract")
                            && !Global.generator.progressText.ToLower().Contains("downloading"))
                        {
                            if(UserConsent.needsConsent)
                            {
                                // Workshop plugin should open workshop page
                                if(UserConsent.consentForm.workshopId != ""
                                    && UserConsent.consentForm.rootPath.Contains("workshop"))
                                {
                                    GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                    ProcessStartInfo startInfo = new ProcessStartInfo();
                                    startInfo.FileName = "https://steamcommunity.com/sharedfiles/filedetails/?id=" + UserConsent.consentForm.workshopId;
                                    startInfo.UseShellExecute = true;
                                    Process.Start(startInfo);
                                }
                                else
                                {
                                    GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                    //UserConsent.ShowConsentForm();
                                }
                                return true;
                            }
                        }
                    }
                    else
                    {
                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    }
                    break;
            }
            return false;
        }
        public bool NextPage1(int i)
        {
            switch(i)
            {
                case 2: // left click
                    if(!UserConsent.needsConsent)
                    {
                        // Get dependencies.
                        if(!Global.generator.progressText.ToLower().Contains("extract")
                            && !Global.generator.progressText.ToLower().Contains("downloading"))
                        {
                            GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
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
                            if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
                            {
                                tween.TweenTo(this, t => t.offset, new Vector2(GlobalGraphics.Scale(-320), 0), 0.5f)
                                    .Easing(EasingFunctions.ExponentialOut);
                            }
                            else
                            {
                                offset = new Vector2(GlobalGraphics.Scale(-320), 0);
                            }
                        }
                        else
                        {
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        }
                    }
                    else
                    {
                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    }
                    return true;
            }
            return false;
        }
        public bool NextPage2(int i)
        {
            switch(i)
            {
                case 2: // left click
                    if(!UserConsent.needsConsent)
                    {
                        if(!Global.generator.progressText.ToLower().Contains("extract")
                            && !Global.generator.progressText.ToLower().Contains("downloading"))
                        {
                            if(UpdateManager.ffmpegInstalled && UpdateManager.ffprobeInstalled)
                            {
                                GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
                                {
                                    tween.TweenTo(this, t => t.offset, new Vector2(GlobalGraphics.Scale(-640), 0), 0.5f)
                                        .Easing(EasingFunctions.ExponentialOut);
                                }
                                else
                                {
                                    offset = new Vector2(GlobalGraphics.Scale(-640), 0);
                                }
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
                                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            }
                        }
                        else
                        {
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        }
                    }
                    else
                    {
                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    }
                    return true;
            }
            return false;
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            // Tutorial window
            GlobalContent.AddTexture("TutorialWindow", contentManager.Load<Texture2D>("graphics/tutorialwindow"));
            // PAGE 1
            controller.Add("Button1", new Button("Next Page", "", new Vector2(237+32+2, 217+12-6), NextPage1));
            // PAGE 2
            controller.Add("Button2", new Button("Next Page", "", new Vector2(237+32+320+2, 217+12-6), NextPage2));
            controller.Add("Button3", new Button("Previous Page", "", new Vector2(28+32+320-4, 217+12-6), PreviousPage1));
            // PAGE 3
            controller.Add("Button4", new Button("Previous Page", "", new Vector2(28+32+640-4, 217+12-6), PreviousPage2));
            controller.Add("Button5", new Button("Continue", "", new Vector2(238+32+640+5, 217+12-6), ContinueButton));
            // PAGE 2 but it's dynamic
            controller.Add("ButtonFFmpeg", new Button("Download from gyan.dev", "", new Vector2(28+32+320-4+135, 91+12-6), ButtonFFmpeg));
            for(int i = 0; i < baseTutorialText.Length; i++)
            {
                tutorialText[i] = new List<string>();
                for(int j = 0; j < baseTutorialText[i].Count; j++)
                {
                    tutorialText[i].Add(baseTutorialText[i][j]);
                }
            }
            // USER CONSENT FORM
            controller.Add("ButtonContinue3", new Button("View Workshop Page", "", new Vector2(28+32+960+4, 217+12-6), ContinueButton2));
            controller.Add("ButtonContinue2", new Button("Continue", "", new Vector2(238+32+960+5, 217+12-6), ContinueButton3));
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
        public void DownloadFFmpegThread()
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "temp", "ffmpeg-release-essentials.zip");

            // Extract ffmpeg to temp folder
            try
            {
                ZipFile.ExtractToDirectory(path, Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "temp"), true);
            }
            catch
            {
                ConsoleOutput.WriteLine("Failed to extract ffmpeg-release-essentials.zip", Color.Red);
            }

            string ffmpegPath = "";
            string ffmpegBinPath = "";
            string ffmpegExePath = "";
            string ffprobeExePath = "";
            
            // Check to see if ffmpeg-*.*-release-build folder exists inside temp folder
            string[] directories = Directory.GetDirectories(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "temp"));
            foreach(string directory in directories)
            {
                if(directory.Contains("ffmpeg-"))
                {
                    ffmpegPath = directory;
                    break;
                }
            }
            
            // Check to see if bin folder exists inside ffmpeg-*.*-release-build folder
            if(ffmpegPath != "")
            {
                directories = Directory.GetDirectories(ffmpegPath);
                foreach(string directory in directories)
                {
                    if(directory.Contains("bin"))
                    {
                        ffmpegBinPath = directory;
                        break;
                    }
                }
            }

            // Check to see if ffmpeg.exe exists inside bin folder
            if(ffmpegBinPath != "")
            {
                string[] files = Directory.GetFiles(ffmpegBinPath);
                foreach(string file in files)
                {
                    if(file.Contains("ffmpeg.exe"))
                    {
                        ffmpegExePath = file;
                    }
                    else if(file.Contains("ffprobe.exe"))
                    {
                        ffprobeExePath = file;
                    }
                    if (ffmpegExePath != "" && ffprobeExePath != "")
                    {
                        break;
                    }
                }
            }

            if(ffmpegExePath != "")
            {
                // Move ffmpeg.exe and ffprobe.exe to the root folder
                try
                {
                    File.Move(ffmpegExePath, Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ffmpeg.exe"));
                }
                catch {}
            }
            if(ffprobeExePath != "")
            {
                try
                {
                    File.Move(ffprobeExePath, Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ffprobe.exe"));
                } catch {}
            }

            // Re-scan for ffmpeg
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
            
            // Play sound effect
            if(ffmpegExePath != "" && ffprobeExePath != "")
            {
                Global.generator.progressText = "Download complete.";
                GlobalContent.GetSound("RenderComplete").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
            }
            else
            {
                Global.generator.progressText = "Failed to download.";
                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
            }
        }
    }
}
#endif
