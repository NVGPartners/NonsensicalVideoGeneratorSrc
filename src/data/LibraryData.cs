using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Net;
using System.Diagnostics;

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
        /// Rendered videos.
        /// </summary>
        Render,
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
        /// Images.
        /// </summary>
        Image,
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
        public static LibraryType Music { get; } = new LibraryType(LibraryRootType.Audio, LibraryFileType.Music, "Random dance music. Also used for images.");
        public static LibraryType Render { get; } = new LibraryType(LibraryRootType.Video, LibraryFileType.Render, "Generated videos.");
        public static LibraryType Material { get; } = new LibraryType(LibraryRootType.Video, LibraryFileType.Material, "Root video to be used in a render.");
        public static LibraryType Transition { get; } = new LibraryType(LibraryRootType.Video, LibraryFileType.Transition, "Played in full at random points.");
        public static LibraryType Intro { get; } = new LibraryType(LibraryRootType.Video, LibraryFileType.Intro, "Played at the start of the video.");
        public static LibraryType Outro { get; } = new LibraryType(LibraryRootType.Video, LibraryFileType.Outro, "Played at the end of the video.");
        public static LibraryType Overlay { get; } = new LibraryType(LibraryRootType.Video, LibraryFileType.Overlay, "Requires pure green chroma key.");
        public static LibraryType Image { get; } = new LibraryType(LibraryRootType.Video, LibraryFileType.Image, "Zooms in (non-gif only) while playing music.");
        public static List<LibraryType> AllTypes { get; } = new List<LibraryType>()
        {
            All,
            Video,
            Audio,
            SFX,
            Music,
            Render,
            Material,
            Transition,
            Intro,
            Outro,
            Overlay,
            Image,
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
            { DefaultLibraryTypes.Render, @"video\renders" },
            { DefaultLibraryTypes.Material, @"video\materials" },
            { DefaultLibraryTypes.Transition, @"video\transitions" },
            { DefaultLibraryTypes.Intro, @"video\intros" },
            { DefaultLibraryTypes.Outro, @"video\outros" },
            { DefaultLibraryTypes.Overlay, @"video\overlays" },
            { DefaultLibraryTypes.Image, @"video\images" },
        };
        public static Dictionary<LibraryType, string[]> libraryFileTypes { get; } = new Dictionary<LibraryType, string[]>()
        {
            { DefaultLibraryTypes.All, new string[] { ".mp4", ".webm", ".mov", ".avi", ".mkv", ".wmv", ".wav", ".mp3", ".ogg", ".m4a", ".flac" } },
            { DefaultLibraryTypes.Video, new string[] { ".mp4", ".webm", ".mov", ".avi", ".mkv", ".wmv" } },
            { DefaultLibraryTypes.Audio, new string[] { ".wav", ".mp3", ".ogg", ".m4a", ".flac" } },
            { DefaultLibraryTypes.SFX, new string[] { ".wav", ".mp3", ".ogg", ".m4a", ".flac" } },
            { DefaultLibraryTypes.Music, new string[] { ".wav", ".mp3", ".ogg", ".m4a", ".flac" } },
            { DefaultLibraryTypes.Render, new string[] { ".mp4", ".webm", ".mov", ".avi", ".mkv", ".wmv" } },
            { DefaultLibraryTypes.Material, new string[] { ".mp4", ".webm", ".mov", ".avi", ".mkv", ".wmv" } },
            { DefaultLibraryTypes.Transition, new string[] { ".mp4", ".webm", ".mov", ".avi", ".mkv", ".wmv" } },
            { DefaultLibraryTypes.Intro, new string[] { ".mp4", ".webm", ".mov", ".avi", ".mkv", ".wmv" } },
            { DefaultLibraryTypes.Outro, new string[] { ".mp4", ".webm", ".mov", ".avi", ".mkv", ".wmv" } },
            { DefaultLibraryTypes.Overlay, new string[] { ".mp4", ".webm", ".mov", ".avi", ".mkv", ".wmv" } },
            { DefaultLibraryTypes.Image, new string[] { ".png", ".jpg", ".jpeg", ".gif" } },
        };
        public static Dictionary<LibraryType, string> libraryNames { get; } = new Dictionary<LibraryType, string>()
        {
            { DefaultLibraryTypes.All, "All" },
            { DefaultLibraryTypes.Video, "Video" },
            { DefaultLibraryTypes.Audio, "Audio" },
            { DefaultLibraryTypes.SFX, "Sound FX" },
            { DefaultLibraryTypes.Music, "Music" },
            { DefaultLibraryTypes.Render, "Renders" },
            { DefaultLibraryTypes.Material, "Materials" },
            { DefaultLibraryTypes.Transition, "Transitions" },
            { DefaultLibraryTypes.Intro, "Intros" },
            { DefaultLibraryTypes.Outro, "Outros" },
            { DefaultLibraryTypes.Overlay, "Overlays" },
        };
        private static void LoadRecursive(string path, LibraryType type)
        {
            foreach (string file in Directory.GetFiles(path))
            {
                foreach (string filetype in libraryFileTypes[type])
                {
                    if (file.EndsWith(filetype))
                    {
                        LibraryFile libFile = new LibraryFile(Path.GetFileNameWithoutExtension(file), file, type);
                        // If path contains .disabled, move to disabled folder (update version).
                        if (libFile.Path.Contains(".disabled"))
                        {
                            string newName = Path.GetFileName(libFile.Path).Replace(".disabled", "");
                            string newPath = Path.Combine(Path.GetDirectoryName(libFile.Path), "disabled", newName);
                            File.Move(libFile.Path, newPath);
                            libFile.Path = newPath;
                        }
                        // If path is in disabled folder, disable it.
                        if (libFile.Path.Contains(@"\disabled\"))
                            libFile.Enabled = false;
                        libraryFiles.Add(libFile);
                        break;
                    }
                }
            }
            foreach (string dir in Directory.GetDirectories(path))
                LoadRecursive(dir, type);
        }
        public static void Load()
        {
            try
            {
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
                    ConsoleOutput.WriteLine("Deleting existing library file", Color.Yellow);
                    File.Delete(libfile.Path);
                }
            }
            try
            {
                File.Copy(file.Path, newfile);
            }
            catch (Exception e)
            {
                ConsoleOutput.WriteLine("Failed to copy library file: " + e.Message, Color.Red);
                return null;
            }
            file.Path = newfile;
            libraryFiles.Add(file);
            return file;
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
                        // Get path before /disabled/
                        string rootPath = file.Path.Substring(0, file.Path.IndexOf(@"\disabled\"));
                        string newpath = Path.Combine(rootPath, Path.GetFileName(file.Path));
                        // If a file already exists in the root path, delete it.
                        if (File.Exists(newpath))
                            File.Delete(newpath);
                        try
                        {
                            File.Move(file.Path, newpath);
                        }
                        catch (Exception e)
                        {
                            ConsoleOutput.WriteLine("Failed to enable library file: " + e.Message, Color.Red);
                            return;
                        }
                        file.Path = newpath;
                    }
                    else
                    {
                        if(file.Path == null)
                        {
                            ConsoleOutput.WriteLine("Cannot disable library file: path is null.", Color.Red);
                            return;
                        }
                        // Move to disabled/
                        string newpath = Path.Combine(Path.GetDirectoryName(file.Path), @"disabled\" + Path.GetFileName(file.Path));
                        // Create disabled/ if it doesn't exist.
                        if (!Directory.Exists(Path.GetDirectoryName(newpath)))
                            Directory.CreateDirectory(Path.GetDirectoryName(newpath));
                        // If a file already exists in disabled/, delete it.
                        if (File.Exists(newpath))
                            File.Delete(newpath);
                        try
                        {
                            File.Move(file.Path, newpath);
                        }
                        catch (Exception e)
                        {
                            ConsoleOutput.WriteLine("Failed to disable library file: " + e.Message, Color.Red);
                            return;
                        }
                        file.Path = newpath;
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
            return libraryFiles.FindAll(x => x.Type == type);
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
                libraryFiles.Add(source);
            else
                ConsoleOutput.WriteLine("Organized file was not found, so it was not re-added.", Color.Yellow);
            return source;
        }
        // Download thread
        private static BackgroundWorker downloadWorker;
        private static string downloadUrl = "";
        private static LibraryType downloadType;
        private static bool youtube = false;
        private static Action<bool> downloadCallback = (bool success) => { };
        private static void DownloadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
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
                    string ytdlp = Global.useSystemYtDlp ? "yt-dlp" : @".\bin\yt-dlp.exe";
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
                    downloadCallback(false);
                    return;
                }
            }
            catch (Exception ex)
            {
                ConsoleOutput.WriteLine("Failed to download clip: " + ex.Message, Color.Red);
                downloadCallback(false);
                return;
            }
            // Add it to the library.
            LibraryFile file = new(Path.GetFileNameWithoutExtension(path), path, downloadType);
            // Run callback
            downloadCallback(true);
            libraryFiles.Add(file);
            Global.justCompletedRender = true; // Refresh the library.
            ConsoleOutput.WriteLine("Downloaded clip to library: " + path, Color.Green);
        }
        internal static bool DownloadClip(string clipUrl, LibraryType key, Action<bool> callback)
        {
            // Download a clip from a URL and add it to the library.
            string filename = Path.GetFileName(clipUrl);
            string path = Path.Combine(libraryRootPath, libraryPaths[key], filename);
            try
            {
                // Does it end in a file extension?
                if (!filename.Contains("."))
                {
                    // If not, is it a YouTube url?
                    if (!clipUrl.Contains("youtube.com") || clipUrl.Contains("youtu.be"))
                    {
                        // Error: We don't know what to do with this.
                        ConsoleOutput.WriteLine("Failed to download clip: URL is unknown.", Color.Red);
                        return false;
                    }
                    // It's a YouTube url.
                    youtube = true;
                }
                // Start download thread
                downloadUrl = clipUrl;
                downloadType = key;
                downloadCallback = callback;
                if(downloadWorker == null)
                {
                    downloadWorker = new BackgroundWorker();
                    downloadWorker.DoWork += DownloadWorker_DoWork;
                }
                downloadWorker.RunWorkerAsync();
                ConsoleOutput.WriteLine("Downloading clip from " + clipUrl + "...", Color.Yellow);
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
