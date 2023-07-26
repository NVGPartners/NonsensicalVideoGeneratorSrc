#if MONOGAME
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Tweening;

namespace NonsensicalVideoGenerator
{
    public class VideoScreen : IScreen
    {
        public string title { get; } = "Video";
        public int layer { get; } = 2;
        public ScreenType screenType { get; set; } = ScreenType.Hidden;
        public int currentPlacement { get; set; } = -1;
        private bool hiding = false;
        private bool showing = false;
        private bool toggle = false;
        public Vector2 offset = new(0, 0);
        private readonly Tweener tween = new();
        public KeyboardState newKeyboardState;
        public KeyboardState oldKeyboardState;
        List<string> lines = new()
        {
            " ",
            " ",
            "If you find any bugs,",
            "please report them on",
            "the GitHub issues page.",
            "Thank you, and enjoy!"
        };
        public void Show()
        {
            toggle = true;
            if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
            {
                offset = new(GlobalGraphics.Scale(-124), 0); // from left to right
                tween.TweenTo(this, t => t.offset, new Vector2(0, 0), 0.5f)
                    .Easing(EasingFunctions.ExponentialOut);
            }
            else
            {
                offset = new(0, 0);
            }
            showing = true;
        }
        public void Hide()
        {
            toggle = false;
            if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
            {
                offset = new(0, 0); // from right to left
                tween.TweenTo(this, t => t.offset, new Vector2(GlobalGraphics.Scale(-124), 0), 0.5f)
                    .Easing(EasingFunctions.ExponentialOut);
            }
            else
            {
                offset = new(GlobalGraphics.Scale(-124), 0);
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
            if (hiding && offset.X == GlobalGraphics.Scale(-124))
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
            bool inputHandled = false;
            if(handleInput)
            {
                // Keyboard input
                oldKeyboardState = newKeyboardState;
                newKeyboardState = Keyboard.GetState();
                if(newKeyboardState.IsKeyDown(Keys.Space) && oldKeyboardState.IsKeyUp(Keys.Space)
                    && !Accessibility.showDisambiguation)
                {
                    if(FramePlayer.currentFrame == FramePlayer.frames.Count-1
                        || FramePlayer.currentFrame == -1)
                    {
                        FramePlayer.playing = false;
                    }
                    else
                    {
                        FramePlayer.currentFrame = -1;
                    }
                    if(FramePlayer.audio != null)
                    {
                        FramePlayer.audio.Stop();
                        FramePlayer.currentAudioTime = 0;
                        FramePlayer.audioPlaying = false;
                        FramePlayer.canPlayBgMusic = true;
                        Global.generator.progressText = "Stopped playback.";
                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    }
                    else
                    {
                        GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    }
                    inputHandled = true;
                }
                Texture2D vidwindow = GlobalContent.GetTexture("VidWindow");
                Rectangle rect = new Rectangle(GlobalGraphics.Scale(0), GlobalGraphics.Scale(45-7), GlobalGraphics.Scale(vidwindow.Width-24), GlobalGraphics.Scale(vidwindow.Height-43));
                if(rect.Contains(MouseInput.MouseState.Position))
                {
                    if(MouseInput.MouseState.LeftButton == ButtonState.Pressed && MouseInput.LastMouseState.LeftButton == ButtonState.Released)
                    {
                        try
                        {
                            if(FramePlayer.currentFrame == FramePlayer.frames.Count-1
                                || FramePlayer.currentFrame == -1)
                            {
                                FramePlayer.playing = false;
                            }
                            else
                            {
                                FramePlayer.currentFrame = -1;
                            }
                            if(FramePlayer.audio != null)
                            {
                                FramePlayer.audio.Stop();
                                FramePlayer.currentAudioTime = 0;
                                FramePlayer.audioPlaying = false;
                                FramePlayer.canPlayBgMusic = true;
                                Global.generator.progressText = "Stopped playback.";
                                GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            }
                            else
                            {
                                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            }
                            inputHandled = true;
                        }
                        catch {}
                    }
                    else if(MouseInput.MouseState.RightButton == ButtonState.Pressed && MouseInput.LastMouseState.RightButton == ButtonState.Released)
                    {
                        if(FramePlayer.audio != null)
                        {
                            FramePlayer.currentFrame = -1;
                            FramePlayer.audio.Stop();
                            FramePlayer.currentAudioTime = 0;
                            FramePlayer.audioPlaying = false;
                            FramePlayer.canPlayBgMusic = true;
                            Global.generator.progressText = "Stopped playback.";
                            FramePlayer.canPlayBgMusic = true;
                        }
                        ProcessStartInfo startInfo = new()
                        {
                            FileName = FramePlayer.currentPath,
                            UseShellExecute = true
                        };
                        try
                        {
                            Process.Start(startInfo);
                        }
                        catch
                        {
                        }
                        GlobalContent.GetSound("Option").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        inputHandled = true;
                    }
                }
            }
            // Tween
            tween.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            if(!inputHandled && (hiding || screenType == ScreenType.Hidden))
                return false;
            return handleInput ? inputHandled : false;
        }
        public int flash = 0;
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Texture2D vidoverlay = GlobalContent.GetTexture("VidOverlay");
            Texture2D pixel = GlobalContent.GetTexture("Pixel");
            if(FramePlayer.playing && !FramePlayer.canPlayBgMusic)
                spriteBatch.Draw(pixel, new Rectangle(GlobalGraphics.Scale(4), GlobalGraphics.Scale(4), GlobalGraphics.Scale(vidoverlay.Width), GlobalGraphics.Scale(vidoverlay.Height)), Color.Black * 0.5f);
            // End existing spritebatch
            spriteBatch.End();
            // Use offset
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(offset.X, offset.Y, 0));
    
