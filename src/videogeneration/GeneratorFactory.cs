using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using System.Threading;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;

namespace NonsensicalVideoGenerator
{
    public enum ProgressState
    {
        Idle,
        Parsing,
        Rendering,
        Concatenating,
        Completed,
        Failed
    }
    public class GeneratorFactory
    {
        public Random globalRandom = new Random();
        public BackgroundWorker? vidThreadWorker { get; set; }
        public float progress { get; set; } = 0;
        public ProgressState progressState { get; set; } = ProgressState.Idle;
        public string progressText { get; set; } = "Idle";
        public string failureReason { get; set; } = "";
        public bool generatorActive = false;
        public bool forceConcatenate = false;
        public BackgroundWorker? timeoutWorker { get; set; }
        
        public BackgroundWorker? killWorker { get; set; }
        public static readonly int defaultTimeout = 30;
        public int timeout = defaultTimeout; // in seconds
        public string tempOutput = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "library", "video", "renders", "temp.mp4");
        public void KillChildProcesses()
        {
            // Find all child processes of the current process and kill them.
            // These are ffmpeg and such, but not the main process.
            // Powershell is used to do this.
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "powershell.exe";
            startInfo.Arguments = "Get-CimInstance Win32_Process | Where-Object {$_.ParentProcessId -eq " + Process.GetCurrentProcess().Id + "} | ForEach-Object {Stop-Process -Id $_.ProcessId}";
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            Process process = new Process();
            // Add console print
            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) => {
                if (e.Data != null)
                    ConsoleOutput.WriteLine(e.Data);
            };
            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => {
                if (e.Data != null)
                    ConsoleOutput.WriteLine(e.Data, Color.Red);
            };
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            process.Close();
        }
        // Kill worker
        public void KillThread(object? sender, DoWorkEventArgs e)
        {
            if (killWorker?.CancellationPending == true)
                return;
            KillChildProcesses();
            failureReason = "Generation cancelled.";
            progressText = failureReason;
            ConsoleOutput.WriteLine("Generation cancelled.", Color.Red);
            if(forceConcatenate)
            {
                // Count videos under Path.Combine(Utilities.temporaryDirectory, "video0.mp4")
                Regex regex = new Regex(@"video(\d+)\.mp4");
                int maxClips = 0;
                foreach (string file in Directory.GetFiles(Utilities.temporaryDirectory))
                {
                    Match match = regex.Match(file);
                    if (match.Success)
                    {
                        int clipNumber = int.Parse(match.Groups[1].Value);
                        if (clipNumber > maxClips)
                            maxClips = clipNumber;
                    }
                }
                if(maxClips > 0)
                {
                    ConsoleOutput.WriteLine("Concatenating clips...", Color.LightGreen);
                    progressText = "Concatenating clips...";
                    progressState = ProgressState.Concatenating;
                    Utilities.ConcatenateVideo(maxClips, tempOutput);
                    bool finished = true;
                    // Save to library if it exists.
                    if (File.Exists(tempOutput))
                    {
                        ConsoleOutput.WriteLine("Saving to library...", Color.LightGreen);
                        LibraryFile libraryFile = new LibraryFile(SaveData.saveValues["ProjectTitle"], tempOutput, DefaultLibraryTypes.Render);
                        progressText = "Saving to library...";
                        if(LibraryData.Load(libraryFile) == null)
                        {
                            ConsoleOutput.WriteLine("Failed to save to library.", Color.Red);
                            progressText = "Failed to save to library.";
                            progressState = ProgressState.Failed;
                            failureReason = "Failed to save to library.";
                            finished = false;
                        }
                    }
                    else
                    {
                        ConsoleOutput.WriteLine("Concatenation failed.", Color.Red);
                        progressText = "Concatenation failed.";
                        progressState = ProgressState.Failed;
                        failureReason = "Concatenation failed.";
                        finished = false;
                    }
                    if(finished)
                    {
                        progressText = "Completed!";
                        progressState = ProgressState.Completed;
                        generatorActive = false;
                        // Open the video in the default video player if the user has that option enabled.
                        if (bool.Parse(SaveData.saveValues["AddToLibrary"]))
                        {
                            ProcessStartInfo startInfo = new()
                            {
                                FileName = tempOutput,
                                UseShellExecute = true
                            };
                            Process.Start(startInfo);
                        }
                        Global.justCompletedRender = true;
                    }
                }
            }
        }
        // Start kill thread
        public void StartKillThread()
        {
            if(killWorker?.IsBusy == true)
            {
                ConsoleOutput.WriteLine("Cancellation already in progress.", Color.Red);
                return;
            }
            if(killWorker == null)
            {
                killWorker = new BackgroundWorker();
                killWorker.DoWork += KillThread;
            }
            killWorker.RunWorkerAsync();
        }
        // Timeout handler
        public void TimeoutThread(object? sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (timeoutWorker?.CancellationPending == true)
                    return;
                // Timeout starts at 30 seconds
                if (timeout == 0)
                {
                    ConsoleOutput.WriteLine("Timed out.", Color.Red);
                    KillChildProcesses();
                }
                if(timeout > -1)
                    timeout--;
                Thread.Sleep(1000);
            }
        }
        public void VidThread(object? sender, DoWorkEventArgs e)
        {
            if (vidThreadWorker?.CancellationPending == true)
                return;
            // Reset progress state
            generatorActive = true; // first time use
            progress = 0;
            progressState = ProgressState.Parsing;

            // Load library.
            progressText = "Parsing library...";
            LibraryData.Load();

            // Check to ensure that the source pool is not empty.
            if(LibraryData.GetFileCount(DefaultLibraryTypes.Material) == 0)
            {
                ConsoleOutput.WriteLine("No material files found in library.", Color.Red);
                failureReason = "No material files found in library.";
                progressText = failureReason;
                CancelGeneration();
                return;
            }

            // Set global random with seed.
            progressText = "Planting seeds...";
            int seed = DateTime.Now.Millisecond;
            // Convert ProjectTitle to int seed
            /*
            string seedString = SaveData.saveValues["ProjectTitle"];
            int seed = 0;
            foreach (char c in seedString)
            {
                seed += (int)c;
            }
            */
            ConsoleOutput.WriteLine("Seed: " + seed, Color.Gray);
            globalRandom = new Random(seed);
            int maxClips = int.Parse(SaveData.saveValues["MaxClipCount"]);
            
            // Clean up previous temporary files.
            progressText = "Cleaning up...";
            CleanUp();

            if (vidThreadWorker?.CancellationPending == true)
                return;

            // Make sure the temporary directory exists.
            Directory.CreateDirectory(Utilities.temporaryDirectory);

            progressText = "Starting generation...";
            progressState = ProgressState.Rendering;
            try
            {
                for (int i = 0; i < maxClips; i++)
                {
                    timeout = defaultTimeout;
                    if (vidThreadWorker?.CancellationPending == true)
                        return;
                    progressText = "Starting clip " + (i + 1) + " of " + maxClips + "...";
                    bool intro = false;
                    if (i == 0 && bool.Parse(SaveData.saveValues["IntrosEnabled"]))
                    {
                        intro = true;
                        // Add the intro.
                        string introPath = LibraryData.PickRandom(DefaultLibraryTypes.Intro, globalRandom);
                        if(introPath == "")
                        {
                            intro = false;
                        }
                        else
                        {
                            maxClips++;
                            ConsoleOutput.WriteLine("Intro clip enabled, adding 1 to max clips. New max clips is " + maxClips + ".", Color.Gray);
                            progress = Convert.ToInt32(((float)i / (float)maxClips));
                            progressText = "Introducing ourselves... (" + (i + 1) + " of " + maxClips + ")";
                            Utilities.CopyVideo(introPath, Path.Combine(Utilities.temporaryDirectory, "video" + i + ".mp4"));
                            // get length of intro and add it to currentTime
                            float introLength = float.Parse(Utilities.GetLength(introPath));
                        }
                    }
                    if (vidThreadWorker?.CancellationPending == true)
                        return;
                    if(!intro)
                    {
                        bool rolledForOverlay = RandomInt(0, 101) < int.Parse(SaveData.saveValues["OverlayChance"]);
                        bool rolledForTransition = RandomInt(0, 101) < int.Parse(SaveData.saveValues["TransitionChance"]);
                        string overlayPath = "";
                        progress = Convert.ToInt32(((float)i / (float)maxClips));
                        progressText = "Clipping... (" + (i + 1) + " of " + maxClips + ")";
                        string sourceToPick = LibraryData.PickRandom(DefaultLibraryTypes.Material, globalRandom);
                        float source = -1;
                        if(sourceToPick == "")
                        {
                            ConsoleOutput.WriteLine("No material files found in library.", Color.Gray);
                            progressText = "No material files found in library.";
                            progressState = ProgressState.Failed;
                            continue;
                        }
                        else
                        {
                            source = float.Parse(Utilities.GetLength(sourceToPick));
                        }
                        string output = source.ToString("0.#########################", new CultureInfo("en-US"));
                        //ConsoleOutput.WriteLine(Utilities.GetLength(sourceToPick) + " -> " + output + " -> " + float.Parse(output));
                        float outputDuration = float.Parse(output);
                        float startOfClip = RandomFloat(0f, outputDuration - float.Parse(SaveData.saveValues["MinStreamDuration"]));
                        float endOfClip = startOfClip + RandomFloat(float.Parse(SaveData.saveValues["MinStreamDuration"]), float.Parse(SaveData.saveValues["MaxStreamDuration"]));
                        // Ensure that the start is not less than 0 and the end is not greater than the source length.
                        if (startOfClip < 0)
                            startOfClip = 0;
                        if (endOfClip > outputDuration)
                            endOfClip = outputDuration;
                        if (vidThreadWorker?.CancellationPending == true)
                            return;
                        // Add an overlay to the video, if rolled for.
                        if(rolledForOverlay)
                        {
                            // Get random overlay.
                            overlayPath = LibraryData.PickRandom(DefaultLibraryTypes.Overlay, globalRandom);
                            if(overlayPath == "")
                            {
                                rolledForOverlay = false;
                            }
                        }
                        if(sourceToPick != "")
                            ConsoleOutput.WriteLine(Path.GetFileName(sourceToPick) + " ("+i+") - " + (endOfClip - startOfClip) + " (" + startOfClip + " to " + endOfClip + ")", Color.Gray);
                        // Insert transition if rolled, ensure that there is a transition as well.
                        //bool alreadySnipped = false;
                        if (!rolledForOverlay && rolledForTransition && LibraryData.GetFileCount(DefaultLibraryTypes.Transition) > 0)
                        {
                            string transitionPath = LibraryData.PickRandom(DefaultLibraryTypes.Transition, globalRandom);
                            if(transitionPath == "")
                            {
                                ConsoleOutput.WriteLine("No transitions found in library.", Color.Yellow);
                                continue;
                            }
                            progressText = "Transitioning... (" + (i + 1) + " of " + maxClips + ")";
                            Utilities.CopyVideo(transitionPath, Path.Combine(Utilities.temporaryDirectory, "video" + i + ".mp4"));
                        }
                        else
                        {
                            // No transition, just snip the video.
                            Utilities.SnipVideo(sourceToPick, startOfClip, endOfClip, Path.Combine(Utilities.temporaryDirectory, "video" + i + ".mp4"));
                        }
                        if (vidThreadWorker?.CancellationPending == true)
                            return;
                        // Parse overlay if rolled.
                        if(rolledForOverlay)
                        {
                            if(overlayPath == null)
                            {
                                ConsoleOutput.WriteLine("No overlays found in library.", Color.Yellow);
                                continue;
                            }
                            progressText = "Chroma keying... (" + (i + 1) + " of " + maxClips + ")";
                            ConsoleOutput.WriteLine("Rolled for overlay, adding overlay to clip " + i + ".", Color.Gray);
                            // We snip the clip here in case it was a transition
                            //if(!alreadySnipped)
                                //Utilities.SnipVideo(sourceToPick, startOfClip, endOfClip, Path.Combine(Utilities.temporaryDirectory, "video" + i + ".mp4"));
                            // Now we'll snip the overlay with another random duration.
                            float overlayDuration = float.Parse(Utilities.GetLength(overlayPath));
                            float startOfOverlay = RandomFloat(0f, overlayDuration - float.Parse(SaveData.saveValues["MinStreamDuration"]));
                            float endOfOverlay = startOfOverlay + RandomFloat(float.Parse(SaveData.saveValues["MinStreamDuration"]), float.Parse(SaveData.saveValues["MaxStreamDuration"]));
                            // Make sure the start is not less than 0 and the end is not greater than the overlay length.
                            if (startOfOverlay < 0)
                                startOfOverlay = 0;
                            if (endOfOverlay > overlayDuration)
                                endOfOverlay = overlayDuration;
                            Utilities.SnipVideo(overlayPath, startOfOverlay, endOfOverlay, Path.Combine(Utilities.temporaryDirectory, "video" + i + "_tempoverlay.mp4"));
                            Utilities.OverlayVideo(Path.Combine(Utilities.temporaryDirectory, "video" + i + ".mp4"), Path.Combine(Utilities.temporaryDirectory, "video" + i + "_tempoverlay.mp4"));
                        }
                        if (vidThreadWorker?.CancellationPending == true)
                            return;
                        if(!rolledForTransition || bool.Parse(SaveData.saveValues["TransitionEffects"]))
                        {
                            int numberOfPlugins = PluginHandler.GetPluginCount();
                            if(numberOfPlugins > 0)
                            {
                                // Roll for effect
                                if(RandomInt(0, 101) < (rolledForTransition ? int.Parse(SaveData.saveValues["TransitionEffectChance"]) : int.Parse(SaveData.saveValues["EffectChance"])))
                                {
                                    progressText = (rolledForTransition ? "Boiling" : "Baking") + " effects... (" + (i + 1) + " of " + maxClips + ")";
                                    // We rolled for an effect, let's pick one.
                                    PluginReturnValue effect = PluginHandler.PickRandom(globalRandom, Path.Combine(Utilities.temporaryDirectory, "video" + i + ".mp4"));
                                    if(effect.success)
                                    {
                                        // Check if effect job path contains output.mp4, if so, plugin was indeed successful.
                                        // so move to videoi.mp4
                                        // Search for output.mp4 in job folder.
                                        string[] files = Directory.GetFiles(effect.jobFolder);
                                        bool foundOutput = false;
                                        foreach(string file in files)
                                        {
                                            if(Path.GetFileName(file) == "output.mp4")
                                            {
                                                // Make sure this is a valid file with ffprobe.
                                                ProcessStartInfo ffprobe = new ProcessStartInfo()
                                                {
                                                    FileName = Global.useSystemFFprobe ? "ffprobe" : @".\ffprobe.exe",
                                                    Arguments = "-v error -select_streams v:0 -show_entries stream=codec_name -of default=noprint_wrappers=1:nokey=1 \"" + file + "\"",
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
                                                if (isVideo != null && isVideo != "" && isVideo != "N/A")
                                                {
                                                    foundOutput = true;
                                                }
                                                break;
                                            }
                                        }
                                        if(foundOutput)
                                        {
                                            // Delete existing videoi.mp4
                                            if(File.Exists(Path.Combine(Utilities.temporaryDirectory, "video" + i + ".mp4")))
                                                File.Delete(Path.Combine(Utilities.temporaryDirectory, "video" + i + ".mp4"));
                                            try
                                            {
                                                File.Move(effect.jobFolder + "output.mp4", Path.Combine(Utilities.temporaryDirectory, "video" + i + ".mp4"));
                                            }
                                            catch(Exception ex)
                                            {
                                                ConsoleOutput.WriteLine("Failed to move output.mp4 to video" + i + ".mp4: " + ex.Message, Color.Red);
                                                effect.success = false;
                                            }
                                        }
                                        else
                                        {
                                            effect.success = false;
                                        }
                                        // Delete job folder.
                                        if(!bool.Parse(SaveData.saveValues["HiddenKeepTemporaryJobFolders"]))
                                            Directory.Delete(effect.jobFolder, true);
                                    }
                                    ConsoleOutput.WriteLine(effect.success ? "Applied "+effect.pluginName+" to " + (rolledForTransition ? "transition" : "clip") + " " + i + "." : "Failed to apply "+effect.pluginName+" to " + (rolledForTransition ? "transition" : "clip") + " " + i + ".", effect.success ? Color.LightGreen : Color.Red);
                                }
                            }
                        }
                    }
                }
                // Finished, throw to get into catch block.
                throw new Exception("Finished");
            }
            catch
            {
                try
                {
                    if (vidThreadWorker?.CancellationPending == true)
                        return;
                    // Concatenate all clips into one video.
                    ConsoleOutput.WriteLine("Concatenating clips...", Color.LightGreen);
                    progressText = "Concatenating clips...";
                    progressState = ProgressState.Concatenating;
                    Utilities.ConcatenateVideo(maxClips, tempOutput);
                    if (vidThreadWorker?.CancellationPending == true)
                        return;
                    bool finished = true;
                    // Save to library if it exists.
                    if (File.Exists(tempOutput))
                    {
                        ConsoleOutput.WriteLine("Saving to library...", Color.LightGreen);
                        LibraryFile libraryFile = new LibraryFile(SaveData.saveValues["ProjectTitle"], tempOutput, DefaultLibraryTypes.Render);
                        progressText = "Saving to library...";
                        if(LibraryData.Load(libraryFile) == null)
                        {
                            ConsoleOutput.WriteLine("Failed to save to library.", Color.Red);
                            progressText = "Failed to save to library.";
                            progressState = ProgressState.Failed;
                            failureReason = "Failed to save to library.";
                            finished = false;
                            CancelGeneration();
                        }
                    }
                    else
                    {
                        ConsoleOutput.WriteLine("Concatenation failed.", Color.Red);
                        progressText = "Concatenation failed.";
                        progressState = ProgressState.Failed;
                        failureReason = "Concatenation failed.";
                        finished = false;
                        CancelGeneration();
                    }
                    if(finished)
                    {
                        if (vidThreadWorker?.CancellationPending == true)
                            return;
                        progressText = "Completed!";
                        progressState = ProgressState.Completed;
                        generatorActive = false;
                        if(vidThreadWorker != null)
                            vidThreadWorker.ReportProgress(100);
                        if(timeoutWorker != null)
                            timeoutWorker.CancelAsync();
                        // Open the video in the default video player if the user has that option enabled.
                        if (bool.Parse(SaveData.saveValues["AddToLibrary"]))
                        {
                            ProcessStartInfo startInfo = new()
                            {
                                FileName = tempOutput,
                                UseShellExecute = true
                            };
                            Process.Start(startInfo);
                        }
                    }
                    else
                    {
                        throw new Exception("Failed to save to library.");
                    }
                }
                catch(Exception ex)
                {
                    progressState = ProgressState.Failed;
                    failureReason = "Error: Press ~ to view console";
                    progressText = failureReason;
                    ConsoleOutput.WriteLine(ex.Message, Color.Red);
                    if(ex.StackTrace != null)
                        ConsoleOutput.WriteLine(ex.StackTrace, Color.Transparent);
                    CancelGeneration();
                }
            }
            //CleanUp();
        }
        public void StartGeneration()
        {
            // Create dummy event handlers for the background worker.
            StartGeneration((sender, e) => { }, (sender, e) => { });
        }
        public void StartGeneration(ProgressChangedEventHandler progressReporter, RunWorkerCompletedEventHandler completedReporter)
        {
            try
            {
                if(vidThreadWorker == null)
                {
                    vidThreadWorker = new BackgroundWorker();
                    vidThreadWorker.DoWork += VidThread;
                    vidThreadWorker.WorkerReportsProgress = true;
                    vidThreadWorker.WorkerSupportsCancellation = true;
                    vidThreadWorker.ProgressChanged += progressReporter;
                    vidThreadWorker.RunWorkerCompleted += completedReporter;
                }
                else
                {
                    vidThreadWorker.CancelAsync();
                }
                if(vidThreadWorker.IsBusy)
                {
                    ConsoleOutput.WriteLine("Generation is busy...", Color.Red);
                    return;
                }
                if(timeoutWorker == null)
                {
                    timeoutWorker = new BackgroundWorker();
                    timeoutWorker.DoWork += TimeoutThread;
                    timeoutWorker.WorkerSupportsCancellation = true;
                }
                forceConcatenate = false;
                timeout = defaultTimeout;
                tempOutput = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "library", "video", "renders", SaveData.saveValues["ProjectTitle"] + ".mp4");
                timeoutWorker.RunWorkerAsync();
                vidThreadWorker.RunWorkerAsync();
                ConsoleOutput.WriteLine("Generation started.", Color.Green);
            }
            catch(Exception ex)
            {
                progressState = ProgressState.Failed;
                failureReason = "Error: Press ~ to view console";
                progressText = failureReason;
                ConsoleOutput.WriteLine(ex.Message, Color.Red);
                if(ex.StackTrace != null)
                    ConsoleOutput.WriteLine(ex.StackTrace, Color.Transparent);
            }
        }
        public void ToggleGeneration(ProgressChangedEventHandler progressReporter, RunWorkerCompletedEventHandler completedReporter)
        {
            StartGeneration(progressReporter, completedReporter);
        }
        public void CancelGeneration(bool user = false, bool forceConcatenate = false)
        {
            if(vidThreadWorker != null)
            {
                // Make sure it's not completed or cancelled already.
                progressState = ProgressState.Failed;
                if(user)
                {
                    this.forceConcatenate = forceConcatenate;
                    if(vidThreadWorker.IsBusy)
                    {
                        failureReason = "Generation cancelling...";
                        progressText = failureReason;
                        ConsoleOutput.WriteLine("Generation cancelling...", Color.Yellow);
                        StartKillThread();
                    }
                    else
                    {
                        failureReason = "Generation cancelled.";
                        progressText = failureReason;
                        ConsoleOutput.WriteLine("Generation cancelled.", Color.Red);
                    }
                }
                else
                {
                    try
                    {
                        vidThreadWorker.ReportProgress(1);
                    }
                    catch
                    {
                        // oh well
                    }
                }
                vidThreadWorker.CancelAsync();
                generatorActive = false;
            }
            if(timeoutWorker != null)
                timeoutWorker.CancelAsync();
        }
        public float RandomFloat(float min, float max)
        {
            return (float)globalRandom.NextDouble() * (max - min) + min;
        }
        public int RandomInt(int min, int max)
        {
            return globalRandom.Next(min, max);
        }
        public void CleanUp()
        {
            if (Directory.Exists(Utilities.temporaryDirectory))
            {
                try
                {
                    Directory.Delete(Utilities.temporaryDirectory, true);
                    ConsoleOutput.WriteLine("Temporary directory deleted.", Color.Gray);
                }
                catch
                {
                    ConsoleOutput.WriteLine("Temporary directory could not be deleted.", Color.Red);
                }
            }
        }
    }
}