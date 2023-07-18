using System;
using System.Collections.Generic;
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
    /// This is a generic modal screen.
    /// </summary>
    public class ModalScreen : IScreen
    {
        /// <summary>
        /// The title of the screen. This is displayed on the header bar.
        /// </summary>
        public string title { get; } = "Modal";
        public int layer { get; } = 10;
        public ScreenType screenType { get; set; } = ScreenType.Hidden;
        public int currentPlacement { get; set; } = -1;
        public Action<int>? callback { get; set; } = null;
        public string modalTitle { get; set; } = "Modal";
        public string[] modalText { get; set; } = new string[] { "Modal" };
        public string[] buttons { get; set; } = new string[] { "Okay" };
        public int defaultButton { get; set; } = -1;
        private bool hiding = false;
        private bool showing = false;
        private bool toggle = false;
        public Vector2 offset = new(0, 0);
        private readonly Tweener tween = new();
        private KeyboardState oldKeyboardState;
        private KeyboardState newKeyboardState;
        private Dictionary<int, Rectangle> buttonRectangles = new Dictionary<int, Rectangle>();
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Draw background.
            spriteBatch.Draw(GlobalGraphics.pixel, new Rectangle(0, 0, GlobalGraphics.scaledWidth, GlobalGraphics.scaledHeight), new Color(0, 0, 0, 128));
            // Draw modal.
            spriteBatch.Draw(GlobalGraphics.pixel, new Rectangle((GlobalGraphics.scaledWidth / 2 - GlobalGraphics.scaledWidth / 4) + GlobalGraphics.Scale(GlobalGraphics.shadowScale), (GlobalGraphics.scaledHeight / 2 - GlobalGraphics.scaledHeight / 4) + GlobalGraphics.Scale(GlobalGraphics.shadowScale), GlobalGraphics.scaledWidth / 2, GlobalGraphics.scaledHeight / 2), new Color(0,0,0));
            spriteBatch.Draw(GlobalGraphics.pixel, new Rectangle(GlobalGraphics.scaledWidth / 2 - GlobalGraphics.scaledWidth / 4, GlobalGraphics.scaledHeight / 2 - GlobalGraphics.scaledHeight / 4, GlobalGraphics.scaledWidth / 2, GlobalGraphics.scaledHeight / 2), new Color(96, 96, 96));
            // Draw message (wrapped).
            int lineHeight = 8 * GlobalGraphics.scale;
            int lineSpacing = 2 * GlobalGraphics.scale;
            int lineY = GlobalGraphics.scaledHeight / 4;
            foreach (string line in modalText)
            {
                int lineWidth = GlobalGraphics.scaledWidth - GlobalGraphics.Scale(32);
                int lineLength = (int)GlobalGraphics.fontMunroSmall.MeasureString(line).X;
                if (lineLength > lineWidth)
                {
                    int lineCount = (int)Math.Ceiling((double)lineLength / lineWidth);
                    for (int i = 0; i < lineCount; i++)
                    {
                        string linePart = line.Substring(i * lineWidth, Math.Min(lineWidth, lineLength - i * lineWidth));
                        Vector2 linePartSize = GlobalGraphics.fontMunroSmall.MeasureString(linePart);
                        spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, linePart, new Vector2((GlobalGraphics.scaledWidth / 2 - linePartSize.X / 2) + GlobalGraphics.Scale(GlobalGraphics.shadowScale), (lineY + lineHeight * i) + GlobalGraphics.Scale(GlobalGraphics.shadowScale)), Color.Black);
                        spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, linePart, new Vector2((GlobalGraphics.scaledWidth / 2 - linePartSize.X / 2), lineY + lineHeight * i), Color.White);
                        lineY += lineHeight;
                    }
                }
                else
                {
                    spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, line, new Vector2((GlobalGraphics.scaledWidth / 2 - lineLength / 2) + GlobalGraphics.Scale(GlobalGraphics.shadowScale), lineY + GlobalGraphics.Scale(GlobalGraphics.shadowScale)), Color.Black);
                    spriteBatch.DrawString(GlobalGraphics.fontMunroSmall, line, new Vector2((GlobalGraphics.scaledWidth / 2 - lineLength / 2), lineY), Color.White);
                    lineY += lineHeight;
                }
            }
            // Draw buttons from the bottom-up.
            for (int i = 0; i < buttons.Length; i++)
            {
                int buttonY = GlobalGraphics.Scale(16) + (GlobalGraphics.scaledHeight - (GlobalGraphics.scaledHeight / 4) - (GlobalGraphics.scaledHeight / 8) - (GlobalGraphics.scaledHeight / 8) * i);
                string buttonText = buttons[i];
                Vector2 buttonTextSize = GlobalGraphics.fontMunroSmall.MeasureString(buttonText);
                GlobalGraphics.DrawButtonShadow(spriteBatch, (int)((GlobalGraphics.scaledWidth / 2) - buttonTextSize.X / 2), buttonY, buttonText);
                buttonRectangles[i] = GlobalGraphics.DrawButton(spriteBatch, (int)((GlobalGraphics.scaledWidth / 2) - buttonTextSize.X / 2), buttonY, buttonText);
            }
        }
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
            if(handleInput)
            {
                bool calledBack = false;
                // Enter press.
                if(oldKeyboardState.IsKeyUp(Keys.Escape) && newKeyboardState.IsKeyDown(Keys.Escape))
                {
                    if (defaultButton != -1)
                    {
                        GlobalContent.GetSound("Back").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                        if(callback != null)
                        {
                            callback(defaultButton);
                        }
                        calledBack = true;
                    }
                }
                // Detect clicks.
                if (MouseInput.LastMouseState.LeftButton == ButtonState.Released && MouseInput.MouseState.LeftButton == ButtonState.Pressed) {
                    if(MouseInput.MouseState.X >= GlobalGraphics.scaledWidth / 2 - GlobalGraphics.scaledWidth / 4 && MouseInput.MouseState.X <= GlobalGraphics.scaledWidth / 2 + GlobalGraphics.scaledWidth / 4)
                    {
                        if(MouseInput.MouseState.Y >= GlobalGraphics.scaledHeight / 2 - GlobalGraphics.scaledHeight / 4 && MouseInput.MouseState.Y <= GlobalGraphics.scaledHeight / 2 + GlobalGraphics.scaledHeight / 4)
                        {
                            for (int i = 0; i < buttons.Length; i++)
                            {
                                Rectangle buttonRectangle = buttonRectangles[i];
                                if (MouseInput.MouseState.X >= buttonRectangle.X && MouseInput.MouseState.X <= buttonRectangle.X + buttonRectangle.Width)
                                {
                                    if (MouseInput.MouseState.Y >= buttonRectangle.Y && MouseInput.MouseState.Y <= buttonRectangle.Y + buttonRectangle.Height)
                                    {
                                        // A button was clicked.
                                        GlobalContent.GetSound("Select").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                                        if (callback != null)
                                        {
                                            callback(i);
                                        }
                                        calledBack = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Out of bounds, so cancel.
                            if (defaultButton != -1)
                            {
                                if(callback != null)
                                {
                                    callback(defaultButton);
                                }
                            }
                            GlobalContent.GetSound("Back").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                            calledBack = true;
                        }
                    }
                    else
                    {
                        // Out of bounds, so cancel.
                        if (defaultButton != -1)
                        {
                            if(callback != null)
                            {
                                callback(defaultButton);
                            }
                        }
                        GlobalContent.GetSound("Back").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                        calledBack = true;
                    }
                }
                oldKeyboardState = newKeyboardState;
                newKeyboardState = Keyboard.GetState();
                if(calledBack)
                {
                    ScreenManager.PopNavigation();
                }
                return true;
            }
            return false;
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            // Do nothing.
        }
    }
}
