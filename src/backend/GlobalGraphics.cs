#if MONOGAME
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// This class stores repeated graphical functions for ease of use and cleanliness.
    /// </summary>
    public static class GlobalGraphics
    {
        public static int width = int.Parse(SaveData.saveValues["ScreenWidth"], System.Globalization.CultureInfo.InvariantCulture);
        public static int height = int.Parse(SaveData.saveValues["ScreenHeight"], System.Globalization.CultureInfo.InvariantCulture);
        public static int scale = int.Parse(SaveData.saveValues["ScreenScale"], System.Globalization.CultureInfo.InvariantCulture);
        public static int scaledWidth = (int)(width * scale);
        public static int scaledHeight = (int)(height * scale);
        public static int Scale(int value)
        {
            return (int)(value * scale);
        }
        public static float Scale(float value)
        {
            return (float)(value * scale);
        }
        /// <summary>
        /// The default 1x1 pixel texture.
        /// </summary>
        public static Rectangle DrawButton(SpriteBatch spriteBatch, int x, int y, Color color, string text, Color textColor, Color borderColor)
        {
            Vector2 measured = GlobalContent.GetFont("MunroSmall").MeasureString(text);
            // Offset measurements.
            measured.X += Scale(3);
            measured.Y -= Scale(5);
            Rectangle generatedRectangle = new Rectangle(x, y, (int)measured.X, (int)measured.Y);
            spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), generatedRectangle, borderColor);
            spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(x + Scale(1), y + Scale(1), (int)measured.X - Scale(2), (int)measured.Y - Scale(2)), color);
            spriteBatch.DrawString(GlobalContent.GetFont("MunroSmall"), text, new Vector2(x+Scale(2), y-Scale(4)), textColor);
            return new Rectangle(x, y, (int)measured.X, (int)measured.Y);
        }
        public static void DrawCircle(SpriteBatch spriteBatch, Vector2 pos, int radius, Color color, bool hollow = true)
        {
            Texture2D circle = GlobalContent.GetTexture($"{(hollow ? "Hollow" : "Filled")}Circle");
            spriteBatch.Draw(circle, new Rectangle((int)pos.X - radius/2, (int)pos.Y - radius/2, radius, radius), color);
        }
    }
}
#endif
