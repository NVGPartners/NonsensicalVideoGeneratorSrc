using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
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
        private bool videoFullscreen = false;
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
            Texture2D pixel = GlobalContent.GetTexture("Pixel");
            // Draw mask.
            Global.mask.Draw(gameTime, spriteBatch);
            spriteBatch.Draw(pixel, new Rectangle(0, 0, GlobalGraphics.scaledWidth, GlobalGraphics.scaledHeight), new Color(0, 0, 0, videoFullscreen ? 255 : exitOpacity));
            if(!videoFullscreen)
            {
                // Draw the border.
                spriteBatch.Draw(pixel, new Rectangle(0, 0, GlobalGraphics.scaledWidth, 4 * GlobalGraphics.scale), bgColor);
                spriteBatch.Draw(pixel, new Rectangle(0, GlobalGraphics.scaledHeight - 4 * GlobalGraphics.scale, GlobalGraphics.scaledWidth, 4 * GlobalGraphics.scale), bgColor);
                spriteBatch.Draw(pixel, new Rectangle(0, 0, 4 * GlobalGraphics.scale, GlobalGraphics.scaledHeight), bgColor);
                spriteBatch.Draw(pixel, new Rectangle(GlobalGraphics.scaledWidth - 4 * GlobalGraphics.scale, 0, 4 * GlobalGraphics.scale, GlobalGraphics.scaledHeight), bgColor);
            }
            if(videoFullscreen)
            {
                if(UserInterface.instance != null && UserInterface.instance.video != null && UserInterface.instance.videoPlayer != null && UserInterface.instance.videoPlayer.State != MediaState.Stopped)
                {
                    try
                    {
                        // Title screen video
                        Texture2D texture = UserInterface.instance.videoPlayer.GetTexture();
                        if (texture != null)
                            spriteBatch.Draw(texture, new Rectangle(0, 0, GlobalGraphics.scaledWidth, GlobalGraphics.scaledHeight), Color.White);
                        // Red progress bar at bottom
                        spriteBatch.Draw(pixel, new Rectangle(0, GlobalGraphics.scaledHeight - GlobalGraphics.Scale(4), (int)(UserInterface.instance.videoPlayer.PlayPosition.TotalMicroseconds * (GlobalGraphics.scaledWidth - GlobalGraphics.Scale(5)) / UserInterface.instance.video.Duration.TotalMicroseconds) % (GlobalGraphics.scaledWidth - GlobalGraphics.Scale(5)), GlobalGraphics.Scale(4)), ThemeManager.GetColor("VideoPlayerProgressBar"));
                    }
                    catch {}
                }
                else if(FramePlayer.audioFrame != null)
                {
                    spriteBatch.Draw(FramePlayer.audioFrame, new Rectangle(0, 0, GlobalGraphics.scaledWidth, GlobalGraphics.scaledHeight), Color.White);
                    if(FramePlayer.currentAudioTime > 0)
                        spriteBatch.Draw(pixel, new Rectangle(0, GlobalGraphics.scaledHeight - GlobalGraphics.Scale(4), (int)(FramePlayer.currentAudioTime * (GlobalGraphics.scaledWidth - GlobalGraphics.Scale(5)) / FramePlayer.audioLength), GlobalGraphics.Scale(4)), ThemeManager.GetColor("VideoPlayerProgressBar"));
                }
            }
            // Draw a red box in the top right corner
            spriteBatch.Draw(pixel, new Rectangle(GlobalGraphics.scaledWidth - GlobalGraphics.Scale(5), 0, GlobalGraphics.Scale(5), GlobalGraphics.Scale(5)), ThemeManager.GetColor("VideoPlayerProgressBar"));
            // Draw an X in the red box
            spriteBatch.Draw(pixel, new Rectangle(GlobalGraphics.scaledWidth - GlobalGraphics.Scale(4), GlobalGraphics.Scale(1), GlobalGraphics.Scale(1), GlobalGraphics.Scale(1)), Color.White);
            spriteBatch.Draw(pixel, new Rectangle(GlobalGraphics.scaledWidth - GlobalGraphics.Scale(2), GlobalGraphics.Scale(1), GlobalGraphics.Scale(1), GlobalGraphics.Scale(1)), Color.White);
            spriteBatch.Draw(pixel, new Rectangle(GlobalGraphics.scaledWidth - GlobalGraphics.Scale(3), GlobalGraphics.Scale(2), GlobalGraphics.Scale(1), GlobalGraphics.Scale(1)), Color.White);
            spriteBatch.Draw(pixel, new Rectangle(GlobalGraphics.scaledWidth - GlobalGraphics.Scale(4), GlobalGraphics.Scale(3), GlobalGraphics.Scale(1), GlobalGraphics.Scale(1)), Color.White);
            spriteBatch.Draw(pixel, new Rectangle(GlobalGraphics.scaledWidth - GlobalGraphics.Scale(2), GlobalGraphics.Scale(3), GlobalGraphics.Scale(1), GlobalGraphics.Scale(1)), Color.White);
            if(Global.ready)
            {
                // Draw a fullscreen button in the bottom right corner
                spriteBatch.Draw(pixel, new Rectangle(GlobalGraphics.scaledWidth - GlobalGraphics.Scale(5), GlobalGraphics.scaledHeight - GlobalGraphics.Scale(5), GlobalGraphics.Scale(5), GlobalGraphics.Scale(5)), Color.Black);
                // Draw a symbol in the fullscreen button
                if(videoFullscreen)
                {
                    spriteBatch.Draw(pixel, new Rectangle(GlobalGraphics.scaledWidth - GlobalGraphics.Scale(4), GlobalGraphics.scaledHeight - GlobalGraphics.Scale(4), GlobalGraphics.Scale(1), GlobalGraphics.Scale(3)), Color.White);
                    spriteBatch.Draw(pixel, new Rectangle(GlobalGraphics.scaledWidth - GlobalGraphics.Scale(3), GlobalGraphics.scaledHeight - GlobalGraphics.Scale(4), GlobalGraphics.Scale(2), GlobalGraphics.Scale(1)), Color.White);
                    spriteBatch.Draw(pixel, new Rectangle(GlobalGraphics.scaledWidth - GlobalGraphics.Scale(2), GlobalGraphics.scaledHeight - GlobalGraphics.Scale(2), GlobalGraphics.Scale(1), GlobalGraphics.Scale(1)), Color.White);
                }
                else
                {
                    spriteBatch.Draw(pixel, new Rectangle(GlobalGraphics.scaledWidth - GlobalGraphics.Scale(4), GlobalGraphics.scaledHeight - GlobalGraphics.Scale(4), GlobalGraphics.Scale(1), GlobalGraphics.Scale(1)), Color.White);
                    spriteBatch.Draw(pixel, new Rectangle(GlobalGraphics.scaledWidth - GlobalGraphics.Scale(2), GlobalGraphics.scaledHeight - GlobalGraphics.Scale(4), GlobalGraphics.Scale(1), GlobalGraphics.Scale(3)), Color.White);
                    spriteBatch.Draw(pixel, new Rectangle(GlobalGraphics.scaledWidth - GlobalGraphics.Scale(4), GlobalGraphics.scaledHeight - GlobalGraphics.Scale(2), GlobalGraphics.Scale(2), GlobalGraphics.Scale(1)), Color.White);
                }
            }
            // Draw tooltip.
            if(MouseInput.MouseState.Position.X > GlobalGraphics.scaledWidth - GlobalGraphics.Scale(5) && MouseInput.MouseState.Position.Y < GlobalGraphics.Scale(5)
                && MouseInput.MouseState.Position.X < GlobalGraphics.scaledWidth && MouseInput.MouseState.Position.Y > 0)
            {
                Global.tooltip = L.T(0, "Overlay:ExitTooltip");
            }
            // Clicking on the fullscreen button in the corner will toggle fullscreen
            if(MouseInput.MouseState.Position.X > GlobalGraphics.scaledWidth - GlobalGraphics.Scale(5) && MouseInput.MouseState.Position.Y > GlobalGraphics.scaledHeight - GlobalGraphics.Scale(5)
                && MouseInput.MouseState.Position.X < GlobalGraphics.scaledWidth && MouseInput.MouseState.Position.Y < GlobalGraphics.scaledHeight)
            {
                Global.tooltip = L.T(0, "Overlay:FullscreenTooltip");
            }
            if(!videoFullscreen && Global.tooltip != "" && tooltipVisible && Global.ready)
            {
                SpriteFont spriteFont = L.FontSmall();
                if(Global.tooltipIsCycler)
                {
                    if (L.cyclerLocale != null)
                    {
                        spriteFont = GlobalContent.GetFont(L.cyclerLocale.fontLarge);
                    }
                    Global.tooltipIsCycler = false;
                }
                string tooltip = Global.tooltip;
                Vector2 tooltipSize = spriteFont.MeasureString(tooltip);
                // Position is relative to mouse position but tries to avoid going off screen
                Vector2 position = new(MouseInput.MouseState.Position.X + 16, MouseInput.MouseState.Position.Y + 16);
                // Make sure it doesn't go off the right side of the screen
                if (position.X + tooltipSize.X + GlobalGraphics.Scale(6) > GlobalGraphics.scaledWidth)
                    position.X = GlobalGraphics.scaledWidth - tooltipSize.X - GlobalGraphics.Scale(6);
                // Make sure it doesn't go off the left side of the screen
                if (position.X < GlobalGraphics.Scale(2))
                    position.X = GlobalGraphics.Scale(2);
                // Make sure it doesn't go off the bottom of the screen
                if (position.Y + tooltipSize.Y + GlobalGraphics.Scale(2) > GlobalGraphics.scaledHeight)
                    position.Y = GlobalGraphics.scaledHeight - tooltipSize.Y - GlobalGraphics.Scale(2);
                // Make sure it doesn't go off the top of the screen
                if (position.Y < GlobalGraphics.Scale(2))
                    position.Y = GlobalGraphics.Scale(2);
                spriteBatch.Draw(pixel, new Rectangle((int)position.X, (int)position.Y, (int)tooltipSize.X + GlobalGraphics.Scale(2), (int)tooltipSize.Y - GlobalGraphics.Scale(2)), ThemeManager.GetColor("BackgroundTooltip"));
                // White text
                GlobalContent.DrawString(spriteBatch, spriteFont, tooltip, new Vector2(position.X + GlobalGraphics.Scale(2), position.Y - GlobalGraphics.Scale(2)), Color.White);
            }
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
                    Global.exiting = false;
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
            if(!handleInput)
                return false;
            // Clicking on the red box in the corner will exit the program
            if(MouseInput.MouseState.Position.X > GlobalGraphics.scaledWidth - GlobalGraphics.Scale(5) && MouseInput.MouseState.Position.Y < GlobalGraphics.Scale(5)
                && MouseInput.MouseState.Position.X < GlobalGraphics.scaledWidth && MouseInput.MouseState.Position.Y > 0)
            {
                if (MouseInput.LastMouseState.LeftButton == ButtonState.Released && MouseInput.MouseState.LeftButton == ButtonState.Pressed)
                {
                    GlobalContent.PlaySound("Select");
                    videoFullscreen = false;
                    if(UserInterface.instance != null)
                        UserInterface.instance.ExitGracefully();
                    return true;
                }
            }
            // Clicking on the fullscreen button in the corner will toggle fullscreen
            if(MouseInput.MouseState.Position.X > GlobalGraphics.scaledWidth - GlobalGraphics.Scale(5) && MouseInput.MouseState.Position.Y > GlobalGraphics.scaledHeight - GlobalGraphics.Scale(5)
                && MouseInput.MouseState.Position.X < GlobalGraphics.scaledWidth && MouseInput.MouseState.Position.Y < GlobalGraphics.scaledHeight)
            {
                if (MouseInput.LastMouseState.LeftButton == ButtonState.Released && MouseInput.MouseState.LeftButton == ButtonState.Pressed)
                {
                    GlobalContent.PlaySound("Select");
                    videoFullscreen = !videoFullscreen;
                    return true;
                }
            }
            // Prevent other input if the video is fullscreen
            if(videoFullscreen && MouseInput.MouseState.Position.X > GlobalGraphics.Scale(5) && MouseInput.MouseState.Position.Y > GlobalGraphics.Scale(5)
                && MouseInput.MouseState.Position.X < GlobalGraphics.scaledWidth - GlobalGraphics.Scale(5) && MouseInput.MouseState.Position.Y < GlobalGraphics.scaledHeight - GlobalGraphics.Scale(5))
            {
                if (MouseInput.LastMouseState.LeftButton == ButtonState.Released && MouseInput.MouseState.LeftButton == ButtonState.Pressed)
                {
                    GlobalContent.PlaySound("Select");
                    videoFullscreen = false;
                    return true;
                }
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
