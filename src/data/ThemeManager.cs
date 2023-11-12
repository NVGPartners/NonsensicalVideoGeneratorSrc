using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if MONOGAME
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;



#else
using System.Drawing;
#endif
using Newtonsoft.Json;

namespace NonsensicalVideoGenerator
{
    public class Theme
    {
        public string name;
        public string description;
        public string prefix;
        public bool mgcb;
        public int songCount;
        public Theme(string name, string description, string prefix, int songCount, bool mgcb = false)
        {
            this.name = name;
            this.description = description;
            this.prefix = prefix;
            this.songCount = songCount;
            this.mgcb = mgcb;
        }
    }
    public static class DefaultThemes
    {
        public static Theme Nonsensical = new Theme("Nonsensical", "The default theme.", "", 5, true);
        public static Theme Studio = new Theme("Studio", "Reminiscent of days past.", "layers/studio/", 5, true);
        public static Theme Halloween = new Theme("Halloween", "A spooky theme.", "layers/halloween/", 1, true);
        public static Theme Christmas = new Theme("Christmas", "A festive theme.", "layers/christmas/", 5, true);
        public static List<Theme> themes = new List<Theme>()
        {
            Nonsensical,
            Studio,
            Halloween,
            Christmas
        };
    }
    public static class ThemeManager
    {
        public static List<Theme> themes = new List<Theme>()
        {
            DefaultThemes.Nonsensical,
            DefaultThemes.Studio,
            DefaultThemes.Halloween,
            DefaultThemes.Christmas
        };
        public static Theme activeTheme = DefaultThemes.Nonsensical;
        public static void LoadThemes()
        {
            themes.Clear();
            foreach(Theme theme in DefaultThemes.themes)
            {
                themes.Add(theme);
            }
            bool themeChanged = false;
            List<Plugin> themeList = PluginHandler.GetEnabledPluginsOfType(AddonType.Theme);
            foreach (Plugin theme in themeList)
            {
                string themeName = theme.GetDisplayName();
                string themeDescription = "";
                int songCount = DefaultThemes.Nonsensical.songCount;
                if(theme.settings.Count > 0)
                {
                    List<string> extsettings = new();
                    foreach(KeyValuePair<string, object> setting in theme.settings)
                    {
                        // Hide display name and create description, add options too
                        if(theme.settingTypes[setting.Key] == SettingType.Label && setting.Key.ToLower() != "addon type" && setting.Key.ToLower() != "display name" && theme.settingTypes.ContainsKey(setting.Key))
                        {
                            themeDescription = $"{setting.Value.ToString()}";
                            break;
                        }
                    }
                }
                // Make sure directory exists
                string layerPath = Path.GetDirectoryName(theme.path) + "/layer/";
                if(!Directory.Exists(layerPath))
                {
                    // Disable the addon
                    PluginHandler.plugins.Find(x => x.path == theme.path).enabled = false;
                    PluginHandler.SavePluginSettings();
                    GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    if(activeTheme.prefix != "")
                    {
                        SaveData.saveValues["ActiveTheme"] = "";
                        SaveData.Save();
                        ApplyTheme(DefaultThemes.Nonsensical);
                    }
                    continue;
                }
                // Count layerpath/music/theme*.ogg (indexed from 1 to 9)
                if(Directory.Exists(layerPath + "music/"))
                {
                    for(int i = 1; i < 10; i++)
                    {
                        if(File.Exists(layerPath + $"music/theme{i}.wma"))
                        {
                            songCount = i;
                        }
                    }
                }
                Theme thisTheme = new Theme(themeName, themeDescription, layerPath, songCount);
                themes.Add(thisTheme);
                if(SaveData.saveValues["ActiveTheme"] == Path.GetFileName(theme.path))
                {
                    activeTheme = thisTheme;
                    themeChanged = true;
                }
                else
                {
                    // Disable the addon
                    PluginHandler.plugins.Find(x => x.path == theme.path).enabled = false;
                    PluginHandler.SavePluginSettings();
                }
            }
            if(themeChanged)
            {
                ConsoleOutput.WriteLine($"Theme changed to {activeTheme.name}.", Color.Yellow);
                ApplyTheme(activeTheme);
            }
            else
            {
                SaveData.saveValues["ActiveTheme"] = "";
                SaveData.Save();
            }
        }
        // Replacement for contentManager.Load<T>(path)
        public static T LoadLayeredContent<T>(string path)
        {
            ContentManager contentManager = UserInterface.instance.Content;
            GraphicsDevice graphicsDevice = UserInterface.instance.GraphicsDevice;
            // If active theme uses mgcb, apply prefix and load
            if(activeTheme.mgcb)
            {
                return contentManager.Load<T>(activeTheme.prefix + path);
            }
            // Otherwise, load assets
            string fullPath = Path.Combine(activeTheme.prefix, path.Replace('/', Path.DirectorySeparatorChar));
            switch (typeof(T).Name)
            {
                case "Texture2D":
                    fullPath += ".png";
                    break;
                case "SoundEffect":
                    fullPath += ".wav";
                    break;
                case "Song":
                    fullPath += ".wma";
                    break;
            }
            // Check to see if the file exists
            if(!File.Exists(fullPath))
            {
                //ConsoleOutput.WriteLine($"Fallback: {path} not found in {activeTheme.name}.", Color.Yellow);
                // If not, load from default theme
                return contentManager.Load<T>(path);
            }
            //ConsoleOutput.WriteLine($"Loading {path} from {activeTheme.name}.", Color.Yellow);
            switch (typeof(T).Name)
            {
                case "Texture2D":
                    return (T)(object)Texture2D.FromFile(graphicsDevice, fullPath);
                case "SoundEffect":
                    return (T)(object)SoundEffect.FromFile(fullPath);
                case "Song":
                    // Create valid uri
                    string uriString = Path.Combine(Directory.GetCurrentDirectory(), fullPath);
                    uriString = uriString.Replace('\\', '/');
                    Uri uri = new Uri(uriString);
                    return (T)(object)Song.FromUri(Path.GetFileNameWithoutExtension(fullPath), uri);
                default: // SpriteFont
                    //ConsoleOutput.WriteLine($"Failed to load {path}.", Color.Red);
                    return contentManager.Load<T>(path);
            }
        }
        public static int GetSongCount()
        {
            return activeTheme.songCount;
        }
        public static void ApplyTheme(Theme theme)
        {
            activeTheme = theme;
            Global.exiting = true;
            Global.exitOpacityIncrease = 16 / 255f;
            Global.fakeExit = true;
            FramePlayer.canPlayBgMusic = false;
            MediaPlayer.Volume = 0f;
            MediaPlayer.Stop();
            Global.exitFunc = () =>
            {
                UserInterface.instance.Content.Unload();
                // Load default content.
                GlobalContent.LoadDefaultContent(UserInterface.instance.Content, UserInterface.instance.GraphicsDevice);
                // Load all screen content.
                ScreenManager.LoadContent(UserInterface.instance.Content, UserInterface.instance.GraphicsDevice);
                GlobalContent.GetSound("Start").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                FramePlayer.canPlayBgMusic = true;
                return true;
            };
        }
    }
}
