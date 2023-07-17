using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Steamworks;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// This is the overlay screen, it draws graphics in place for the hard-coded title text alongside a border.
    /// </summary>
    public class OverlayScreen : IScreen
    {
        /// <summary>
        /// The title of the screen. This is displayed on the header bar.
        /// </summary>
        public string title { get; } = "Border";
        public int layer { get; } = 100;
        public ScreenType screenType { get; set; } = ScreenType.Drawn;
        public int currentPlacement { get; set; } = -1;
        private float exitOpacity = 0f;
        public Color bgColor = new Color(128, 128, 128);
        public void Show()
        {
        }
        public void Hide()
        {
        }
        public bool Toggle(bool useBool = false, bool toggleTo = false)
        {
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Draw mask.
            Global.mask.Draw(gameTime, spriteBatch);
            if (Global.exiting)
                spriteBatch.Draw(GlobalGraphics.pixel, new Rectangle(0, 0, GlobalGraphics.scaledWidth, GlobalGraphics.scaledHeight), new Color(0, 0, 0, exitOpacity));
            // Draw the border.
            spriteBatch.Draw(GlobalGraphics.pixel, new Rectangle(0, 0, GlobalGraphics.scaledWidth, 4 * GlobalGraphics.scale), bgColor);
            spriteBatch.Draw(GlobalGraphics.pixel, new Rectangle(0, GlobalGraphics.scaledHeight - 4 * GlobalGraphics.scale, GlobalGraphics.scaledWidth, 4 * GlobalGraphics.scale), bgColor);
            spriteBatch.Draw(GlobalGraphics.pixel, new Rectangle(0, 0, 4 * GlobalGraphics.scale, GlobalGraphics.scaledHeight), bgColor);
            spriteBatch.Draw(GlobalGraphics.pixel, new Rectangle(GlobalGraphics.scaledWidth - 4 * GlobalGraphics.scale, 0, 4 * GlobalGraphics.scale, GlobalGraphics.scaledHeight), bgColor);
        }
        public bool Update(GameTime gameTime, bool handleInput)
        {
            // Update the mask.
            if(Global.mask.Update(gameTime, handleInput))
                return true;
            if(Global.exiting)
            {
                exitOpacity += 0.0075f;
                if(exitOpacity >= 1)
                {
                    SteamAPI.Shutdown();
                    if(UserInterface.instance != null)
                        UserInterface.instance.Exit();
                }
                return true;
            }
            if(Global.dragDrop)
            {
                // Math sine the color to flash
                bgColor = new Color((int)(Math.Sin(gameTime.TotalGameTime.TotalSeconds * 10) * 64 + 128), (int)(Math.Sin(gameTime.TotalGameTime.TotalSeconds * 10) * 64 + 128), (int)(Math.Sin(gameTime.TotalGameTime.TotalSeconds * 10) * 64 + 128));
            }
            else
            {
                // Reset the color
                bgColor = new Color(128, 128, 128);
            }
            return false;
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            // Load the mask.
            Global.mask.LoadContent(contentManager, graphicsDevice);
        }
    }
}
