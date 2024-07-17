#if MONOGAME
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended;
using MonoGame.Extended.Tweening;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// This is the background screen, it draws a scrolling tiled pattern.
    /// </summary>
    public class BackgroundScreen : IScreen
    {
        /// <summary>
        /// The title of the screen. This is displayed on the header bar.
        /// </summary>
        public string title { get; } = "Background";
        public int layer { get; set; } = 0;
        public int currentPlacement { get; set; } = -1;
        public ScreenType screenType { get; set; } = ScreenType.Hidden;
        private static int totalCount = 1024;
        private int scrollX = 0;
        private int scrollY = 0;
        private float counter = 360;
        private float mouseX = 0;
        private float mouseY = 0;
        private float lerpMouseX = 0;
        private float lerpMouseY = 0;
        private float lerpSpeed = 0.025f;
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
        public bool Update(GameTime gameTime, bool handleInput)
        {
            // Move background.
            counter -= 0.125f;
            if(counter <= 0)
            {
                // Print time taken to scroll.
                //ConsoleOutput.WriteLine("Time taken to scroll: " + gameTime.TotalGameTime.TotalSeconds);
                counter = 360;
            }
            // I started off making this scroll from top right to bottom left.
            // Then I changed it to scroll in a circular motion.
            // Now it scrolls in some sort of zig-zag pattern.
            scrollX = (-totalCount/4) - (int)(Math.Sin(counter * Math.PI / -90) * GlobalGraphics.width / 2);
            scrollY = (-totalCount/4) - (int)(Math.Cos(counter * Math.PI / -180) * GlobalGraphics.height / 2);
            /*
            if(scrollX >= -GlobalGraphics.width-1)
                scrollX = -totalCount + GlobalGraphics.width;
            if(scrollY >= -GlobalGraphics.height-1)
                scrollY = -totalCount + GlobalGraphics.height;
            */
            // Input.
            if(handleInput)
            {                
                // Pan the screen slightly when moving mouse (from center)
                mouseX = MouseInput.MouseState.Position.X + GlobalGraphics.scaledWidth / 2;
                mouseY = MouseInput.MouseState.Position.Y + GlobalGraphics.scaledHeight / 2;

                // Detect clicks.
                if (MouseInput.LastMouseState.LeftButton == ButtonState.Released && MouseInput.MouseState.LeftButton == ButtonState.Pressed) {
                    // Play a sound.
                    GlobalContent.GetSound("Hover").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    return true;
                }
            }
            else
            {
                // Lerp last mouse position to center (smooth)
                mouseX = MathHelper.Lerp(mouseX, GlobalGraphics.scaledWidth / 2, GlobalGraphics.Scale(lerpSpeed));
                mouseY = MathHelper.Lerp(mouseY, GlobalGraphics.scaledHeight / 2, GlobalGraphics.Scale(lerpSpeed));
            }
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Draw background with new hue.
            spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(GlobalGraphics.Scale((int)-Global.drawOffset.X), GlobalGraphics.Scale((int)-Global.drawOffset.Y), (int)UserInterface.instance.preferredResolution.X, (int)UserInterface.instance.preferredResolution.Y), ThemeManager.GetColor("BackgroundScreen"));
            
            if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
            {
                // End existing spritebatch
                spriteBatch.End();

                // Lerp the mouse position (limit mouse movement to lerpSpeed so it smooths out)
                lerpMouseX = MathHelper.Lerp(lerpMouseX, mouseX, GlobalGraphics.Scale(lerpSpeed));
                lerpMouseY = MathHelper.Lerp(lerpMouseY, mouseY, GlobalGraphics.Scale(lerpSpeed));

                // Clamp to screen bounds
                lerpMouseX = Math.Clamp(lerpMouseX, -GlobalGraphics.scaledWidth, GlobalGraphics.scaledWidth*2);
                lerpMouseY = Math.Clamp(lerpMouseY, -GlobalGraphics.scaledHeight, GlobalGraphics.scaledHeight*2);

                float mX = Global.drawOffset.X /*- lerpMouseX*/;
                float mY = Global.drawOffset.Y /*- lerpMouseY*/;

                // Create matrix
                Matrix matrix = Matrix.CreateTranslation(mX, mY, 0);
                
                // Create spritebatch with panning (and respect draw offset)
                spriteBatch.Begin(SpriteSortMode.Deferred,
                    BlendState.AlphaBlend,
                    SamplerState.PointClamp,
                    null, null, null, matrix);

                // Draw the background.
                // This is done by drawing four background layers, each with a different direction for the illusion of infinite scrolling.
                Texture2D tile = GlobalContent.GetTexture("Tile");
                for(int x = 0; x < totalCount; x += tile.Width)
                {
                    for(int y = 0; y < totalCount; y += tile.Height)
                    {
                        spriteBatch.Draw(tile, new Rectangle(GlobalGraphics.Scale(x + scrollX), GlobalGraphics.Scale(y + scrollY), GlobalGraphics.Scale(tile.Width), GlobalGraphics.Scale(tile.Height)), ThemeManager.GetColor("TileBackgroundScreen"));
                    }
                }

                // (DEBUG) Draw scroll position.
                //GlobalContent.DrawString(spriteBatch, L.FontSmall(), $"{scrollX}, {scrollY}", new Vector2(GlobalGraphics.Scale(16), GlobalGraphics.Scale(16)), Color.White);
                // (DEBUG) Draw count of circles.
                //GlobalContent.DrawString(spriteBatch, L.FontSmall(), $"{circles.Count}", new Vector2(GlobalGraphics.Scale(16), GlobalGraphics.Scale(32)), Color.White);
                // (DEBUG) Draw mouse click state.
                // GlobalContent.DrawString(spriteBatch, L.FontSmall(), $"{mouseReleased}", new Vector2(GlobalGraphics.Scale(16), GlobalGraphics.Scale(48)), Color.White);
                
                // End offset spritebatch
                spriteBatch.End();
                // Remake spritebatch
                spriteBatch.Begin(SpriteSortMode.Deferred,
                    BlendState.AlphaBlend,
                    SamplerState.PointClamp,
                    null, null, null, Matrix.CreateTranslation(GlobalGraphics.Scale(Global.drawOffset.X), GlobalGraphics.Scale(Global.drawOffset.Y), 0));
            }
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            GlobalContent.AddTexture("Tile", ThemeManager.LoadLayeredContent<Texture2D>("graphics/tile"));
        }
    }
}
#endif
