#if MONOGAME
using System;
using System.Linq;
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
        public int layer { get; set; } = 99;
        public ScreenType screenType { get; set; } = ScreenType.Drawn;
        public int currentPlacement { get; set; } = -1;
        private float exitOpacity = 0f;
        private bool tooltipVisible = false;
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
            // Draw tooltip.
            if(Global.tooltip != "" && tooltipVisible && Global.ready)
            {
                string tooltip = Global.tooltip;
                Vector2 tooltipSize = L.FontSmall().MeasureString(tooltip);
                // Position is relative to mouse position but tries to avoid going off screen
                Vector2 position = new(MouseInput.MouseState.Position.X + 16, MouseInput.MouseState.Position.Y + 16);
                // Make sure it doesn't go off the right side of the screen
                if (position.X + tooltipSize.X + GlobalGraphics.Scale(6) > GlobalGraphics.scaledWidth)
                    position.X = GlobalGraphics.scaledWidth - tooltipSize.X - GlobalGraphics.Scale(6);
                // Make sure it doesn't go off the bottom of the screen
                if (position.Y + tooltipSize.Y + GlobalGraphics.Scale(2) > GlobalGraphics.scaledHeight)
                    position.Y = GlobalGraphics.scaledHeight - tooltipSize.Y - GlobalGraphics.Scale(2); 
                spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle((int)position.X, (int)position.Y, (int)tooltipSize.X + GlobalGraphics.Scale(2), (int)tooltipSize.Y - GlobalGraphics.Scale(2)), ThemeManager.GetColor("BackgroundTooltip"));
                // White text
                GlobalContent.DrawString(spriteBatch, L.FontSmall(), tooltip, new Vector2(position.X + GlobalGraphics.Scale(2), position.Y - GlobalGraphics.Scale(2)), Color.White);
            }
            // Draw mask.
            Global.mask.Draw(gameTime, spriteBatch);
            spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(0, 0, GlobalGraphics.scaledWidth, GlobalGraphics.scaledHeight), new Color(0, 0, 0, exitOpacity));
            // Draw the border.
            spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(0, 0, GlobalGraphics.scaledWidth, 4 * GlobalGraphics.scale), bgColor);
            spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(0, GlobalGraphics.scaledHeight - 4 * GlobalGraphics.scale, GlobalGraphics.scaledWidth, 4 * GlobalGraphics.scale), bgColor);
            spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(0, 0, 4 * GlobalGraphics.scale, GlobalGraphics.scaledHeight), bgColor);
            spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.scaledWidth - 4 * GlobalGraphics.scale, 0, 4 * GlobalGraphics.scale, GlobalGraphics.scaledHeight), bgColor);
        }
        public bool Update(GameTime gameTime, bool handleInput)
        {
            tooltipVisible = handleInput;
            ConsoleScreen? consoleScreen = ScreenManager.GetScreen<ConsoleScreen>("Console");
            if(consoleScreen != null)
            {
                if(consoleScreen.offset.Y < GlobalGraphics.scaledHeight
                    && consoleScreen.offset.Y > -1024)
                    tooltipVisible = false;
            }
            // Update the mask.
            if(Global.mask.Update(gameTime, handleInput))
                return true;
            if(Global.exiting)
            {
                exitOpacity += Global.exitOpacityIncrease;
                if(exitOpacity >= 1)
                {
                    if(!Global.fakeExit)
                    {
                        try
                        {
                            SteamAPI.Shutdown();
                        } catch {}
                        if(UserInterface.instance != null)
                            UserInterface.instance.Exit();
                    }
                    else
                    {
                        Global.exiting = false;
                        if(Global.exitFunc())
                        {
                            Global.readyTime = gameTime.TotalGameTime.TotalMilliseconds;
                        }
                        Global.exitFunc = () => false;
                    }
                }
                return true;
            }
            else if(exitOpacity > 0)
            {
                exitOpacity -= Global.exitOpacityIncrease;
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
                bgColor = ThemeManager.GetColor("BackgroundOverlayScreen");
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
#endif
