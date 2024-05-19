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
            Vector2 measured = L.FontSmall().MeasureString(text);
            // Offset measurements.
            measured.X += Scale(3);
            measured.Y -= Scale(5);
            Rectangle generatedRectangle = new Rectangle(x, y, (int)measured.X, (int)measured.Y);
            spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), generatedRectangle, borderColor);
            spriteBatch.Draw(GlobalContent.GetTexture("Pixel"), new Rectangle(x + Scale(1), y + Scale(1), (int)measured.X - Scale(2), (int)measured.Y - Scale(2)), color);
            spriteBatch.DrawString(L.FontSmall(), text, new Vector2(x+Scale(2), y-Scale(4)), textColor);
            return new Rectangle(x, y, (int)measured.X, (int)measured.Y);
        }
        public static void DrawCircle(SpriteBatch spriteBatch, Vector2 pos, int radius, Color color, bool hollow = true)
        {
            Texture2D circle = GlobalContent.GetTexture($"{(hollow ? "Hollow" : "Filled")}Circle");
            spriteBatch.Draw(circle, new Rectangle((int)pos.X - radius/2, (int)pos.Y - radius/2, radius, radius), color);
        }
        public static int GetSmallStringWidth(string input)
        {
            int smallestWidth = 1; // I, !, ., :, ;, ', |
            int smallerWidth = 2; // J, 1, `, (, ), [, ], SPACE
            int smallWidth = 3; // C, E, F, L, S, T, Z, 2, 3, 5, 7, $, ^, *, {, }, ?, /, \, ", -, +
            int largeWidth = 4; // A, B, D, G, H, K, N, O, P, Q, R, U, 0, 4, 6, 8, 9, <, >
            int largerWidth = 5; // M, V, W, X, Y, #, &, =, _, ~
            int largestWidth = 7; // @, %
            int width = 0;
            foreach (char c in input)
            {
                if (c == 'I' || c == '!' || c == '.' || c == ':' || c == ';' || c == '\'' || c == '|')
                    width += smallestWidth;
                else if (c == 'J' || c == '1' || c == '`' || c == '(' || c == ')' || c == '[' || c == ']' || c == ' ')
                    width += smallerWidth;
                else if (c == 'C' || c == 'E' || c == 'F' || c == 'L' || c == 'S' || c == 'T' || c == 'Z' || c == '2' || c == '3' || c == '5' || c == '7' || c == '$' || c == '^' || c == '*' || c == '{' || c == '}' || c == '?' || c == '/' || c == '\\' || c == '"' || c == '-' || c == '+')
                    width += smallWidth;
                else if (c == 'A' || c == 'B' || c == 'D' || c == 'G' || c == 'H' || c == 'K' || c == 'N' || c == 'O' || c == 'P' || c == 'Q' || c == 'R' || c == 'U' || c == '0' || c == '4' || c == '6' || c == '8' || c == '9' || c == '<' || c == '>')
                    width += largeWidth;
                else if (c == 'M' || c == 'V' || c == 'W' || c == 'X' || c == 'Y' || c == '#' || c == '&' || c == '=' || c == '_' || c == '~')
                    width += largerWidth;
                else if (c == '@' || c == '%')
                    width += largestWidth;
                else
                    width += smallWidth;
            }
            return width;
        }
        public static int GetSmallStringHeight(string input)
        {
            int smallestHeight = 1; // _
            int smallerHeight = 2; // ., ,
            int smallHeight = 4; // -, :, ;
            int midSmallHeight = 5; // +, ~
            int midLargeHeight = 6; // A-Z, 0-9, !, #, %, &, =, ?
            int largeHeight = 7; // $, ^, *, (, ), [, ], \, ', /, {, |, ", <, >
            int largerHeight = 8; // @, }
            int largestHeight = 9; // `
            int height = 0;
            foreach (char c in input)
            {
                if (c == '_')
                    height = smallestHeight;
                else if (c == '.' || c == ',')
                    height = smallerHeight;
                else if (c == '-' || c == ':' || c == ';')
                    height = smallHeight;
                else if (c == '+' || c == '~')
                    height = midSmallHeight;
                else if (c >= 'A' && c <= 'Z' || c >= '0' && c <= '9' || c == '!' || c == '#' || c == '%' || c == '&' || c == '=' || c == '?')
                    height = midLargeHeight;
                else if (c == '$' || c == '^' || c == '*' || c == '(' || c == ')' || c == '[' || c == ']' || c == '\\' || c == '\'' || c == '/' || c == '{' || c == '|' || c == '"' || c == '<' || c == '>')
                    height = largeHeight;
                else if (c == '@' || c == '}')
                    height = largerHeight;
                else if (c == '`')
                    height = largestHeight;
                else
                    height = midLargeHeight;
            }
            return height;
        }
    }
}
#endif
