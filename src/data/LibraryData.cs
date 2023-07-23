using System;
using System.Collections.Generic;
using System.IO;
#if MONOGAME
using Microsoft.Xna.Framework;
#else
using System.Drawing;
#endif
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Net;
using System.Diagnostics;
using System.Linq;
using Steamworks;

namespace NonsensicalVideoGenerator
{
    public enum LibraryRootType
    {
        /// <summary>
        /// All files.
        /// </summary>
        All = 0,
        /// <summary>
        /// Video files.
        /// </summary>
        Video,
        /// <summary>
        /// Audio files.
        /// </summary>
        Audio,
    }
    public enum LibraryFileType
    {
        /// <summary>
        /// All files.
        /// </summary>
        All = 0,
        /// <summary>
        /// Sound effects.
        /// </summary>
        SFX,
        /// <summary>
        /// Music.
        /// </summary>
        Music,
        /// <summary>
        /// Material videos.
        /// </summary>
        Material,
        /// <summary>
        /// Transition videos.
        /// </summary>
        Transition,
        /// <summary>
        /// Intro videos.
        /// </summary>
        Intro,
        /// <summary>
        /// Outro videos.
        /// </summary>
        Outro,
        /// <summary>
        /// Overlay videos.
        /// </summary>
        Overlay,
        /// <summary>
        /// Rendered videos.
        /// </summary>
        Render,
        /// <summary>
        /// Custom library type.
        /// </summary>
        Custom,
    }
    public class LibraryType
    {
        public LibraryRootType RootType { get; set; }
        public LibraryFileType FileType { get; set; }
        public bool Special { get; set; } = false;
        public string CustomName { get; set; } = "";
        public string Description { get; set; } = "";
        public LibraryType(LibraryRootType rootType, LibraryFileType fileType, bool special = false)
        {
            RootType = rootType;
            FileType = fileType;
            Special = special;
        }
        public LibraryType(LibraryRootType rootType, string customName)
        {
            RootType = rootType;
            FileType = LibraryFileType.Custom;
            Special = false;
            CustomName = customName;
        }
        public LibraryType(LibraryRootType rootType, LibraryFileType fileType, string description)
        {
            RootType = rootType;
            FileType = fileType;
            Description = description;
        }
        public LibraryType(LibraryRootType rootType, string customName, string description)
        {
            RootType = rootType;
            FileType = LibraryFileType.Custom;
            Special = false;
            CustomName = customName;
            Description = description;
        }
    }
    public static class DefaultLibraryTypes
    {
        public static LibraryType All { get; } = new LibraryType(LibraryRootType.All, LibraryFileType.All, true);
        public static LibraryType Video { get; } = new LibraryType(LibraryRootType.Video, LibraryFileType.All, true);
        public static LibraryType Audio { get; } = new LibraryType(LibraryRootType.Audio, LibraryFileType.All, true);
        public static LibraryType SFX { get; } = new LibraryType(LibraryRootType.Audio, LibraryFileType.SFX, "Random sound effects.");
        public static LibraryType Music { get; } = new LibraryType(LibraryRootType.Audio, LibraryFileType.Music, "Random dance music.");
        public static LibraryType Render { get; } = new LibraryType(LibraryRootType.Video, LibraryFileType.Render, "Generated videos.");
        public static LibraryType Material { get; } = new LibraryType(LibraryRootType.Video, LibraryFileType.Material, "Root video to be used in a render.");
        public static LibraryType Transition { get; } = new LibraryType(LibraryRootType.Video, LibraryFileType.Transition, "Played in full at random points.");
        public static LibraryType Intro { get; } = new LibraryType(LibraryRootType.Video, LibraryFileType.Intro, "Played at the start of the video.");
        public static LibraryType Outro { get; } = new LibraryType(LibraryRootType.Video, LibraryFileType.Outro, "Played at the end of the video.");
        public static LibraryType Overlay { get; } = new LibraryType(LibraryRootType.Video, LibraryFileType.Overlay, "Requires pure green chroma key.");
        public static List<LibraryType> AllTypes { get; } = new List<LibraryType>()
        {
            All,
            Video,
            Audio,
            SFX,
            Music,
            Material,
            Transition,
            Intro,
            Outro,
            Overlay,
            Render
        };
    }
    public class LibraryFile
    {
        public string? Nickname { get; set; }
        public string? Path { get; set; }
        public LibraryType? Type { get; set; }
        public bool Enabled { get; set; } = true;
        public LibraryFile(string? nickname, string? path, LibraryType? type, bool enabled = true)
        {
            Nickname = nickname;
            Path = path;
            Type = type;
            Enabled = enabled;
        }
    }
    public static class LibraryData
    {
        public static List<LibraryFile> libraryFiles { get; } = new List<LibraryFile>();
        public static string libraryRootPath { get; set; } = @".\library";
        public static Dictionary<LibraryType, string> libraryPaths { get; } = new Dictionary<LibraryType, string>()
        {
            { DefaultLibraryTypes.All, @"" },
            { DefaultLibraryTypes.Video, @"video" },
            { DefaultLibraryTypes.Audio, @"audio" },
            { DefaultLibraryTypes.SFX, @"audio\sfx" },
            { DefaultLibraryTypes.Music, @"audio\music" },
            { DefaultLibraryTypes.Material, @"video\materials" },
            { DefaultLibraryTypes.Transition, @"video\transitions" },
            { DefaultLibraryTypes.Intro, @"video\intros" },
            { DefaultLibraryTypes.Outro, @"video\outros" },
            { DefaultLibraryTypes.Overlay, @"video\overlays" },
            { DefaultLibraryTypes.Render, @"video\renders" }
        };
        public static Dictionary<LibraryType, string[]> libraryFileTypes { get; } = new Dictionary<LibraryType, string[]>()
        {
            { DefaultLibraryTypes.All, new string[] { ".mp4", ".webm", ".mov", ".avi", ".mkv", ".wmv", ".wav", ".mp3", ".ogg", ".m4a", ".flac" } },
            { DefaultLibraryTypes.Video, new string[] { ".mp4", ".webm", ".mov", ".avi", ".mkv", ".wmv" } },
            { DefaultLibraryTypes.Audio, new string[] { ".wav", ".mp3", ".ogg", ".m4a", ".flac" } },
            { DefaultLibraryTypes.SFX, new string[] { ".wav", ".mp3", ".ogg", ".m4a", ".flac" } },
            { DefaultLibraryTypes.Music, new string[] { ".wav", ".mp3", ".ogg", ".m4a", ".flac" } },
            { DefaultLibraryTypes.Material, new string[] { ".mp4", ".webm", ".mov", ".avi", ".mkv", ".wmv" } },
            { DefaultLibraryTypes.Transition, new string[] { ".mp4", ".webm", ".mov", ".avi", ".mkv", ".wmv" } },
            { DefaultLibraryTypes.Intro, new string[] { ".mp4", ".webm", ".mov", ".avi", ".mkv", ".wmv" } },
            { DefaultLibraryTypes.Outro, new string[] { ".mp4", ".webm", ".mov", ".avi", ".mkv", ".wmv" } },
            { DefaultLibraryTypes.Overlay, new string[] { ".mp4", ".webm", ".mov", ".avi", ".mkv", ".wmv" } },
            { DefaultLibraryTypes.Render, new string[] { ".mp4", ".webm", ".mov", ".avi", ".mkv", ".wmv" } }
        };
        public static Dictionary<LibraryType, string> libraryNames { get; } = new Dictionary<LibraryType, string>()
        {
            { DefaultLibraryTypes.All, "All" },
            { DefaultLibraryTypes.Video, "Video" },
            { DefaultLibraryTypes.Audio, "Audio" },
            { DefaultLibraryTypes.SFX, "Sound FX" },
            { DefaultLibraryTypes.Music, "Music" },
            { DefaultLibraryTypes.Material, "Materials" },
            { DefaultLibraryTypes.Transition, "Transitions" },
            { DefaultLibraryTypes.Intro, "Intros" },
            { DefaultLibraryTypes.Outro, "Outros" },
            { DefaultLibraryTypes.Overlay, "Overlays" },
            { DefaultLibraryTypes.Render, "Renders" },
        };
        private static void LoadRecursive(string path, LibraryType type)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            // sort by date
            foreach (string file in Directory.GetFiles(path).OrderByDescending(f => new FileInfo(f).CreationTime))
            {
                foreach (string filetype in libraryFileTypes[type])
                {
                    if (file.EndsWith(filetype))
                    {
                        LibraryFile libFile = new LibraryFile(Path.GetFileNameWithoutExtension(file), file, type);
                        // If path is disabled, disable it.
                        // SaveData.saveValues["DisabledMedia"] is a string-stored json object
                        // that contains a list of disabled media paths.
                        if (SaveData.saveValues["DisabledMedia"] != "")
                        {
                            List<string> disabled = JsonConvert.DeserializeObject<List<string>>(SaveData.saveValues["DisabledMedia"]);
                            if (disabled.Contains(file))
                                libFile.Enabled = false;
                        }
                        libraryFiles.Add(libFile);
                        break;
                    }
                }
            }
            foreach (string dir in Directory.GetDirectories(path))
                LoadRecursive(dir, type);
            if(!Global.generatorFactory.generatorActive)
                SequentialName();
        }
        public static void Load()
        {
            try
            {
                if(!Global.generatorFactory.generatorActive)
                    Global.videoTitle = "Render1";
                libraryFiles.Clear();
                foreach (LibraryType type in libraryPaths.Keys)
                {
                    if(type.Special)
                        continue; // Skip special types for now.
                    string path = Path.Combine(libraryRootPath, libraryPaths[type]);
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    LoadRecursive(path, type);
                }
                // Once complete, re-make SaveData.saveValues["DisabledMedia"] with disabled media
                List<string> disabled = new();
                foreach (LibraryFile file in libraryFiles)
                {
                    if (!file.Enabled)
                    {
                        if (!disabled.Contains(file.Path))
                            disabled.Add(file.Path);
                    }
                }
                SaveData.saveValues["DisabledMedia"] = JsonConvert.SerializeObject(disabled);
                SaveData.Save();
            }
            catch (Exception e)
            {
                ConsoleOutput.WriteLine("Error loading library files: " + e.Message, Color.Red);
            }
        }
        public static LibraryFile? Load(LibraryFile file)
        {
            // Import the library file by copying it.
            if(file.Type == null)
            {
                ConsoleOutput.WriteLine("Cannot import library file: type is null.", Color.Red);
                return null;
            }
            string newpath = Path.Combine(libraryRootPath, libraryPaths[file.Type]);
            if (!Directory.Exists(newpath))
                Directory.CreateDirectory(newpath);
            string newfile = Path.Combine(newpath, file.Nickname + Path.GetExtension(file.Path));
            if(file.Path == null)
            {
                ConsoleOutput.WriteLine("Cannot import library file: path is null.", Color.Red);
                return null;
            }
            // Check to make sure the file isn't already in the library.
            foreach(LibraryFile libfile in libraryFiles)
            {
                if(libfile.Path == newfile)
                {
                    // Make sure they're not the *exact* same file.
                    if (!File.ReadAllBytes(libfile.Path).SequenceEqual(File.ReadAllBytes(file.Path)))
                    {
                        ConsoleOutput.WriteLine("Deleting existing library file", Color.Yellow);
                        File.Delete(libfile.Path);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            try
            {
                File.Copy(file.Path, newfile);
            }
            catch(Exception e)
            {
            }
            file.Path = newfile;
            libraryFiles.Add(file);
            if(file.Type != DefaultLibraryTypes.Render)
            {
                string achievement = "ACHIEVEMENT_LIBRARY_IMPORT";
                ConsoleOutput.WriteLine("Awarding achievement: "+achievement, Color.LightBlue);
                SteamUserStats.SetAchievement(achievement);
                SaveData.saveValues["TotalMediaImported"] = (int.Parse(SaveData.saveValues["TotalMediaImported"], System.Globalization.CultureInfo.InvariantCulture) + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
                SaveData.Save();
            }
            SequentialName();
            return file;
        }
        public static void SequentialName()
        {
            string pa = Path.Combine(libraryRootPath, libraryPaths[DefaultLibraryTypes.Render]);
            // does the directory exist?
            if (!Directory.Exists(pa))
                Directory.CreateDirectory(pa);
            // Set Global.videoTitle to next sequential render name
            string[] files = Directory.GetFiles(pa);
            int max = 0;
            foreach(string f in files)
            {
                string name = Path.GetFileNameWithoutExtension(f);
                if (name.StartsWith("Render"))
                {
                    int num = 0;
                    if (int.TryParse(name.Substring(6), out num))
                        max = Math.Max(max, num);
                }
            }
            Global.videoTitle = "Render" + (max + 1);
        }
        public static void Unload(LibraryFile file)
        {
            // Delete the library file.
            if(file.Path == null)
            {
                ConsoleOutput.WriteLine("Cannot delete library file: path is null.", Color.Red);
                return;
            }
            try
            {
                // if disabled and in library, remove from SaveData.saveValues["DisabledMedia"]
                // a string-stored json object
                // that contains a list of disabled media paths.
                if (SaveData.saveValues["DisabledMedia"] != "")
                {
                    List<string> disabled = JsonConvert.DeserializeObject<List<string>>(SaveData.saveValues["DisabledMedia"]);
                    if (disabled.Contains(file.Path))
                        disabled.Remove(file.Path);
                    SaveData.saveValues["DisabledMedia"] = JsonConvert.SerializeObject(disabled);
                    SaveData.Save();
                }
                File.Delete(file.Path);
                //if(FileOperationAPIWrapper.Send(file.Path))
                    //libraryFiles.Remove(file);
            }
            catch (Exception e)
            {
                ConsoleOutput.WriteLine("Failed to delete library file: " + e.Message, Color.Red);
            }
        }
        public static void SetEnabled(LibraryFile file, bool enabled)
        {
            // Set whether or not the file is enabled in libraryFiles
            for(int i = 0; i < libraryFiles.Count; i++)
            {
                if(libraryFiles[i] == file)
                {
                    // Move to disabled/ if it's being disabled.
                    if(enabled)
                    {
                        if(file.Path == null)
                        {
                            ConsoleOutput.WriteLine("Cannot enable library file: path is null.", Color.Red);
                            return;
                        }
                        // Enable: Remove from SaveData.saveValues["DisabledMedia"]
                        // a string-stored json object
                        // that contains a list of disabled media paths.
                        if (SaveData.saveValues["DisabledMedia"] != "")
                        {
                            List<string> disabled = JsonConvert.DeserializeObject<List<string>>(SaveData.saveValues["DisabledMedia"]);
                            if (disabled.Contains(file.Path))
                                disabled.Remove(file.Path);
                            SaveData.saveValues["DisabledMedia"] = JsonConvert.SerializeObject(disabled);
                            SaveData.Save();
                        }
                    }
                    else
                    {
                        if(file.Path == null)
                        {
                            ConsoleOutput.WriteLine("Cannot disable library file: path is null.", Color.Red);
                            return;
                        }
                        // Disable: Add to SaveData.saveValues["DisabledMedia"]
                        // a string-stored json object
                        // that contains a list of disabled media paths.
                        if (SaveData.saveValues["DisabledMedia"] != "")
                        {
                            List<string> disabled = JsonConvert.DeserializeObject<List<string>>(SaveData.saveValues["DisabledMedia"]);
                            if (!disabled.Contains(file.Path))
                                disabled.Add(file.Path);
                            SaveData.saveValues["DisabledMedia"] = JsonConvert.SerializeObject(disabled);
                            SaveData.Save();
                        }
                    }
                    libraryFiles[i] = file;
                    libraryFiles[i].Enabled = enabled;
                    return;
                }
            }
        }
        public static string PickRandom(LibraryType type, Random rnd)
        {
            // Pick a random file from the library.
            List<LibraryFile> files = libraryFiles.FindAll(x => x.Type == type && x.Path != null && File.Exists(x.Path) && x.Enabled);
            if (files.Count == 0)
                return "";
            int index = rnd.Next(files.Count);
            string? path = files[index].Path;
            if(path != null)
                return path;
            return "";
        }
        public static List<LibraryFile> GetFiles(LibraryType type)
        {
            // Get all files of a certain type.
            return libraryFiles.FindAll(x => x.Type == type && x.Path != null && File.Exists(x.Path) && x.Enabled);
        }
        public static int GetFileCount(LibraryType type)
        {
            // Get the number of files of a certain type.
            return GetFiles(type).Count;
        }
        public static List<string> GetLibraryNames(LibraryRootType type)
        {
            List<string> names = new();
            foreach(KeyValuePair<LibraryType, string> pair in libraryNames)
            {
                if(pair.Key.Special)
                    continue;
                if(pair.Key.RootType == type || type == LibraryRootType.All)
                {
                    names.Add(pair.Value);
                }
            }
            return names;
        }
        public static LibraryFile Organize(LibraryFile source, LibraryType newType)
        {
            // Move the file to the new type's folder and re-import it.
            if(source.Type == null)
            {
                ConsoleOutput.WriteLine("Cannot organize library file: type is null.", Color.Red);
                return source;
            }
            if(source.Path == null)
            {
                ConsoleOutput.WriteLine("Cannot organize library file: path is null.", Color.Red);
                return source;
            }
            string newpath = Path.Combine(libraryRootPath, libraryPaths[newType]);
            string finalpath = Path.Combine(newpath, Path.GetFileName(source.Path));
            try
            {
                File.Move(source.Path, finalpath);
            }
            catch
            {
                // It probably already exists, so try to delete it.
                try
                {
                    libraryFiles.Remove(source);
                    File.Delete(finalpath);
                    File.Move(source.Path, finalpath);
                }
                catch (Exception e)
                {
                    ConsoleOutput.WriteLine("Failed to move library file: " + e.Message, Color.Red);
                    return source;
                }
            }
            // File is now in the new folder, so we can re-import it.
            bool removed = libraryFiles.Remove(source);
            source.Path = finalpath;
            source.Type = newType;
            if(removed)
            {
                libraryFiles.Add(source);
                SequentialName();
            }
            else
            {
                ConsoleOutput.WriteLine("Organized file was not found, so it was not re-added.", Color.Yellow);
            }
            return source;
        }
        // Download thread
        private static BackgroundWorker downloadWorker;
        public static List<string> downloadUrls = new();
        private static LibraryType downloadType;
        private static List<bool> youtubes = new();
        private static void DownloadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            for(int i = 0; i < downloadUrls.Count; i++)
            {
                string downloadUrl = downloadUrls[i];
                bool youtube = youtubes[i];
                ConsoleOutput.WriteLine("Downloading clip from " + downloadUrl + "...", Color.Yellow);
                // Download the clip.
                string filename = Path.GetFileName(downloadUrl);
                string path = Path.Combine(libraryRootPath, libraryPaths[downloadType], filename);
                try
                {
                    if(!Directory.Exists(Path.GetDirectoryName(path)))
                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                    if(File.Exists(path))
                        File.Delete(path); // Delete the file if it already exists.
                    if(!youtube)
                    {
                        using (WebClient client = new WebClient())
                        {
                            client.DownloadFile(downloadUrl, path);
                        }
                    }
                    else if(UpdateManager.ytDlpInstalled)
                    {
                        path = Path.Combine(libraryRootPath, libraryPaths[downloadType], "%(title)s.%(ext)s");
                        string ytdlp = Global.useSystemYtDlp ? "yt-dlp" : @".\yt-dlp.exe";
                        bool sound = downloadType.RootType == LibraryRootType.Audio;
                        ProcessStartInfo startInfo = new ProcessStartInfo(ytdlp, "-o \"" + path + "\" " + (sound ? "--extract-audio --audio-format mp3 " : "--format mp4 ") + downloadUrl);
                        startInfo.UseShellExecute = false;
                        startInfo.RedirectStandardOutput = true;
                        startInfo.RedirectStandardError = true;
                        Process process = new Process();
                        process.StartInfo = startInfo;
                        process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                        {
                            ConsoleOutput.WriteLine(e.Data, Color.Transparent);
                        };
                        process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                        {
                            ConsoleOutput.WriteLine(e.Data, Color.Red);
                        };
                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        process.WaitForExit();
                    }
                    else
                    {
                        ConsoleOutput.WriteLine("Failed to download clip: yt-dlp is not installed.", Color.Red);
#if MONOGAME
                        LibraryPage.Done(false);
#endif
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    ConsoleOutput.WriteLine("Failed to download clip: " + ex.Message, Color.Red);
#if MONOGAME
                    LibraryPage.Done(false);
#endif
                    continue;
                }
                // Add it to the library.
                LibraryFile file = new(Path.GetFileNameWithoutExtension(path), path, downloadType);
                // Run callback
#if MONOGAME
                LibraryPage.Done(true);
#endif
                libraryFiles.Add(file);
                Global.justCompletedRender = true; // Refresh the library.
                ConsoleOutput.WriteLine("Downloaded clip to library: " + path, Color.Green);
            }
            downloadUrls.Clear();
        }
        internal static bool DownloadClip(string[] clipUrls, LibraryType key)
        {
            foreach(string clipUrl in clipUrls)
            {
                // Download a clip from a URL and add it to the library.
                string filename = Path.GetFileName(clipUrl);
                string path = Path.Combine(libraryRootPath, libraryPaths[key], filename);
                // Does it end in a file extension?
                if (!filename.Contains("."))
                {
                    // If not, is it a YouTube url?
                    if (!clipUrl.Contains("youtube.com") && !clipUrl.Contains("youtu.be"))
                    {
                        // Error: We don't know what to do with this.
                        ConsoleOutput.WriteLine("Failed to download clip: URL is unknown.", Color.Red);
                        return false;
                    }
                    // It's a YouTube url.
                    youtubes.Add(true);
                }
                else
                {
                    // check against file extensions in libraryFileTypes[DefaultLibraryTypes.All]
                    bool found = false;
                    foreach(string filetype in libraryFileTypes[key])
                    {
                        if (filename.EndsWith(filetype))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        // Error: We don't know what to do with this.
                        ConsoleOutput.WriteLine("Failed to download clip: Invalid file extension.", Color.Red);
                        return false;
                    }
                    youtubes.Add(false);
                }
                downloadUrls.Add(clipUrl);
            }
            try
            {
                // Start download thread
                downloadType = key;
                if(downloadWorker == null)
                {
                    downloadWorker = new BackgroundWorker();
                    downloadWorker.DoWork += DownloadWorker_DoWork;
                }
                downloadWorker.RunWorkerAsync();
                SequentialName();
                if(key != DefaultLibraryTypes.Render)
                {
                    string achievement = "ACHIEVEMENT_LIBRARY_IMPORT";
                    ConsoleOutput.WriteLine("Awarding achievement: "+achievement, Color.LightBlue);
                    SteamUserStats.SetAchievement(achievement);
                    SaveData.saveValues["TotalMediaImported"] = (int.Parse(SaveData.saveValues["TotalMediaImported"], System.Globalization.CultureInfo.InvariantCulture) + 1).ToString();
                    SaveData.Save();
                }
                return true;
            }
            catch (Exception e)
            {
                ConsoleOutput.WriteLine("Failed to download clip: " + e.Message, Color.Red);
                return false;
            }
        }
    }
}
