using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
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
        public float percentageComplete { get; set; } = 0;
        public int totalTokens { get; set; } = 0;
        public Locale(string name, string localizedName, List<Dictionary<string, string>>? localizationTokens = null, string fontLarge = "Munro", string fontSmall = "MunroSmall", int totalTokens = 0, float percentageComplete = 0)
        {
            this.name = name;
            this.localizedName = localizedName;
            this.fontLarge = fontLarge;
            this.fontSmall = fontSmall;
            this.localizationTokens = localizationTokens ?? new List<Dictionary<string, string>>();
            this.totalTokens = totalTokens;
            this.percentageComplete = percentageComplete;
        }
    }
    /// <summary>
    /// Localization class.
    /// </summary>
    public static class L
    {
        public static Locale dummyLocale = new Locale("dummy", "Dummy");
        public static List<Locale> locales = new List<Locale> {
            dummyLocale
        };
        public static int localeIndex = 0; // set when locales are loaded.
        public static string invalid { get; set; } = "%1";
        public static string defaultLocale { get; set; } = "english";
        public static string localeFolder { get; set; } = "locales";
        public static int maxVersion { get; set; } = 0;
        public static double cyclerTimer { get; set; } = 0;
        public static Locale? cyclerLocale = null;
        public static int cyclerLocaleIndex = 1;
        public static Locale GetLocale()
        {
            if(locales.Count == 0)
                return dummyLocale;
            if (localeIndex < 0 || localeIndex >= locales.Count)
                localeIndex = 0;
            return locales[localeIndex];
        }

        // String translation with optional %1, %2, etc. placeholders.
        public static string T(int version, string text, params string[] args)
        {
            Locale locale = GetLocale();
            int curLocaleIndex = localeIndex;
            // If text contains ViewLocalizationOptions then cycle language variables.
            if (text.Contains("ViewLocalizationOptions") || text.Contains("SelectLanguageOptionsPage"))
            {
                if (cyclerLocale == null)
                    cyclerLocale = locale;
                locale = cyclerLocale;
                curLocaleIndex = cyclerLocaleIndex;
                if (cyclerTimer >= 1)
                {
                    cyclerTimer = 0;
                    cyclerLocaleIndex++;
                    if (cyclerLocaleIndex >= locales.Count)
                        cyclerLocaleIndex = 1;
                    cyclerLocale = locales[curLocaleIndex];
                }
            }
            string? result = text;
            // Version is used to ensure updated strings are used.
            if (curLocaleIndex >= 0 && curLocaleIndex < locales.Count)
            {
                if (version >= 0 && version <= maxVersion)
                {
                    if (locale.localizationTokens.Count > version
                        && locale.localizationTokens[version].ContainsKey(text))
                    {
                        result = locale.localizationTokens[version][text];
                    }
                }
            }
            // Fall back to default locale if translation is missing.
            if (result == text && locales.Count > 1)
            {
                if (locales[1].localizationTokens.Count > version
                    && locales[1].localizationTokens[version].ContainsKey(text))
                {
                    result = locales[1].localizationTokens[version][text];
                }
            }
            // Fall back to invalid string if translation is missing.
            if (result == text)
            {
                result = invalid.Replace("%1", text);
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
            if (GlobalContent.CheckFont(GetLocale().fontLarge))
            {
                return GlobalContent.GetFont(GetLocale().fontLarge);
            }
            return GlobalContent.GetFont("Munro");
        }

        // Get small font.
        public static SpriteFont FontSmall()
        {
            // Fall back to default font if font is missing.
            if (GlobalContent.CheckFont(GetLocale().fontSmall))
            {
                return GlobalContent.GetFont(GetLocale().fontSmall);
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
            SaveData.saveValues["Locale"] = name;
            SaveData.Save();
            // Check if locale is already loaded.
            for (int i = 0; i < locales.Count; i++)
            {
                if (locales[i].name == name)
                {
                    localeIndex = i;
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
                        // Don't count "" or "[ ]" as tokens.
                        foreach (KeyValuePair<string, string> token in version)
                        {
                            if (token.Value != "" && token.Value != "[ ]" && token.Value != " ")
                            {
                                tokenCount++;
                            }
                        }
                    }
                    float percentageComplete = 1f;
                    if(name != defaultLocale)
                    {
                        percentageComplete = 0f;
                        int defaultTokenCount = locales[1].totalTokens;
                        // Calculate how many tokens are included vs the default locale.
                        if (defaultTokenCount > 0)
                        {
                            percentageComplete = (float)tokenCount / (float)defaultTokenCount;
                        }
                    }
                    locales.Add(new Locale(name, localizedName, localizationList, fontLarge, fontSmall, tokenCount, percentageComplete));
                    localeIndex = locales.Count - 1;
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

        public static void ReloadLocales()
        {
            string currentLocale = SaveData.saveValues["Locale"];
            locales.Clear();
            locales.Add(dummyLocale);
            LoadLocale(defaultLocale); // First entry other than dummyLocale is always the default locale.
            // Load all locales in /locales.
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", L.localeFolder);
            string[] files = Directory.GetFiles(path, "*.json");
            for(int i = 0; i < files.Length; i++)
            {
                if(files[i].StartsWith(defaultLocale))
                    continue;
                string newLocale = Path.GetFileNameWithoutExtension(files[i]).Replace(".json", "");
                LoadLocale(newLocale);
            }
            // Load desired locale from save data.
            LoadLocale(currentLocale);
        }

        public static void UnloadLocales()
        {
            locales.Clear();
            locales.Add(dummyLocale);
            localeIndex = 0;
            cyclerLocale = null;
            cyclerLocaleIndex = 1;
            cyclerTimer = 0;
        }
    }
}
