using System;
using System.Collections.Generic;
using System.Linq;
#if MONOGAME
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
#else
using System.Drawing;
using System.Windows.Forms;
#endif

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
        private static Dictionary<string, Song> songs = new Dictionary<string, Song>();
        private static Dictionary<string, string[]> songTitlesAndArtists = new Dictionary<string, string[]>();
        public static void LoadDefaultContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
#else
        private static ContentManager contentManager;
        public static void LoadDefaultContent()
#endif
        {
            // Load default sounds.
            AddSound("AddSource", contentManager.Load<SoundEffect>("sound/addsource"));
            AddSound("Back", contentManager.Load<SoundEffect>("sound/back"));
            AddSound("Error", contentManager.Load<SoundEffect>("sound/error"));
            AddSound("Hover", contentManager.Load<SoundEffect>("sound/hover"));
            AddSound("Option", contentManager.Load<SoundEffect>("sound/option"));
            AddSound("Prompt", contentManager.Load<SoundEffect>("sound/prompt"));
            AddSound("Quit", contentManager.Load<SoundEffect>("sound/quit"));
            AddSound("RenderComplete", contentManager.Load<SoundEffect>("sound/rendercomplete"));
            AddSound("Select", contentManager.Load<SoundEffect>("sound/select"));
            AddSound("Start", contentManager.Load<SoundEffect>("sound/start"));
            AddSound("CompatSelect", contentManager.Load<SoundEffect>("sound/compatselect"));
            AddSound("Disambiguation", contentManager.Load<SoundEffect>("sound/disambiguation"));
#if MONOGAME
            // Load default fonts.
            int scale = int.Parse(SaveData.saveValues["ScreenScale"], System.Globalization.CultureInfo.InvariantCulture);
            AddFont("Munro", contentManager.Load<SpriteFont>("fonts/munro-x"+scale));
            AddFont("MunroNarrow", contentManager.Load<SpriteFont>("fonts/munro-narrow-x"+scale));
            AddFont("MunroSmall", contentManager.Load<SpriteFont>("fonts/munro-small-x"+scale));
            // Load default songs.
            AddSong("Theme", contentManager.Load<Song>("music/theme"), "This Is Nonsense!", "KiwifruitDev");
            AddSong("Theme2", contentManager.Load<Song>("music/theme2"), "A Nonsensical Song", "Bobby I Guess");
            //AddSong("Theme3", contentManager.Load<Song>("music/theme3"), "Creation", "Bobby I Guess");
            // Dynamic implementation is required for these two songs.
            // They're context-sensitive, so we'll add them at a later time.
            // TODO: Add songs to mgcb when they're added.
            //AddSong("Theme4", contentManager.Load<Song>("music/theme4"));
            //AddSong("Theme5", contentManager.Load<Song>("music/theme5"));
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
            float hollowInside = GlobalGraphics.scale;
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
#endif
        }
        public static void UnloadContent()
        {
            foreach(string key in sounds.Keys)
            {
                sounds[key].Dispose();
            }
#if MONOGAME
            foreach(string key in textures.Keys)
            {
                textures[key].Dispose();
            }
            foreach(string key in songs.Keys)
            {
                songs[key].Dispose();
            }
#endif
        }
#if MONOGAME
        public static bool AddTexture(string name, Texture2D texture)
        {
            if (textures.ContainsKey(name))
                return false;
            textures.Add(name, texture);
            return true;
        }
        public static bool AddFont(string name, SpriteFont font)
        {
            if (fonts.ContainsKey(name))
                return false;
            fonts.Add(name, font);
            return true;
        }
#endif
        public static bool AddSound(string name, SoundEffect sound)
        {
            if (sounds.ContainsKey(name))
                return false;
            sounds.Add(name, sound);
            return true;
        }
        public static SoundEffect GetSound(string name)
        {
            return sounds[name];
        }
#if MONOGAME
        public static bool AddSong(string name, Song song, string title = null, string artist = null)
        {
            if (songs.ContainsKey(name))
                return false;
            songs.Add(name, song);
            songTitlesAndArtists.Add(name, new string[] { title ?? name, artist ?? "Unknown" });
            return true;
        }
        public static int GetSongCount()
        {
            return songs.Count;
        }
        public static Texture2D GetTexture(string name)
        {
            return textures[name];
        }
        public static SpriteFont GetFont(string name)
        {
            return fonts[name];
        }
        public static Song GetSong(string name)
        {
            return songs[name];
        }
        public static Song GetSongByIndex(int index)
        {
            return songs.Values.ElementAt(index);
        }
        public static string GetSongTitleByIndex(int index)
        {
            return songTitlesAndArtists.Values.ElementAt(index)[0];
        }
        public static string GetSongArtistByIndex(int index)
        {
            return songTitlesAndArtists.Values.ElementAt(index)[1];
        }
#endif
    }
}