            /*
            Texture2D vidbutton = GlobalContent.GetTexture("VidButton");
            spriteBatch.Draw(vidbutton, new Rectangle(GlobalGraphics.Scale(14), GlobalGraphics.Scale(104), GlobalGraphics.Scale(vidbutton.Width), GlobalGraphics.Scale(vidbutton.Height)), Color.White);
            spriteBatch.Draw(vidbutton, new Rectangle(GlobalGraphics.Scale(47), GlobalGraphics.Scale(104), GlobalGraphics.Scale(vidbutton.Width), GlobalGraphics.Scale(vidbutton.Height)), Color.White);
            spriteBatch.Draw(vidbutton, new Rectangle(GlobalGraphics.Scale(80), GlobalGraphics.Scale(104), GlobalGraphics.Scale(vidbutton.Width), GlobalGraphics.Scale(vidbutton.Height)), Color.White);
            */
            Texture2D vidwindow = GlobalContent.GetTexture("VidWindow");
            Texture2D vidbg = GlobalContent.GetTexture("VidBG");
            spriteBatch.Draw(vidbg, new Rectangle(GlobalGraphics.Scale(0), GlobalGraphics.Scale(43), GlobalGraphics.Scale(104), GlobalGraphics.Scale(78)), Color.White);
            // Draw flashing box in vidbg bounds if FramePlayer.processing is true
            if(FramePlayer.processing)
            {
                if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
                {
                    // Flash using FramePlayer.startedProcessing (seconds) and gameTime seconds
                    // 0-255
                    flash = (int)(Math.Sin((FramePlayer.startedProcessing + gameTime.TotalGameTime.TotalSeconds)*6) * 64);
                    // Absolute value
                    if(flash < 0)
                        flash *= -1;
                }
                else
                {
                    // Set to middle
                    flash = 48;
                }
                spriteBatch.Draw(pixel, new Rectangle(GlobalGraphics.Scale(0), GlobalGraphics.Scale(43), GlobalGraphics.Scale(104), GlobalGraphics.Scale(78)), new Color(flash, flash, flash, 255));
            }
            /*
            Texture2D vidbg = GlobalContent.GetTexture("VidBG");
            spriteBatch.Draw(vidbg, new Rectangle(GlobalGraphics.Scale(6), GlobalGraphics.Scale(45), GlobalGraphics.Scale(vidbg.Width), GlobalGraphics.Scale(vidbg.Height)), Color.White);
            Vector2 lastPos = new(GlobalGraphics.Scale(8), GlobalGraphics.Scale(45));
            for(int i = 0; i < lines.Count; i++)
            {
                spriteBatch.DrawString(munro, lines[i], new Vector2(lastPos.X + GlobalGraphics.Scale(1), lastPos.Y + GlobalGraphics.Scale(1)), Color.Black);
                spriteBatch.DrawString(munro, lines[i], new Vector2(lastPos.X, lastPos.Y), Color.White);
                lastPos.Y += munro.MeasureString(lines[i]).Y;
            }
            */
            // Draw media
            if(FramePlayer.frames.Count > 0)
            {
                int frame = FramePlayer.currentFrame >= 0 ? FramePlayer.currentFrame : 0;
                if(frame >= FramePlayer.frames.Count)
                    frame = 0;
                spriteBatch.Draw(FramePlayer.frames[frame], new Rectangle(GlobalGraphics.Scale(2), GlobalGraphics.Scale(41), GlobalGraphics.Scale(104), GlobalGraphics.Scale(82)), Color.White);
            }
            // Video Window
            spriteBatch.Draw(vidwindow, new Rectangle(GlobalGraphics.Scale(0), GlobalGraphics.Scale(36), GlobalGraphics.Scale(vidwindow.Width), GlobalGraphics.Scale(vidwindow.Height)), Color.White);
            SpriteFont munro = GlobalContent.GetFont("MunroSmall");
            // Draw window title on left side (90 degrees)
            string altTitle = "Video Player";
            Vector2 titleSize = munro.MeasureString(altTitle);
            spriteBatch.DrawString(munro, altTitle, new Vector2(GlobalGraphics.Scale(111), GlobalGraphics.Scale(110)), Color.White, MathHelper.ToRadians(90), new Vector2(titleSize.X, titleSize.Y), 1, SpriteEffects.None, 0);
            // End offset spritebatch
            spriteBatch.End();
            // Remake spritebatch
            spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                null, null, null, null);
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            // Video Window
            GlobalContent.AddTexture("VidWindow", contentManager.Load<Texture2D>("graphics/vidwindow"));
            GlobalContent.AddTexture("VidButton", contentManager.Load<Texture2D>("graphics/vidbutton"));
            GlobalContent.AddTexture("VidBG", contentManager.Load<Texture2D>("graphics/vidbg"));
            GlobalContent.AddTexture("VidOverlay", contentManager.Load<Texture2D>("graphics/vidoverlay"));
        }
    }
}
#endif
