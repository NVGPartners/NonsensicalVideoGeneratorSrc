using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;

namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// This class stores text and color together.
    /// </summary>
    public class ColoredString
    {
        private string text;
        private Color color = Color.White;
        public ColoredString(string text, Color? color = null)
        {
            this.text = text;
            if (color != null)
                this.color = (Color)color;
        }
        public string Text
        {
            get { return text; }
            set { text = value; }
        }
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }
    }
    /// <summary>
    /// Handle static console input/output.
    /// </summary>
    public static class ConsoleOutput
    {
        public static List<ColoredString> output = new List<ColoredString>();
        public static List<ColoredString> proxyOutput = new List<ColoredString>();
        public static List<ColoredString> scrolledOutput = new List<ColoredString>();
        public static readonly int maxLines = 25;
        private static readonly int lineLength = 70;
        public static int scrollAmount = -1;
        
        public static List<ColoredString> GetOutput()
        {
            return scrollAmount > -1 ? scrolledOutput : output;
        }
        private static void WriteLineInternal(string line, bool newLine = true, Color? color = null)
        {
            Color c = Color.White;
            if (color != null)
                c = (Color)color;
            // Wrap lines.
            if((color == Color.Transparent && bool.Parse(SaveData.saveValues["HiddenVerbose"]) == true) || color != Color.Transparent)
            {
                int lineCount = line.Length / lineLength;
                if (line.Length % lineLength > 0)
                    lineCount++;
                for (int i = 0; i < lineCount; i++)
                {
                    int start = i * lineLength;
                    int end = (i + 1) * lineLength;
                    if (end > line.Length)
                        end = line.Length;
                    if (newLine)
                    {
                        output.Add(new ColoredString(line.Substring(start, end - start), color));
                        proxyOutput.Add(new ColoredString(line.Substring(start, end - start), color));
                    }
                    else
                    {
                        if (output.Count > 0)
                            output[output.Count - 1].Text += line.Substring(start, end - start);
                        else
                            output.Add(new ColoredString(line.Substring(start, end - start), color));
                        if(proxyOutput.Count > 0)
                            proxyOutput[proxyOutput.Count - 1].Text += line.Substring(start, end - start);
                        else
                            proxyOutput.Add(new ColoredString(line.Substring(start, end - start), color));
                    }
                }
            }
            // Remove old lines.
            while (output.Count > maxLines)
                output.RemoveAt(0);
            // Write to file.
            try
            {
                using (StreamWriter writer = new StreamWriter("console.txt", true))
                {
                    if (!newLine)
                        writer.Write(line);
                    else
                        writer.WriteLine(line);
                }
            }
            catch
            {
            }
        }
        // Split newlines.
        public static void WriteLine(string line, Color? color = null)
        {
            if(line == null)
            {
                return;
            }
            Color c = Color.White;
            if (color != null)
                c = (Color)color;
            // Look for color identifiers (plugins).
            // <[255,255,255]>This is one line.
            if(line.StartsWith("<["))
            {
                int end = line.IndexOf("]>");
                if(end > 0)
                {
                    string colorString = line.Substring(2, end - 2);
                    string[] colorValues = colorString.Split(',');
                    if(colorValues.Length == 3)
                    {
                        if (int.TryParse(colorValues[0], out int r) && int.TryParse(colorValues[1], out int g) && int.TryParse(colorValues[2], out int b))
                        {
                            c = new Color(r, g, b);
                            line = line.Substring(end + 2);
                        }
                    }
                }
            }
            string[] lines = line.Replace("\r", "").Split('\n');
            foreach (string l in lines)
                WriteLineInternal(l, true, c);
        }
        public static void Scroll(int deltaY)
        {
            if(scrollAmount <= -1)
                scrollAmount = proxyOutput.Count - maxLines;
            int normalizedDeltaY = (deltaY / 120) * -1;
            scrollAmount += normalizedDeltaY;
            if (scrollAmount < 0)
                scrollAmount = 0;
            else if (scrollAmount > proxyOutput.Count - maxLines)
                scrollAmount = proxyOutput.Count - maxLines;
            if (scrollAmount == proxyOutput.Count - maxLines)
                scrollAmount = -1;
            if(scrollAmount > -1)
            {
                // proxyOutput contains all output
                // maxLines applies
                scrolledOutput.Clear();
                for(int i = scrollAmount; i < scrollAmount + maxLines; i++)
                {
                    if(i < proxyOutput.Count)
                    {
                        scrolledOutput.Add(proxyOutput[i]);
                    }
                }
            }
        }
        public static void ResetScroll()
        {
            scrollAmount = -1;
        }
        public static void Clear()
        {
            output.Clear();
            // DEBUG: Delete file.
            try
            {
                File.Delete("console.txt");
            }
            catch
            {
            }
        }
    }
}