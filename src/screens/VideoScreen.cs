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
        public int layer { get; set; } = 2;
        public ScreenType screenType { get; set; } = ScreenType.Hidden;
        public int currentPlacement { get; set; } = -1;
        private bool hiding = false;
        private bool showing = false;
        private bool toggle = false;
        public Vector2 offset = new(0, 0);
        private readonly Tweener tween = new();
        public KeyboardState newKeyboardState;
        public KeyboardState oldKeyboardState;
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
                        if(FramePlayer.audioPlaying || FramePlayer.playing)
                        {
                            FramePlayer.audio.Stop();
                            FramePlayer.currentAudioTime = 0;
                            FramePlayer.audioPlaying = false;
                            FramePlayer.canPlayBgMusic = true;
                            Global.generator.progressText = L.T(0, "Video:StatusStop");
                        }
                        else
                        {
                            FramePlayer.audio.Play();
                            FramePlayer.audioPlaying = true;
                            FramePlayer.canPlayBgMusic = false;
                            Global.generator.progressText = L.T(0, "Video:StatusPlay");
                        }
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
                                if(FramePlayer.audioPlaying || FramePlayer.playing)
                                {
                                    FramePlayer.audio.Stop();
                                    FramePlayer.currentAudioTime = 0;
                                    FramePlayer.audioPlaying = false;
                                    FramePlayer.canPlayBgMusic = true;
                                    Global.generator.progressText = L.T(0, "Video:StatusStop");
                                }
                                else
                                {
                                    FramePlayer.audio.Play();
                                    FramePlayer.audioPlaying = true;
                                    FramePlayer.canPlayBgMusic = false;
                                    Global.generator.progressText = L.T(0, "Video:StatusPlay");
                                }
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
                            Global.generator.progressText = L.T(0, "Video:StatusStop");
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
            Texture2D pixel = GlobalContent.GetTexture("Pixel");
            // End existing spritebatch
            spriteBatch.End();
            // Use offset
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(GlobalGraphics.Scale(GlobalGraphics.drawOffset.X)+offset.X, GlobalGraphics.Scale(GlobalGraphics.drawOffset.Y)+offset.Y, 0));
            Texture2D vidwindow = GlobalContent.GetTexture("VidWindow");
            Texture2D vidbg = GlobalContent.GetTexture("VidBG");
            spriteBatch.Draw(vidbg, new Rectangle(GlobalGraphics.Scale(0), GlobalGraphics.Scale(43), GlobalGraphics.Scale(104), GlobalGraphics.Scale(78)), Color.White);
            // Draw media
            if(FramePlayer.frames.Count > 0)
            {
                int frame = FramePlayer.currentFrame >= 0 ? FramePlayer.currentFrame : 0;
                if(frame >= FramePlayer.frames.Count)
                    frame = 0;
                spriteBatch.Draw(FramePlayer.frames[frame], new Rectangle(GlobalGraphics.Scale(4), GlobalGraphics.Scale(43), GlobalGraphics.Scale(100), GlobalGraphics.Scale(78)), Color.White);
                // Red progress bar at bottom
                if(FramePlayer.currentFrame > 0)
                {
                    spriteBatch.Draw(pixel, new Rectangle(GlobalGraphics.Scale(4), GlobalGraphics.Scale(118), GlobalGraphics.Scale(100), GlobalGraphics.Scale(3)), ThemeManager.GetColor("VideoPlayerProgressBarBackground"));
                    spriteBatch.Draw(pixel, new Rectangle(GlobalGraphics.Scale(4), GlobalGraphics.Scale(118), GlobalGraphics.Scale(100) * FramePlayer.currentFrame / FramePlayer.frames.Count, GlobalGraphics.Scale(4)), ThemeManager.GetColor("VideoPlayerProgressBar"));
                }
            }
            else if(FramePlayer.audioFrame != null)
            {
                spriteBatch.Draw(FramePlayer.audioFrame, new Rectangle(GlobalGraphics.Scale(4), GlobalGraphics.Scale(43), GlobalGraphics.Scale(100), GlobalGraphics.Scale(78)), Color.White);
                if(FramePlayer.currentAudioTime > 0)
                {
                    spriteBatch.Draw(pixel, new Rectangle(GlobalGraphics.Scale(4), GlobalGraphics.Scale(118), GlobalGraphics.Scale(100), GlobalGraphics.Scale(3)), ThemeManager.GetColor("VideoPlayerProgressBarBackground"));
                    spriteBatch.Draw(pixel, new Rectangle(GlobalGraphics.Scale(4), GlobalGraphics.Scale(118), (int)(FramePlayer.currentAudioTime * GlobalGraphics.Scale(100) / FramePlayer.audioLength), GlobalGraphics.Scale(3)), ThemeManager.GetColor("VideoPlayerProgressBar"));
                }
            }
            // Video Window
            spriteBatch.Draw(vidwindow, new Rectangle(GlobalGraphics.Scale(0), GlobalGraphics.Scale(36), GlobalGraphics.Scale(vidwindow.Width), GlobalGraphics.Scale(vidwindow.Height)), Color.White);
            SpriteFont munro = L.FontSmall();
            // Draw window title on left side (90 degrees)
            string altTitle = L.T(0, "Video:Title");
            Vector2 titleSize = munro.MeasureString(altTitle);
            GlobalContent.DrawString(spriteBatch, munro, altTitle, new Vector2(GlobalGraphics.Scale(111), GlobalGraphics.Scale(110)), Color.White, MathHelper.ToRadians(90), new Vector2(titleSize.X, titleSize.Y), 1, SpriteEffects.None, 0);
            // End offset spritebatch
            spriteBatch.End();
            // Remake spritebatch
            spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                null, null, null, Matrix.CreateTranslation(GlobalGraphics.Scale(GlobalGraphics.drawOffset.X), GlobalGraphics.Scale(GlobalGraphics.drawOffset.Y), 0));
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            // Video Window
            GlobalContent.AddTexture("VidWindow", ThemeManager.LoadLayeredContent<Texture2D>("graphics/vidwindow"));
            GlobalContent.AddTexture("VidBG", ThemeManager.LoadLayeredContent<Texture2D>("graphics/vidbg"));
        }
    }
}
#endif
