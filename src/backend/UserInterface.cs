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
        Stopped,
    }
    public class UserInterface : Game
    {
        public static UserInterface? instance;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch? _spriteBatch;
        private WindowState _windowState = WindowState.Unfocused;
        private MusicState _musicState = MusicState.Stopped;
        private int _musicActive = 0;
        public UserInterface()
        {
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
            SteamManager.Initialize();
            // File drag and drop support.
            Form gameForm = (Form)Form.FromHandle(Window.Handle);
            gameForm.AllowDrop = true;
            gameForm.DragEnter += new DragEventHandler(DragEnter);
            gameForm.DragDrop += new DragEventHandler(DragDrop);
            gameForm.DragLeave += new EventHandler(DragLeave);
            // Set window title.
            Window.Title = "Nonsensical Video Generator";
            // Disable anti-aliasing.
            _graphics.PreferMultiSampling = false;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            // Set screen resolution.
            int scale = int.Parse(SaveData.saveValues["ScreenScale"]);
            _graphics.PreferredBackBufferWidth = (int)(int.Parse(SaveData.saveValues["ScreenWidth"]) * scale);
            _graphics.PreferredBackBufferHeight = (int)(int.Parse(SaveData.saveValues["ScreenHeight"]) * scale);
            _graphics.ApplyChanges();
            ConsoleOutput.Clear();
            // Delete update.bat
            try
            {
                if(System.IO.File.Exists("update.bat"))
                    System.IO.File.Delete("update.bat");
            }
            catch
            {
            }
            // Load plugins.
            if(!bool.Parse(SaveData.saveValues["FirstBoot"]) && SaveData.saveValues["FirstBootVersion"] == Global.productVersion)
            {
                //Global.pluginsLoaded = PluginHandler.LoadPlugins(); // Only load plugins after first boot.
            }
            else
            {
                SaveData.saveValues["FirstBoot"] = "true";
                SaveData.saveValues["FirstBootVersion"] = Global.productVersion;
                SaveData.Save();
            }
            // Load all screens.
            ScreenManager.LoadScreens();
            ConsoleOutput.WriteLine("Initialization complete for v" + Global.productVersion + ".");
            base.Initialize();
        }
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            // Load default content.
            GlobalContent.LoadDefaultContent(Content, GraphicsDevice);
            // Find music.
            FindMusic();
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
            int music = int.Parse(SaveData.saveValues["ActiveMusic"]);
            if(!Global.shuffled && SaveData.saveValues["ShuffleMusic"] == "true")
            {
                // Shuffle music.
                Global.shuffled = true;
                music = Global.generatorFactory.globalRandom.Next(0, GlobalContent.GetSongCount());
                SaveData.saveValues["ActiveMusic"] = music.ToString();
                SaveData.Save();
            }
            // Make sure music is in range.
            if(music < 0 || music >= GlobalContent.GetSongCount())
            {
                music = 0;
                SaveData.saveValues["ActiveMusic"] = music.ToString();
                SaveData.Save();
            }
        }
        protected override void Update(GameTime gameTime)
        {
            SteamManager.Update();
            // Play music after 500ms.
            if(gameTime.TotalGameTime.TotalMilliseconds > Global.readyTime + 2500 && Global.ready)
            {
                // Exchange music if it's not the same as the active music.
                if(_musicActive != int.Parse(SaveData.saveValues["ActiveMusic"]))
                {
                    _musicActive = int.Parse(SaveData.saveValues["ActiveMusic"]);
                    MediaPlayer.Play(GlobalContent.GetSongByIndex(_musicActive));
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
                    switch(_windowState)
                    {
                        case WindowState.Focused:
                            if(_musicState == MusicState.Playing)
                            {
                                // Fade in music.
                                float vol = int.Parse(SaveData.saveValues["MusicVolume"]) / 100f;
                                if(MediaPlayer.Volume < vol)
                                    MediaPlayer.Volume += 0.1f;
                                // Clamp music if it's over the volume level.
                                if(MediaPlayer.Volume > vol)
                                    MediaPlayer.Volume = vol;
                            }
                            else if(_musicState == MusicState.Stopped)
                            {
                                MediaPlayer.Play(GlobalContent.GetSongByIndex(_musicActive));
                                _musicState = MusicState.Playing;
                            }
                            else if(_musicState == MusicState.Paused)
                            {
                                MediaPlayer.Resume();
                                _musicState = MusicState.Playing;
                            }
                            // Loop music.
                            if(MediaPlayer.State == MediaState.Stopped)
                            {
                                MediaPlayer.Play(GlobalContent.GetSongByIndex(_musicActive));
                            }

                            break;
                        case WindowState.Unfocused:
                            if(_musicState == MusicState.Playing)
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
                            break;
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
            SteamManager.Shutdown();
        }
    }
}
