using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;

namespace NonsensicalVideoGenerator
{
    public enum PluginType
    {
        None,
        PowerShell,
        Lua,
    }
    public class PluginReturnValue
    {
        public bool success;
        public string pluginName;
        public string jobFolder;
        public PluginReturnValue(bool success = true, string pluginName = "", string jobFolder = "")
        {
            this.success = success;
            this.pluginName = pluginName;
            this.jobFolder = jobFolder;
        }
    }
    public class Plugin
    {
        public string path { get; set; }
        public PluginType type { get; set; }
        public bool enabled { get; set; }
        public Script? luaScript;
        public string workshopId = "";
        public List<LibraryType> libraryTypes = new List<LibraryType>();
        public Dictionary<string, object> settings = new();
        public Dictionary<string, string> settingTooltips = new();
        public Plugin(string path, PluginType type, bool enabled = true)
        {
            this.path = path;
            this.type = type;
            this.enabled = enabled;
        }
        // RunFFmpeg for lua
        public static string RunFFmpeg(string args)
        {
            Console.WriteLine("Running ffmpeg with args: " + args, Color.Transparent);
            // Validate input
            if (args.Contains("&&") || args.Contains("|") || args.Contains(";") || args.Contains("&") || args.Contains("||"))
            {
                Console.WriteLine("A plugin attempted to perform an illegal operation.", Color.Red);
                return "Invalid input";
            }
            ProcessStartInfo startInfo = new()
            {
                FileName = Global.useSystemFFmpeg ? "ffmpeg" : @".\bin\ffmpeg.exe",
                Arguments = args,
                WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                UseShellExecute = true
            };
            Process process = new()
            {
                StartInfo = startInfo
            };
            process.Start();
            process.WaitForExit();
            return "";
        }
        // RunFFprobe for lua
        public static string RunFFprobe(string args)
        {
            // Validate input
            if (args.Contains("&&") || args.Contains("|") || args.Contains(";") || args.Contains("&") || args.Contains("||"))
            {
                Console.WriteLine("A plugin attempted to perform an illegal operation.", Color.Red);
                return "Invalid input";
            }
            ProcessStartInfo startInfo = new()
            {
                FileName = Global.useSystemFFmpeg ? "ffprobe" : @".\bin\ffprobe.exe",
                Arguments = args,
                WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            Process process = new()
            {
                StartInfo = startInfo
            };
            // Output data
            string output = "";
            process.OutputDataReceived += (sender, e) =>
            {
                output += e.Data + "\n";
                if(e.Data != null)
                    Console.WriteLine(e.Data, Color.Transparent);
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                output += e.Data + "\n";
                if(e.Data != null)
                    Console.WriteLine(e.Data, Color.Transparent);
            };
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
            return output;
        }
        // RunMagick for lua
        public static string RunMagick(string args)
        {
            // Validate input
            if (args.Contains("&&") || args.Contains("|") || args.Contains(";") || args.Contains("&") || args.Contains("||"))
            {
                Console.WriteLine("A plugin attempted to perform an illegal operation.", Color.Red);
                return "Invalid input";
            }
            ProcessStartInfo startInfo = new()
            {
                FileName = "magick",
                Arguments = args,
                WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            Process process = new()
            {
                StartInfo = startInfo
            };
            // Output data
            string output = "";
            process.OutputDataReceived += (sender, e) =>
            {
                output += e.Data + "\n";
                if(e.Data != null)
                    Console.WriteLine(e.Data, Color.Transparent);
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                output += e.Data + "\n";
                if(e.Data != null)
                    Console.WriteLine(e.Data, Color.Transparent);
            };
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
            return output;
        }
        // GetRandomLibraryFile for lua
        public static string GetRandomLibraryFile(string path, string rootType = "video", string subType = "materials")
        {
            // Validate rootType and subType
            if (rootType != "video" && rootType != "audio")
            {
                throw new Exception("Invalid rootType");
            }
            // Validate subType
            foreach (KeyValuePair<LibraryType, string> pair in LibraryData.libraryPaths)
            {
                if (pair.Value == rootType + @"\" + subType)
                {
                    break;
                }
                if (pair.Key == LibraryData.libraryPaths.Last().Key)
                {
                    throw new Exception("Invalid subType");
                }
            }
            string[] files = Directory.GetFiles(Path.Join(LibraryData.libraryRootPath, rootType, subType));
            return files[Global.generatorFactory.globalRandom.Next(files.Length)];
        }
        // GetVideoDuration for lua
        public static double GetVideoDuration(string path)
        {
            string output = RunFFprobe($"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{path}\"");
            return double.Parse(output);
        }
        public PluginReturnValue Call(string video)
        {
            // Create .\temp\job_%time%\
            string oldVideo = video;
            string jobPath = Path.Join(@".\temp\", "job_" + DateTime.Now.ToString("HHmmssfff"));
            // Delete job path if it already exists
            if (Directory.Exists(jobPath))
            {
                Directory.Delete(jobPath, true);
            }
            Directory.CreateDirectory(jobPath);
            // Copy video to job path as result.mp4
            File.Copy(video, Path.Join(jobPath, "result.mp4"));
            video = Path.Join(jobPath, "result.mp4");            
            if (enabled == false)
            {
                return new PluginReturnValue(false, Path.GetFileName(path));
            }
            ConsoleOutput.WriteLine($"Calling plugin {Path.GetFileName(path)}", Color.LightBlue);
            switch (type)
            {
                case PluginType.PowerShell:
                    // ps1 plugins use Set-ExecutionPolicy Bypass -Scope Process; path
                    List<string> psArgs = new()
                    {
                        "Set-ExecutionPolicy", "Bypass",
                        "-Scope", "Process;",
                        path,
                        video,
                        SaveData.saveValues["VideoWidth"],
                        SaveData.saveValues["VideoHeight"],
                        jobPath + @"\",
                        Global.useSystemFFmpeg ? "ffmpeg" : @".\bin\ffmpeg.exe",
                        Global.useSystemFFmpeg ? "ffprobe" : @".\bin\ffprobe.exe",
                        "magick",
                        LibraryData.libraryRootPath + @"\", // legacy resource path
                        @".\" +Path.Join("library", "audio", "sfx") + @"\",
                        @".\" +Path.Join("library", "videos", "transitions") + @"\",
                        @".\" +Path.Join("library", "audio", "music") + @"\",
                        LibraryData.libraryRootPath + @"\",
                        SaveData.saveFileName, // powershell plugins can access the JSON save file
                        jobPath + @"\output.mp4",
                        settings.Count.ToString(), // number of settings
                    };
                    // Add settings to args
                    foreach (KeyValuePair<string, object> setting in settings)
                    {
                        psArgs.Add(setting.Key);
                        psArgs.Add(setting.Value.ToString());
                    }
                    string psArgsString = string.Join(" ", psArgs);
                    ProcessStartInfo psStartInfo = new()
                    {
                        FileName = "powershell",
                        Arguments = psArgsString,
                        WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };
                    Process psProcess = new()
                    {
                        StartInfo = psStartInfo
                    };
                    psProcess.OutputDataReceived += (sender, e) =>
                    {
                        ConsoleOutput.WriteLine(e.Data, Color.LightBlue);
                    };
                    psProcess.ErrorDataReceived += (sender, e) =>
                    {
                        ConsoleOutput.WriteLine(e.Data, Color.Transparent);
                    };
                    psProcess.Start();
                    psProcess.BeginOutputReadLine();
                    psProcess.BeginErrorReadLine();
                    psProcess.WaitForExit();
                    if (psProcess.ExitCode == 0)
                    {
                        return new PluginReturnValue(true, Path.GetFileName(path), jobPath + @"\");
                    }
                    return new PluginReturnValue(false, Path.GetFileName(path));
                case PluginType.Lua:
                    if(luaScript == null)
                        return new PluginReturnValue(false, Path.GetFileName(path));
                    // Create RunFFmpeg, RunFFprobe, and RunMagick functions
                    // Run Generate function if it exists.
                    if (luaScript.Globals["Generate"] != null)
                    {
                        try
                        {
                            luaScript = new Script();
                            luaScript.Options.ScriptLoader = new CustomScriptLoader();
                            luaScript.Options.DebugPrint = (s) => ConsoleOutput.WriteLine(s, Color.DarkCyan);
                            luaScript.DoFile(path);
                            // Create videoOptions table
                            Table videoOptions = new(luaScript);
                            videoOptions["inputVideo"] = video;
                            videoOptions["workingDirectory"] = jobPath + @"\";
                            videoOptions["videoOptionsFileName"] = SaveData.saveFileName;
                            videoOptions["pluginSettingsFileName"] = PluginHandler.pluginSettingsPath;
                            videoOptions["outputVideo"] = jobPath + @"\output.mp4";
                            // Add settings to pluginSettings table
                            Table pluginSettings = new(luaScript);
                            foreach (KeyValuePair<string, object> setting in settings)
                            {
                                pluginSettings[setting.Key] = setting.Value;
                            }
                            // Add functions
                            Table functions = new(luaScript);
                            functions["runFFmpeg"] = (Func<string, string>)RunFFmpeg;
                            functions["runFFprobe"] = (Func<string, string>)RunFFprobe;
                            functions["runMagick"] = (Func<string, string>)RunMagick;
                            functions["getLength"] = (Func<string, double>)GetVideoDuration;
                            // Call Generate function
                            DynValue result = luaScript.Call(luaScript.Globals["Generate"], videoOptions, pluginSettings, functions);
                            if (result.Type == DataType.Boolean)
                            {
                                return new PluginReturnValue(result.Boolean, Path.GetFileName(path), jobPath + @"\");
                            }
                            else
                            {
                                ConsoleOutput.WriteLine("Generate function did not return a boolean.", Color.Red);
                                return new PluginReturnValue(false, Path.GetFileName(path));
                            }
                        }
                        catch (Exception e)
                        {
                            ConsoleOutput.WriteLine(e.Message, Color.Red);
                            return new PluginReturnValue(false, Path.GetFileName(path));
                        }
                    }
                    else
                    {
                        ConsoleOutput.WriteLine("Generate function not found.", Color.Red);
                        return new PluginReturnValue(false, Path.GetFileName(path));
                    }
                default:
                    return new PluginReturnValue(false, Path.GetFileName(path));
            }
        }
        public bool Query()
        {
            switch(type)
            {
                default:
                    return true;
                case PluginType.PowerShell:
                    // Call plugin with query argument.
                    string fileName = path;
                    List<string> batchArgs = new();
                    if(type == PluginType.PowerShell)
                    {
                        fileName = "powershell";
                        batchArgs.Add("Set-ExecutionPolicy");
                        batchArgs.Add("Bypass");
                        batchArgs.Add("-Scope");
                        batchArgs.Add("Process;");
                        batchArgs.Add(path);
                    }
                    batchArgs.Add("query");
                    string batchArgsString = string.Join(" ", batchArgs);
                    ProcessStartInfo batchStartInfo = new()
                    {
                        FileName = fileName,
                        Arguments = batchArgsString,
                        WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };
                    Process batchProcess = new()
                    {
                        StartInfo = batchStartInfo
                    };
                    string output = "";
                    batchProcess.OutputDataReceived += (sender, e) =>
                    {
                        output += e.Data;
                    };
                    batchProcess.ErrorDataReceived += (sender, e) =>
                    {
                        ConsoleOutput.WriteLine(e.Data, Color.Transparent);
                    };
                    batchProcess.Start();
                    batchProcess.BeginOutputReadLine();
                    batchProcess.BeginErrorReadLine();
                    batchProcess.WaitForExit();
                    if (batchProcess.ExitCode != 0)
                    {
                        ConsoleOutput.WriteLine(output, Color.Red);
                        return false;
                    }
                    // Parse output.
                    string[] outputarray = output.Replace("\r", "").Replace("\n", "").Split("|");
                    if(outputarray.Length < 1)
                    {
                        return true; // no query results
                    }
                    string[] query = outputarray[0].Split(';');
                    int count = 0;
                    for(int i = 0; i < query.Length; i++)
                    {
                        // First character is either a 0 or 1 for video/audio
                        // Second character is a colon for separation
                        // Rest is the pretty name, then a colon, then the base name.
                        string[] split = query[i].Split(':');
                        if (split.Length < 3)
                        {
                            continue;
                        }
                        LibraryRootType rootType = split[0] == "0" ? LibraryRootType.Video : LibraryRootType.Audio;
                        // split[3] is description
                        string description = split.Length >= 4 ? split[3].Replace("_", " ") : "";
                        LibraryType dummyType = new(rootType, split[2].Replace("_", ""), description);
                        string libPath = Path.Join(rootType == LibraryRootType.Video ? "video" : "audio", split[2]);
                        string[] fileExts = LibraryData.libraryFileTypes[rootType == LibraryRootType.Video ? DefaultLibraryTypes.Video : DefaultLibraryTypes.Audio];
                        // Check to see if library already exists.
                        if (DefaultLibraryTypes.AllTypes.Contains(dummyType))
                        {
                            continue;
                        }
                        DefaultLibraryTypes.AllTypes.Add(dummyType);
                        LibraryData.libraryPaths.Add(dummyType, libPath);
                        LibraryData.libraryFileTypes.Add(dummyType, fileExts);
                        LibraryData.libraryNames.Add(dummyType, split[1].Replace("_", " "));
                        libraryTypes.Add(dummyType);
                        Global.justCompletedRender = true; // demand a refresh
                        // Print to console.
                        ConsoleOutput.WriteLine($"Added {(rootType == LibraryRootType.Video ? "video" : "audio")} library {split[1]} from plugin {Path.GetFileName(path)}.", Color.LightBlue);
                        count++;
                    }
                    // Print count
                    if (count > 0)
                        ConsoleOutput.WriteLine($"Plugin {Path.GetFileName(path)} added {count} libraries.", Color.LightBlue);
                    // Library query successful, check for settings query.
                    if (outputarray.Length < 2)
                    {
                        return true; // no settings query results
                    }
                    // Format: setting_name:default_setting_value:tooltip;setting_name:default_setting_value:tooltip
                    string[] settingsQuery = outputarray[1].Split(';');
                    count = 0;
                    for(int i = 0; i < settingsQuery.Length; i++)
                    {
                        string[] split = settingsQuery[i].Split(':');
                        if (split.Length < 2)
                        {
                            continue;
                        }
                        string settingName = split[0];
                        string settingValue = split[1];
                        string settingTooltip = split.Length >= 3 ? split[2] : "";
                        settings.Add(settingName, settingValue);
                        settingTooltips.Add(settingName, settingTooltip);
                        count++;
                    }
                    // Print count
                    if (count > 0)
                        ConsoleOutput.WriteLine($"Plugin {Path.GetFileName(path)} added {count} settings.", Color.LightBlue);
                    return true;
                case PluginType.Lua:
                    // If root is not "workshop", set workshopId to name of plugin folder.
                    if (Path.GetFileName(Path.GetDirectoryName(path)) != "workshop")
                    {
                        workshopId = Path.GetFileName(Path.GetDirectoryName(path));
                    }
                    // Moonsharp is used to run Lua plugins in a sandbox.
                    // Steam Workshop plugins are Lua.
                    luaScript = new Script();
                    luaScript.Options.ScriptLoader = new CustomScriptLoader();
                    luaScript.Options.DebugPrint = (s) => ConsoleOutput.WriteLine(s, Color.DarkCyan);
                    luaScript.DoFile(path);
                    // Call plugin with query argument.
                    if(luaScript.Globals["Query"] != null)
                    {
                        DynValue luaQuery = luaScript.Call(luaScript.Globals["Query"]);
                        if (luaQuery.Type != DataType.Table)
                        {
                            ConsoleOutput.WriteLine($"Plugin {Path.GetFileName(path)} returned invalid query.", Color.Red);
                            return false;
                        }
                        // Parse output.
                        DynValue luaLibraries = luaQuery.Table.Get("libraries");
                        if (luaLibraries.Type == DataType.Table)
                        {
                            int libraryCount = 0;
                            foreach (DynValue library in luaLibraries.Table.Values)
                            {
                                if (library.Type != DataType.Table)
                                {
                                    continue;
                                }
                                LibraryRootType rootType = library.Table.Get("type").String == "video" ? LibraryRootType.Video : LibraryRootType.Audio;
                                string libraryPrettyName = library.Table.Get("name").String;
                                string libraryName = library.Table.Get("path").String;
                                string libraryTooltip = library.Table.Get("tooltip").String;
                                LibraryType dummyType = new(rootType, libraryName, libraryTooltip);
                                string libPath = Path.Join(rootType == LibraryRootType.Video ? "video" : "audio", libraryName);
                                string[] fileExts = LibraryData.libraryFileTypes[rootType == LibraryRootType.Video ? DefaultLibraryTypes.Video : DefaultLibraryTypes.Audio];
                                // Check to see if library already exists.
                                if (DefaultLibraryTypes.AllTypes.Contains(dummyType))
                                {
                                    continue;
                                }
                                DefaultLibraryTypes.AllTypes.Add(dummyType);
                                LibraryData.libraryPaths.Add(dummyType, libPath);
                                LibraryData.libraryFileTypes.Add(dummyType, fileExts);
                                LibraryData.libraryNames.Add(dummyType, libraryPrettyName);
                                libraryTypes.Add(dummyType);
                                Global.justCompletedRender = true; // demand a refresh
                                // Print to console.
                                ConsoleOutput.WriteLine($"Added {(rootType == LibraryRootType.Video ? "video" : "audio")} library {libraryPrettyName} from plugin {Path.GetFileName(path)}.", Color.LightBlue);
                                libraryCount++;
                            }
                            // Print count
                            if (libraryCount > 0)
                                ConsoleOutput.WriteLine($"Plugin {Path.GetFileName(path)} added {libraryCount} libraries.", Color.LightBlue);
                        }
                        DynValue luaSettings = luaQuery.Table.Get("settings");
                        if (luaSettings.Type == DataType.Table)
                        {
                            int settingCount = 0;
                            foreach (DynValue setting in luaSettings.Table.Values)
                            {
                                if (setting.Type != DataType.Table)
                                {
                                    continue;
                                }
                                string settingName = setting.Table.Get("name").String;
                                string settingValue = setting.Table.Get("value").String;
                                string settingTooltip = setting.Table.Get("tooltip").String;
                                settings.Add(settingName, settingValue);
                                settingTooltips.Add(settingName, settingTooltip);
                                settingCount++;
                            }
                            // Print count
                            if (settingCount > 0)
                                ConsoleOutput.WriteLine($"Plugin {Path.GetFileName(path)} added {settingCount} settings.", Color.LightBlue);
                        }
                    }
                    return true;
            }
        }
    }
    /// <summary>
    /// Plugin support.
    /// </summary>
    public static class PluginHandler
    {
        public static List<Plugin> plugins = new();
        private static string pluginPath = @".\plugins";
        public static string pluginSettingsPath = @".\PluginSettings.json";
        public static void LoadPluginSettings()
        {
            if (!File.Exists(pluginSettingsPath))
            {
                // Create empty settings file.
                File.WriteAllText(pluginSettingsPath, "{}");
            }
            // {"pluginname.ps1": {"settings": {"settingname": "settingvalue"}, "disabled": false}}
            Dictionary<string, Dictionary<string, object>>? pluginSettings = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(File.ReadAllText(pluginSettingsPath));
            if (pluginSettings == null)
            {
                // Delete invalid settings file.
                File.Delete(pluginSettingsPath);
                LoadPluginSettings();
                return;
            }
            foreach(KeyValuePair<string, Dictionary<string, object>> pluginSetting in pluginSettings)
            {
                string pluginName = pluginSetting.Key;
                int index = plugins.FindIndex(plugin => Path.GetFileName(plugin.path) == pluginName);
                if (index == -1)
                    continue;
                Plugin plugin = plugins[index];
                Dictionary<string, object>? settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(pluginSetting.Value["settings"].ToString());
                foreach(KeyValuePair<string, object> setting in settings)
                {
                    ConsoleOutput.WriteLine($"Setting {setting.Key} to {setting.Value} for plugin {pluginName}...", Color.LightBlue);
                    if(!plugin.settings.ContainsKey(setting.Key))
                    {
                        ConsoleOutput.WriteLine($"Setting {setting.Key} not found for plugin {pluginName}.", Color.Red);
                        continue;
                    }
                    plugin.settings[setting.Key] = setting.Value;
                }
                plugin.enabled = !(pluginSetting.Value["disabled"] as bool? ?? false);
            }
            SavePluginSettings();
        }
        public static void SavePluginSettings()
        {
            Dictionary<string, Dictionary<string, object>> pluginSettings = new();
            foreach(Plugin plugin in plugins)
            {
                Dictionary<string, object> pluginSetting = new();
                pluginSetting.Add("settings", plugin.settings);
                pluginSetting.Add("disabled", !plugin.enabled);
                pluginSettings.Add(Path.GetFileName(plugin.path), pluginSetting);
            }
            File.WriteAllText(pluginSettingsPath, JsonConvert.SerializeObject(pluginSettings, Formatting.Indented));
        }
        public static void LoadPlugin(string path, PluginType type)
        {
            Plugin plugin = new(path, type);
            Global.generatorFactory.progressText = $"Loading plugin {Path.GetFileName(path)}...";
            if(!plugin.Query())
                throw new Exception($"Failed to query plugin {Path.GetFileName(path)}.");
            plugins.Add(plugin);
            // Add entry to pluginsettings if it doesn't exist.
            Dictionary<string, object> pluginSettings = new();
            pluginSettings.Add("settings", plugin.settings);
            pluginSettings.Add("disabled", !plugin.enabled);
            if (!File.Exists(pluginSettingsPath))
            {
                // Create empty settings file.
                File.WriteAllText(pluginSettingsPath, "{}");
            }
            Dictionary<string, Dictionary<string, object>>? existingPluginSettings = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(File.ReadAllText(pluginSettingsPath));
            if (existingPluginSettings != null)
            {
                if (!existingPluginSettings.ContainsKey(Path.GetFileName(path)))
                {
                    existingPluginSettings.Add(Path.GetFileName(path), pluginSettings);
                    File.WriteAllText(pluginSettingsPath, JsonConvert.SerializeObject(existingPluginSettings, Formatting.Indented));
                }
            }
            ConsoleOutput.WriteLine($"Loaded plugin {Path.GetFileName(path)}.", Color.LightBlue);
        }
        private static void LoadPluginsRecursive(string path, PluginType type)
        {
            foreach (string file in Directory.GetFiles(path))
            {
                switch(type)
                {
                    case PluginType.PowerShell:
                        if (file.EndsWith(".ps1"))
                        {
                            LoadPlugin(file, type);
                        }
                        break;
                    case PluginType.Lua:
                        if (file.EndsWith(".lua"))
                        {
                            LoadPlugin(file, type);
                        }
                        break;
                }
            }
            // Recurse into subdirectories.
            foreach(string file in Directory.GetDirectories(path))
            {
                LoadPluginsRecursive(file, type);
            }
        }
        public static bool LoadPlugins()
        {
            // Clear plugins.
            plugins.Clear();
            // Create plugin directory if it doesn't exist.
            if(Directory.Exists(pluginPath) == false)
            {
                Directory.CreateDirectory(pluginPath);
            }
            ConsoleOutput.WriteLine($"Searching for plugins in {pluginPath}...", Color.LightBlue);
            List<string> pluginDirs = new()
            {
                "stock",
                "workshop",
            };
            // Create plugin subdirectories if they don't exist.
            foreach(string subdir in pluginDirs)
            {
                if(Directory.Exists(Path.Combine(pluginPath, subdir)) == false)
                {
                    Directory.CreateDirectory(Path.Combine(pluginPath, subdir));
                }
            }
            try
            {
                // Load from plugin path using subdirectories for each plugin type.
                foreach (string file in Directory.GetDirectories(pluginPath))
                {
                    string dirName = Path.GetFileName(file);
                    PluginType type = PluginType.Lua;
                    switch (dirName)
                    {
                        case "stock":
                            type = PluginType.PowerShell;
                            break;
                    }
                    ConsoleOutput.WriteLine($"Loading {dirName} plugins...", Color.LightBlue);
                    LoadPluginsRecursive(file, type);
                }
                LoadPluginSettings();
                Global.generatorFactory.progressText = $"{plugins.Count} plugins, check console for details.";
                Global.canRender = true;
            }
            catch (Exception e)
            {
                ConsoleOutput.WriteLine($"Error loading plugins: {e.Message}", Color.Red);
                return false;
            }
            return true;
        }
        public static PluginReturnValue PickRandom(Random rnd, string video)
        {
            if(plugins.Count == 0)
            {
                return new PluginReturnValue()
                {
                    success = false,
                    pluginName = "",
                };
            }
            // Pick a random plugin that isn't disabled.
            List<Plugin> enabledPlugins = plugins.FindAll(plugin => plugin.enabled);
            if(enabledPlugins.Count == 0)
            {
                return new PluginReturnValue()
                {
                    success = false,
                    pluginName = "",
                };
            }
            Plugin plugin = enabledPlugins[rnd.Next(enabledPlugins.Count)];
            // Call the plugin.
            return plugin.Call(video);
        }
        public static int GetPluginCount()
        {
            return plugins.Count;
        }
    }
}