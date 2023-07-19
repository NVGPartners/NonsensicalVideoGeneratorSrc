using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tweening;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// Show console output through a screen, acts as a modal but doesn't follow modal rules.
    /// </summary>
    public class ConsoleScreen : IScreen
    {
        public string title { get; } = "Console";
        public int layer { get; } = 100;
        public ScreenType screenType { get; set; } = ScreenType.Hidden;
        public int currentPlacement { get; set; } = -1;
        private bool hiding = true;
        private bool showing = false;
        private bool toggle = false;
        public Vector2 offset = new(0, 0);
        private readonly Tweener tween = new();
        private KeyboardState oldKeyboardState;
        private KeyboardState newKeyboardState;
        public void Show()
        {
            toggle = true;
            if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
            {
                offset = new(0, GlobalGraphics.Scale(240)); // from bottom to top
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
            bool overrideReturn = false;
            int scrollWeight = newKeyboardState.IsKeyDown(Keys.LeftShift) || newKeyboardState.IsKeyDown(Keys.RightShift) ? 5 : 1;
            // Show/hide console when you press ` (tilde).
            if (!oldKeyboardState.IsKeyDown(Keys.OemTilde) && newKeyboardState.IsKeyDown(Keys.OemTilde) && Global.ready)
            {
                overrideReturn = true;
                if(Toggle())
                {
                    ConsoleOutput.ResetScroll();
                    GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                    if(Accessibility.showDisambiguation)
                        Accessibility.TTS("Console shown.");
                }
                else
                {
                    GlobalContent.GetSound("Back").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                    if(Accessibility.showDisambiguation)
                        Accessibility.TTS("Console hidden.");
                }
            }
            // Scrolling will set ConsoleOutput.paused to true.
            if (MouseInput.MouseState.ScrollWheelValue != MouseInput.LastMouseState.ScrollWheelValue)
            {
                int wheel = MouseInput.MouseState.ScrollWheelValue - MouseInput.LastMouseState.ScrollWheelValue;
                ConsoleOutput.Scroll(wheel * scrollWeight); // automatically divides by 120
            }
            // Clicking or pressing enter will also reset the scroll.
            if (MouseInput.MouseState.LeftButton == ButtonState.Pressed && MouseInput.LastMouseState.LeftButton == ButtonState.Released)
            {
                ConsoleOutput.ResetScroll();
            }
            oldKeyboardState = newKeyboardState;
            newKeyboardState = Keyboard.GetState();
            // And you can scroll with the arrow keys.
            if (!oldKeyboardState.IsKeyDown(Keys.Up) && newKeyboardState.IsKeyDown(Keys.Up))
            {
                ConsoleOutput.Scroll(scrollWeight * 120);
            }
            if (!oldKeyboardState.IsKeyDown(Keys.Down) && newKeyboardState.IsKeyDown(Keys.Down))
            {
                ConsoleOutput.Scroll(-scrollWeight * 120);
            }
            // Page up scrolls up by 10 lines.
            if (!oldKeyboardState.IsKeyDown(Keys.PageUp) && newKeyboardState.IsKeyDown(Keys.PageUp))
            {
                ConsoleOutput.Scroll(ConsoleOutput.maxLines * 120);
            }
            // Page down scrolls down by 10 lines.
            if (!oldKeyboardState.IsKeyDown(Keys.PageDown) && newKeyboardState.IsKeyDown(Keys.PageDown))
            {
                ConsoleOutput.Scroll(-ConsoleOutput.maxLines * 120);
            }
            // (DEBUG) Fill the console with nonsense.
            //ConsoleOutput.WriteLine(Math.Sin(gameTime.TotalGameTime.TotalSeconds).ToString());
            return handleInput && !overrideReturn;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // End existing spritebatch
            spriteBatch.End();
            // Use offset
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(offset.X, offset.Y, 0));
            // Draw the background.
            Texture2D pixel = GlobalContent.GetTexture("Pixel");
            spriteBatch.Draw(pixel, new Rectangle(0, 0, GlobalGraphics.scaledWidth, GlobalGraphics.scaledHeight), new Color(0, 0, 0, 255));
            // Draw the center title bar text.`
            string newTitle = title + " - Toggle with ~ (tilde key)";
            Vector2 titleSize = GlobalGraphics.fontMunroSmall.MeasureString(newTitle);
            spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, newTitle, new Vector2(GlobalGraphics.scaledWidth / 2 - titleSize.X / 2, (6 * GlobalGraphics.scale) - GlobalGraphics.Scale(1)), Color.White);
            // Draw lines.
            int lineHeight = 8 * GlobalGraphics.scale;
            int lineSpacing = 2 * GlobalGraphics.scale;
            int lineY = GlobalGraphics.Scale(16) + lineSpacing;
            try
            {
                foreach (ColoredString line in ConsoleOutput.GetOutput())
                {
                    Vector2 lineSize = GlobalGraphics.fontMunroSmall.MeasureString(line.Text);
                    spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, line.Text, new Vector2(GlobalGraphics.Scale(8), lineY), line.Color);
                    lineY += lineHeight;
                }
            }
            catch {}
            // Draw assembly version.
            string version = "- v" + Global.productVersion + " - View full output in console.txt -" + (ConsoleOutput.proxyOutput.Count > ConsoleOutput.maxLines ? (" Line " + (ConsoleOutput.scrollAmount > -1 ? (ConsoleOutput.scrollAmount + 1).ToString() : (ConsoleOutput.proxyOutput.Count - ConsoleOutput.maxLines + 1).ToString()) + "/" + ConsoleOutput.proxyOutput.Count.ToString() + " -") : "");
            spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, version, new Vector2(GlobalGraphics.Scale(8), lineY), Color.White);
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
            // Set offset to hidden position.
            offset = new(0, GlobalGraphics.Scale(240));
        }
    }
}
