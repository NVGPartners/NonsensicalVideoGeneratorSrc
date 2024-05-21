using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;
using System.Diagnostics;
using System.Windows.Forms;

#if MONOGAME
using Microsoft.Xna.Framework;
#else
using System.Drawing;
#endif

namespace NonsensicalVideoGenerator
{    
    /// <summary>
    /// Automatic update checking.
    /// </summary>
    public static class UpdateManager
    {
        public static string updateUrl = "";
        public static string updateTag = "";
        public static bool ffmpegInstalled = false;
        public static bool ffprobeInstalled = false;
        public static bool imagemagickInstalled = false;
        public static bool ytDlpInstalled = false;
        public static bool frei0rInstalled = false;
        public static bool DoesCommandExist(string command)
        {
            // skip if needed and return false
            if(Global.parameters.Contains("-nopath") || Global.parameters.Contains("-noenv"))
                return false;
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
            return exists;
        }
        public static bool DoesEnvironmentVariableExist(string variable)
        {
            // skip if needed and return false
            if(Global.parameters.Contains("-noenv"))
                return false;
            string? value = Environment.GetEnvironmentVariable(variable);
            return value != null;
        }
        public static void GetDependencyStatus()
        {
            // Test for dependencies.
            ConsoleOutput.WriteLine("Checking for dependencies...", Color.Magenta);
            bool[] status = new bool[5];
            // Check if .\ffmpeg.exe and .\ffprobe.exe exist.
            status[0] = File.Exists(@".\ffmpeg.exe");
            status[1] = File.Exists(@".\ffprobe.exe");
            status[2] = File.Exists(@".\magick.exe");
            status[3] = File.Exists(@".\yt-dlp.exe");
            status[4] = Directory.Exists(@".\frei0r-1");
            // If these don't exist, set Global.useSystemFFmpeg to true
            // so that the program will use the system ffmpeg and ffprobe.
            if (!status[0])
            {
                Global.useSystemFFmpeg = true;
            }
            else
            {
                Global.useSystemFFmpeg = false;
            }
            if (!status[1])
            {
                Global.useSystemFFprobe = true;
            }
            else
            {
                Global.useSystemFFprobe = false;
            }
            if (!status[2])
            {
                Global.useSystemMagick = true;
            }
            else
            {
                Global.useSystemMagick = false;
            }
            if(!status[3])
            {
                Global.useSystemYtDlp = true;
            }
            else
            {
                Global.useSystemYtDlp = false;
            }
            if(!status[4])
            {
                Global.useSystemFrei0r = true;
            }
            else
            {
                Global.useSystemFrei0r = false;
            }
            if(!Global.useSystemFFmpeg)
            {
                ffmpegInstalled = status[0];
            }
            else
            {
                ffmpegInstalled = DoesCommandExist("ffmpeg");
            }
            if(!Global.useSystemFFprobe)
            {
                ffprobeInstalled = status[1];
            }
            else
            {
                ffprobeInstalled = DoesCommandExist("ffprobe");
            }
            if(!Global.useSystemMagick)
            {
                imagemagickInstalled = status[2];
            }
            else
            {
                imagemagickInstalled = DoesCommandExist("magick");
            }
            if(!Global.useSystemYtDlp)
            {
                ytDlpInstalled = status[3];
            }
            else
            {
                ytDlpInstalled = DoesCommandExist("yt-dlp");
            }
            if(!Global.useSystemFrei0r)
            {
                frei0rInstalled = status[4];
            }
            else
            {
                frei0rInstalled = DoesEnvironmentVariableExist("FREI0R_PATH");
            }
        }
        public static void DownloadUpdate()
        {
            if (updateUrl == "")
            {
                ConsoleOutput.WriteLine("No update URL.", Color.Magenta);
                return;
            }
            try
            {
                ConsoleOutput.WriteLine("Downloading update...", Color.Magenta);
                // Download update.
                HttpClient client = new();
                byte[] data = client.GetByteArrayAsync(updateUrl).Result;
                // Save update.
                string[]? version = Global.productVersion?.Split('.');
                if (version != null)
                {
                    string fileName = "v" + version[0] + version[1] + version[2] + ".zip";
                    File.WriteAllBytes(fileName, data);
                    // Unzip update to a subfolder.
                    string? path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    if (path != null)
                    {
                        ConsoleOutput.WriteLine("Unzipping update...", Color.Magenta);
                        string updatePath = Path.Combine(path, "update");
                        if (!Directory.Exists(updatePath))
                        {
                            Directory.CreateDirectory(updatePath);
                        }
                        System.IO.Compression.ZipFile.ExtractToDirectory(fileName, updatePath);
                        ConsoleOutput.WriteLine("Update extracted. Applying update...", Color.Magenta);
                        // Create a batch script to move the update to the main folder.
                        // We can't do this directly because the program is still running.
                        List<string> batchScript = new()
                        {
                            "@echo off",
                            "title Update",
                            "echo Moving files...",
                            "robocopy update " + path + " /e /move /njh /njs /ndl /nc /ns /np",
                            "echo Deleting update folder...",
                            "del /f /s /q update",
                            "rmdir update /s /q",
                            "echo Deleting update archive...",
                            "del " + fileName,
                            "echo Update complete, starting...",
                            "start NonsensicalVideoGenerator.exe",
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
                        ConsoleOutput.WriteLine("Failed to obtain path.", Color.Red);
                        return;
                    }
                }
                else
                {
                    ConsoleOutput.WriteLine("Failed to obtain version.", Color.Red);
                    return;
                }
            }
            catch
            {
                ConsoleOutput.WriteLine("Failed to download update.", Color.Red);
            }
        }
    }
}