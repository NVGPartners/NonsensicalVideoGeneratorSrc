using System;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Xna.Framework;

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
                startInfo.FileName = Global.useSystemFFmpeg ? "ffprobe" : @".\bin\ffprobe.exe";
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
                startInfo.FileName = Global.useSystemFFmpeg ? "ffmpeg" : @".\bin\ffmpeg.exe";
                startInfo.Arguments = "-i \"" + video
                        + "\" -ss " + startTime
                        + " -to " + endTime
                        + " -ac 1"
                        + " -ar 44100"
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
                startInfo.FileName = Global.useSystemFFmpeg ? "ffprobe" : @".\bin\ffprobe.exe";
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
                startInfo.FileName = Global.useSystemFFmpeg ? "ffmpeg" : @".\bin\ffmpeg.exe";
                startInfo.Arguments = "-i \"" + video + "\" " + appendNoAudio
                        + " -ar 44100"
                        + " -ac 1"
                        //+ " -filter:v fps=fps=30,setsar=1:1"
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
                string command1 = "";

                // Rename all files in temporary directory that start with video to video0, video1, etc.
                string[] files = Directory.GetFiles(temporaryDirectory);
                int i2 = 0;
                for (int i = 0; i < count; i++)
                {
                    if (File.Exists(Path.Combine(temporaryDirectory, "video" + i + ".mp4")))
                    {
                        File.Move(Path.Combine(temporaryDirectory, "video" + i + ".mp4"), Path.Combine(temporaryDirectory, "concat" + i2 + ".mp4"));
                        i2++;
                    }
                }

                for (int i = 0; i < i2; i++)
                {
                    if (File.Exists(Path.Combine(temporaryDirectory, "concat" + i + ".mp4")))
                    {
                        command1 += " -i " + Path.Combine(temporaryDirectory, "concat" + i + ".mp4");
                    }
                }

                command1 += " -filter_complex \"";

                int realCount = 0;
                for (int i = 0; i < i2; i++)
                {
                    if (File.Exists(Path.Combine(temporaryDirectory, "concat" + i + ".mp4")))
                    {
                        command1 += "[" + i + ":v:0][" + i + ":a:0]";
                        realCount += 1;
                    }
                }

                //realcount +=1;
                command1 += "concat=n=" + realCount + ":v=1:a=1[outv][outa]\" -map [outv] -map [outa] -shortest -fps_mode vfr -y \"" + ou + "\"";

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = Global.useSystemFFmpeg ? "ffmpeg" : @".\bin\ffmpeg.exe";
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
                startInfo.FileName = Global.useSystemFFmpeg ? "ffmpeg" : @".\bin\ffmpeg.exe";
                startInfo.Arguments = "-i \"" + video
                        + "\" -i \"" + overlay
                        + "\" -filter_complex \"[1:v]colorkey=0x00FF00:0.3:0.2,scale=" + SaveData.saveValues["VideoWidth"] + "x" + SaveData.saveValues["VideoHeight"] + ",setsar=1:1,fps=fps=30[outv];[0:v][outv]overlay=shortest=1[finalv];[0:a][1:a]amix=inputs=2:duration=shortest[outa]\" -map \"[finalv]\" -map \"[outa]\" -y \"" + overlayed_video + "\"";
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