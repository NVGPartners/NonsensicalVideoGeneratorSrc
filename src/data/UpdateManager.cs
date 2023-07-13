using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;
using System.Diagnostics;
using System.Drawing;

namespace YTPPlusPlusPlus
{    
    /// <summary>
    /// Automatic update checking.
    /// </summary>
    public static class UpdateManager
    {
        // Query URL
        private static readonly string queryUrl = "https://api.github.com/repos/YTP-Plus/YTPPlusPlusPlus/releases/latest";
        public static string updateUrl = "";
        public static string updateTag = "";
        public static bool ffmpegInstalled = false;
        public static bool ffprobeInstalled = false;
        public static bool imagemagickInstalled = false;
        public static bool ytDlpInstalled = false;
        public static bool updateFailed = false;
        public static bool updateAvailable = false;
        public static bool checkedForUpdates = false;
        public static bool DoesCommandExist(string command)
        {
            string output = "";
            ProcessStartInfo startInfo = new()
            {
                FileName = "where",
                Arguments = command,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            Process process = new()
            {
                StartInfo = startInfo
            };
            process.OutputDataReceived += (sender, args) => output += args.Data;
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
            bool exists = output != "" && !output.Contains("Could not find");
            ConsoleOutput.WriteLine(command + (exists ? " found." : " not found."), exists ? Microsoft.Xna.Framework.Color.Magenta : Microsoft.Xna.Framework.Color.Red);
            return exists;
        }
        public static bool[] GetDependencyStatus()
        {
            // Test for dependencies.
            ConsoleOutput.WriteLine("Checking for dependencies...", Microsoft.Xna.Framework.Color.Magenta);
            bool[] status = new bool[4];
            imagemagickInstalled = DoesCommandExist("magick");
            // Check if .\bin\ffmpeg.exe and .\bin\ffprobe.exe exist.
            status[0] = File.Exists(@".\bin\ffmpeg.exe");
            status[1] = File.Exists(@".\bin\ffprobe.exe");
            status[3] = File.Exists(@".\bin\yt-dlp.exe");
            // If these don't exist, set Global.useSystemFFmpeg to true
            // so that the program will use the system ffmpeg and ffprobe.
            if (!status[0] || !status[1])
            {
                Global.useSystemFFmpeg = true;
            }
            if(!status[3])
            {
                Global.useSystemYtDlp = true;
            }
            if(!Global.useSystemFFmpeg)
            {
                ffmpegInstalled =  status[0];
                ffprobeInstalled = status[1];
            }
            else
            {
                ffmpegInstalled = DoesCommandExist("ffmpeg");
                ffprobeInstalled = DoesCommandExist("ffprobe");
            }
            if(!Global.useSystemYtDlp)
            {
                ytDlpInstalled = status[3];
            }
            else
            {
                ytDlpInstalled = DoesCommandExist("yt-dlp");
            }
            return status;
        }
        public static bool CheckForUpdates()
        {
            return false;
            try
            {
                updateFailed = false;
                // Get latest release info.
                HttpClient client = new();
                client.DefaultRequestHeaders.Add("User-Agent", "YTPPlusPlusPlus");
                string json = client.GetStringAsync(queryUrl).Result;
                // Parse JSON.
                dynamic? data = JsonConvert.DeserializeObject(json);
                string? latestVersion = data?.tag_name;
                checkedForUpdates = true;
                // Compare versions.
                if (latestVersion != "v" + Global.productVersion)
                {
                    // Parse major, minor, and patch versions.
                    string[]? currentVersion = Global.productVersion?.Split('.');
                    string[]? latestVersionSplit = latestVersion?.Replace("v", "")?.Split('.');
                    int[] currentVersionInt = new int[3];
                    int[] latestVersionInt = new int[3];
                    for (int i = 0; i < 3; i++)
                    {
                        currentVersionInt[i] = int.Parse(currentVersion?[i] ?? "0");
                        latestVersionInt[i] = int.Parse(latestVersionSplit?[i] ?? "0");
                    }
                    // Compare versions.
                    bool update = latestVersionInt[0] > currentVersionInt[0] ||
                                  latestVersionInt[1] > currentVersionInt[1] ||
                                  latestVersionInt[2] > currentVersionInt[2];
                    if (!update)
                    {
                        ConsoleOutput.WriteLine("No updates available.", Microsoft.Xna.Framework.Color.Magenta);
                        return false;
                    }
                    // Get download URL.
                    string? assetUrl = data?.assets[0].browser_download_url;
                    if (assetUrl != null)
                    {
                        updateUrl = assetUrl;
                    }
                    if(latestVersion != null)
                    {
                        updateTag = latestVersion;
                    }
                    ConsoleOutput.WriteLine("Update available: " + latestVersion, Microsoft.Xna.Framework.Color.Magenta);
                    updateAvailable = true;
                    return true;
                }
                else
                {
                    ConsoleOutput.WriteLine("No updates available.", Microsoft.Xna.Framework.Color.Magenta);
                    return false;
                }
            }
            catch(Exception e)
            {
                ConsoleOutput.WriteLine("Failed to check for updates: " + e.Message, Microsoft.Xna.Framework.Color.Red);
                updateFailed = true;
                return false;
            }
        }
        public static void DownloadUpdate()
        {
            if (updateUrl == "")
            {
                ConsoleOutput.WriteLine("No update URL.", Microsoft.Xna.Framework.Color.Magenta);
                return;
            }
            try
            {
                ConsoleOutput.WriteLine("Downloading update...", Microsoft.Xna.Framework.Color.Magenta);
                // Download update.
                HttpClient client = new();
                byte[] data = client.GetByteArrayAsync(updateUrl).Result;
                // Save update.
                string[]? version = Global.productVersion?.Split('.');
                if (version != null)
                {
                    string fileName = "ytpplusplusplus" + version[0] + version[1] + version[2] + ".zip";
                    File.WriteAllBytes(fileName, data);
                    // Unzip update to a subfolder.
                    string? path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    if (path != null)
                    {
                        ConsoleOutput.WriteLine("Unzipping update...", Microsoft.Xna.Framework.Color.Magenta);
                        string updatePath = Path.Combine(path, "update");
                        if (!Directory.Exists(updatePath))
                        {
                            Directory.CreateDirectory(updatePath);
                        }
                        System.IO.Compression.ZipFile.ExtractToDirectory(fileName, updatePath);
                        ConsoleOutput.WriteLine("Update extracted. Applying update...", Microsoft.Xna.Framework.Color.Magenta);
                        // Create a batch script to move the update to the main folder.
                        // We can't do this directly because the program is still running.
                        List<string> batchScript = new()
                        {
                            "@echo off",
                            "title YTP+++ Update",
                            "echo Moving files...",
                            "robocopy update " + path + " /e /move /njh /njs /ndl /nc /ns /np",
                            "echo Deleting update folder...",
                            "del /f /s /q update",
                            "rmdir update /s /q",
                            "echo Deleting update archive...",
                            "del " + fileName,
                            "echo Update complete, starting YTP+++...",
                            "start YTP+++.exe",
                            "exit"
                        };
                        // Save the batch script.
                        File.WriteAllText("update.bat", string.Join(Environment.NewLine, batchScript));
                        // Run the batch script asynchronously.
                        ProcessStartInfo startInfo = new()
                        {
                            FileName = "update.bat",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                        Process.Start(startInfo);
                        // Exit the program.
                        Environment.Exit(0);                        
                    }
                    else
                    {
                        ConsoleOutput.WriteLine("Failed to obtain path.", Microsoft.Xna.Framework.Color.Red);
                        return;
                    }
                }
                else
                {
                    ConsoleOutput.WriteLine("Failed to obtain version.", Microsoft.Xna.Framework.Color.Red);
                    return;
                }
            }
            catch
            {
                ConsoleOutput.WriteLine("Failed to download update.", Microsoft.Xna.Framework.Color.Red);
            }
        }
    }
}