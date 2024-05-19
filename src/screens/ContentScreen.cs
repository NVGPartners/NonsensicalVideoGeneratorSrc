#if MONOGAME
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
    /// This is the help screen.
    /// </summary>
    public class ContentScreen : IScreen
    {
        /// <summary>
        /// The title of the screen. This is displayed on the header bar.
        /// </summary>
        public string title { get; } = "Content";
        public int layer { get; set; } = 3;
        public ScreenType screenType { get; set; } = ScreenType.Hidden;
        public int currentPlacement { get; set; } = -1;
        private bool hiding = false;
        private bool showing = false;
        private bool toggle = false;
        public Vector2 offset = new(0, 0);
        private readonly Tweener tween = new();
        public void Show()
        {
            layer = 3;
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
            if(hiding || screenType == ScreenType.Hidden)
                return false;
            // Pagination
            if(Pagination.Update(gameTime, handleInput))
                return true;
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // End existing spritebatch
            spriteBatch.End();
            // Use offset
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(offset.X, offset.Y, 0));
            if(Pagination.SelectedPage != Pagination.TopPageCount)
            {
                // Main Window
                Texture2D mainwindow = GlobalContent.GetTexture("MainWindow");
                spriteBatch.Draw(mainwindow, new Rectangle(GlobalGraphics.Scale(128-33), GlobalGraphics.Scale(36), GlobalGraphics.Scale(mainwindow.Width), GlobalGraphics.Scale(mainwindow.Height)), Color.White);
                // Draw the center title bar text.
                string pageTitle = Pagination.GetSubPageName();
                // Center within bounds of x 128 and x 312
                Vector2 titleSize = L.FontSmall().MeasureString(pageTitle);
                spriteBatch.DrawString(L.FontSmall(), pageTitle, new Vector2(GlobalGraphics.Scale(220) - titleSize.X / 2, GlobalGraphics.Scale(37)), Color.White);
                // Draw action window text
                string altTitle = "Actions";
                Vector2 titleSize2 = L.FontSmall().MeasureString(altTitle);
                spriteBatch.DrawString(L.FontSmall(), altTitle, new Vector2(GlobalGraphics.Scale(108), GlobalGraphics.Scale(151)), Color.White, MathHelper.ToRadians(-90), new Vector2(titleSize2.X, titleSize2.Y), 1, SpriteEffects.None, 0);
            }
            // Pagination
            Pagination.Draw(gameTime, spriteBatch);
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
            // Main Window
            GlobalContent.AddTexture("MainWindow", ThemeManager.LoadLayeredContent<Texture2D>("graphics/mainwindow"));
            // Pagination
            Pagination.LoadContent(contentManager, graphicsDevice);
            if(!Global.canRender)
            {
                if(Global.pluginsLoaded)
                    Show();
                else
                    Hide();
            }
        }
    }
}
#endif
