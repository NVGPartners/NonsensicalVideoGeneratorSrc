using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace NonsensicalVideoGenerator
{
    // ISO 639-1 language codes.
    // ISO_3166-1_alpha-2 country codes.
    // Format: en-us (English, United States)
    public class Locale
    {
        public string name { get; set; } = "unknown";
        public string localizedName { get; set; } = "Unknown";
        public string fontLarge { get; set; } = "Munro";
        public string fontSmall { get; set; } = "MunroSmall";
        public List<Dictionary<string, string>> localizationTokens { get; set; } = new List<Dictionary<string, string>>();
        public Locale(string name, string localizedName, List<Dictionary<string, string>>? localizationTokens = null, string fontLarge = "Munro", string fontSmall = "MunroSmall")
        {
            this.name = name;
            this.localizedName = localizedName;
            this.fontLarge = fontLarge;
            this.fontSmall = fontSmall;
            this.localizationTokens = localizationTokens ?? new List<Dictionary<string, string>>();
        }
    }
    /// <summary>
    /// Localization class.
    /// </summary>
    public static class L
    {
        public static List<Locale> locales = new List<Locale> {};
        public static int localeIndex = -1; // set to 0 when locales are loaded.
        public static Locale locale {
            get => locales[localeIndex];
            set => localeIndex = locales.IndexOf(value);
        }
        public static string invalid { get; set; } = "[??]";
        public static string defaultLocale { get; set; } = "en_us";
        public static string localeFolder { get; set; } = "locales";
        public static int maxVersion { get; set; } = 0;

        // String translation with optional %1, %2, etc. placeholders.
        public static string T(int version, string text, params string[] args)
        {
            string? result = text;
            // Version is used to ensure updated strings are used.
            if (localeIndex >= 0 && localeIndex < locales.Count)
            {
                if (version >= 0 && version <= maxVersion)
                {
                    if (locale.localizationTokens[version].ContainsKey(text))
                    {
                        result = locale.localizationTokens[version][text];
                    }
                }
            }
            // Fall back to default locale if translation is missing.
            if (result == text && localeIndex > 0)
            {
                if (locales[0].localizationTokens[version].ContainsKey(text))
                {
                    result = locales[0].localizationTokens[version][text];
                }
            }
            // Fall back to invalid string if translation is missing.
            if (result == text)
            {
                result = invalid + text + invalid;
            }
            // Replace placeholders globally.
            for (int i = 0; i < args.Length; i++)
            {
                result = result.Replace($"%{i + 1}", args[i]);
            }
            return result;
        }

        // Get large font.
        public static SpriteFont FontLarge()
        {
            // Fall back to default font if font is missing.
            if (GlobalContent.CheckFont(locale.fontLarge))
            {
                return GlobalContent.GetFont(locale.fontLarge);
            }
            return GlobalContent.GetFont("Munro");
        }

        // Get small font.
        public static SpriteFont FontSmall()
        {
            // Fall back to default font if font is missing.
            if (GlobalContent.CheckFont(locale.fontSmall))
            {
                return GlobalContent.GetFont(locale.fontSmall);
            }
            return GlobalContent.GetFont("MunroSmall");
        }

        public static void LoadLocale(string? name)
        {
            if (name == null)
            {
                name = defaultLocale;
            }
            // Before trying to load a new locale, make sure at least the default locale is loaded.
            if (locales.Count == 0 && name != defaultLocale)
            {
                LoadLocale(defaultLocale);
            }
            if (locales.Count == 0 && name != defaultLocale)
            {
                ConsoleOutput.WriteLine("Fatal error: Could not load default locale.", Color.Red);
                return;
            }
            // Check if locale is already loaded.
            foreach (Locale l in locales)
            {
                if (l.name == name)
                {
                    locale = l;
                    ConsoleOutput.WriteLine($"Switched to locale {name}: {locale.localizedName}", Color.Green);
                    return;
                }
            }
            // Attempt to load in /locales/{name}.json
            // If successful, add to locales and set as current locale.
            string path = $"{localeFolder}/{name}.json";
            path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", path);
            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    Dictionary<string, Dictionary<string, string>> localizationTokens = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json) ?? new Dictionary<string, Dictionary<string, string>>();
                    // Special metadata keys.
                    string localizedName = "Unknown";
                    string fontLarge = "Munro";
                    string fontSmall = "MunroSmall";
                    if (localizationTokens.ContainsKey("Metadata"))
                    {
                        if (localizationTokens["Metadata"].ContainsKey("LocalizedName"))
                        {
                            localizedName = localizationTokens["Metadata"]["LocalizedName"];
                        }
                        if (localizationTokens["Metadata"].ContainsKey("FontLarge"))
                        {
                            fontLarge = localizationTokens["Metadata"]["FontLarge"];
                        }
                        if (localizationTokens["Metadata"].ContainsKey("FontSmall"))
                        {
                            fontSmall = localizationTokens["Metadata"]["FontSmall"];
                        }
                    }
                    List<Dictionary<string, string>> localizationList = new List<Dictionary<string, string>>();
                    for (int i = 0; i <= maxVersion; i++)
                    {
                        if (localizationTokens.ContainsKey($"Version{i}"))
                        {
                            localizationList.Add(localizationTokens[$"Version{i}"]);
                        }
                        else
                        {
                            localizationList.Add(new Dictionary<string, string>());
                        }
                    }
                    int tokenCount = 0;
                    foreach (Dictionary<string, string> version in localizationList)
                    {
                        tokenCount += version.Count;
                    }
                    locales.Add(new Locale(name, localizedName, localizationList, fontLarge, fontSmall));
                    locale = locales[^1];
                    ConsoleOutput.WriteLine($"Loaded {tokenCount} localization tokens for locale {name}: {localizedName}", Color.Green);
                }
                catch (Exception e)
                {
                    ConsoleOutput.WriteLine($"Failed to load locale {name}: {e.Message}", Color.Red);
                }
            }
            else
            {
                ConsoleOutput.WriteLine($"Locale {name} not found.", Color.Yellow);
            }

        }
    }
}
