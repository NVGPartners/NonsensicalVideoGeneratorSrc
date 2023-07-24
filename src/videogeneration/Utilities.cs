using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
#if MONOGAME
using Microsoft.Xna.Framework;
#else
using System.Drawing;
#endif

namespace NonsensicalVideoGenerator
{
    public static class Utilities
    {
        public static string temporaryDirectory = @".\temp";
        public static string GetLength(string file)
        {
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = Global.useSystemFFprobe ? "ffprobe" : @".\ffprobe.exe";
                startInfo.Arguments = "-i \"" + file
                        + "\" -show_entries format=duration"
                        + " -v quiet"
                        + " -of csv=\"p=0\"";
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                startInfo.CreateNoWindow = true;
                process.StartInfo = startInfo;
                string s = "";
                process.OutputDataReceived += (sender, e) =>
                {
                    s += e.Data;
                    if (e.Data != null)
                        ConsoleOutput.WriteLine(e.Data, Color.Transparent);
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                        ConsoleOutput.WriteLine(e.Data, Color.Transparent);
                };
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                return s;

            }
            catch(Exception ex)
            {
                ConsoleOutput.WriteLine(ex.Message);
                ConsoleOutput.WriteLine("Fatal error while getting length of video.", Color.Red);
                Global.generatorFactory.failureReason = "Fatal error while getting length of video.";
                Global.generatorFactory.progressText = Global.generatorFactory.failureReason;
                Global.generatorFactory.CancelGeneration();
                return "0";
            }
        }
        public static void SnipVideo(string video, double startTime, double endTime, string output)
        {
            SnipVideo(video, startTime.ToString(CultureInfo.InvariantCulture), endTime.ToString(CultureInfo.InvariantCulture), output);
        }
        public static void SnipVideo(string video, string startTime, string endTime, string output)
        {
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = Global.useSystemFFmpeg ? "ffmpeg" : @".\ffmpeg.exe";
                startInfo.Arguments = "-i \"" + video
                        + "\" -ss " + startTime
                        + " -to " + endTime
                        + " -c:v libx264"
                        + " -crf 18"
                        + " -preset veryfast"
                        + " -vf scale=" + SaveData.saveValues["VideoWidth"] + "x" + SaveData.saveValues["VideoHeight"] + ",setsar=1:1,fps=fps=30"
                        + " -y"
                        + " \"" + output + "\"";
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardError = true;
                startInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                startInfo.CreateNoWindow = true;
                process.StartInfo = startInfo;
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                        ConsoleOutput.WriteLine(e.Data, Color.Transparent);
                };
                process.Start();
                process.BeginErrorReadLine();
                process.WaitForExit();

