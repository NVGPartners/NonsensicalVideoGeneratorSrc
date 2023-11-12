#if MONOGAME
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
        private int _musicActive = 0;
        private int music;
        private bool alreadyPlayedFirstSong = true;
        public UserInterface()
        {
            ConsoleOutput.WriteLine("Creating new UserInterface instance...", Color.Transparent);
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            instance = this;
        }
        // Drag and drop support.
        private void DragEnter(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            Global.dragDrop = true;
        }
        private void DragDrop(object sender, DragEventArgs e)
        {
            Global.dragDropFiles = ((string[])e.Data.GetData(DataFormats.FileDrop)).ToList();
            Global.dragDrop = false;
        }
        private void DragLeave(object sender, EventArgs e)
        {
            Global.dragDrop = false;
        }
        protected override void Initialize()
        {
            if(bool.Parse(SaveData.saveValues["EnableDiscordRPC"]))
                DiscordRPC.Initialize();
            ConsoleOutput.WriteLine("Starting initialization for v" + Global.productVersion + "...", Color.Transparent);
            try
            {
                SteamManager.Initialize();
            }
            catch(Exception ex)
            {
                ConsoleOutput.WriteLine("SteamManager failed to initialize: " + ex.Message, Color.Red);
            }
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
            Window.Title = "Nonsensical Video Generator";
            // Disable anti-aliasing.
            _graphics.PreferMultiSampling = false;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            // Set screen resolution.
            ConsoleOutput.WriteLine("Setting screen resolution...", Color.Transparent);
            int scale = int.Parse(SaveData.saveValues["ScreenScale"], System.Globalization.CultureInfo.InvariantCulture);
            _graphics.PreferredBackBufferWidth = (int)(int.Parse(SaveData.saveValues["ScreenWidth"], System.Globalization.CultureInfo.InvariantCulture) * scale);
            _graphics.PreferredBackBufferHeight = (int)(int.Parse(SaveData.saveValues["ScreenHeight"], System.Globalization.CultureInfo.InvariantCulture) * scale);
            _graphics.ApplyChanges();
            ConsoleOutput.WriteLine("Screen resolution set.", Color.Transparent);
            ScreenManager.LoadScreens();
            ConsoleOutput.WriteLine("Initialization complete.", Color.Transparent);
            LibraryData.SequentialName();
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
        private void FindMusic()
        {
            if(!alreadyPlayedFirstSong)
            {
                alreadyPlayedFirstSong = true;
                music = 5; // halloween
                _musicState = MusicState.Playing;
                MediaPlayer.Play(GlobalContent.GetSongByIndex(music));
                return;
            }
            music = Global.generator.globalRandom.Next(0, GlobalContent.GetSongCount());
            // Make sure music is in range.
            if(music < 0 || music >= GlobalContent.GetSongCount())
            {
                music = 0;
            }
            _musicState = MusicState.Playing;
            try
            {
                MediaPlayer.Play(GlobalContent.GetSongByIndex(music));
            }
            catch(Exception ex)
            {
                music = 0;
                //ConsoleOutput.WriteLine("Failed to play music: " + ex.Message, Color.Red);
            }
        }
        protected override void Update(GameTime gameTime)
        {
            // DEBUG: Pressing Mouse3 at any time will FindMusic()
            /*
            if(MouseInput.MouseState.MiddleButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && MouseInput.LastMouseState.MiddleButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
            {
                FindMusic();
            }
            */
            if(bool.Parse(SaveData.saveValues["EnableDiscordRPC"]))
                DiscordRPC.Update();
            // fix text entries
            bool handleInput = Accessibility.showDisambiguation || (IsActive && MouseInput.MouseState.X >= 0 && MouseInput.MouseState.X <= GlobalGraphics.scaledWidth &&
                MouseInput.MouseState.Y >= 0 && MouseInput.MouseState.Y <= GlobalGraphics.scaledHeight && !Global.dragDrop);
            try
            {
                if(SteamManager.initialized)
                    SteamManager.Update();
            }
            catch {}
            FramePlayer.Update(gameTime);
            // Play music after 500ms.
            if(gameTime.TotalGameTime.TotalMilliseconds > Global.readyTime + 2500 && Global.ready)
            {
                // Exchange music if it's not the same as the active music.
                if(_musicActive != music)
                {
                    _musicActive = music;
                    try
                    {
                        MediaPlayer.Play(GlobalContent.GetSongByIndex(_musicActive));
                    }
                    catch(Exception ex)
                    {
                        _musicActive = 0;
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
                        float vol = int.Parse(SaveData.saveValues["MusicVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f;
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
            GraphicsDevice.Clear(new Color(0, 0, 0)); // Black background.
            if(_spriteBatch != null)
            {
                _spriteBatch.Begin(SpriteSortMode.Deferred,
                    BlendState.AlphaBlend,
                    SamplerState.PointClamp,
                    null, null, null, null);
                ScreenManager.Draw(gameTime, _spriteBatch);
                _spriteBatch.End();
            }
            base.Draw(gameTime);
        }
        protected override void OnExiting(object sender, EventArgs args)
        {
            base.OnExiting(sender, args);
            if(SteamManager.initialized)
                SteamManager.Shutdown();
            DiscordRPC.Shutdown();
        }
    }
}
#endif
