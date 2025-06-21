using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System.IO;
using System.Windows.Forms;
using System.Globalization;
using MonoGame.Extended.VideoPlayback;
using System.Reflection;
using System.Collections.Generic;

namespace NonsensicalVideoGenerator
{
    public enum WindowState
    {
        Focused,
        Unfocused,
    }
    public enum MusicState
    {
        Playing,
        Paused,
    }
    public class UserInterface : Game
    {
        public static UserInterface? instance;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch? _spriteBatch;
        private WindowState _windowState = WindowState.Unfocused;
        private MusicState _musicState = MusicState.Paused;
        public MonoGame.Extended.Framework.Media.VideoPlayer? videoPlayer;
        public MonoGame.Extended.Framework.Media.Video? video;
        public string videoPath = "";
        public int music = 0;
        public bool introFinished = false;
        public bool introStarted = false;
        public double introGoal = 0;
        public UserInterface()
        {
            ConsoleOutput.WriteLine("Creating new UserInterface instance...", Color.Transparent);
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            instance = this;
            if(Global.parameters.Contains("-unlockfps"))
                SetFPSUnlock(true);
        }
        public void SetFPSUnlock(bool unlock)
        {
            IsFixedTimeStep = !unlock;
            _graphics.SynchronizeWithVerticalRetrace = !unlock;
        }
        public void Resize(int width, int height)
        {
            _graphics.PreferredBackBufferWidth = width;
            _graphics.PreferredBackBufferHeight = height;
            _graphics.ApplyChanges();
            GlobalGraphics.preferredResolution = new Point(width, height);
        }
        public AspectRatio GetClosestHardwareAspectRatio()
        {
            // Get the aspect ratio of the screen as a fraction (4:3, 16:9, etc).
            double aspectRatioDouble = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.AspectRatio;
            (int, int) aspectRatioFraction = Global.ConvertToFraction(aspectRatioDouble);
            // Find the closest aspect ratio to the screen's aspect ratio.
            AspectRatio closestAspectRatio = AspectRatio.All.OrderBy(x => Math.Abs(x.width / (double)x.height - aspectRatioFraction.Item1 / (double)aspectRatioFraction.Item2)).First();
            return closestAspectRatio;
        }
        public void ToggleFullscreen()
        {
            SetFullscreen(!GlobalGraphics.fullScreen);
        }
        public void SetFullscreen(bool fullscreen)
        {
            AspectRatio aspectRatio = new();
            if(SaveData.saveValues["MatchAspectRatio"] == "true")
                aspectRatio = GlobalGraphics.FindMatchingAspectRatio();
            GlobalGraphics.fullScreen = fullscreen;
            // Borderless
            Window.IsBorderless = fullscreen;
            _graphics.HardwareModeSwitch = fullscreen;
            if(fullscreen)
            {
                // Set preferred resolution to screen resolution.
                aspectRatio.preferredResolution = new Point(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / GlobalGraphics.scale, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / GlobalGraphics.scale);
                // Calculate draw offset into center of screen.
                aspectRatio.drawOffset = new Vector2((aspectRatio.preferredResolution.X - (GlobalGraphics.scaledWidth / GlobalGraphics.scale)) / 2, (aspectRatio.preferredResolution.Y - (GlobalGraphics.scaledHeight / GlobalGraphics.scale)) / 2);
            }
            GlobalGraphics.SetAspectRatio(aspectRatio);
            _graphics.ApplyChanges();
            Form? windowForm = Control.FromHandle(Window.Handle) as Form;
            if(windowForm != null)
            {
                // Place window in center of screen.
                if(fullscreen)
                    windowForm.Location = new System.Drawing.Point(0, 0);
                else
                    CenterToScreen();
            }
            if(!fullscreen)
                GlobalGraphics.SetAspectRatio(SaveData.saveValues["MatchAspectRatio"] == "true" ? GlobalGraphics.FindMatchingAspectRatio() : new AspectRatio());
        }
        public void CenterToScreen()
        {
            Form? windowForm = Control.FromHandle(Window.Handle) as Form;
            if(windowForm != null && Screen.PrimaryScreen != null)
            {
                // Place window in center of screen.
                windowForm.Location = new System.Drawing.Point(Screen.PrimaryScreen.WorkingArea.Width / 2 - windowForm.Width / 2, Screen.PrimaryScreen.WorkingArea.Height / 2 - windowForm.Height / 2);
            }
        }
        public void SetAlwaysOnTop(bool alwaysOnTop)
        {
            Form? windowForm = Control.FromHandle(Window.Handle) as Form;
            if(windowForm != null)
            {
                windowForm.TopMost = alwaysOnTop;
            }
        }
        public void SetNativeCursor(bool useNativeCursor)
        {
            IsMouseVisible = useNativeCursor;
        }
        // Drag and drop support.
        private void DragEnter(object? sender, DragEventArgs e)
        {
            if(e != null && e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            Global.dragDrop = true;
        }
        private void DragDrop(object? sender, DragEventArgs e)
        {
            if(e != null && e.Data != null)
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                Global.dragDropFiles = files != null ? files.ToList() : new List<string>();
                Global.dragDrop = false;
            }
        }
        private void DragLeave(object? sender, EventArgs e)
        {
            Global.dragDrop = false;
        }
        protected override void Initialize()
        {
            if(bool.Parse(SaveData.saveValues["EnableDiscordRPC"]))
                DiscordRPC.Initialize();
            ConsoleOutput.WriteLine("Starting initialization for v" + Global.productVersion + "...", Color.Transparent);
            Kiwano.Check();
            Global.generator.CleanUp();
            VideoCache.ClearCache();
            // File drag and drop support.
#if WINDOWSDX
            Form? gameForm = Control.FromHandle(Window.Handle) as Form;
            if (gameForm != null)
            {
                gameForm.AllowDrop = true;
                gameForm.DragEnter += new DragEventHandler(DragEnter);
                gameForm.DragDrop += new DragEventHandler(DragDrop);
                gameForm.DragLeave += new EventHandler(DragLeave);
            }
            ConsoleOutput.WriteLine("Form supports drag and drop.", Color.Transparent);
#else
            ConsoleOutput.WriteLine("Form does not support drag and drop.", Color.Transparent);
#endif
            // Set window title.
            Window.Title = Global.productName + " v" + Global.productVersion;
            // Disable anti-aliasing.
            _graphics.PreferMultiSampling = false;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            // Set screen resolution.
            ConsoleOutput.WriteLine("Setting screen resolution...", Color.Transparent);
            GlobalGraphics.scale = int.Parse(SaveData.saveValues["ScreenScale"], CultureInfo.InvariantCulture);
            GlobalGraphics.SetAspectRatio(new AspectRatio());
            Resize(GlobalGraphics.scaledWidth, GlobalGraphics.scaledHeight);
            ConsoleOutput.WriteLine("Screen resolution set.", Color.Transparent);
            ConsoleOutput.WriteLine("Detected system: " + (Architecture.IsRunningThroughProton() ? ("Proton " + Architecture.translatorVersion) : Architecture.IsRunningThroughWine() ? ("Wine " + Architecture.translatorVersion) : "Native"), Color.Transparent);
            ConsoleOutput.WriteLine("Rendering API: " + Global.productSku, Color.Transparent);
            ScreenManager.LoadScreens();
            ConsoleOutput.WriteLine("Initialization complete.", Color.Transparent);
            LibraryData.SequentialName();
            // Show blog if new version.
            if(SaveData.saveValues["LastVersion"] != Global.productVersion)
            {
                // Fix plugin list filter flags (new plugin type was added)
                if(SaveData.saveValues["PluginListFilterFlags"] == "7")
                    SaveData.saveValues["PluginListFilterFlags"] = "15";
                Pagination.SetPage(4);
                SaveData.saveValues["LastVersion"] = Global.productVersion;
                SaveData.Save();
            }
#if WINDOWSDX
            Window.AllowAltF4 = false;
            Form? _GameForm = Control.FromHandle(Window.Handle) as Form;
            if (_GameForm != null)
            {
                _GameForm.Closing += ClosingForm;
            }
#endif
            // match aspect ratio
            AspectRatio aspectRatio = new();
            if(SaveData.saveValues["MatchAspectRatio"] == "true")
                aspectRatio = GlobalGraphics.FindMatchingAspectRatio();
            GlobalGraphics.SetAspectRatio(aspectRatio);
            // fullscreen
            if(bool.Parse(SaveData.saveValues["Fullscreen"]))
                SetFullscreen(true);
            // always on top
            if(bool.Parse(SaveData.saveValues["AlwaysOnTop"]))
                SetAlwaysOnTop(true);
            // hide cursor
                SetNativeCursor(bool.Parse(SaveData.saveValues["UseNativeCursor"]));
            base.Initialize();
        }
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            // Load default content.
            GlobalContent.LoadDefaultContent(Content, GraphicsDevice);
            // Load all screen content.
            ScreenManager.LoadContent(Content, GraphicsDevice);
            if(bool.Parse(SaveData.saveValues["SkipPhotosensitiveWarningScreen"]))
            {
                introFinished = true;
                ScreenManager.PushNavigation("Intro");
                var screen = ScreenManager.GetScreen<PhotosensitiveWarningScreen>("Intro");
                if (screen != null)
                {
                    screen.Show();
                }
            }
            base.LoadContent();
        }
        protected override void UnloadContent()
        {
            if(_spriteBatch != null)
                _spriteBatch.Dispose();
            // Unload all content.
            GlobalContent.UnloadContent();
            if(videoPlayer != null)
            {
                videoPlayer.Dispose();
                videoPlayer = null;
            }
            videoPath = "";
            if(video != null)
            {
                video.Dispose();
            }
            FramePlayer.canPlayBgMusic = true;
            base.UnloadContent();
        }
        public void FindMusic()
        {
            if(!Global.firstSongPlayed)
            {
                try
                {
                    music = int.Parse(SaveData.saveValues["LastMusicTrack"], CultureInfo.InvariantCulture);
                }
                catch
                {
                    music = -1;
                }
                Global.firstSongPlayed = true;
            }
#if WINDOWSDX
            else
            {
                music = ThemeManager.GetNextSongIndex(music);
            }
            if (music < 0)
            {
                // Pick random music from the theme.
                int max = ThemeManager.GetSongCount();
                if (max > 0)
                {
                    music = Global.generator.globalRandom.Next(0, max);
                }
                else
                {
                    music = 0; // Fallback to the first song if no songs are available.
                }
            }
            _musicState = MusicState.Playing;
            try
            {
                MediaPlayer.Play(GlobalContent.GetSong(music));
            }
            catch
            {
                //ConsoleOutput.WriteLine("Failed to play music: " + ex.Message, Color.Red);
            }
#else
            _musicState = MusicState.Paused;
#endif
            SaveData.saveValues["LastMusicTrack"] = music.ToString(CultureInfo.InvariantCulture);
            SaveData.Save();
        }
        protected override void Update(GameTime gameTime)
        {
            L.cyclerTimer += gameTime.ElapsedGameTime.TotalSeconds;
            // DEBUG: Pressing Mouse3 at any time will FindMusic()
            /*
            if(MouseInput.MouseState.MiddleButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && MouseInput.LastMouseState.MiddleButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
            {
                FindMusic();
            }
            */
            if(bool.Parse(SaveData.saveValues["EnableDiscordRPC"]))
                DiscordRPC.Update();
            SteamRichPresence.Update();
            try
            {
                if(SteamManager.initialized)
                    SteamManager.Update();
            }
            catch {}
            // Update window state.
            if(IsActive)
                _windowState = WindowState.Focused;
            else
                _windowState = WindowState.Unfocused;
            // Title screen video
            if(!introStarted)
            {
                introStarted = true;
                videoPlayer = new MonoGame.Extended.Framework.Media.VideoPlayer(GraphicsDevice);
                try
                {
                    string? randomBootMovie = PluginHandler.PickRandomBootMovie();
                    if(randomBootMovie != null && File.Exists(randomBootMovie))
                    {
                        videoPath = VideoCache.GetCachePath(randomBootMovie);
                        video = VideoHelper.LoadFromFile(videoPath);
                        // current time + intro duration
                        introGoal = gameTime.TotalGameTime.TotalSeconds + video.Duration.Seconds + 1;
                        if(randomBootMovie.ToLower().EndsWith("kiwifruitdevlogo.mp4"))
                        {
                            introGoal -= 2;
                        }
                    }
                    else
                    {
                        ConsoleOutput.WriteLine("Could not load boot video: " + videoPath, Color.Red);
                    }
                }
                catch (Exception e)
                {
                    ConsoleOutput.WriteLine($"Failed to load video: {e.Message}", Color.Red);
                    introFinished = true;
                }
                if(video != null)
                {
                    videoPlayer.Play(video);
                    videoPlayer.Volume = float.Parse(SaveData.saveValues["VideoVolume"], CultureInfo.InvariantCulture) / 100f;
                    FramePlayer.canPlayBgMusic = false;
                }
            }
            if(introStarted && (introFinished || videoPath == "" || (videoPlayer != null && video != null && gameTime.TotalGameTime.TotalSeconds >= introGoal)))
            {
                if(!introFinished)
                {
                    videoPath = "";
                    introFinished = true;
                    if(video != null)
                        video.Dispose();
                    ScreenManager.PushNavigation("Intro");
                    var screen = ScreenManager.GetScreen<PhotosensitiveWarningScreen>("Intro");
                    if (screen != null)
                    {
                        screen.Show();
                    }
                }
                FramePlayer.Update(gameTime);
                // Play music after 500ms.
                if(gameTime.TotalGameTime.TotalMilliseconds > Global.readyTime + Global.waitReady && Global.ready)
                {
                    if(Global.exiting)
                    {
                        if(MediaPlayer.Volume > 0.01f)
                            MediaPlayer.Volume -= 0.01f;
                        else
                        {
                            MediaPlayer.Pause();
                            _musicState = MusicState.Paused;
                        }
                    }
                    else
                    {
                        if((SaveData.saveValues["MuteMusicWhileTabbedOut"] == "true" ? _windowState == WindowState.Focused : true) && _musicState == MusicState.Playing && FramePlayer.canPlayBgMusic)
                        {
                            // Fade in music.
                            float vol = int.Parse(SaveData.saveValues["MusicVolume"], CultureInfo.InvariantCulture) / 100f;
                            if(MediaPlayer.Volume < vol)
                                MediaPlayer.Volume += 0.1f;
                            // Clamp music if it's over the volume level.
                            if(MediaPlayer.Volume > vol)
                                MediaPlayer.Volume = vol;
                        }
                        // Loop music
                        if(MediaPlayer.State == MediaState.Stopped)
                        {
                            FindMusic();
                        }
                        if((SaveData.saveValues["MuteMusicWhileTabbedOut"] == "true" ? _windowState == WindowState.Focused : true) && _musicState == MusicState.Paused && FramePlayer.canPlayBgMusic)
                        {
                            MediaPlayer.Resume();
                            _musicState = MusicState.Playing;
                        }
                        if(((SaveData.saveValues["MuteMusicWhileTabbedOut"] == "true" ? _windowState == WindowState.Unfocused : false) && _musicState == MusicState.Playing) || !FramePlayer.canPlayBgMusic)
                        {
                            // Fade out music.
                            if(MediaPlayer.Volume > 0.1f)
                                MediaPlayer.Volume -= 0.1f;
                            else
                            {
                                MediaPlayer.Pause();
                                _musicState = MusicState.Paused;
                            }
                        }
                    }
                }
            }
            // Update screens.
            ScreenManager.Update(gameTime);
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(ThemeManager.GetColor("ClearColor")); // Black background.
            Global.tooltip = "";
            if(_spriteBatch != null)
            {
                _spriteBatch.Begin(SpriteSortMode.Deferred,
                    BlendState.AlphaBlend,
                    SamplerState.PointClamp,
                    null, null, null, Matrix.CreateTranslation(GlobalGraphics.Scale(GlobalGraphics.drawOffset.X), GlobalGraphics.Scale(GlobalGraphics.drawOffset.Y), 0));
                if(!introFinished)
                {
                    try
                    {
                        if(videoPlayer != null)
                        {
                            // Title screen video
                            Texture2D texture = videoPlayer.GetTexture();
                            if (texture != null)
                                _spriteBatch.Draw(texture, new Rectangle(0, 0, GlobalGraphics.scaledWidth, GlobalGraphics.scaledHeight), Color.White);
                        }
                    }
                    catch {}
                }
                try
                {
                    ScreenManager.Draw(gameTime, _spriteBatch);
                }
                catch(Exception e)
                {
                    ConsoleOutput.WriteLine("Error while drawing screen: " + e.Message, Color.Red);
                }
                _spriteBatch.End();
                _spriteBatch.Begin(SpriteSortMode.Deferred,
                    BlendState.AlphaBlend,
                    SamplerState.PointClamp,
                    null, null, null, Matrix.CreateTranslation(0, 0, 0));
                // Debug pause indicator
                if(Debug.paused)
                {
                    SpriteFont font = L.FontLarge();
                    string debugPaused = "Debug Paused";
                    Vector2 debugPausedSize = font.MeasureString(debugPaused);
                    GlobalContent.DrawString(_spriteBatch, font, debugPaused, new Vector2(GlobalGraphics.preferredResolution.X-GlobalGraphics.Scale(8-1)-debugPausedSize.X, GlobalGraphics.Scale(8+1)), Color.Black);
                    GlobalContent.DrawString(_spriteBatch, font, debugPaused, new Vector2(GlobalGraphics.preferredResolution.X-GlobalGraphics.Scale(8)-debugPausedSize.X, GlobalGraphics.Scale(8)), ThemeManager.GetColor("VideoPlayerProgressBar"));
                }
                _spriteBatch.End();
            }
            base.Draw(gameTime);
        }
        private void ClosingForm(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            ExitGracefully();
            e.Cancel = true;
        }
        public void ExitGracefully()
        {
            if(!Global.exiting)
            {
                videoPath = "";
                if(videoPlayer != null)
                {
                    videoPlayer.Dispose();
                    videoPlayer = null;
                }
                FramePlayer.canPlayBgMusic = true;
                if(video != null)
                {
                    video.Dispose();
                    video = null;
                }
                Global.generator.progressText = L.T(0, "Content:StatusExiting");
                Global.generator.failureReason = L.T(0, "Content:StatusExiting");
                Global.exiting = true;
                // Exit page is always the last
                Global.exitOpacityIncrease = 0.0075f;
                Global.fakeExit = false;
                GlobalContent.PlaySound("Quit");
                if(SteamManager.initialized)
                    SteamManager.Shutdown();
                DiscordRPC.Shutdown();
                ConsoleOutput.WriteLine("Exiting gracefully...", Color.Transparent);
            }
        }
        protected override void OnExiting(object sender, ExitingEventArgs args)
        {
            if(Global.exiting)
            {
                args.Cancel = true;
                return;
            }
            base.OnExiting(sender, args);
            if(SteamManager.initialized)
                SteamManager.Shutdown();
            DiscordRPC.Shutdown();
        }
    }
}
