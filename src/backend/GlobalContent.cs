using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace NonsensicalVideoGenerator
{
#if !MONOGAME
    public class SoundEffect
    {
        // Just a wrapper for System.Media.SoundPlayer.
        private System.Media.SoundPlayer soundPlayer;
        public SoundEffect(string path)
        {
            soundPlayer = new System.Media.SoundPlayer(path);
        }
        public void Play(float volume = 1.0f, float pitch = 0.0f, float pan = 0.0f)
        {
            soundPlayer.Play();
        }
        public void Dispose()
        {
            soundPlayer.Dispose();
        }
    }
    // Wrapper to load content.
    public class ContentManager
    {
        public ContentManager()
        {
        }
        public T Load<T>(string path)
        {
            if(typeof(T) == typeof(SoundEffect))
            {
                return (T)(object)new SoundEffect(path);
            }
            else
            {
                throw new Exception("Unsupported type.");
            }
        }
    }
#endif
    /// <summary>
    /// Store content for access by other classes.
    /// </summary>
    public static class GlobalContent
    {
        private static Dictionary<string, SoundEffect> sounds = new Dictionary<string, SoundEffect>();
#if MONOGAME
        private static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        private static Dictionary<string, SpriteFont> fonts = new Dictionary<string, SpriteFont>();
        private static Dictionary<SpriteFont, Vector2> fontOffsets = new Dictionary<SpriteFont, Vector2>();
        private static Dictionary<string, Song> songs = new Dictionary<string, Song>();
        private static Dictionary<string, XmlDocument> xmls = new Dictionary<string, XmlDocument>();
        public static void LoadDefaultContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
#else
        private static ContentManager contentManager;
        public static void LoadDefaultContent()
#endif
        {
            // Load default sounds.
            AddSound("AddSource", ThemeManager.LoadLayeredContent<SoundEffect>("sound/addsource"));
            AddSound("Back", ThemeManager.LoadLayeredContent<SoundEffect>("sound/back"));
            AddSound("Error", ThemeManager.LoadLayeredContent<SoundEffect>("sound/error"));
            AddSound("Hover", ThemeManager.LoadLayeredContent<SoundEffect>("sound/hover"));
            AddSound("Option", ThemeManager.LoadLayeredContent<SoundEffect>("sound/option"));
            AddSound("Prompt", ThemeManager.LoadLayeredContent<SoundEffect>("sound/prompt"));
            AddSound("Quit", ThemeManager.LoadLayeredContent<SoundEffect>("sound/quit"));
            AddSound("RenderComplete", ThemeManager.LoadLayeredContent<SoundEffect>("sound/rendercomplete"));
            AddSound("Select", ThemeManager.LoadLayeredContent<SoundEffect>("sound/select"));
            AddSound("Start", ThemeManager.LoadLayeredContent<SoundEffect>("sound/start"));
            AddSound("CompatSelect", ThemeManager.LoadLayeredContent<SoundEffect>("sound/compatselect"));
            AddSound("Disambiguation", ThemeManager.LoadLayeredContent<SoundEffect>("sound/disambiguation"));
#if MONOGAME
            // Load default fonts.
            int scale = int.Parse(SaveData.saveValues["ScreenScale"], System.Globalization.CultureInfo.InvariantCulture);
            AddFont("Munro", ThemeManager.LoadLayeredContent<SpriteFont>("fonts/munro-x"+scale), new Vector2(0, 0));
            AddFont("MunroSmall", ThemeManager.LoadLayeredContent<SpriteFont>("fonts/munro-small-x"+scale), new Vector2(0, 0));
            AddFont("NotoSans", ThemeManager.LoadLayeredContent<SpriteFont>("fonts/notosans-x"+scale), new Vector2(-scale/2, -scale*2f));
            // Fallback (multi-language support) system font.
            //AddFont("Arial", UserInterface.instance.Content.Load<SpriteFont>("Arial"));
            // Load default songs.
            for(int i = 1; i < ThemeManager.GetSongCount()+1; i++)
            {
                Song? song = null;
                try
                {
                    song = ThemeManager.LoadLayeredContent<Song>($"music/theme{i}");
                }
                catch(Exception ex)
                {
                    ConsoleOutput.WriteLine($"Failed to load theme{i} song: {ex.Message}", Color.Red);
                }
                if(song != null)
                {
                    AddSong($"Theme{i}", song);
                }
            }
            // Create pixel shape.
            Texture2D pixel = new Texture2D(graphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            AddTexture("Pixel", pixel);
            // Create a hollow circle.
            int circleSize = GlobalGraphics.Scale(128);
            Texture2D circle = new Texture2D(graphicsDevice, circleSize, circleSize);
            Color[] data = new Color[circleSize * circleSize];
            for(int x = 0; x < circleSize; x++)
            {
                for(int y = 0; y < circleSize; y++)
                {
                    int distance = (int)Math.Sqrt(Math.Pow(x - circleSize/2, 2) + Math.Pow(y - circleSize/2, 2));
                    if(distance < circleSize/2)
                    {
                        data[x + y * circleSize] = Color.White;
                    }   
                    else
                    {
                        data[x + y * circleSize] = Color.Transparent;
                    }
                }
            }
            float hollowInside = GlobalGraphics.scale * 4;
            for(int x = 0; x < circleSize-hollowInside; x++)
            {
                for(int y = 0; y < circleSize-hollowInside; y++)
                {
                    int distance = (int)Math.Sqrt(Math.Pow(x - circleSize/2, 2) + Math.Pow(y - circleSize/2, 2));
                    if(distance < (circleSize/2)-hollowInside)
                    {
                        data[x + y * circleSize] = Color.Transparent;
                    }
                }
            }
            circle.SetData(data);
            AddTexture("HollowCircle", circle);
            // Create a filled circle.
            Texture2D filledCircle = new Texture2D(graphicsDevice, circleSize, circleSize);
            Color[] data2 = new Color[circleSize * circleSize];
            for (int i = 0; i < data2.Length; ++i)
            {
                int x = i % circleSize;
                int y = i / circleSize;
                int distance = (int)Math.Sqrt(x * x + y * y);
                if (distance <= circleSize/2)
                {
                    data2[i] = Color.White;
                }
                else
                {
                    data2[i] = Color.Transparent;
                }
            }
            filledCircle.SetData(data2);
            AddTexture("FilledCircle", filledCircle);
            // Interactables
            AddTexture("ActionButton", ThemeManager.LoadLayeredContent<Texture2D>("graphics/actionbutton"));
            AddTexture("InteractiveButtonSide", ThemeManager.LoadLayeredContent<Texture2D>("graphics/interactivebuttonside"));
            AddTexture("InteractiveButtonInner", ThemeManager.LoadLayeredContent<Texture2D>("graphics/interactivebuttoninner"));
            // Why are dials still sticking around?
            /*
            AddTexture("InteractiveDial", ThemeManager.LoadLayeredContent<Texture2D>("graphics/interactivedial"));
            // Values
            for(int i = 0; i < 30; i++)
                AddTexture("InteractiveDialValue" + i, ThemeManager.LoadLayeredContent<Texture2D>("graphics/interactivedialvalue" + i));
            */
            AddTexture("InteractiveSwitch", ThemeManager.LoadLayeredContent<Texture2D>("graphics/interactiveswitch"));
            AddTexture("InteractiveSwitchOn", ThemeManager.LoadLayeredContent<Texture2D>("graphics/interactiveswitchon"));
            AddTexture("InteractiveSwitchOff", ThemeManager.LoadLayeredContent<Texture2D>("graphics/interactiveswitchoff"));
            AddTexture("InteractiveTextEntrySide", ThemeManager.LoadLayeredContent<Texture2D>("graphics/interactivetextentryside"));
            AddTexture("InteractiveTextEntryInner", ThemeManager.LoadLayeredContent<Texture2D>("graphics/interactivetextentryinner"));
            AddTexture("PluginPage", ThemeManager.LoadLayeredContent<Texture2D>("graphics/pluginpage"));
            AddTexture("ScrollHandle", ThemeManager.LoadLayeredContent<Texture2D>("graphics/scrollhandle"));
#endif
        }
        public static void UnloadContent()
        {
            foreach(string key in sounds.Keys)
            {
                sounds[key].Dispose();
            }
            foreach(string key in textures.Keys)
            {
                textures[key].Dispose();
            }
            foreach(string key in songs.Keys)
            {
                songs[key].Dispose();
            }
        }
        public static bool AddTexture(string name, Texture2D texture)
        {
            if (textures.ContainsKey(name))
            {
                textures[name].Dispose();
                textures[name] = texture;
            }
            else
            {
                textures.Add(name, texture);
            }
            return true;
        }
        public static bool AddFont(string name, SpriteFont font, Vector2 offset)
        {
            if (fonts.ContainsKey(name))
            {
                fonts[name] = font;
                fontOffsets[font] = offset;
            }
            else
            {
                fonts.Add(name, font);
                fontOffsets.Add(font, offset);
            }
            return true;
        }
        public static bool AddSound(string name, SoundEffect sound)
        {
            if (sounds.ContainsKey(name))
            {
                sounds[name].Dispose();
                sounds[name] = sound;
            }
            else
            {
                sounds.Add(name, sound);
            }
            return true;
        }
        public static bool AddSong(string name, Song song)
        {
            if (songs.ContainsKey(name))
            {
                songs[name].Dispose();
                songs[name] = song;
            }
            else
            {
                songs.Add(name, song);
            }
            return true;
        }
        public static bool AddXml(string name, XmlDocument xml)
        {
            if (xmls.ContainsKey(name))
            {
                xmls[name] = xml;
            }
            else
            {
                xmls.Add(name, xml);
            }
            return true;
        }
        public static SoundEffect GetSound(string name)
        {
            return sounds[name];
        }
        public static int GetSoundCount()
        {
            return sounds.Count;
        }
        public static Texture2D GetTexture(string name)
        {
            return textures[name];
        }
        public static int GetTextureCount()
        {
            return textures.Count;
        }
        public static SpriteFont GetFont(string name)
        {
            return fonts[name];
        }
        public static int GetFontCount()
        {
            return fonts.Count;
        }
        public static bool CheckFont(string name)
        {
            return fonts.ContainsKey(name);
        }
        public static Song GetSong(string name)
        {
            return songs[name];
        }
        public static int GetSongCount()
        {
            return songs.Count;
        }
        public static Song GetSongByIndex(int index)
        {
            // MUST BE IN RANGE!
            if (index < 0 || index >= songs.Count)
                index = 0;
            return songs.Values.ElementAt(index);
        }
        public static XmlDocument GetXml(string name)
        {
            return xmls[name];
        }
        public static int GetXmlCount()
        {
            return xmls.Count;
        }
        public static void DrawString(SpriteBatch spriteBatch, SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            Vector2 scale2 = new Vector2(scale, scale);
            DrawString(spriteBatch, spriteFont, text, position, color, rotation, origin, scale2, effects, layerDepth);
        }
        public static void DrawString(SpriteBatch spriteBatch, SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            // Offset text if font has an offset.
            if (fontOffsets.ContainsKey(spriteFont))
            {
                position += fontOffsets[spriteFont];
            }
            spriteBatch.DrawString(spriteFont, text, position, color, rotation, origin, scale, effects, layerDepth);
        }
        public static void DrawString(SpriteBatch spriteBatch, SpriteFont spriteFont, string text, Vector2 position, Color color)
        {
            // Offset text if font has an offset.
            if (fontOffsets.ContainsKey(spriteFont))
            {
                position += fontOffsets[spriteFont];
            }
            spriteBatch.DrawString(spriteFont, text, position, color);
        }
    }
}
