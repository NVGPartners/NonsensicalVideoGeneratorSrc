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
        public int circleSaturation = 100;
        public int circleValue = 100;
        public int circleAlpha = 255;
        public MisclickCircle(Vector2 circleClick)
        {
            this.circleClick = circleClick;
        }
        public void Update()
        {
            if(circleClick.X != 0 && circleClick.Y != 0)
            {
                /*
                if(circleSaturation <= 25 && circleValue <= 25)
                {
                    // By setting the circle size to 0, the circle will be removed from the screen.
                    circleClick = new Vector2(0, 0);
                    circleSize = 0;
                }
                else
                {
                    if(circleSaturation > 25)
                        circleSaturation -= 1;
                    if(circleValue > 25)
                        circleValue -= 1;
                    circleSize += GlobalGraphics.Scale(1);
                }
                */
                circleAlpha -= 16;
                //if(circleSaturation > 25)
                    //circleSaturation -= 1;
                //if(circleValue > 25)
                    //circleValue -= 1;
                circleValue -= 6;
                circleSize += GlobalGraphics.Scale(4);
                if(circleAlpha <= 0)
                {
                    // By setting the circle size to 0, the circle will be removed from the screen.
                    circleClick = new Vector2(0, 0);
                    circleSize = 0;
                }
            }
        }
    }
    /// <summary>
    /// This is the background screen, it draws a scrolling tiled pattern.
    /// </summary>
    public class MisclickScreen : IScreen
    {
        /// <summary>
        /// The title of the screen. This is displayed on the header bar.
        /// </summary>
        public string title { get; } = "Misclick";
        public int layer { get; } = 99;
        public int currentPlacement { get; set; } = -1;
        public ScreenType screenType { get; set; } = ScreenType.Drawn;
        private float hueColor = 0;
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
            // Input.
            if(handleInput)
            {
                // Detect clicks.
                if (MouseInput.LastMouseState.LeftButton == ButtonState.Released && MouseInput.MouseState.LeftButton == ButtonState.Pressed) {
                    // Add a circle.
                    circles.Add(new MisclickCircle(new Vector2(MouseInput.MouseState.X, MouseInput.MouseState.Y)));
                }
            }
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
            if(!bool.Parse(SaveData.saveValues["DisableMotion"]))
            {
                // Draw circles indicating misclicks.
                foreach(MisclickCircle circle in circles)
                {
                    Color circleColor = Color.Black;
                    HSVToRGB((int)hueColor, circle.circleSaturation, circle.circleValue, out circleColor);
                    circleColor = new Color(circleColor.R, circleColor.G, circleColor.B, circle.circleAlpha);
                    GlobalGraphics.DrawCircle(spriteBatch, circle.circleClick, circle.circleSize, circleColor);
                }
            }
        }
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
        }
    }
}
#endif
