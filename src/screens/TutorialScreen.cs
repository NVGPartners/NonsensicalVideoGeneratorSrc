using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using System.Globalization;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives.Zip;
using SharpCompress.Archives;
using SharpCompress.Common;

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
        public string title { get; } = "Tutorial";
        public int layer { get; set; } = 9;
        public ScreenType screenType { get; set; } = ScreenType.Hidden;
        public int currentPlacement { get; set; } = -1;
        private bool hiding = false;
        private bool showing = false;
        private bool toggle = false;
        private bool check = false;
        private bool extracting = false;
        private bool downloading = false;
        private string zip = "ffmpeg-release-full.7z";
        private string frie0r = "frei0r-v2.3.3_win64.zip";
        private int totalDownloads = 2;
        private int downloads = 0;
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
                    bool isTitleText = tutorialText[3][i].Contains("%EFFECT%");
                    bool isPerm1Text = tutorialText[3][i].Contains("%PERM1%");
                    bool isPerm2Text = tutorialText[3][i].Contains("%PERM2%");
                    bool isPerm3Text = tutorialText[3][i].Contains("%PERM3%");
                    // consents is a flag enum with 1 bit for each consent (3 bits)
                    if(UserConsent.consentForm != null)
                    {
                        tutorialText[3][i] = tutorialText[3][i].Replace("%EFFECT%", L.T(0, "Tutorial:PluginConsentForm_0", UserConsent.consentForm.name ?? L.T(0, "Addons:UnknownName")));
                        // first bit
                        tutorialText[3][i] = tutorialText[3][i].Replace("%PERM1%", UserConsent.consentForm.consents.HasFlag(Consents.DownloadFiles) ? L.T(0, "Addons:ConsentDownloadFiles") : "");
                        // second bit
                        tutorialText[3][i] = tutorialText[3][i].Replace("%PERM2%", UserConsent.consentForm.consents.HasFlag(Consents.ExecutePrograms) ? L.T(0, "Addons:ConsentExecutePrograms") : "");
                        // third bit
                        tutorialText[3][i] = tutorialText[3][i].Replace("%PERM3%", UserConsent.consentForm.consents.HasFlag(Consents.AddToLibrary) ? L.T(0, "Addons:ConsentAddToLibrary") : "");

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
            }
            showing = true;
            controller.overrideMousePosition = true;
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
            controller.overrideMousePosition = false;
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
            bool returned = false;
            if(Accessibility.selectedDisambiguationOption != -1
                && Accessibility.disambiguationOptions.Count > Accessibility.selectedDisambiguationOption)
            {
                switch(Accessibility.selectedDisambiguationOption)
                {
                    case 0:
                        if(offset.X == 0)
                            NextPage1(2, "");
                        break;
                    case 1:
                        if(offset.X == -GlobalGraphics.scaledWidth)
                            NextPage2(2, "");
                        break;
                    case 2:
                        if(offset.X == -GlobalGraphics.scaledWidth)
                            PreviousPage1(2, "");
                        break;
                    case 3:
                        if(offset.X == -GlobalGraphics.scaledWidth * 2)
                            PreviousPage2(2, "");
                        break;
                    case 4:
                        if(offset.X == -GlobalGraphics.scaledWidth * 2)
                            ContinueButton(2, "");
                        break;
                    case 5:
                        if(offset.X == -GlobalGraphics.scaledWidth)
                            ButtonFFmpeg(2, "");
                        break;
                    case 6:
                        if(offset.X == -GlobalGraphics.scaledWidth*3)
                            ContinueButton2(2, "");
                        break;
                    case 7:
                        if(offset.X == -GlobalGraphics.scaledWidth*3)
                            ContinueButton3(2, "");
                        break;
                }
                returned = true;
            }
            else
            {
                // Update controller
                //MouseState offsetMouseState = new MouseState((int)mouseState.X - (int)offset.X, mouseState.Y - (int)offset.Y, mouseState.ScrollWheelValue, mouseState.LeftButton, mouseState.MiddleButton, mouseState.RightButton, mouseState.XButton1, mouseState.XButton2);
                controller.mousePosition = new Vector2(MouseInput.MouseState.X - offset.X, MouseInput.MouseState.Y - offset.Y);
                if(controller.Update(gameTime, handleInput))
                    returned = true;
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
                "Tutorial:Page1_0",
                "Tutorial:Page1_1",
                "Tutorial:Page1_2",
                " ",
                "Tutorial:Page1_3",
                "%FFMPEGNAME%",
                "%FFPROBENAME%",
                " ",
                "Tutorial:Page1_4",
                "Tutorial:Page1_7",
            },
            new List<string>()
            { // PAGE 2
                "Tutorial:Page2_0",
                "Tutorial:Page2_1",
                "%FFMPEGSTATUS%",
                "%FFPROBESTATUS%",
                " ",
                "Tutorial:Page2_3",
                "Tutorial:Page2_4",
                "Tutorial:Page2_5",
            },
            new List<string>()
            { // PAGE 3
                "Tutorial:Page3_0",
                "Tutorial:Page3_1",
                "Tutorial:Page3_2",
                "Tutorial:Page3_3",
                " ",
                "Tutorial:Page3_4",
                "Tutorial:Page3_5",
                "Tutorial:Page3_6",
                "Tutorial:Page3_7",
            },
            new List<string>()
            { // PLUGIN CONSENT FORM
                "%EFFECT%",
                " ",
                "%PERM1%",
                " ",
                "%PERM2%",
                " ",
                "%PERM3%",
                " ",
                "Tutorial:PluginConsentForm_1",
                "Tutorial:PluginConsentForm_2",
                " ",
                "Tutorial:PluginConsentForm_3",
                "Tutorial:PluginConsentForm_4"
            }
        };
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // End existing spritebatch
            spriteBatch.End();
            // Use offset
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(GlobalGraphics.Scale(GlobalGraphics.drawOffset.X)+offset.X, GlobalGraphics.Scale(GlobalGraphics.drawOffset.Y)+offset.Y, 0));
            // Tutorial window
            Texture2D tutorialWindow = GlobalContent.GetTexture("TutorialWindow");
            spriteBatch.Draw(tutorialWindow, new Rectangle(GlobalGraphics.Scale(8), GlobalGraphics.Scale(36), GlobalGraphics.Scale(tutorialWindow.Width), GlobalGraphics.Scale(tutorialWindow.Height)), Color.White);
            spriteBatch.Draw(tutorialWindow, new Rectangle(GlobalGraphics.Scale(8+320), GlobalGraphics.Scale(36), GlobalGraphics.Scale(tutorialWindow.Width), GlobalGraphics.Scale(tutorialWindow.Height)), Color.White);
            spriteBatch.Draw(tutorialWindow, new Rectangle(GlobalGraphics.Scale(8+640), GlobalGraphics.Scale(36), GlobalGraphics.Scale(tutorialWindow.Width), GlobalGraphics.Scale(tutorialWindow.Height)), Color.White);
            spriteBatch.Draw(tutorialWindow, new Rectangle(GlobalGraphics.Scale(8+960), GlobalGraphics.Scale(36), GlobalGraphics.Scale(tutorialWindow.Width), GlobalGraphics.Scale(tutorialWindow.Height)), Color.White);
            controller.Draw(gameTime, spriteBatch);
            // Draw the center title bar text
            string title1 = L.T(0, "Tutorial:Title", "1", "3");
            string title2 = L.T(0, "Tutorial:Title", "2", "3");
            string title3 = L.T(0, "Tutorial:Title", "3", "3");
            string title4 = L.T(0, "Tutorial:UserConsentFormTitle");
            Vector2 titleSize1 = L.FontSmall().MeasureString(title1);
            GlobalContent.DrawString(spriteBatch, L.FontSmall(), title1, new Vector2(GlobalGraphics.scaledWidth / 2 - titleSize1.X / 2, (6 * GlobalGraphics.scale) - GlobalGraphics.Scale(1-32)), Color.White);
            Vector2 titleSize2 = L.FontSmall().MeasureString(title2);
            GlobalContent.DrawString(spriteBatch, L.FontSmall(), title2, new Vector2(GlobalGraphics.scaledWidth / 2 - titleSize2.X / 2 + GlobalGraphics.Scale(320), (6 * GlobalGraphics.scale) - GlobalGraphics.Scale(1-32)), Color.White);
            Vector2 titleSize3 = L.FontSmall().MeasureString(title3);
            GlobalContent.DrawString(spriteBatch, L.FontSmall(), title3, new Vector2(GlobalGraphics.scaledWidth / 2 - titleSize3.X / 2 + GlobalGraphics.Scale(640), (6 * GlobalGraphics.scale) - GlobalGraphics.Scale(1-32)), Color.White);
            Vector2 titleSize4 = L.FontSmall().MeasureString(title4);
            GlobalContent.DrawString(spriteBatch, L.FontSmall(), title4, new Vector2(GlobalGraphics.scaledWidth / 2 - titleSize4.X / 2 + GlobalGraphics.Scale(960), (6 * GlobalGraphics.scale) - GlobalGraphics.Scale(1-32)), Color.White);
            // Draw tutorial text
            for(int i = 0; i < tutorialText.Length; i++)
            {
                int offsetText = 0;
                for(int j = 0; j < tutorialText[i].Count; j++)
                {
                    string curText = tutorialText[i][j].Replace("%FFMPEGNAME%", L.T(0, "Tutorial:ProgramName", "ffmpeg"));
                    curText = curText.Replace("%FFPROBENAME%", L.T(0, "Tutorial:ProgramName", "ffprobe"));
                    curText = curText.Replace("%FFMPEGSTATUS%", L.T(0, "Tutorial:ProgramStatus", "ffmpeg", L.T(0, "Tutorial:ProgramChecking")));
                    curText = curText.Replace("%FFPROBESTATUS%", L.T(0, "Tutorial:ProgramStatus", "ffprobe", L.T(0, "Tutorial:ProgramChecking")));
                    if(curText.StartsWith("Tutorial:"))
                        curText = L.T(0, curText);
                    GlobalContent.DrawString(spriteBatch, L.FontSmall(), curText, new Vector2(GlobalGraphics.Scale(8+16+1+320*i), GlobalGraphics.Scale(60+offsetText+1)), Color.Black);
                    GlobalContent.DrawString(spriteBatch, L.FontSmall(), curText, new Vector2(GlobalGraphics.Scale(8+16+320*i), GlobalGraphics.Scale(60+offsetText)), Color.White);
                    offsetText += 16;
                }
            }
            // End offset spritebatch
            spriteBatch.End();
            // Remake spritebatch
            spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                null, null, null, Matrix.CreateTranslation(GlobalGraphics.Scale(GlobalGraphics.drawOffset.X), GlobalGraphics.Scale(GlobalGraphics.drawOffset.Y), 0));
        }
        private BackgroundWorker? dependencyWorker;
        private void DependencyCheckThread(object? sender, DoWorkEventArgs e)
        {
            // Get dependencies.
            UpdateManager.GetDependencyStatus();
            for (int h = 0; h < tutorialText.Length; h++)
            {
                for (int j = 0; j < tutorialText[h].Count; j++)
                {
                    string localizedInstalled = L.T(0, "Tutorial:ProgramInstalled");
                    string localizedNotFound = L.T(0, "Tutorial:ProgramNotFound");
                    string localizedUsingSystemPath = L.T(0, "Tutorial:ProgramUsingSystemPath");
                    string localizedWrongVersion = L.T(0, "Tutorial:ProgramWrongVersion");
                    string ffmpeg = UpdateManager.ffmpegInstalled ? localizedInstalled : localizedNotFound;
                    string ffprobe = UpdateManager.ffprobeInstalled ? localizedInstalled : localizedNotFound;
                    if(Global.useSystemFFmpeg)
                    {
                        if(!UpdateManager.ffmpegInstalled)
                            ffmpeg = localizedNotFound;
                        else
                        {
                            ffmpeg = localizedUsingSystemPath;
                        }
                    }
                    if(UpdateManager.ffmpegWrongVersion)
                        ffmpeg = localizedWrongVersion;
                    if(Global.useSystemFFprobe)
                    {
                        if(!UpdateManager.ffprobeInstalled)
                            ffprobe = localizedNotFound;
                        else
                            ffprobe = localizedUsingSystemPath;
                    }
                    if(UpdateManager.ffprobeWrongVersion)
                        ffprobe = localizedWrongVersion;
                    // FIXME: This button never gets removed because system ffmpeg is deprecated
                    if(Global.useSystemFFmpeg && Global.useSystemFFprobe
                        && UpdateManager.ffmpegInstalled && UpdateManager.ffprobeInstalled)
                    {
                        // Remove ffmpeg button
                        controller.Remove("ButtonFFmpeg");
                    }
                    tutorialText[h][j] = tutorialText[h][j].Replace("%FFMPEGSTATUS%", L.T(0, "Tutorial:ProgramStatus", "ffmpeg", ffmpeg));
                    tutorialText[h][j] = tutorialText[h][j].Replace("%FFPROBESTATUS%", L.T(0, "Tutorial:ProgramStatus", "ffprobe", ffprobe));
                }
            }
            check = false;
            // Dispose of worker
            if(dependencyWorker != null)
                dependencyWorker.Dispose();
            dependencyWorker = null;
        }
        private bool check3 = false;
        private BackgroundWorker? ffmpegDownloadWorker;
        private BackgroundWorker? freiorDownloadWorker;
        public bool PreviousPage1(int i, string n)
        {
            switch(i)
            {
                case 2: // left click
                    if(!UserConsent.needsConsent)
                    {
                        if(!Global.generator.progressText.ToLower().Contains("extract")
                            && !Global.generator.progressText.ToLower().Contains("downloading"))
                        {
                            GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
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
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        }
                    }
                    else
                    {
                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    }
                    return true;
            }
            return false;
        }
        public bool PreviousPage2(int i, string n)
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
                            GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
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
                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    }
                    return true;
            }
            return false;
        }
        public bool ButtonFFmpeg(int i, string n)
        {
            switch(i)
            {
                case 2: // left click
                    if(!UserConsent.needsConsent)
                    {
                        Global.generator.CleanUp();
                        Global.generator.progressText = L.T(0, "Tutorial:StatusStartDownload");

                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        tutorialText[1][2] = L.T(0, "Tutorial:ProgramStatus", "ffmpeg", L.T(0, "Tutorial:ProgramDownloading"));
                        tutorialText[1][3] = L.T(0, "Tutorial:ProgramStatus", "ffprobe", L.T(0, "Tutorial:ProgramDownloading"));
                        controller.Remove("ButtonFFmpeg");

                        // Create temp folder
                        Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "temp"));

                        // Download ffmpeg essential release from gyan.dev
                        Uri url = new("https://www.gyan.dev/ffmpeg/builds/"+zip);
                        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "temp", zip);

                        // Delete file if it exists
                        if(File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "temp", zip)))
                        {
                            File.Delete(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "temp", zip));
                        }
                        
                        // Download ffmpeg
                        try
                        {
                            using (WebClient client = new WebClient())
                            {
                                client.DownloadProgressChanged += (sender, e) => {
                                    downloading = true;
                                    Global.generator.progressText = L.T(0, "Tutorial:StatusDownload", e.ProgressPercentage.ToString(CultureInfo.InvariantCulture)) + " (" + (downloads+1) + "/" + totalDownloads + ")";
                                    tutorialText[1][2] = L.T(0, "Tutorial:ProgramStatus", "ffmpeg", Global.generator.progressText);
                                    tutorialText[1][3] = L.T(0, "Tutorial:ProgramStatus", "ffprobe", Global.generator.progressText);
                                };
                                client.DownloadFileCompleted += (sender, e) => {
                                    downloading = false;
                                    extracting = true;
                                    Global.generator.progressText = L.T(0, "Tutorial:ProgramExtracting") + " (" + (downloads+1) + "/" + totalDownloads + ")";
                                    tutorialText[1][2] = L.T(0, "Tutorial:ProgramStatus", "ffmpeg", Global.generator.progressText);
                                    tutorialText[1][3] = L.T(0, "Tutorial:ProgramStatus", "ffprobe", Global.generator.progressText);
                                    freiorDownloadWorker = new BackgroundWorker();
                                    freiorDownloadWorker.DoWork += (object? sender, DoWorkEventArgs e) => {
                                        DownloadFFmpegThread();
                                    };
                                    freiorDownloadWorker.RunWorkerAsync();
                                };
                                client.DownloadFileAsync(url, path);
                            }
                        }
                        catch
                        {
                            ConsoleOutput.WriteLine("Failed to download zip", Color.Red);
                            // Re-scan for ffmpeg
                            for(int h = 0; h < baseTutorialText.Length; h++)
                            {
                                tutorialText[h] = new List<string>();
                                for(int j = 0; j < baseTutorialText[h].Count; j++)
                                {
                                    tutorialText[h].Add(baseTutorialText[h][j]);
                                }
                            }
                            downloading = false;
                            extracting = false;
                            check = true;
                            dependencyWorker = new BackgroundWorker();
                            dependencyWorker.DoWork += DependencyCheckThread;
                            dependencyWorker.RunWorkerAsync();
                            Global.generator.progressText = L.T(0, "Tutorial:StatusFailDownload");
                            // Play sound effect
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        }
                    }
                    else
                    {
                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    }
                    return true;
            }
            return false;
        }
        public bool ContinueButton(int i, string n)
        {
            switch(i)
            {
                case 2: // left click
                    if(!UserConsent.needsConsent)
                    {
                        if(!extracting && !downloading)
                        {
                            GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            if(!check3)
                            {
                                check3 = true;
                                SaveData.Save();
                                GlobalContent.GetSound("Back").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
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
                                ScreenManager.PushNavigation("Menu");
                                ScreenManager.GetScreen<MenuScreen>("Menu")?.Show();
                                if(FramePlayer.audio != null && SaveData.saveValues["UseExternalVideoPlayer"] == "false")
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
                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    }
                    return true;
            }
            return false;
        }
        public bool ContinueButton3(int i, string n)
        {
            switch(i)
            {
                case 2: // left click
                    if(UserConsent.needsConsent)
                    {
                        if(!extracting && !downloading)
                        {
                            GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            if(UserConsent.consentForm != null)
                                UserConsent.Accept(UserConsent.consentForm.pluginName);
                            GlobalContent.GetSound("Back").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            toggle = false;
                            if(UserConsent.consentForm != null)
                                Global.generator.progressText = L.T(0, "Addons:StatusAddonConsentAccepted", UserConsent.consentForm?.pluginName);
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
                            ScreenManager.PushNavigation("Menu");
                            ScreenManager.GetScreen<MenuScreen>("Menu")?.Show();
                            if(FramePlayer.audio != null && SaveData.saveValues["UseExternalVideoPlayer"] == "false")
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
                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    }
                    break;
            }
            return false;
        }
        public bool ContinueButton2(int i, string n)
        {
            switch(i)
            {
                case 2: // left click
                    if(UserConsent.needsConsent)
                    {
                        if(!extracting && !downloading)
                        {
                            if(UserConsent.needsConsent)
                            {
                                // Workshop plugin should open workshop page
                                if(UserConsent.consentForm != null
                                    && UserConsent.consentForm.workshopId != ""
                                    && UserConsent.consentForm.rootPath.Contains("workshop"))
                                {
                                    GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                    ProcessStartInfo startInfo = new ProcessStartInfo();
                                    startInfo.FileName = "https://steamcommunity.com/sharedfiles/filedetails/?id=" + UserConsent.consentForm.workshopId;
                                    startInfo.UseShellExecute = true;
                                    Process.Start(startInfo);
                                }
                                else
                                {
                                    GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                                    //UserConsent.ShowConsentForm();
                                }
                                return true;
                            }
                        }
                    }
                    else
                    {
                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    }
                    break;
            }
            return false;
        }
        public bool NextPage1(int i, string n)
        {
            switch(i)
            {
                case 2: // left click
                    
                    if(!UserConsent.needsConsent)
                    {
                        // Get dependencies.
                        if(!extracting && !downloading)
                        {
                            GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
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
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        }
                    }
                    else
                    {
                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    }
                    return true;
            }
            return false;
        }
        public bool NextPage2(int i, string n)
        {
            switch(i)
            {
                case 2: // left click
                    if(!UserConsent.needsConsent)
                    {
                        if(!extracting && !downloading)
                        {
                            if(UpdateManager.ffmpegInstalled && UpdateManager.ffprobeInstalled)
                            {
                                GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
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
                                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            }
                        }
                        else
                        {
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        }
                    }
                    else
                    {
                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    }
                    return true;
            }
            return false;
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            // Clear all controllers
            controller.Clear();
            // Tutorial window
            GlobalContent.AddTexture("TutorialWindow", ThemeManager.LoadLayeredContent<Texture2D>("graphics/tutorialwindow"));
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
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "temp", zip);

            // Extract ffmpeg to temp folder
            try
            {
                //ZipFile.ExtractToDirectory(path, Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "temp"), true);
                using(SevenZipArchive archive = SevenZipArchive.Open(path))
                {
                    foreach(SevenZipArchiveEntry entry in archive.Entries)
                    {
                        entry.WriteToDirectory(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "temp"), new ExtractionOptions
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                }
            }
            catch(Exception e)
            {
                ConsoleOutput.WriteLine("Failed to extract zip: " + e.Message, Color.Red);
            }

            string ffmpegPath = "";
            string ffmpegBinPath = "";
            string ffmpegExePath = "";
            string ffprobeExePath = "";
            
            // Check to see if ffmpeg-*.*-release-build folder exists inside temp folder
            string[] directories = Directory.GetDirectories(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "temp"));
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
                    if(File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "ffmpeg.exe")))
                    {
                        File.Delete(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "ffmpeg.exe"));
                    }
                    File.Move(ffmpegExePath, Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "ffmpeg.exe"));
                }
                catch(Exception e)
                {
                    ConsoleOutput.WriteLine("Failed to move ffmpeg.exe: " + e.Message, Color.Red);
                    ffmpegExePath = "";
                }
            }
            if(ffprobeExePath != "")
            {
                try
                {
                    if(File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "ffprobe.exe")))
                    {
                        File.Delete(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "ffprobe.exe"));
                    }
                    File.Move(ffprobeExePath, Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "ffprobe.exe"));
                } catch(Exception e)
                {
                    ConsoleOutput.WriteLine("Failed to move ffprobe.exe: " + e.Message, Color.Red);
                    ffprobeExePath = "";
                }
            }

            // Continue to next download
            if(ffmpegExePath != "" && ffprobeExePath != "")
            {
                downloads += 1;
                // Download frei0r!!!
                Uri freiorUrl = new("https://github.com/dyne/frei0r/releases/download/v2.3.3/"+frie0r);
                string freiorPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "temp", frie0r);

                // Delete file if it exists
                if(File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "temp", frie0r)))
                {
                    File.Delete(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "temp", frie0r));
                }

                try
                {
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadProgressChanged += (sender, e) => {
                            downloading = true;
                            Global.generator.progressText = L.T(0, "Tutorial:StatusDownload", e.ProgressPercentage.ToString(CultureInfo.InvariantCulture)) + " (" + (downloads+1) + "/" + totalDownloads + ")";
                            tutorialText[1][2] = L.T(0, "Tutorial:ProgramStatus", "ffmpeg", Global.generator.progressText);
                            tutorialText[1][3] = L.T(0, "Tutorial:ProgramStatus", "ffprobe", Global.generator.progressText);
                        };
                        client.DownloadFileCompleted += (sender, e) => {
                            downloading = false;
                            extracting = true;
                            Global.generator.progressText = L.T(0, "Tutorial:ProgramExtracting") + " (" + (downloads+1) + "/" + totalDownloads + ")";
                            tutorialText[1][2] = L.T(0, "Tutorial:ProgramStatus", "ffmpeg", Global.generator.progressText);
                            tutorialText[1][3] = L.T(0, "Tutorial:ProgramStatus", "ffprobe", Global.generator.progressText);
                            freiorDownloadWorker = new BackgroundWorker();
                            freiorDownloadWorker.DoWork += (object? sender, DoWorkEventArgs e) => {
                                DownloadFrei0rThread();
                            };
                            freiorDownloadWorker.RunWorkerAsync();
                        };
                        client.DownloadFileAsync(freiorUrl, freiorPath);
                    }
                }
                catch
                {
                    ConsoleOutput.WriteLine("Failed to download zip", Color.Red);
                    // Re-scan for ffmpeg
                    for(int h = 0; h < baseTutorialText.Length; h++)
                    {
                        tutorialText[h] = new List<string>();
                        for(int j = 0; j < baseTutorialText[h].Count; j++)
                        {
                            tutorialText[h].Add(baseTutorialText[h][j]);
                        }
                    }
                    downloading = false;
                    extracting = false;
                    check = true;
                    dependencyWorker = new BackgroundWorker();
                    dependencyWorker.DoWork += DependencyCheckThread;
                    dependencyWorker.RunWorkerAsync();
                    Global.generator.progressText = L.T(0, "Tutorial:StatusFailDownload");
                    // Play sound effect
                    GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                }
            }
            else
            {
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
                extracting = false;
                downloading = false;
                Global.generator.progressText = L.T(0, "Tutorial:StatusFailDownload");
                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
            }
        }
        public void DownloadFrei0rThread()
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "temp", frie0r);

            // Extract frei0r to temp folder
            try
            {
                //ZipFile.ExtractToDirectory(path, Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "temp"), true);
                using(ZipArchive archive = ZipArchive.Open(path))
                {
                    foreach(ZipArchiveEntry entry in archive.Entries)
                    {
                        entry.WriteToDirectory(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "temp"), new ExtractionOptions
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                }
            }
            catch(Exception e)
            {
                ConsoleOutput.WriteLine("Failed to extract zip: " + e.Message, Color.Red);
            }

            string freiorPath = "";
            string freiorFilterPath = "";
            
            // Check to see if ffmpeg-*.*-release-build folder exists inside temp folder
            string[] directories = Directory.GetDirectories(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "temp"));
            foreach(string directory in directories)
            {
                if(directory.Contains("frei0r-"))
                {
                    freiorPath = directory;
                    break;
                }
            }
            
            // Check to see if bin folder exists inside ffmpeg-*.*-release-build folder
            if(freiorPath != "")
            {
                directories = Directory.GetDirectories(freiorPath);
                foreach(string directory in directories)
                {
                    if(directory.Contains("filter"))
                    {
                        freiorFilterPath = directory;
                        break;
                    }
                }
            }

            bool success = false;

            // Check to see if ffmpeg.exe exists inside bin folder
            if(freiorFilterPath != "")
            {
                string[] files = Directory.GetFiles(freiorFilterPath);
                foreach(string file in files)
                {
                    // Move dll to .\frei0r-1
                    string freior1 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "frei0r-1");
                    try
                    {
                        // Create frei0r-1 folder if it doesn't exist
                        if(!Directory.Exists(freior1))
                        {
                            Directory.CreateDirectory(freior1);
                        }
                        // Delete dll files if they already exist
                        if(File.Exists(Path.Combine(freior1, Path.GetFileName(file))))
                        {
                            File.Delete(Path.Combine(freior1, Path.GetFileName(file)));
                        }
                        File.Move(file, Path.Combine(freior1, Path.GetFileName(file)));
                        success = true;
                    }
                    catch(Exception e)
                    {
                        ConsoleOutput.WriteLine("Failed to move frei0r plugins: " + e.Message, Color.Red);
                    }
                }
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
            extracting = false;
            downloading = false;
            downloads += 1;

            // Play sound effect
            if(success)
            {
                Global.generator.progressText = L.T(0, "Tutorial:StatusDownloadComplete");
                GlobalContent.GetSound("RenderComplete").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
            }
            else
            {
                Global.generator.progressText = L.T(0, "Tutorial:StatusFailDownload");
                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
            }
        }
    }
}