                if (process.HasExited && process.ExitCode == 1)
                {
                    //ConsoleOutput.WriteLine("ERROR");
                }
            }
            catch(Exception ex)
            {
                ConsoleOutput.WriteLine(ex.Message);
                ConsoleOutput.WriteLine("Fatal error while snipping video.", Color.Red);
                Global.generatorFactory.failureReason = "Fatal error while snipping video.";
                Global.generatorFactory.progressText = Global.generatorFactory.failureReason;
                Global.generatorFactory.CancelGeneration();
            }
        }

        // Check if a video has audio
        public static bool HasAudio(string video)
        {
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = Global.useSystemFFprobe ? "ffprobe" : @".\ffprobe.exe";
                startInfo.Arguments = "-i \"" + video
                        + "\" -show_streams"
                        + " -v quiet"
                        + " -of csv=\"p=0\"";
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                startInfo.CreateNoWindow = true;
                process.StartInfo = startInfo;
                string s = "";
                process.OutputDataReceived += (sender, e) =>
                {
                    s += e.Data;
                    if (e.Data != null)
                        ConsoleOutput.WriteLine(e.Data, Color.Transparent);
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                        ConsoleOutput.WriteLine(e.Data, Color.Transparent);
                };
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                return s.Contains("Audio");

            }
            catch(Exception ex)
            {
                ConsoleOutput.WriteLine(ex.Message);
                ConsoleOutput.WriteLine("Fatal error while checking if video has audio.", Color.Red);
                Global.generatorFactory.failureReason = "Fatal error while checking if video has audio.";
                Global.generatorFactory.progressText = Global.generatorFactory.failureReason;
                Global.generatorFactory.CancelGeneration();
                return false;
            }
        }

        public static void CopyVideo(string video, string output)
        {
            try
            {
                // If the video has no audio, add silence to it
                bool noAudio = HasAudio(video);
                string appendNoAudio = "";
                if(!noAudio)
                {
                    // Get video length
                    string length = GetLength(video);
                    appendNoAudio = "-f lavfi -i anullsrc=channel_layout=mono:sample_rate=44100 -t " + length + " ";
                }
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = Global.useSystemFFmpeg ? "ffmpeg" : @".\ffmpeg.exe";
                startInfo.Arguments = "-i \"" + video + "\" " + appendNoAudio
                        + " -c:v libx264"
                        + " -crf 18"
                        + " -preset veryfast"
                        + " -vf scale=" + SaveData.saveValues["VideoWidth"] + "x" + SaveData.saveValues["VideoHeight"] + ",setsar=1:1,fps=fps=30"
                        + " -y"
                        + " \"" + output + "\"";
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardError = true;
                startInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                startInfo.CreateNoWindow = true;
                process.StartInfo = startInfo;
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                        ConsoleOutput.WriteLine(e.Data, Color.Transparent);
                };
                process.Start();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }
            catch(Exception ex)
            {
                ConsoleOutput.WriteLine(ex.Message);
                ConsoleOutput.WriteLine("Fatal error while copying video.", Color.Red);
                Global.generatorFactory.failureReason = "Fatal error while copying video.";
                Global.generatorFactory.progressText = Global.generatorFactory.failureReason;
                Global.generatorFactory.CancelGeneration();
            }
        }
        public static void ConcatenateVideo(int count, string ou)
        {
            try
            {
                if (File.Exists(ou))
                    File.Delete(ou);
            }
            catch (Exception ex)
            {
                ConsoleOutput.WriteLine(ex.Message);
            }
            // try to continue anyways
            try
            {
                count++;
                // Add outro if enabled
                if (bool.Parse(SaveData.saveValues["OutrosEnabled"]))
                {
                    string outroPath = LibraryData.PickRandom(DefaultLibraryTypes.Outro, Global.generatorFactory.globalRandom);
                    if(outroPath == "")
                    {
                        ConsoleOutput.WriteLine("No outros found in library.", Color.Yellow);
                    }
                    else
                    {
                        if(!outroPath.Contains("defaultoutro.mp4"))
                            Global.usedDifferentOutro = true;
                        Global.generatorFactory.progressText = "Closing the film spool... (" + count + "/" + count + ")";
                        ConsoleOutput.WriteLine("Outro clip enabled, adding 1 to max clips. New max clips is " + count, Color.Gray);
                        Utilities.CopyVideo(outroPath, Path.Combine(Utilities.temporaryDirectory, "video" + count + ".mp4"));
                        count++;
                    }
                }

                string command1 = "";

                // Re-encode all files in temporary directory that start with video to video0, video1, etc.
                string[] files = Directory.GetFiles(temporaryDirectory);
                int i2 = 0;
                for (int i = 0; i < count; i++)
                {
                    if (File.Exists(Path.Combine(temporaryDirectory, "video" + i + ".mp4")))
                    {
                        ConsoleOutput.WriteLine("Re-encoding... (" + (i + 1) + "/" + count + ")", Color.Gray);
                        Global.generatorFactory.progressText = "Re-encoding... (" + (i + 1) + "/" + count + ")";
                        //File.Move(Path.Combine(temporaryDirectory, "video" + i + ".mp4"), Path.Combine(temporaryDirectory, "concat" + i2 + ".mp4"));
                        // Run ffmpeg to re-encode the video and add audio if it doesn't have any
                        string appendNoAudio = "";
                        if (!HasAudio(Path.Combine(temporaryDirectory, "video" + i + ".mp4")))
                        {
                            // Get video length
                            string length = GetLength(Path.Combine(temporaryDirectory, "video" + i + ".mp4"));
                            appendNoAudio = "-f lavfi -i anullsrc=channel_layout=mono:sample_rate=44100 -t " + length + " ";
                        }
                        System.Diagnostics.Process process2 = new System.Diagnostics.Process();
                        System.Diagnostics.ProcessStartInfo startInfo2 = new System.Diagnostics.ProcessStartInfo();
                        startInfo2.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                        startInfo2.FileName = Global.useSystemFFmpeg ? "ffmpeg" : @".\ffmpeg.exe";
                        startInfo2.Arguments = "-i \"" + Path.Combine(temporaryDirectory, "video" + i + ".mp4") + "\" " + appendNoAudio
                                + " -c:v libx264"
                                + " -crf 18"
                                + " -preset veryfast"
                                + " -vf scale=" + SaveData.saveValues["VideoWidth"] + "x" + SaveData.saveValues["VideoHeight"] + ",setsar=1:1,fps=fps=30"
                                + " -y"
                                + " \"" + Path.Combine(temporaryDirectory, "concat" + i2 + ".mp4") + "\"";
                        startInfo2.UseShellExecute = false;
                        startInfo2.RedirectStandardError = true;
                        startInfo2.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        startInfo2.CreateNoWindow = true;
                        process2.StartInfo = startInfo2;
                        process2.ErrorDataReceived += (sender, e) =>
                        {
                            if (e.Data != null)
                                ConsoleOutput.WriteLine(e.Data, Color.Transparent);
                        };
                        process2.Start();
                        process2.BeginErrorReadLine();
                        process2.WaitForExit();
                        i2++;
                    }
                }

                List<bool> validFiles = new List<bool>();

                for (int i = 0; i < i2; i++)
                {
                    ConsoleOutput.WriteLine("Checking for validity... (" + (i + 1) + "/" + i2 + ")", Color.Gray);
                    Global.generatorFactory.progressText = "Checking for validity... (" + (i + 1) + "/" + i2 + ")";
                    // Make sure this is a valid file with ffprobe.
                    ProcessStartInfo ffprobe = new ProcessStartInfo()
                    {
                        FileName = Global.useSystemFFprobe ? "ffprobe" : @".\ffprobe.exe",
                        Arguments = "-v error -select_streams v:0 -show_entries stream=codec_name -of default=noprint_wrappers=1:nokey=1 \"" + Path.Combine(temporaryDirectory, "concat" + i + ".mp4") + "\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };
                    Process? ffprobeProcess = new Process();
                    ffprobeProcess.StartInfo = ffprobe;
                    string isVideo = null;
                    ffprobeProcess.OutputDataReceived += (sender, e) =>
                    {
                        isVideo += e.Data;
                    };
                    ffprobeProcess.Start();
                    ffprobeProcess.BeginOutputReadLine();
                    ffprobeProcess.WaitForExit();
                    if(isVideo != null && isVideo != "" && isVideo != "N/A")
                    {
                        validFiles.Add(true);
                        command1 += " -i \"" + Path.Combine(temporaryDirectory, "concat" + i + ".mp4") + "\"";
                    }
                    else
                    {
                        validFiles.Add(false);
                        ConsoleOutput.WriteLine("File " + Path.Combine(temporaryDirectory, "concat" + i + ".mp4") + " is not a valid video file, skipping.", Color.Yellow);
                    }
                }

                command1 += " -filter_complex \"";

                int realCount = 0;
                for (int i = 0; i < i2; i++)
                {
                    if (validFiles[i])
                    {
                        command1 += "[" + realCount + ":v:0][" + realCount + ":a:0]";
                        realCount += 1;
                    }
                }

                //realcount +=1;
                command1 += "concat=n=" + realCount + ":v=1:a=1[outv][outa];[outv]scale=" + SaveData.saveValues["VideoWidth"] + "x" + SaveData.saveValues["VideoHeight"] + ",setsar=1:1,fps=fps=30[outv]\" -map [outv] -map [outa] -shortest -c:v libx264 -crf 18 -preset veryfast -y \"" + ou + "\"";

                ConsoleOutput.WriteLine("Concatenating clips...", Color.Gray);
                Global.generatorFactory.progressText = "Concatenating clips...";

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = Global.useSystemFFmpeg ? "ffmpeg" : @".\ffmpeg.exe";
                startInfo.Arguments = command1;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardError = true;
                startInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                startInfo.CreateNoWindow = true;
                process.StartInfo = startInfo;
                process.ErrorDataReceived += (sender, e) =>
                {
                    if(e.Data != null)
                    {
                        ConsoleOutput.WriteLine(e.Data, Color.Transparent);
                        // Conversion failed?
                        if (e.Data.Contains("Conversion failed!"))
                        {
                            // We don't want to try to concatenate again
                            ConsoleOutput.WriteLine("Fatal error while concatenating videos.");
                            Global.generatorFactory.failureReason = "Fatal error while concatenating videos.";
                            Global.generatorFactory.progressText = Global.generatorFactory.failureReason;
                            Global.generatorFactory.CancelGeneration();
                        }
                    }
                };
                process.Start();
                process.BeginErrorReadLine();
                process.WaitForExit();
                if(process.HasExited && process.ExitCode == 1)
                {
                    throw new Exception("Concatenation failed.");
                }
            }
            catch(Exception ex2)
            {
                ConsoleOutput.WriteLine(ex2.Message);
                ConsoleOutput.WriteLine("Fatal error while concatenating videos.");
                Global.generatorFactory.failureReason = "Fatal error while concatenating videos.";
                Global.generatorFactory.progressText = Global.generatorFactory.failureReason;
                Global.generatorFactory.CancelGeneration();
            }
        }
        public static void OverlayVideo(string video, string overlay)
        {
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                string overlayed_video = video.Replace(".mp4", "_chromakey.mp4");
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = Global.useSystemFFmpeg ? "ffmpeg" : @".\ffmpeg.exe";
                startInfo.Arguments = "-i \"" + video
                        + "\" -i \"" + overlay
                        + "\" -filter_complex \"[1:v]colorkey=0x00FF00:0.3:0.2,scale=" + SaveData.saveValues["VideoWidth"] + "x" + SaveData.saveValues["VideoHeight"] + ",setsar=1:1,fps=fps=30[outv];[0:v][outv]overlay=shortest=1[finalv];[0:a][1:a]amix=inputs=2:duration=shortest[outa]\" -map \"[finalv]\" -map \"[outa]\" -c:v libx264 -crf 18 -preset veryfast -y \"" + overlayed_video + "\"";
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardError = true;
                startInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                startInfo.CreateNoWindow = true;
                process.StartInfo = startInfo;
                process.ErrorDataReceived += (sender, e) =>
                {if (e.Data != null)
                        
                    ConsoleOutput.WriteLine(e.Data, Color.Transparent);
                };
                process.Start();
                process.BeginErrorReadLine();
                process.WaitForExit();
                Global.rolledForOverlay = true;

                // Rename the temporary file to the original file
                File.Delete(video);
                File.Move(overlayed_video, video);
                //File.Delete(overlay);
            }
            catch(Exception ex)
            {
                ConsoleOutput.WriteLine(ex.Message);
                ConsoleOutput.WriteLine("Skipping overlaying video.", Color.Yellow);
                Global.generatorFactory.progressText = "Skipping overlaying video.";
            }
        }
    }
}
