using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NonsensicalVideoGenerator
{
    public class AspectRatio
    {
        public int width;
        public int height;
        public Vector2 drawOffset;
        public Point preferredResolution;
        public AspectRatio()
        {
            width = 4;
            height = 3;
            drawOffset = new Vector2(0, 0);
            preferredResolution = new Point(320, 240);
        }
        public AspectRatio(int width, int height, Vector2 drawOffset, Point preferredResolution)
        {
            this.width = width;
            this.height = height;
            this.drawOffset = drawOffset;
            this.preferredResolution = preferredResolution;
        }
        public readonly static List<AspectRatio> All = new()
        {
            // Television
            new AspectRatio(4, 3, new Vector2(0, 0), new Point(320, 240)), // Standard Television
            new AspectRatio(5, 4, new Vector2(0, 8), new Point(320, 256)), // Standard Monitor
            // Mobile
            //new AspectRatio(9, 16, new Vector2(0, 164), new Point(320, 569)),
            //new AspectRatio(9, 20, new Vector2(0, 235), new Point(320, 720)), // Pixel 7a
            // Tablet
            new AspectRatio(3, 2, new Vector2(21, 0), new Point(360, 240)), // Microsoft Surface
            // Widescreen
            new AspectRatio(16, 9, new Vector2(53, 0), new Point(427, 240)), // Widescreen Television
            new AspectRatio(16, 10, new Vector2(32, 0), new Point(384, 240)), // Widescreen Monitor
            // Ultrawide
            new AspectRatio(64, 27, new Vector2(124, 0), new Point(569, 240)), // Consumer ultrawide (21:9)
            //new AspectRatio(32, 9, new Vector2(266, 0), new Point(853, 240)), // Super ultrawide
            // Square
            new AspectRatio(1, 1, new Vector2(0, 40), new Point(320, 320)), // Square
        };
    }
    /// <summary>
    /// This class stores repeated graphical functions for ease of use and cleanliness.
    /// </summary>
    public static class GlobalGraphics
    {
        public static float scale = float.Parse(SaveData.saveValues["ScreenScale"], CultureInfo.InvariantCulture);
        public static Vector2 drawOffset = new(0, 0);
        public static Point preferredResolution = new(320, 240);
        public static bool fullScreen = false;
        public static int scaledWidth = (int)(new AspectRatio().preferredResolution.X * scale);
        public static int scaledHeight = (int)(new AspectRatio().preferredResolution.Y * scale);
        public static AspectRatio FindMatchingAspectRatio()
        {
            AspectRatio closestMatch = new AspectRatio();
            // Get the current aspect ratio of the current screen
            int curWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            int curHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            float curAspectRatio = (float)curWidth / curHeight;
            // Get the closest match in AspectRatio.All (aspectRatio.width / aspectRatio.height)
            float closestMatchDifference = Math.Abs(curAspectRatio - (float)closestMatch.width / closestMatch.height);
            foreach(AspectRatio aspectRatio in AspectRatio.All)
            {
                float difference = Math.Abs(curAspectRatio - (float)aspectRatio.width / aspectRatio.height);
                if(difference < closestMatchDifference)
                {
                    closestMatch = aspectRatio;
                    closestMatchDifference = difference;
                }
            }
            // Set the aspect ratio to the closest match
            return new AspectRatio(closestMatch.width, closestMatch.height, closestMatch.drawOffset, new Point(closestMatch.preferredResolution.X, closestMatch.preferredResolution.Y));
        }
        public static void SetAspectRatio(AspectRatio aspectRatio)
        {
            drawOffset = aspectRatio.drawOffset;
            preferredResolution = new Point((int)(aspectRatio.preferredResolution.X * scale), (int)(aspectRatio.preferredResolution.Y * scale));
            if(UserInterface.instance != null)
            {
                UserInterface.instance.Resize(preferredResolution.X, preferredResolution.Y);
                // center to screen
                UserInterface.instance.CenterToScreen();
            }
        }
        public static AspectRatio GetAspectRatio()
        {
            foreach (AspectRatio aspectRatio in AspectRatio.All)
            {
                if (aspectRatio.drawOffset == drawOffset && aspectRatio.preferredResolution == new Point((int)(preferredResolution.X / scale), (int)(preferredResolution.Y / scale)))
                    return aspectRatio;
            }
            return new AspectRatio();
        }
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
            GlobalContent.DrawString(spriteBatch, L.FontSmall(), text, new Vector2(x+Scale(2), y-Scale(4)), textColor);
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
        public static int GetGCD(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }
    }
}
