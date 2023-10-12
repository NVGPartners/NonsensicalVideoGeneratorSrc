using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using System.Linq;
#if MONOGAME
using Microsoft.Xna.Framework;
#else
using System.Drawing;
#endif
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using Steamworks;
using System.ComponentModel;
using System.Threading;
using System.Net;

namespace NonsensicalVideoGenerator
{
    [Flags]
    public enum WorkshopTag
    {
        None = 0,
        Effect_AudioOnly = 1,
        Effect_VideoOnly = 2,
        Library_SFX = 4,
        Library_Music = 8,
        Library_Material = 16,
        Library_Transition = 32,
        Library_Intro = 64,
        Library_Outro = 128,
        Library_Overlay = 256,
        Library_Render = 512,
        Library_Custom = 1024,
    }
    public enum PluginType
    {
        None,
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
    public enum CommandType
    {
        Custom,
        FFmpeg,
        FFprobe,
        Magick,
        YtDlp,
        Download,
    }
    public class Command
    {
        public CommandType type;
        public string customCommand = "";
        public string? args;
        public string? workingDirectory;
        public string command
        {
            get
            {
                switch (type)
                {
                    case CommandType.FFmpeg:
                        return Global.useSystemFFmpeg ? "ffmpeg" : Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ffmpeg.exe");
                    case CommandType.FFprobe:
                        return Global.useSystemFFprobe ? "ffprobe" : Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ffprobe.exe");
                    case CommandType.Magick:
                        return "magick"; // only system PATH is supported
                    case CommandType.YtDlp:
                        return Global.useSystemYtDlp ? "yt-dlp" : Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "yt-dlp.exe");
                    default:
                        return customCommand;
                }
            }
            set
            {
                switch (value)
                {
                    case "ffmpeg":
                        type = CommandType.FFmpeg;
                        break;
                    case "ffprobe":
                        type = CommandType.FFprobe;
                        break;
                    case "magick":
                        type = CommandType.Magick;
                        break;
                    case "yt-dlp":
                        type = CommandType.YtDlp;
                        break;
                    default:
                        type = CommandType.Custom;
                        customCommand = value;
                        break;
                }
            }
        }
        public Command(CommandType type, string? args = null, string? workingDirectory = null)
        {
            this.type = type;
            this.args = args;
            this.workingDirectory = workingDirectory;
        }
        public Command(string command, string? args = null, string? workingDirectory = null)
        {
            this.command = command;
            this.args = args;
            this.workingDirectory = workingDirectory;
        }
        public string[] Call()
        {
            if(type != CommandType.Download)
            {
                string args = this.args ?? "";
                ProcessStartInfo startInfo = new()
                {
                    FileName = command,
                    Arguments = args,
                    WorkingDirectory = workingDirectory ?? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                };
                Process process = new()
                {
                    StartInfo = startInfo
                };
                process.Start();
                string output = "";
                string error = "";
                process.OutputDataReceived += (sender, e) => {
                    if(e.Data != null)
                    {
                        output += e.Data;
                        ConsoleOutput.WriteLine(e.Data, Color.Transparent);
                    }
                };
                process.ErrorDataReceived += (sender, e) => {
                    if(e.Data != null)
                    {
                        error += e.Data;
                        ConsoleOutput.WriteLine(e.Data, Color.Transparent);
                    }
                };
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                return new string[] { output, error };
            }
            else
            {
                // Split args into url, rootType, and subType
                string[] args = this.args?.Split(' ') ?? new string[] { };
                if(args.Length < 1)
                {
                    ConsoleOutput.WriteLine("Download command missing arguments.", Color.Red);
                    return new string[] { "", "" };
                }
                string url = args[0];
                string rootType = args.Length > 1 ? args[1] : "";
                string subType = args.Length > 2 ? args[2] : "";
                // Validate url
                if(!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    ConsoleOutput.WriteLine("Download command has invalid URL.", Color.Red);
                    return new string[] { "", "" };
                }
                // Validate rootType and subType
                LibraryType? dummyType = null;
                if(rootType == "video" || rootType == "audio")
                {
                    // Validate subType
                    foreach (KeyValuePair<LibraryType, string> pair in LibraryData.libraryPaths)
                    {
                        if (pair.Value == rootType + "\\" + subType)
                        {
                            dummyType = pair.Key;
                            break;
                        }
                        if (pair.Key == LibraryData.libraryPaths.Last().Key)
                        {
                            throw new Exception("Invalid subType");
                        }
                    }
                    if (dummyType == null)
                    {
                        throw new Exception("Invalid subType");
                    }
                }
                else
                {
                    subType = "";
                }
                // If libraryRoot is not empty, download to library instead of job folder
                string downloadPath = subType != "" ? Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "library", rootType, subType) : rootType;
                string combinedPath = Path.Combine(downloadPath, Path.GetFileName(url));
                // get complete path
                combinedPath = Path.GetFullPath(combinedPath);
                // create directory if it doesn't exist
                Directory.CreateDirectory(Path.GetDirectoryName(combinedPath));
                if (File.Exists(combinedPath))
                {
                    ConsoleOutput.WriteLine($"File {combinedPath} already exists.", Color.LightBlue);
                    return new string[] { "", "" };
                }
                // Download file
                ConsoleOutput.WriteLine($"Downloading {url}" + (subType != "" ? " to library " + rootType + "\\" + subType : ""), Color.LightBlue);
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(url, combinedPath);
                }
                // Load to library
                if(dummyType != null)
                {
                    ConsoleOutput.WriteLine($"Loading {combinedPath} to library", Color.LightBlue);
                    LibraryFile libfile = new(Path.GetFileNameWithoutExtension(combinedPath), Path.Join(rootType, subType, Path.GetFileName(combinedPath)), dummyType);
                    if(LibraryData.Load(libfile) == null)
                    {
                        ConsoleOutput.WriteLine($"Failed to load {combinedPath} to library", Color.Red);
                        return new string[] { "", "" };
                    }
                    Global.justCompletedRender = true;
                }
                return new string[] { "", "" };
            }
        }
    }
    public enum SettingType
    {
        TextInput,
        TextInputNumbers,
        TextInputDecimals,
        TextInputLetters,
        TextInputLettersNumbers,
        TextInputLettersNumbersSpaces,
        Label,
        Switch,
    }
    public class Plugin
    {
        public string path { get; set; }
        public PluginType type { get; set; }
        public bool enabled { get; set; }
        public string submittedId = "";
        public Script? luaScript;
        public string workshopId = "";
        public string rootPath = "";
        public Dictionary<string, object> settings = new();
        public Dictionary<string, string> settingTooltips = new();
        public Dictionary<string, SettingType> settingTypes = new();
        public static Dictionary<string, string> placeholders = new();
        public ConsentForm? consentForm;
        public Plugin(string path, PluginType type, string rootPath, bool enabled = true)
        {
            this.path = path;
            this.type = type;
            this.rootPath = rootPath;
            this.enabled = enabled;
        }
        public string GetDisplayName()
        {
            // Setting "Display Name" is optional
            if (settings.ContainsKey("Display Name"))
            {
                return (string)settings["Display Name"];
            }
            else
            {
                return Path.GetFileName(path);
            }
        }
        public static bool ValidateInput(string args)
        {
            bool illegal = false;
            // We're only going to allow special characters if they're inside of quotes and not exposed to the shell
            // Match everything outside of quotes
            Regex regex = new Regex(@"[^\s""]+|""([^""]*)""");
            MatchCollection matches = regex.Matches(args);
            foreach (Match match in matches)
            {   
                string matchString = match.ToString();
                // If the match is not inside of quotes, check for pipes
                if (!matchString.StartsWith("\"") && !matchString.EndsWith("\""))
                {
                    // FFmpeg uses pipes to separate filters
                    if (matchString.Contains("|")
                        || matchString.Contains("&")
                        || matchString.Contains(">")
                        || matchString.Contains("<"))
                    {
                        ConsoleOutput.WriteLine("An addon attempted to pipe or redirect output.", Color.Red);
                        illegal = true;
                        break;
                    }
                }
            }
            // Disallow directory and drive traversal
            if (args.Contains("..") || args.Contains(":\\"))
            {
                ConsoleOutput.WriteLine("An addon attempted to perform directory traversal.", Color.Red);
                illegal = true;
            }
            // Don't allow batch or ps1 files to be created
            if (args.Contains(".bat") || args.Contains(".ps1"))
            {
                ConsoleOutput.WriteLine("An addon attempted to create a batch or powershell script.", Color.Red);
                illegal = true;
            }
            if(illegal)
            {
                ConsoleOutput.WriteLine("Offending command: "+args, Color.Red);
            }
            return !illegal;
        }
        public static string jobDirectory = "";
        // Replace placeholders in args with values from settings
        public static string ReplacePlaceholders(string args)
        {
            string result = args;
            foreach(KeyValuePair<string, string> placeholder in placeholders)
            {
                result = result.Replace(placeholder.Key, placeholder.Value);
            }
            return result;
        }
        // RunFFmpeg for lua
        public static void RunFFmpeg(string args)
        {
            if(!ValidateInput(args)) return;
            PluginHandler.commands.Add(new Command(CommandType.FFmpeg, ReplacePlaceholders(args), jobDirectory));
        }
        // RunFFprobe for lua
        public static void RunFFprobe(string args)
        {
            if(!ValidateInput(args)) return;
            PluginHandler.commands.Add(new Command(CommandType.FFprobe, ReplacePlaceholders(args), jobDirectory));
        }
        // RunMagick for lua
        public static void RunMagick(string args)
        {
            if(!ValidateInput(args)) return;
            PluginHandler.commands.Add(new Command(CommandType.Magick, ReplacePlaceholders(args), jobDirectory));
        }
        // FolderCreate for lua
        public static void FolderCreate(string path)
        {
            if(!ValidateInput(path)) return;
            string combinedPath = Path.Combine(jobDirectory, ReplacePlaceholders(path));
            Directory.CreateDirectory(combinedPath);
        }
        // FileCopy for lua
        public static void FileCopy(string source, string dest)
        {
            if(!ValidateInput(source)) return;
            if(!ValidateInput(dest)) return;
            string combinedSource = Path.Combine(jobDirectory, ReplacePlaceholders(source));
            string combinedDest = Path.Combine(jobDirectory, ReplacePlaceholders(dest));
            File.Copy(combinedSource, combinedDest);
        }
        // FileDelete for lua
        public static void FileDelete(string path)
        {
            if(!ValidateInput(path)) return;
            string combinedPath = Path.Combine(jobDirectory, ReplacePlaceholders(path));
            if (File.Exists(combinedPath))
                File.Delete(combinedPath);
        }
        // FileMove for lua
        public static void FileMove(string source, string dest)
        {
            if(!ValidateInput(source)) return;
            if(!ValidateInput(dest)) return;
            string combinedSource = Path.Combine(jobDirectory, ReplacePlaceholders(source));
            string combinedDest = Path.Combine(jobDirectory, ReplacePlaceholders(dest));
            if (File.Exists(combinedSource))
                File.Move(combinedSource, combinedDest);
        }
        // FileExists for lua
        public static bool FileExists(string path)
        {
            if(!ValidateInput(path)) return false;
            string combinedPath = Path.Combine(jobDirectory, ReplacePlaceholders(path));
            return File.Exists(combinedPath);
        }
        // FolderExists for lua
        public static bool FolderExists(string path)
        {
            if(!ValidateInput(path)) return false;
            string combinedPath = Path.Combine(jobDirectory, ReplacePlaceholders(path));
            return Directory.Exists(combinedPath);
        }
        // EnumerateFiles for lua
        public static IEnumerable<string> EnumerateFiles(string path)
        {
            if(!ValidateInput(path)) return new List<string>();
            string combinedPath = Path.Combine(jobDirectory, ReplacePlaceholders(path));
            // use relative path
            return Directory.EnumerateFiles(combinedPath, "*", SearchOption.AllDirectories).Select(file =>
            {
                return Path.GetRelativePath(combinedPath, file);
            });

        }
        // WriteFile for lua
        public static void WriteFile(string path, string contents)
        {
            if(!ValidateInput(path)) return;
            string combinedPath = Path.Combine(jobDirectory, ReplacePlaceholders(path));
            if (File.Exists(combinedPath))
                File.Delete(combinedPath);
            File.WriteAllText(combinedPath, contents);
        }
        // RandomDouble for lua (uses global random)
        public static double RandomDouble(double min, double max)
        {
            return Global.generator.globalRandom.NextDouble() * (max - min) + min;
        }
        // RandomInt for lua (uses global random)
        public static int RandomInt(int min, int max)
        {
            return Global.generator.globalRandom.Next(min, max);
        }
        // RandomBool for lua (uses global random)
        public static bool RandomBool()
        {
            return Global.generator.globalRandom.Next(2) == 0;
        }
        // FFmpeg installed for lua
        public static bool FFmpegInstalled()
        {
            return Global.useSystemFFmpeg || File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ffmpeg.exe"));
        }
        // FFprobe installed for lua
        public static bool FFprobeInstalled()
        {
            return Global.useSystemFFprobe || File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ffprobe.exe"));
        }
        // Magick installed for lua
        public static bool MagickInstalled()
        {
            return UpdateManager.DoesCommandExist("magick");
        }
        // GetRandomLibraryFile for lua
        public static string GetRandomLibraryFile(string rootType = "video", string subType = "materials")
        {
            // Validate rootType and subType
            if (rootType != "video" && rootType != "audio")
            {
                throw new Exception("Invalid rootType");
            }
            LibraryType? dummyType = null;
            // Validate subType
            foreach (KeyValuePair<LibraryType, string> pair in LibraryData.libraryPaths)
            {
                if (pair.Value == rootType + "\\" + subType)
                {
                    dummyType = pair.Key;
                    break;
                }
                if (pair.Key == LibraryData.libraryPaths.Last().Key)
                {
                    throw new Exception("Invalid subType");
                }
            }
            if (dummyType == null)
            {
                throw new Exception("Invalid subType");
            }
            string file = LibraryData.PickRandom(dummyType, Global.generator.globalRandom);
            // remove placeholder if it already exists
            if (placeholders.ContainsKey("{LibraryFile_" + subType + "}"))
            {
                placeholders.Remove("{LibraryFile_" + subType + "}");
            }
            string thepath = Path.Join("..", "..", file);
            if(thepath != "..\\..")
            {
                placeholders.Add("{LibraryFile_" + subType + "}", thepath);
                return "{LibraryFile_" + subType + "}";
            }
            return "";
        }
        // AddToLibrary for lua
        public void AddToLibrary(string rootType, string subType, string path)
        {
            if(consentForm != null && !consentForm.CheckConsentParam(Consents.AddToLibrary, ReplacePlaceholders(path)))
            {
                ConsoleOutput.WriteLine("Addon attempted to add a file to the library without permission.", Color.Red);
                return;
            }
            if(!ValidateInput(path))
            {
                ConsoleOutput.WriteLine("Addon attempted to add a file to the library with an invalid path.", Color.Red);
                return;
            }
            // Validate rootType and subType
            if (rootType != "video" && rootType != "audio")
            {
                throw new Exception("Invalid rootType");
            }
            LibraryType? dummyType = null;
            // Validate subType
            foreach (KeyValuePair<LibraryType, string> pair in LibraryData.libraryPaths)
            {
                if (pair.Value == rootType + "\\" + subType)
                {
                    dummyType = pair.Key;
                    break;
                }
                if (pair.Key == LibraryData.libraryPaths.Last().Key)
                {
                    throw new Exception("Invalid subType");
                }
            }
            if (dummyType == null)
            {
                throw new Exception("Invalid subType");
            }
            // Add to library
            ConsoleOutput.WriteLine($"Loading {ReplacePlaceholders(path)} to library", Color.LightBlue);
            LibraryFile libfile = new(Path.GetFileNameWithoutExtension(ReplacePlaceholders(path)), Path.Combine("temp", Path.GetDirectoryName(jobDirectory), ReplacePlaceholders(path)), dummyType);
            if(LibraryData.Load(libfile) == null)
            {
                ConsoleOutput.WriteLine($"Failed to load {ReplacePlaceholders(path)} to library", Color.Red);
                return;
            }
            Global.justCompletedRender = true;
        }
        // LibraryHasFile for lua
        public static bool LibraryHasFile(string rootType, string subType, string path)
        {
            // Validate rootType and subType
            if (rootType != "video" && rootType != "audio")
            {
                throw new Exception("Invalid rootType");
            }
            // Check library directory
            string combinedPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "library", rootType, subType, ReplacePlaceholders(path));
            // Make sure folder exists
            if (!Directory.Exists(Path.GetDirectoryName(combinedPath)))
            {
                return false;
            }
            // Check if file exists
            if (File.Exists(combinedPath))
            {
                return true;
            }
            return false;
        }
        // DownloadFile for lua
        public void DownloadFile(string url, string rootType = "", string subType = "")
        {
            if(consentForm != null && !consentForm.CheckConsentParam(Consents.DownloadFiles, ReplacePlaceholders(url)))
            {
                ConsoleOutput.WriteLine("Addon attempted to download a file without permission.", Color.Red);
                return;
            }
            if(!ValidateInput(ReplacePlaceholders(url)))
            {
                ConsoleOutput.WriteLine("Addon attempted to download a file with an invalid URL.", Color.Red);
                return;
            }
            if(subType == "")
                rootType = jobDirectory;
            else if(consentForm != null && !consentForm.CheckConsentParam(Consents.AddToLibrary, Path.GetFileName(ReplacePlaceholders(url))))
            {
                ConsoleOutput.WriteLine("Addon attempted to add a file to the library without permission.", Color.Red);
                return;
            }
            PluginHandler.commands.Add(new Command(CommandType.Download, ReplacePlaceholders(url) + " " + rootType + " " + subType, jobDirectory));
        }
        // ExecuteProgram for lua
        public void ExecuteProgram(string program, string args)
        {
            if(consentForm != null && !consentForm.CheckConsentParam(Consents.ExecutePrograms, ReplacePlaceholders(program)))
            {
                ConsoleOutput.WriteLine("Addon attempted to execute a program without permission.", Color.Red);
                return;
            }
            if(!ValidateInput(ReplacePlaceholders(program)))
            {
                ConsoleOutput.WriteLine("Addon attempted to execute a program with an invalid program name.", Color.Red);
                return;
            }
            if(!ValidateInput(ReplacePlaceholders(args)))
            {
                ConsoleOutput.WriteLine("Addon attempted to execute a program with invalid arguments.", Color.Red);
                return;
            }
            // If program is yt-dlp, allow it to be executed from cwd
            if(program == "yt-dlp")
            {
                program = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "yt-dlp.exe");
            }
            // Execute program
            ConsoleOutput.WriteLine($"Executing {ReplacePlaceholders(program)} {ReplacePlaceholders(args)}", Color.LightBlue);
            ProcessStartInfo startInfo = new()
            {
                FileName = ReplacePlaceholders(program),
                Arguments = ReplacePlaceholders(args),
                WorkingDirectory = jobDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            Process process = new()
            {
                StartInfo = startInfo
            };
            process.Start();
            string output = "";
            string error = "";
            process.OutputDataReceived += (sender, e) => {
                if(e.Data != null)
                {
                    output += e.Data;
                    ConsoleOutput.WriteLine(e.Data, Color.Transparent);
                }
            };
            process.ErrorDataReceived += (sender, e) => {
                if(e.Data != null)
                {
                    error += e.Data;
                    ConsoleOutput.WriteLine(e.Data, Color.Transparent);
                }
            };
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }
        public PluginReturnValue Call(string video)
        {
            // Create .\temp\job_%time%\
            string oldVideo = video;
            string jobPath = Path.Join(@".\temp\", "job_" + DateTime.Now.ToString("HHmmssfff", System.Globalization.CultureInfo.InvariantCulture));
            // Delete job path if it already exists
            if (Directory.Exists(jobPath))
            {
                Directory.Delete(jobPath, true);
            }
            Directory.CreateDirectory(jobPath);
            jobDirectory = Path.GetFullPath(jobPath) + @"\";
            // Copy video to job path as result.mp4
            File.Copy(video, Path.Join(jobPath, "result.mp4"));
            video = Path.Join(jobPath, "result.mp4");
            if (enabled == false)
            {
                return new PluginReturnValue(false, Path.GetFileName(path));
            }
            ConsoleOutput.WriteLine($"Calling Addon {Path.GetFileName(path)}", Color.LightBlue);
            switch (type)
            {
                case PluginType.Lua:
                    if(luaScript == null)
                        return new PluginReturnValue(false, Path.GetFileName(path));
                    // Run Generate function if it exists.
                    if (luaScript.Globals["StartGeneration"] != null)
                    {
                        try
                        {
                            placeholders.Clear();
                            // Create videoOptions table
                            Table videoOptions = new(luaScript);
                            videoOptions["width"] = int.Parse(SaveData.saveValues["VideoWidth"], System.Globalization.CultureInfo.InvariantCulture);
                            videoOptions["height"] = int.Parse(SaveData.saveValues["VideoHeight"], System.Globalization.CultureInfo.InvariantCulture);
                            videoOptions["inputVideo"] = Path.GetFileName(video);
                            videoOptions["outputVideo"] = "output.mp4";
                            videoOptions["workingDirectory"] = "";
                            // Add settings to pluginSettings table
                            Table pluginSettings = new(luaScript);
                            foreach (KeyValuePair<string, object> setting in settings)
                            {
                                pluginSettings[setting.Key] = setting.Value;
                            }
                            // Add functions
                            Table functions = new(luaScript);
                            functions["runFFmpeg"] = (Action<string>)RunFFmpeg;
                            functions["runFFprobe"] = (Action<string>)RunFFprobe;
                            functions["runMagick"] = (Action<string>)RunMagick;
                            functions["randomDouble"] = (Func<double, double, double>)RandomDouble;
                            functions["randomInt"] = (Func<int, int, int>)RandomInt;
                            functions["randomBool"] = (Func<bool>)RandomBool;
                            functions["folderCreate"] = (Action<string>)FolderCreate;
                            functions["fileCopy"] = (Action<string, string>)FileCopy;
                            functions["fileDelete"] = (Action<string>)FileDelete;
                            functions["fileMove"] = (Action<string, string>)FileMove;
                            functions["fileExists"] = (Func<string, bool>)FileExists;
                            functions["fileWrite"] = (Action<string, string>)WriteFile;
                            functions["folderExists"] = (Func<string, bool>)FolderExists;
                            functions["enumerateFiles"] = (Func<string, IEnumerable<string>>)EnumerateFiles;
                            functions["getRandomLibraryFile"] = (Func<string, string, string>)GetRandomLibraryFile;
                            functions["ffmpegInstalled"] = (Func<bool>)FFmpegInstalled;
                            functions["ffprobeInstalled"] = (Func<bool>)FFprobeInstalled;
                            functions["magickInstalled"] = (Func<bool>)MagickInstalled;
                            functions["libraryHasFile"] = (Func<string, string, string, bool>)LibraryHasFile;
                            // Requires permission
                            functions["addToLibrary"] = (Action<string, string, string>)AddToLibrary;
                            functions["downloadFile"] = (Action<string, string, string>)DownloadFile;
                            functions["executeProgram"] = (Action<string, string>)ExecuteProgram;
                            PluginHandler.commands.Clear();
                            // Call generation
                            DynValue result = luaScript.Call(luaScript.Globals["StartGeneration"], videoOptions, pluginSettings, functions);
                            if (result.Type == DataType.Boolean && result.Boolean)
                            {
                                for (int i = 0; i < PluginHandler.commands.Count; i++)
                                {
                                    // PostCommand function is called with the index of the command
                                    string[] outputs = PluginHandler.commands[i].Call();
                                    luaScript.Call(luaScript.Globals["PostCommand"], i+1, outputs[0], outputs[1], videoOptions, pluginSettings, functions);
                                }
                                DynValue result2 = luaScript.Call(luaScript.Globals["StopGeneration"], videoOptions, pluginSettings, functions);
                                if (result2.Type == DataType.Boolean && result2.Boolean)
                                {
                                    return new PluginReturnValue(true, Path.GetFileName(path), jobPath + @"\");
                                }
                                else
                                {
                                    return new PluginReturnValue(false, Path.GetFileName(path));
                                }
                            }
                            else
                            {
                                return new PluginReturnValue(false, Path.GetFileName(path));
                            }
                        }
                        catch (ScriptRuntimeException e)
                        {
                            ConsoleOutput.WriteLine(e.DecoratedMessage, Color.Red);
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
                case PluginType.Lua:
                    // If root is not "workshop", set workshopId to name of plugin folder.
                    if (Path.GetFileName(Path.GetDirectoryName(path)) != "workshop"
                        && Path.GetFileName(Path.GetDirectoryName(path)) != "user")
                    {
                        workshopId = Path.GetFileName(Path.GetDirectoryName(path));
                        // check for .publish inside folder
                        if (File.Exists(Path.Join(Path.GetDirectoryName(path), ".publish")))
                        {
                            submittedId = File.ReadAllText(Path.Join(Path.GetDirectoryName(path), ".publish")); // contains submitted workshopId
                        }
                    }
                    // Moonsharp is used to run Lua plugins in a sandbox.
                    // Steam Workshop plugins are Lua.
                    luaScript = new Script(CoreModules.Preset_SoftSandbox);
                    luaScript.Options.ScriptLoader = new CustomScriptLoader();
                    luaScript.Options.DebugPrint = (s) => ConsoleOutput.WriteLine(s, Color.DarkCyan);
                    try
                    {
                        luaScript.DoFile(path);
                    }
                    catch(SyntaxErrorException e)
                    {
                        if(workshopId != "" && rootPath.Contains("user"))
                        {
                            string achievement = "ACHIEVEMENT_LUA_ERROR";
                            ConsoleOutput.WriteLine("Awarding achievement: "+achievement, Color.LightBlue);
                            SteamUserStats.SetAchievement(achievement);
                        }
                        throw e;
                    }
                    // Call plugin with query argument.
                    if(luaScript.Globals["Query"] != null)
                    {
                        DynValue luaQuery = luaScript.Call(luaScript.Globals["Query"]);
                        if (luaQuery.Type != DataType.Table)
                        {
                            ConsoleOutput.WriteLine($"Addon {Path.GetFileName(path)} returned invalid query.", Color.Red);
                            return false;
                        }
                        // Parse output.
                        DynValue luaLibraries = luaQuery.Table.Get("libraries");
                        if (luaLibraries.Type == DataType.Table)
                        {
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
                                PluginHandler.queriedLibraryTypes.Add(new LibraryCombinedType(dummyType, libraryPrettyName, libPath, fileExts));
                            }
                            // Print count
                            //if (libraryCount > 0)
                                //ConsoleOutput.WriteLine($"Addon {Path.GetFileName(path)} added {libraryCount} libraries.", Color.LightBlue);
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
                                // Only "name" and "type" are required.
                                string settingName = setting.Table.Get("name").String;
                                DynValue dynvalue = setting.Table.Get("value");
                                string settingValue = dynvalue.Type == DataType.String ? dynvalue.String : "";
                                DynValue dyntooltip = setting.Table.Get("tooltip");
                                string settingTooltip = dyntooltip.Type == DataType.String ? dyntooltip.String : "";
                                string settingValueType = setting.Table.Get("type").String;
                                settings.Add(settingName, settingValue);
                                settingTooltips.Add(settingName, settingTooltip);
                                switch(settingValueType)
                                {
                                    case "int":
                                    case "integer":
                                    case "number":
                                        settingTypes.Add(settingName, SettingType.TextInputNumbers);
                                        break;
                                    case "float":
                                    case "double":
                                    case "decimal":
                                        settingTypes.Add(settingName, SettingType.TextInputDecimals);
                                        break;
                                    case "alphabetic":
                                    case "alphabetical":
                                    case "alphabet":
                                        settingTypes.Add(settingName, SettingType.TextInputLetters);
                                        break;
                                    case "alphanumeric":
                                    case "alphanumeral":
                                    case "alphanumerical":
                                        settingTypes.Add(settingName, SettingType.TextInputLettersNumbers);
                                        break;
                                    case "string":
                                    case "text":
                                        settingTypes.Add(settingName, SettingType.TextInputLettersNumbersSpaces);
                                        break;
                                    case "bool":
                                    case "boolean":
                                    case "switch":
                                        settingTypes.Add(settingName, SettingType.Switch);
                                        break;
                                    case "label":
                                        settingTypes.Add(settingName, SettingType.Label);
                                        break;
                                    default:
                                        settingTypes.Add(settingName, SettingType.TextInput);
                                        break;
                                }
                                settingCount++;
                            }
                            // Print count
                            //if (settingCount > 0)
                                //ConsoleOutput.WriteLine($"Addon {Path.GetFileName(path)} added {settingCount} settings.", Color.LightBlue);
                        }
                        DynValue luaConsentForms = luaQuery.Table.Get("userconsent");
                        if (luaConsentForms.Type == DataType.Table)
                        {
                            // Parse flags table as flags
                            Consents consentFlags = Consents.None;
                            Dictionary<Consents, List<string>> consentSettings = new();
                            DynValue luaConsentFlags = luaConsentForms.Table.Get("consents");
                            if (luaConsentFlags.Type == DataType.Table)
                            {
                                foreach (DynValue flag in luaConsentFlags.Table.Values)
                                {
                                    if (flag.Type != DataType.Table)
                                    {
                                        continue;
                                    }
                                    DynValue flagString = flag.Table.Get("flag");
                                    if (flagString.Type == DataType.String)
                                    {
                                        string flagStringParsed = flagString.String;
                                        switch (flagStringParsed)
                                        {
                                            case "DownloadFiles":
                                                consentFlags |= Consents.DownloadFiles;
                                                break;
                                            case "ExecutePrograms":
                                                consentFlags |= Consents.ExecutePrograms;
                                                break;
                                            case "AddToLibrary":
                                                consentFlags |= Consents.AddToLibrary;
                                                break;
                                        }
                                        // Get parameters
                                        DynValue luaConsentSettings = flag.Table.Get("params");
                                        List<string> param = new();
                                        if (luaConsentSettings.Type == DataType.Table)
                                        {
                                            foreach (DynValue setting in luaConsentSettings.Table.Values)
                                            {
                                                if (setting.Type != DataType.String)
                                                {
                                                    continue;
                                                }
                                                // param is either a url or a program name
                                                param.Add(setting.String);
                                            }
                                        }
                                        Consents singleFlag = flagStringParsed == "DownloadFiles" ? Consents.DownloadFiles : (flagStringParsed == "ExecutePrograms" ? Consents.ExecutePrograms : Consents.AddToLibrary);
                                        consentSettings.Add(singleFlag, param);
                                    }
                                }
                            }
                            consentForm = new ConsentForm(Path.GetFileName(path), GetDisplayName(), consentFlags, workshopId, rootPath, consentSettings);
                        }
                    }
                    return true;
            }
        }
        public bool CheckConsent()
        {
            if (consentForm == null)
                return false;
            return UserConsent.CheckConsentForm(consentForm);
        }
    }
    public class LibraryCombinedType
    {
        public LibraryType type;
        public string prettyName;
        public string path;
        public string[] fileExts;
        public LibraryCombinedType(LibraryType type, string prettyName, string path, string[] fileExts)
        {
            this.type = type;
            this.prettyName = prettyName;
            this.path = path;
            this.fileExts = fileExts;
        }
    }
    /// <summary>
    /// Plugin support.
    /// </summary>
    public static class PluginHandler
    {
        public static List<LibraryCombinedType> queriedLibraryTypes = new();
        public static List<Plugin> plugins = new();
        private static string pluginPath = @".\plugins";
        public static string pluginSettingsPath = @".\PluginSettings.json";
        public static List<Command> commands = new();
        public static PublishedFileId_t[]? subscribedItems;
        public static bool publishing = false;
        public static bool updating = false;
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
                int index = plugins.FindIndex(plugin => {
                    if(plugin.workshopId != "")
                    {
                        if(plugin.workshopId + "/" + Path.GetFileName(plugin.path) == pluginName)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if(Path.GetFileName(plugin.path) == pluginName)
                        {
                            return true;
                        }
                    }
                    return false;
                });
                if (index == -1)
                    continue;
                plugins[index].enabled = !(pluginSetting.Value["disabled"] as bool? ?? false);
                Dictionary<string, object>? settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(pluginSetting.Value["settings"].ToString());
                foreach(KeyValuePair<string, object> setting in settings)
                {
                    if(!plugins[index].settings.ContainsKey(setting.Key))
                    {
                        continue;
                    }
                    plugins[index].settings[setting.Key] = setting.Value;
                }
            }
            SavePluginSettings();
        }
        public static void SavePluginSettings()
        {
            Dictionary<string, Dictionary<string, object>> pluginSettings = new();
            foreach(Plugin plugin in plugins)
            {
                Dictionary<string, object> pluginSetting = new();
                // Copy settings so we can remove labels from it.
                pluginSetting.Add("settings", plugin.settings.ToDictionary(setting => setting.Key, setting => setting.Value));
                pluginSetting.Add("disabled", !plugin.enabled);
                // Remove labels from settings
                foreach(KeyValuePair<string, SettingType> setting in plugin.settingTypes)
                {
                    if (setting.Value == SettingType.Label)
                    {
                        ((Dictionary<string, object>)pluginSetting["settings"]).Remove(setting.Key);
                    }
                }
                // Key
                string key = Path.GetFileName(plugin.path);
                // If workshopId is set, prepend it to the key.
                if (plugin.workshopId != "")
                {
                    key = $"{plugin.workshopId}/{key}";
                }
                // Already contains key?
                if (!pluginSettings.ContainsKey(key))
                {
                    pluginSettings.Add(key, pluginSetting);
                }
            }
            File.WriteAllText(pluginSettingsPath, JsonConvert.SerializeObject(pluginSettings, Formatting.Indented));
        }
        public static void LoadPlugin(string path, PluginType type, string rootPath, string workshopid = "")
        {
            Plugin plugin = new(path, type, rootPath);
            Global.generator.progressText = $"Loading Addon {Path.GetFileName(path)}...";
            if(!plugin.Query())
                throw new Exception($"Failed to query Addon {Path.GetFileName(path)}.");
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
            // Key
            string key = Path.GetFileName(path);
            // If workshopId is set, prepend it to the key.
            if (workshopid != "")
            {
                key = $"{workshopid}/{key}";
            }
            if (existingPluginSettings != null)
            {
                if (!existingPluginSettings.ContainsKey(key))
                {
                    existingPluginSettings.Add(key, pluginSettings);
                    File.WriteAllText(pluginSettingsPath, JsonConvert.SerializeObject(existingPluginSettings, Formatting.Indented));
                }
            }
            ConsoleOutput.WriteLine($"Loaded Addon {key}.", Color.LightBlue);
        }
        private static void LoadPluginsRecursive(string path, PluginType type, string root = "")
        {
            foreach (string file in Directory.GetFiles(path))
            {
                switch(type)
                {
                    case PluginType.Lua:
                        if (file.EndsWith(".lua"))
                        {
                            // The basename of the directory is a steam workshop id.
                            string basename = Path.GetFileName(path);
                            // Is this in subscribedItems?
                            if(root.Contains("workshop") && subscribedItems != null)
                            {
                                if (subscribedItems.Any(item => item.m_PublishedFileId.ToString(System.Globalization.CultureInfo.InvariantCulture) == basename))
                                {
                                    LoadPlugin(file, type, root, basename);
                                }
                                else if(basename != "user")
                                {
                                    ConsoleOutput.WriteLine($"Deleting Addon {basename} because it is not subscribed to.", Color.Red);
                                    File.Delete(file);
                                }
                            }
                            else
                            {
                                LoadPlugin(file, type, root, basename);
                            }
                        }
                        break;
                }
            }
            // Recurse into subdirectories.
            foreach(string file in Directory.GetDirectories(path))
            {
                LoadPluginsRecursive(file, type, path);
            }
        }
        private static uint allDoneCount = 0;
        private static BackgroundWorker pluginWorker = new();
        public static void AllDone(uint count)
        {
            allDoneCount++;
            if(count == 0 || allDoneCount == count)
            {
                foreach (PublishedFileId_t item in subscribedItems)
                {
                    string itemPath = Path.Combine(pluginPath, "workshop", item.m_PublishedFileId.ToString());
                    SteamUGC.GetItemInstallInfo(item, out ulong size, out string folder, 1024, out uint timestamp);
                    if (Directory.Exists(itemPath) == false)
                    {
                        Directory.CreateDirectory(itemPath);
                    }
                    // Delete files currently in plugin folder.
                    foreach (string file in Directory.GetFiles(itemPath))
                    {
                        File.Delete(file);
                    }
                    // Copy files from workshop folder to plugin folder.
                    foreach (string file in Directory.GetFiles(folder))
                    {
                        string dest = Path.Combine(itemPath, Path.GetFileName(file));
                        // File hash check.
                        if(File.Exists(dest))
                        {
                            if (File.ReadAllBytes(file).SequenceEqual(File.ReadAllBytes(dest)))
                            {
                                continue;
                            }
                        }
                        if(File.Exists(dest))
                        {
                            File.Delete(dest);
                        }
                        File.Copy(file, dest);
                        ConsoleOutput.WriteLine($"Installed ID {Path.GetFileName(file)} from workshop.", Color.RoyalBlue);
                    }
                }
                pluginWorker = new();
                pluginWorker.DoWork += PluginWorker_DoWork;
                pluginWorker.RunWorkerAsync();
                ConsoleOutput.WriteLine("All done.", Color.RoyalBlue);
            }
        }
        private static void PluginWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Global.pluginsLoaded = PluginHandler.LoadPlugins();
        }
        public static List<Callback<DownloadItemResult_t>> downloadItemResult = new();
        public static Callback<UserStatsReceived_t>? m_UserStatsReceived;
        public static void LoadWorkshop()
        {
            if(SteamUserStats.RequestCurrentStats())
            {
                // Add callback
                if(m_UserStatsReceived == null)
                {
                    m_UserStatsReceived = Callback<UserStatsReceived_t>.Create((param) => {
                        if(param.m_eResult != EResult.k_EResultOK)
                        {
                            ConsoleOutput.WriteLine("Error getting user stats.", Color.Red);
                            return;
                        }
                        if (param.m_nGameID == SteamUtils.GetAppID().m_AppId)
                        {
                            ConsoleOutput.WriteLine("Got user stats.", Color.LightBlue);
                            Global.canAchieve = true;
                        }
                    });
                }
            }
            else
            {
                Console.WriteLine("Error requesting user stats.");
            }
            allDoneCount = 0;
            downloadItemResult.Clear();
            // Create "plugins\workshop" if they don't exist.
            Directory.CreateDirectory(Path.Combine(pluginPath, "workshop"));
            // Load subscribed workshop items.
            uint subscribedItemCount = SteamUGC.GetNumSubscribedItems();
            ConsoleOutput.WriteLine($"Found {subscribedItemCount} subscribed workshop items.", Color.LightBlue);
            subscribedItems = new PublishedFileId_t[subscribedItemCount];
            if(subscribedItemCount > 0)
            {
                SteamUGC.GetSubscribedItems(subscribedItems, (uint)subscribedItems.Length);
                ConsoleOutput.WriteLine("Updating " + subscribedItemCount + " subscribed workshop items...", Color.LightBlue);
                foreach (PublishedFileId_t item in subscribedItems)
                {
                    string itemPath = Path.Combine(pluginPath, "workshop", item.m_PublishedFileId.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    bool download = SteamUGC.DownloadItem(item, true);
                    if (download)
                    {
                        // Register callback.
                        downloadItemResult.Add(Callback<DownloadItemResult_t>.Create((result) =>
                        {
                            if (result.m_nPublishedFileId == item)
                            {
                                if (result.m_eResult == EResult.k_EResultOK)
                                {
                                    ConsoleOutput.WriteLine($"Downloaded ID {item.m_PublishedFileId.ToString(System.Globalization.CultureInfo.InvariantCulture)} from workshop.", Color.RoyalBlue);
                                }
                                else
                                {
                                    ConsoleOutput.WriteLine($"Failed to download ID {item.m_PublishedFileId.ToString()} from workshop.", Color.Red);
                                }
                                AllDone(subscribedItemCount);
                            }
                        }));
                    }
                    else
                    {
                        ConsoleOutput.WriteLine($"Failed to download ID {item.m_PublishedFileId.ToString()} from workshop.", Color.Red);
                        AllDone(subscribedItemCount);
                    }
                }
            }
            else
            {
                AllDone(0);
            }
        }
        public static BackgroundWorker loadPluginsThread = new();
        public static void LoadPluginsThreaded()
        {
            loadPluginsThread = new();
            loadPluginsThread.DoWork += (sender, e) =>
            {
                LoadPlugins();
            };
            loadPluginsThread.RunWorkerCompleted += (sender, e) =>
            {
                Global.pluginsLoaded = true;
            };
            loadPluginsThread.RunWorkerAsync();
        }
        public static bool LoadPlugins()
        {
            // Find all custom library types and remove them
            foreach (Plugin plugin in plugins)
            {
                foreach(LibraryCombinedType libtype in queriedLibraryTypes)
                {
                    DefaultLibraryTypes.AllTypes.Remove(libtype.type);
                    LibraryData.libraryPaths.Remove(libtype.type);
                    LibraryData.libraryFileTypes.Remove(libtype.type);
                    LibraryData.libraryNames.Remove(libtype.type);
                }
            }
            queriedLibraryTypes.Clear();
            // Clear plugins.
            plugins.Clear();
            Global.pluginsLoaded = false;
            // Create plugin directory if it doesn't exist.
            if(Directory.Exists(pluginPath) == false)
            {
                Directory.CreateDirectory(pluginPath);
            }
            ConsoleOutput.WriteLine($"Searching for addons in {pluginPath}...", Color.LightBlue);
            List<string> pluginDirs = new()
            {
                "workshop",
                "user"
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
                    PluginType type = PluginType.None;
                    switch (dirName)
                    {
                        case "workshop":
                            type = PluginType.Lua;
                            break;
                        case "user":
                            type = PluginType.Lua;
                            break;
                    }
                    ConsoleOutput.WriteLine($"Loading {dirName} addons...", Color.LightBlue);
                    if (type == PluginType.None)
                        continue;
                    LoadPluginsRecursive(file, type);
                }
                List<string> typesAdded = new();
                foreach(LibraryCombinedType dummyType in queriedLibraryTypes)
                {
                    // Check to see if library already exists.
                    if (typesAdded.Contains(dummyType.path))
                    {
                        continue;
                    }
                    DefaultLibraryTypes.AllTypes.Add(dummyType.type);
                    LibraryData.libraryPaths.Add(dummyType.type, dummyType.path);
                    LibraryData.libraryFileTypes.Add(dummyType.type, dummyType.fileExts);
                    LibraryData.libraryNames.Add(dummyType.type, dummyType.prettyName);
                    // Print to console.
                    ConsoleOutput.WriteLine($"Added {(dummyType.type.RootType == LibraryRootType.Video ? "video" : "audio")} library {dummyType.prettyName}.", Color.LightBlue);
                    typesAdded.Add(dummyType.path);
                }
                Global.justCompletedRender = true; // demand a refresh
                LoadPluginSettings();
                Global.generator.progressText = $"{plugins.Count} effects loaded.";
                Global.canRender = true;
                LibraryData.SequentialName();
                // Check to see if there are any plugins that need consent forms filled out.
                foreach(Plugin plugin in plugins)
                {
                    if(plugin.CheckConsent())
                    {
                        UserConsent.needsConsent = true;
                        UserConsent.consentForm = plugin.consentForm;
                        Global.generator.progressText = $"Addon {plugin.GetDisplayName()} requires consent.";
                        FramePlayer.canPlayBgMusic = false;
                        ScreenManager.PushNavigation("Initial Setup");
                        ScreenManager.GetScreen<TutorialScreen>("Initial Setup")?.Show();
                        ScreenManager.GetScreen<ContentScreen>("Content")?.Hide();
                        ScreenManager.GetScreen<MenuScreen>("Main Menu")?.Hide();
                        if(FramePlayer.audio != null)
                            ScreenManager.GetScreen<VideoScreen>("Video")?.Hide();
                        ScreenManager.GetScreen<BackgroundScreen>("Background")?.Hide();
                        ScreenManager.GetScreen<SocialScreen>("Socials")?.Hide();
                        GlobalContent.GetSound("Prompt").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        break;
                    }
                }
            }
            catch (SyntaxErrorException e)
            {
                ConsoleOutput.WriteLine(e.DecoratedMessage, Color.Red);
                Global.generator.progressText = $"Error loading addons!";
                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
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
            PluginReturnValue called = plugin.Call(video);
            if(called.success)
            {
                if(plugin.workshopId != ""
                    && plugin.rootPath.Contains("workshop"))
                {
                    Global.usedWorkshopPlugin = true;
                }
            }
            return called;
        }
        public static bool CreatePlugin(string filename, string prettyname, bool minimal, out string file)
        {
            try
            {
                // copy .\templates\minimal.lua or .\templates\effect.lua to .\plugins\user\filename\filename.lua
                string template = minimal ? "minimal.lua" : "effect.lua";
                string templatePath = Path.Combine("templates", template);
                string pluginPath = Path.Combine("plugins", "user", filename);
                string pluginFile = Path.Combine(pluginPath, filename + ".lua");
                if(Directory.Exists(pluginPath) == false)
                {
                    Directory.CreateDirectory(pluginPath);
                }
                File.Copy(templatePath, pluginFile);
                // Replace "effect" with filename in the file.
                string fileContents = File.ReadAllText(pluginFile);
                fileContents = fileContents.Replace("%filename%", filename + ".lua");
                fileContents = fileContents.Replace("%prettyname%", prettyname);
                File.WriteAllText(pluginFile, fileContents);
                file = pluginFile;
                return true;
            }
            catch(Exception e)
            {
                ConsoleOutput.WriteLine($"Error creating addon: {e.Message}", Color.Red);
                file = "";
                return false;
            }
        }
        public static Plugin? publishPlugin = null;
        public static WorkshopTag flags = WorkshopTag.None;
        public static string workshopIcon = "";
        public static void PublishPlugin(Plugin plugin, WorkshopTag tagflags, string iconPath)
        {
            if(publishing)
                return;
            // Is Steam API available?
            if(SteamAPI.IsSteamRunning() == false || !SteamManager.initialized)
            {
                Global.generator.progressText = "Steam is not running.";
                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                ConsoleOutput.WriteLine("Steam is not running, cannot publish addon.", Color.Red);
                publishPlugin = null;
                publishing = false;
                return;
            }
            publishing = true;
            publishPlugin = plugin;
            flags = tagflags;
            workshopIcon = iconPath;
            // Delete .publish in the plugin's directory if it exists
            string publishFile = Path.Combine(Path.GetDirectoryName(plugin.path), ".publish");
            if(File.Exists(publishFile))
            {
                File.Delete(publishFile);
            }
            // Also delete non-lua files so Steam doesn't upload them
            foreach(string file in Directory.GetFiles(Path.GetDirectoryName(plugin.path)))
            {
                if(Path.GetExtension(file) != ".lua")
                {
                    File.Delete(file);
                }
            }
            if(createItemResult == null)
            {
                createItemResult = CallResult<CreateItemResult_t>.Create(OnWorkshopItemCreated);
            }
            if(updateItemResult == null)
            {
                updateItemResult = CallResult<SubmitItemUpdateResult_t>.Create(OnWorkshopItemUpdated);
            }
            if(publishPlugin.submittedId == "")
            {
                updating = false;
                SteamAPICall_t call = SteamUGC.CreateItem(SteamUtils.GetAppID(), EWorkshopFileType.k_EWorkshopFileTypeCommunity);
                createItemResult.Set(call);
            }
            else
            {
                updating = true;
                UpdateWorkshopItem(new PublishedFileId_t(ulong.Parse(publishPlugin.submittedId)));
            }
        }
        private static CallResult<CreateItemResult_t>? createItemResult;
        private static CallResult<SubmitItemUpdateResult_t>? updateItemResult;
        private static void OnWorkshopItemCreated(CreateItemResult_t param, bool bIOFailure)
        {
            if(!publishing)
                return;
            if (param.m_eResult != EResult.k_EResultOK || bIOFailure)
            {
                ConsoleOutput.WriteLine($"Error creating workshop item: {param.m_eResult}", Color.Red);
                Global.generator.progressText = "Error creating workshop item.";
                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                publishPlugin = null;
                publishing = false;
                return;
            }
            Global.generator.progressText = "Creating Workshop item...";
        
            string achievement = "ACHIEVEMENT_WORKSHOP_SUBMIT";
            ConsoleOutput.WriteLine("Awarding achievement: "+achievement, Color.LightBlue);
            SteamUserStats.SetAchievement(achievement);

            UpdateWorkshopItem(param.m_nPublishedFileId);
        }
        public static void UpdateWorkshopItem(PublishedFileId_t id)
        {
            if(!publishing)
                return;
            // Start updating.
            UGCUpdateHandle_t handle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), id);
            if(handle.m_UGCUpdateHandle == 0)
            {
                ConsoleOutput.WriteLine($"Error updating workshop item: Invalid handle.", Color.Red);
                Global.generator.progressText = "Error updating workshop item.";
                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                publishPlugin = null;
                publishing = false;
                return;
            }
            // Set the title.
            bool cont = true;
            if(!updating)
            {
                string nam = publishPlugin.GetDisplayName();
                if(nam == "")
                    nam = "My";
                if(nam.Contains(".lua"))
                    nam = nam.Replace(".lua", "");
                // Capitalize first letter
                nam = nam.First().ToString().ToUpper() + nam.Substring(1);
                nam += " Effect";
                cont = SteamUGC.SetItemTitle(handle, nam);
            }
            if(!cont)
            {
                ConsoleOutput.WriteLine($"Error updating workshop item: Invalid title.", Color.Red);
                Global.generator.progressText = "Error updating workshop item.";
                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                publishPlugin = null;
                publishing = false;
                return;
            }
            // Set the description if one exists
            if(publishPlugin.settings.Count > 0)
            {
                string description = "";
                List<string> extsettings = new();
                foreach(KeyValuePair<string, object> setting in publishPlugin.settings)
                {
                    // Hide display name and create description, add options too
                    if(setting.Key.ToLower() != "display name" && publishPlugin.settingTypes.ContainsKey(setting.Key))
                    {
                        if(publishPlugin.settingTypes[setting.Key] == SettingType.Label)
                        {
                            description += $"{setting.Value.ToString()}\n";
                        }
                        else
                        {
                            extsettings.Add($"{setting.Key} (\"{setting.Value}\")");
                        }
                    }
                }
                if(extsettings.Count > 0)
                {
                    description += "\n";
                    description += "Settings:\n";
                    foreach(string setting in extsettings)
                    {
                        description += setting + "\n";
                    }
                }
                if(description != "" && !updating)
                    SteamUGC.SetItemDescription(handle, description);
            }
            // Set the visibility.
            if(!updating)
                cont = SteamUGC.SetItemVisibility(handle, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate);
            if(!cont)
            {
                ConsoleOutput.WriteLine($"Error updating workshop item: Invalid visibility.", Color.Red);
                Global.generator.progressText = "Error updating workshop item.";
                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                publishPlugin = null;
                publishing = false;
                return;
            }
            // Set the content.
            string contentPath = Path.GetDirectoryName(publishPlugin.path);
            // Remove leading .\ and such
            while(contentPath.StartsWith("."))
            {
                contentPath = contentPath.Substring(1);
            }
            while(contentPath.StartsWith("\\"))
            {
                contentPath = contentPath.Substring(1);
            }
            while(contentPath.StartsWith("/"))
            {
                contentPath = contentPath.Substring(1);
            }
            contentPath = contentPath.Replace("/", "\\");
            // Get full path
            contentPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), contentPath);
            ConsoleOutput.WriteLine($"Content path: {contentPath}");
            cont = SteamUGC.SetItemContent(handle, contentPath);
            if(!cont)
            {
                ConsoleOutput.WriteLine($"Error updating workshop item: Invalid content path.", Color.Red);
                Global.generator.progressText = "Error updating workshop item.";
                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                publishPlugin = null;
                publishing = false;
                return;
            }
            // Set the preview image
            cont = SteamUGC.SetItemPreview(handle, workshopIcon);
            if(!cont)
            {
                ConsoleOutput.WriteLine($"Error updating workshop item: Invalid preview path.", Color.Red);
                Global.generator.progressText = "Error updating workshop item.";
                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                publishPlugin = null;
                publishing = false;
                return;
            }
            // Set the tags.
            List<string> tags = new();
            if((flags & WorkshopTag.Effect_VideoOnly) != 0)
            {
                tags.Add("Video");
            }
            if((flags & WorkshopTag.Effect_AudioOnly) != 0)
            {
                tags.Add("Audio");
            }
            if((flags & WorkshopTag.Library_Material) != 0)
            {
                tags.Add("Material");
            }
            if((flags & WorkshopTag.Library_Transition) != 0)
            {
                tags.Add("Transition");
            }
            if((flags & WorkshopTag.Library_Overlay) != 0)
            {
                tags.Add("Overlay");
            }
            if((flags & WorkshopTag.Library_SFX) != 0)
            {
                tags.Add("Sound FX");
            }
            if((flags & WorkshopTag.Library_Music) != 0)
            {
                tags.Add("Music");
            }
            if((flags & WorkshopTag.Library_Intro) != 0)
            {
                tags.Add("Intro");
            }
            if((flags & WorkshopTag.Library_Outro) != 0)
            {
                tags.Add("Outro");
            }
            if((flags & WorkshopTag.Library_Custom) != 0)
            {
                tags.Add("Custom");
            }
            // Submit tags
            cont = SteamUGC.SetItemTags(handle, tags.ToArray());
            if(!cont)
            {
                ConsoleOutput.WriteLine($"Error updating workshop item: Invalid tags.", Color.Red);
                Global.generator.progressText = "Error updating workshop item.";
                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                publishPlugin = null;
                publishing = false;
                return;
            }
            // Submit the update.
            SteamAPICall_t call = SteamUGC.SubmitItemUpdate(handle, "Updated addon.");
            updateItemResult.Set(call);
            Global.generator.progressText = "Publishing...";
        }
        public static void OnWorkshopItemUpdated(SubmitItemUpdateResult_t param, bool bIOFailure)
        {
            if(!publishing)
                return;
            if (param.m_eResult != EResult.k_EResultOK || bIOFailure)
            {
                Global.generator.progressText = "Error publishing.";
                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                ConsoleOutput.WriteLine($"Error updating workshop item: {param.m_eResult}", Color.Red);
                publishPlugin = null;
                publishing = false;
                return;
            }
            Global.generator.progressText = "Successfully published.";
            GlobalContent.GetSound("RenderComplete").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f, 0f, 0f);
            ConsoleOutput.WriteLine($"Successfully published addon {publishPlugin.GetDisplayName()} to the workshop.", Color.Green);
            publishPlugin.submittedId = param.m_nPublishedFileId.ToString();
            // Write the plugin's workshop id to .publish in the plugin's directory.
            string publishFile = Path.Combine(Path.GetDirectoryName(publishPlugin.path), ".publish");
            File.WriteAllText(publishFile, publishPlugin.submittedId.ToString());
            publishPlugin = null;
            publishing = false;
        }
        public static int GetPluginCount(bool enabledOnly = false)
        {
            if(!Global.pluginsLoaded)
                return 0;
            // Get enabled plugin count.
            if(enabledOnly)
            {
                return plugins.FindAll(plugin => plugin.enabled).Count;
            }
            else
            {
                return plugins.Count;
            }
        }
    }
}