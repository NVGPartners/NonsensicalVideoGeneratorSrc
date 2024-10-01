using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
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
        private int _musicActive = 2;
        public int music = 2;
        public UserInterface()
        {
            ConsoleOutput.WriteLine("Creating new UserInterface instance...", Color.Transparent);
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            instance = this;
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
            GlobalGraphics.fullScreen = !GlobalGraphics.fullScreen;
            AspectRatio aspectRatio = new();
            // Borderless
            Window.IsBorderless = GlobalGraphics.fullScreen;
            _graphics.HardwareModeSwitch = GlobalGraphics.fullScreen;
            if(GlobalGraphics.fullScreen)
            {
                // Set preferred resolution to screen resolution.
                aspectRatio.preferredResolution = new Point(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / GlobalGraphics.scale, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / GlobalGraphics.scale);
                // Calculate draw offset into center of screen.
                aspectRatio.drawOffset = new Vector2((aspectRatio.preferredResolution.X - (GlobalGraphics.scaledWidth / GlobalGraphics.scale)) / 2, (aspectRatio.preferredResolution.Y - (GlobalGraphics.scaledHeight / GlobalGraphics.scale)) / 2);
            }
            GlobalGraphics.SetAspectRatio(aspectRatio);
            _graphics.ApplyChanges();
            Form? windowForm = (Form)Form.FromHandle(Window.Handle);
            if(windowForm != null)
            {
                // Hide taskbar.
                windowForm.TopMost = GlobalGraphics.fullScreen;
                // Place window in center of screen.
                windowForm.Location = GlobalGraphics.fullScreen ? new System.Drawing.Point(0, 0) : new System.Drawing.Point(Screen.PrimaryScreen.WorkingArea.Width / 2 - windowForm.Width / 2, Screen.PrimaryScreen.WorkingArea.Height / 2 - windowForm.Height / 2);
            }
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
                Global.dragDropFiles = ((string[])e.Data.GetData(DataFormats.FileDrop)).ToList();
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
            // File drag and drop support.
#if WINDOWSDX
            Form gameForm = (Form)Form.FromHandle(Window.Handle);
            gameForm.AllowDrop = true;
            gameForm.DragEnter += new DragEventHandler(DragEnter);
            gameForm.DragDrop += new DragEventHandler(DragDrop);
            gameForm.DragLeave += new EventHandler(DragLeave);
            ConsoleOutput.WriteLine("Form supports drag and drop.", Color.Transparent);
#elif DESKTOPGL
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
            ScreenManager.LoadScreens();
            ConsoleOutput.WriteLine("Initialization complete.", Color.Transparent);
            LibraryData.SequentialName();
            // Show blog if new version.
            if(SaveData.saveValues["LastVersion"] != Global.productVersion)
            {
                Pagination.SetPage(4);
                SaveData.saveValues["LastVersion"] = Global.productVersion;
                SaveData.Save();
            }
#if WINDOWSDX
            Window.AllowAltF4 = false;
            Form _GameForm = (Form)Form.FromHandle(Window.Handle);
            _GameForm.Closing += ClosingForm;
#endif
            base.Initialize();
        }
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            // Load default content.
            GlobalContent.LoadDefaultContent(Content, GraphicsDevice);
            // Load all screen content.
            ScreenManager.LoadContent(Content, GraphicsDevice);
            base.LoadContent();
        }
        protected override void UnloadContent()
        {
            if(_spriteBatch != null)
                _spriteBatch.Dispose();
            // Unload all content.
            GlobalContent.UnloadContent();
            base.UnloadContent();
        }
        public void FindMusic()
        {
            if(music < 0)
            {
                music = Global.generator.globalRandom.Next(0, 11);
            }
            else
            {
                music++;
            }
            // Make sure music is in range.
            if(music < 2 || music >= 11)
            {
                music = 0;
            }
            _musicState = MusicState.Playing;
            try
            {
                MediaPlayer.Play(GlobalContent.GetSongByIndex(music));
            }
            catch
            {
                music = 0;
                //ConsoleOutput.WriteLine("Failed to play music: " + ex.Message, Color.Red);
            }
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
            try
            {
                if(SteamManager.initialized)
                    SteamManager.Update();
            }
            catch {}
            FramePlayer.Update(gameTime);
            // Play music after 500ms.
            if(gameTime.TotalGameTime.TotalMilliseconds > Global.readyTime + Global.waitReady && Global.ready)
            {
                // Exchange music if it's not the same as the active music.
                if(_musicActive != music)
                {
                    _musicActive = music;
                    try
                    {
                        MediaPlayer.Play(GlobalContent.GetSongByIndex(_musicActive));
                    }
                    catch
                    {
                        _musicActive = 2;
                        //ConsoleOutput.WriteLine("Failed to play music: " + ex.Message, Color.Red);
                    }
                    MediaPlayer.Volume = 0f;
                }
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
                    if(SaveData.saveValues["MuteMusicWhileTabbedOut"] == "true")
                    {
                        if(_windowState == WindowState.Focused && _musicState == MusicState.Paused && FramePlayer.canPlayBgMusic)
                        {
                            MediaPlayer.Resume();
                            _musicState = MusicState.Playing;
                        }
                        if((_windowState == WindowState.Unfocused && _musicState == MusicState.Playing) || !FramePlayer.canPlayBgMusic)
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
            // Update window state.
            if(IsActive)
                _windowState = WindowState.Focused;
            else
                _windowState = WindowState.Unfocused;
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
                ScreenManager.Draw(gameTime, _spriteBatch);
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
            if(!Global.exiting)
            {
                e.Cancel = true;
                Global.exiting = true;
                // Exit page is always the last
                Global.exitOpacityIncrease = 0.0075f;
                Global.fakeExit = false;
                GlobalContent.GetSound("Quit").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                //ScreenManager.GetScreen<MenuScreen>("Menu")?.Hide();
                //if(FramePlayer.audio != null)
                //    ScreenManager.GetScreen<VideoScreen>("Video")?.Hide();
                //ScreenManager.GetScreen<ContentScreen>("Content")?.Hide();
                //ScreenManager.GetScreen<HeaderScreen>("Header")?.Hide();
                //ScreenManager.GetScreen<SocialScreen>("Socials")?.Hide();
                if(SteamManager.initialized)
                    SteamManager.Shutdown();
                DiscordRPC.Shutdown();
            }
        }
        protected override void OnExiting(object sender, ExitingEventArgs args)
        {
            base.OnExiting(sender, args);
            if(!Global.exiting)
            {
                args.Cancel = true;
                if(SteamManager.initialized)
                    SteamManager.Shutdown();
                DiscordRPC.Shutdown();
            }
        }
    }
}
