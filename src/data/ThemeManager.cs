using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

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
        public int misclickCount;
        public Dictionary<string, Color> colorTable;
        public Theme(string name, string description, string prefix, int songCount, int misclickCount, bool mgcb = false)
        {
            this.name = name;
            this.description = description;
            this.prefix = prefix;
            this.songCount = songCount;
            this.misclickCount = misclickCount;
            this.mgcb = mgcb;
            this.colorTable = new Dictionary<string, Color>();
        }
        public Theme(string name, string description, string prefix, int songCount, int misclickCount, Dictionary<string, Color> colorTable, bool mgcb = false)
        {
            this.name = name;
            this.description = description;
            this.prefix = prefix;
            this.songCount = songCount;
            this.misclickCount = misclickCount;
            this.colorTable = colorTable;
            this.mgcb = mgcb;
        }
        public Color GetColor(string colorName)
        {
            if(colorTable.ContainsKey(colorName))
            {
                return colorTable[colorName];
            }
            else
            {
                return Color.Transparent;
            }
        }
    }
    public static class DefaultThemes
    {
        public static Theme Nonsensical = new Theme("Nonsensical", "The default theme.", "", 6, 4, new Dictionary<string, Color>() {
            {"ClearColor", new Color(0, 0, 0, 255)},
            {"ShadowActionButtonInteractable", new Color(0, 0, 0, 255)},
            {"ShadowButtonInteractable", new Color(0, 0, 0, 255)},
            {"ShadowDialInteractable", new Color(0, 0, 0, 255)},
            {"ShadowSwitchInteractable", new Color(0, 0, 0, 255)},
            {"ShadowTextInputInteractable", new Color(0, 0, 0, 255)},
            {"BackgroundTooltip", new Color(0, 0, 0, 255)},
            {"BackgroundConsoleScreen", new Color(0, 0, 0, 255)},
            {"OverlayContentScreen", new Color(0, 0, 0, 96)},
            {"VideoHolderStaticAnimFilledLibraryPage", new Color(64, 64, 64, 255)},
            {"VideoHolderAddOverlayLibraryPage", new Color(255, 255, 255, 128)},
            {"PluginEntryGenericPluginsPage", new Color(255, 255, 255, 255)},
            {"PluginEntryGenericAltPluginsPage", new Color(192, 192, 192, 255)},
            {"PluginEntryEffectPluginsPage", new Color(192, 192, 255, 255)},
            {"PluginEntryEffectAltPluginsPage", new Color(128, 128, 192, 255)},
            {"PluginEntryPostRenderEffectPluginsPage", new Color(128, 255, 128, 255)},
            {"PluginEntryPostRenderEffectAltPluginsPage", new Color(64, 192, 64, 255)},
            {"PluginEntryThemePluginsPage", new Color(255, 128, 128, 255)},
            {"PluginEntryThemeAltPluginsPage", new Color(192, 64, 64, 255)},
            {"BackgroundOverlayScreen", new Color(128, 128, 128, 255)},
            {"BackgroundLibraryPage", Color.Gray},
            {"ObstaclePastimeGameScreen", Color.Black},
            {"BackgroundScreen", new Color(64, 64, 64, 255)},
            {"TileBackgroundScreen", new Color(102, 102, 102, 255)},
            {"VideoPlayerProgressBar", new Color(255, 0, 0, 255)},
            {"VideoPlayerProgressBarBackground", new Color(0, 0, 0, 0)},
        }, true);
        public static Theme Anniversary = new Theme("Anniversary", "Celebrate NVG's anniversary!", "themes/anniversary/", 6, 4, new Dictionary<string, Color>() {}, true);
        public static Theme Birthday = new Theme("Birthday", "Happy birthday to KiwifruitDev!", "themes/birthday/", 6, 5, new Dictionary<string, Color>() {
            {"BackgroundScreen", new Color(100, 122, 58, 255)},
            {"TileBackgroundScreen", new Color(134, 165, 79, 255)}
        }, true);
        public static Theme Spooky = new Theme("Spooky", "Trick or treat!", "themes/halloween/", 1, 5, new Dictionary<string, Color>() {
            {"VideoPlayerProgressBar", new Color(60, 0, 128, 255)},
        }, true);
        public static Theme Holiday = new Theme("Holiday", "Merry Christmas!", "themes/holiday/", 6, 8, new Dictionary<string, Color>() {}, true);
        public static List<Theme> themes = new List<Theme>()
        {
            Nonsensical,
            Anniversary,
            Spooky,
            Birthday,
            Holiday
        };
        public static Theme defaultTheme = Nonsensical;
    }
    public static class ThemeManager
    {
        public static List<Theme> themes = new List<Theme>()
        {
            DefaultThemes.Nonsensical,
            DefaultThemes.Spooky
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
            Theme newTheme = DefaultThemes.defaultTheme;
            List<Plugin> themeList = PluginHandler.GetEnabledPluginsOfType(AddonType.Theme);
            foreach (Plugin theme in themeList)
            {
                theme.GetThemeMetadata();
                string themeName = theme.GetDisplayName();
                string themeDescription = "";
                int songCount = DefaultThemes.Nonsensical.songCount;
                int misclickCount = DefaultThemes.Nonsensical.misclickCount;
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
                    Plugin? foundPlugin = PluginHandler.plugins.Find(x => x.path == theme.path);
                    if(foundPlugin != null)
                        foundPlugin.enabled = false;
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
                // Count layerpath/graphics/misclick/*.png (indexed from 1 to 9)
                if(Directory.Exists(layerPath + "graphics/misclick/"))
                {
                    for(int i = 1; i < 10; i++)
                    {
                        if(File.Exists(layerPath + $"graphics/misclick/{i}.png"))
                        {
                            misclickCount = i;
                        }
                    }
                }
                Theme thisTheme = new Theme(themeName, themeDescription, layerPath, songCount, misclickCount)
                {
                    colorTable = theme.themeColors
                };
                themes.Add(thisTheme);
                if(SaveData.saveValues["ActiveTheme"] == Path.GetFileName(theme.path))
                {
                    themeChanged = true;
                    newTheme = thisTheme;
                }
                else
                {
                    // Disable the addon
                    Plugin? foundPlugin = PluginHandler.plugins.Find(x => x.path == theme.path);
                    if(foundPlugin != null)
                        foundPlugin.enabled = false;
                    PluginHandler.SavePluginSettings();
                }
            }
            if(themeChanged)
            {
                ApplyTheme(newTheme);
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
            if(UserInterface.instance == null)
            {
                // Can't proceed, MonoGame isn't initialized
                throw new InvalidOperationException("MonoGame isn't initialized. Cannot load content.");
            }
            ContentManager contentManager = UserInterface.instance.Content;
            GraphicsDevice graphicsDevice = UserInterface.instance.GraphicsDevice;
            // If active theme uses mgcb, apply prefix and load
            if(activeTheme.mgcb)
            {
                // Make sure xnb file exists
                if(!File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "Content", activeTheme.prefix + path + ".xnb")))
                {
                    //ConsoleOutput.WriteLine($"Fallback: {path} not found in {activeTheme.name}.", Color.Yellow);
                    // If not, load from default theme
                    return contentManager.Load<T>(path);
                }
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
                    if(activeTheme.mgcb)
                    {
                        fullPath += ".ogg";
                        break;
                    }
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
            ConsoleOutput.WriteLine($"Theme changed to {theme.name}.", Color.Yellow);
            Global.exiting = true;
            Global.exitOpacityIncrease = 16 / 255f;
            Global.fakeExit = true;
            FramePlayer.canPlayBgMusic = false;
            MediaPlayer.Volume = 0f;
            MediaPlayer.Stop();
            Global.exitFunc = () =>
            {
                activeTheme = theme;
                if(UserInterface.instance != null)
                {
                    UserInterface.instance.Content.Unload();
                    // Load default content.
                    GlobalContent.LoadDefaultContent(UserInterface.instance.Content, UserInterface.instance.GraphicsDevice);
                    // Load all screen content.
                    ScreenManager.LoadContent(UserInterface.instance.Content, UserInterface.instance.GraphicsDevice);
                }
                Global.waitReady = 2500;
                GlobalContent.GetSound("Start").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                FramePlayer.canPlayBgMusic = true;
                return true;
            };
        }
        public static Color GetColor(string colorName)
        {
            Color color = activeTheme.GetColor(colorName);
            if(color == Color.Transparent)
            {
                color = DefaultThemes.Nonsensical.GetColor(colorName);
            }
            return color;
        }
    }
}
