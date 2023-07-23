#if MONOGAME
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace NonsensicalVideoGenerator
{
    public class MisclickCircle
    {
        public Vector2 circleClick = new Vector2(0, 0);
        public int circleSize = 1;
        public int circleRGB = 100;
        public MisclickCircle(Vector2 circleClick)
        {
            this.circleClick = circleClick;
        }
        public void Update()
        {
            if(circleClick.X != 0 && circleClick.Y != 0)
            {
                if(circleRGB <= 25)
                {
                    // By setting the circle size to 0, the circle will be removed from the screen.
                    circleClick = new Vector2(0, 0);
                    circleSize = 0;
                    circleRGB = 0;
                }
                else
                {
                    circleRGB -= 1;
                    circleSize += GlobalGraphics.Scale(2);
                }
            }
        }
    }
    /// <summary>
    /// This is the background screen, it draws a scrolling tiled pattern.
    /// </summary>
    public class BackgroundScreen : IScreen
    {
        /// <summary>
        /// The title of the screen. This is displayed on the header bar.
        /// </summary>
        public string title { get; } = "Background";
        public int layer { get; } = 0;
        public int currentPlacement { get; set; } = -1;
        public ScreenType screenType { get; set; } = ScreenType.Hidden;
        private static int totalCount = 1024;
        private int scrollX = 0;
        private int scrollY = 0;
        private float counter = 360;
        private float hueColor = 0;
        private Color backgroundColor = Color.Black;
        private Color tileColor = Color.Black;
        private List<MisclickCircle> circles = new List<MisclickCircle>();
        /// <summary>
        /// Converts HSV color values to RGB
        /// </summary>
        /// <param name="h">0 - 360</param>
        /// <param name="s">0 - 100</param>
        /// <param name="v">0 - 100</param>
        /// <param name="r">0 - 255</param>
        /// <param name="g">0 - 255</param>
        /// <param name="b">0 - 255</param>
        // https://stackoverflow.com/a/70905450
        private void HSVToRGB(int h, int s, int v, out Color color)
        {
            var rgb = new int[3];

            var baseColor = (h + 60) % 360 / 120;
            var shift = (h + 60) % 360 - (120 * baseColor + 60 );
            var secondaryColor = (baseColor + (shift >= 0 ? 1 : -1) + 3) % 3;

            //Setting Hue
            rgb[baseColor] = 255;
            rgb[secondaryColor] = (int) ((Math.Abs(shift) / 60.0f) * 255.0f);

            //Setting Saturation
            for (var i = 0; i < 3; i++)
                rgb[i] += (int) ((255 - rgb[i]) * ((100 - s) / 100.0f));

            //Setting Value
            for (var i = 0; i < 3; i++)
                rgb[i] -= (int) (rgb[i] * (100-v) / 100.0f);

            color = new Color(rgb[0], rgb[1], rgb[2]);
        }
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
            // Change color by hue.
            hueColor += 0.0625f;
            // Wrap to 0 after 360.
            if (hueColor >= 360)
                hueColor = 0;
            // Update background color and tile color hue.
            HSVToRGB((int)hueColor, int.Parse(SaveData.saveValues["BackgroundSaturation"]), 25, out backgroundColor);
            HSVToRGB((int)hueColor, int.Parse(SaveData.saveValues["BackgroundSaturation"]), 40, out tileColor);
            // Input.
            if(handleInput)
            {
                // Detect clicks.
                if (MouseInput.LastMouseState.LeftButton == ButtonState.Released && MouseInput.MouseState.LeftButton == ButtonState.Pressed) {
                    // Add a circle.
                    circles.Add(new MisclickCircle(new Vector2(MouseInput.MouseState.X, MouseInput.MouseState.Y)));
                    // Play a sound.
                    GlobalContent.GetSound("Hover").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                    return true;
                }
            }
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
            // Draw circles indicating misclicks.
            for(int i = 0; i < circles.Count; i++)
            {
                if(circles[i].circleSize <= 0)
                {
                    circles.RemoveAt(i);
                    i--;
                }
                else
                    circles[i].Update();
            }
            return false;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Draw background with new hue.
            spriteBatch.Draw(GlobalGraphics.pixel, new Rectangle(0, 0, GlobalGraphics.scaledWidth, GlobalGraphics.scaledHeight), backgroundColor);
            if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
            {
                // Draw circles indicating misclicks.
                foreach(MisclickCircle circle in circles)
                {
                    Color circleColor = Color.Black;
                    HSVToRGB((int)hueColor, 50, circle.circleRGB, out circleColor);
                    GlobalGraphics.DrawCircle(spriteBatch, circle.circleClick, circle.circleSize, circleColor);
                }
                // Draw the tiled background.
                // This is done by drawing four background layers, each with a different direction for the illusion of infinite scrolling.
                Texture2D tile = GlobalContent.GetTexture("Tile");
                for(int x = 0; x < totalCount; x += tile.Width)
                {
                    for(int y = 0; y < totalCount; y += tile.Height)
                    {
                        spriteBatch.Draw(tile, new Rectangle(GlobalGraphics.Scale(x + scrollX), GlobalGraphics.Scale(y + scrollY), GlobalGraphics.Scale(tile.Width), GlobalGraphics.Scale(tile.Height)), tileColor);
                    }
                }
                // (DEBUG) Draw scroll position.
                //spriteBatch.DrawString(GlobalContent.GetFont("MunroSmall"), $"{scrollX}, {scrollY}", new Vector2(GlobalGraphics.Scale(16), GlobalGraphics.Scale(16)), Color.White);
                // (DEBUG) Draw count of circles.
                //spriteBatch.DrawString(GlobalContent.GetFont("MunroSmall"), $"{circles.Count}", new Vector2(GlobalGraphics.Scale(16), GlobalGraphics.Scale(32)), Color.White);
                // (DEBUG) Draw mouse click state.
                // spriteBatch.DrawString(GlobalContent.GetFont("MunroSmall"), $"{mouseReleased}", new Vector2(GlobalGraphics.Scale(16), GlobalGraphics.Scale(48)), Color.White);
            }
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            GlobalContent.AddTexture("Tile", contentManager.Load<Texture2D>("graphics/tile"));
        }
    }
}
#endif
