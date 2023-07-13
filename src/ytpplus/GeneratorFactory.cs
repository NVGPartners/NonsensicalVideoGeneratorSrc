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

namespace YTPPlusPlusPlus
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
        public CaptionFile captionFile = new CaptionFile();
        public BackgroundWorker? timeoutWorker { get; set; }
        
        public BackgroundWorker? killWorker { get; set; }
        public static readonly int defaultTimeout = 30;
        public int timeout = defaultTimeout; // in seconds
        public static readonly string tempOutput = Path.Combine(Utilities.temporaryDirectory, "tempoutput.mp4");
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
            process.OutputDataReceived += (object? sender, DataReceivedEventArgs e) => {
                ConsoleOutput.WriteLine(e.Data);
            };
            process.ErrorDataReceived += (object? sender, DataReceivedEventArgs e) => {
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
                // Count videos under Path.Combine(Utilities.temporaryDirectory, "video(NUMBER).mp4")
                Regex regex = new Regex(@"video(\d+).mp4");
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
                ConsoleOutput.WriteLine("Max clips: " + maxClips);
                if(maxClips > 0)
                {
                    ConsoleOutput.WriteLine("Concatenating clips...", Color.LightGreen);
                    progressText = "Concatenating clips...";
                    progressState = ProgressState.Concatenating;
                    Utilities.ConcatenateVideo(maxClips, tempOutput, null);
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
        public static CaptionText? Caption(string text, float startTime, float endTime)
        {
            // float (seconds) to int (ms)
            int startTimeMs = (int)(startTime * 1000);
            int endTimeMs = (int)(endTime * 1000);
            return new CaptionText(text, CaptionFile.EncodeTime(startTimeMs), CaptionFile.EncodeTime(endTimeMs));
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
            if(LibraryData.GetFileCount(DefaultLibraryTypes.Material) == 0 && LibraryData.GetFileCount(DefaultLibraryTypes.Image) == 0)
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

            // Delete the temporary output file if it exists.
            try
            {
                if (File.Exists(tempOutput))
                    File.Delete(tempOutput);
            }
            catch
            {
                ConsoleOutput.WriteLine("Failed to delete temporary output file.", Color.Red);
                failureReason = "Failed to delete temporary output file.";
                progressText = failureReason;
                CancelGeneration();
                return;
            }

            progressText = "Starting generation...";
            progressState = ProgressState.Rendering;
            float currentTime = 0;
            try
            {
                bool usingTennis = false;
                if(bool.Parse(SaveData.saveValues["TennisMode"]) == true)
                {
                    // get a random tennis video and get its caption file
                    string tennisVideo = LibraryData.PickRandom(DefaultLibraryTypes.Tennis, globalRandom);
                    if(tennisVideo != "")
                    {
                        usingTennis = true;
                        string srt = Path.Combine(Utilities.temporaryDirectory, Path.GetFileNameWithoutExtension(tennisVideo) + ".srt");
                        // extract caption file
                        Utilities.ExtractSrt(tennisVideo, srt);
                        // does it exist?
                        if(File.Exists(srt))
                        {
                            progressText = "Playing tennis...";
                            if (vidThreadWorker?.CancellationPending == true)
                                return;
                            // load it
                            CaptionFile tennisCaption = new();
                            tennisCaption.Load(srt);
                            // parse each caption as a clip
                            for(int i = 0; i < tennisCaption.captions.Count; i++)
                            {
                                string startTime = tennisCaption.captions[i].startTime.Replace(",", ".");
                                string endTime = tennisCaption.captions[i].endTime.Replace(",", ".");
                                Utilities.SnipVideo(tennisVideo, startTime, endTime, Path.Combine(Utilities.temporaryDirectory, "video" + i + "_tennis.mp4"));
                                progressText = "Serving the ball... (" + (i + 1) + "/" + tennisCaption.captions.Count + ")";
                                if (vidThreadWorker?.CancellationPending == true)
                                    return;
                            }
                            progressText = "Swinging the racket...";
                            if (vidThreadWorker?.CancellationPending == true)
                                return;
                            int offsetTime = 0;
                            for(int i = 0; i < tennisCaption.captions.Count; i++)
                            {
                                ConsoleOutput.WriteLine("Parsing caption " + tennisCaption.captions[i].text + " (" + (i + 1) + "/" + tennisCaption.captions.Count + ")", Color.Gray);
                                progressText = "Hitting the ball... (" + (i + 1) + "/" + tennisCaption.captions.Count + ")";
                                if (vidThreadWorker?.CancellationPending == true)
                                    return;
                                // Roll for effect
                                int effectChance = RandomInt(0, 101);
                                bool useEffect = false;
                                bool useClip = false;
                                string preset = tennisCaption.captions[i].text;
                                if(effectChance < int.Parse(SaveData.saveValues["EffectChance"]))
                                {
                                    /*
                                    // 25% of this chance means to choose a random preset effect
                                    if(effectChance < int.Parse(SaveData.saveValues["EffectChance"]) / 4)
                                    {
                                        List<string> presets = new()
                                        {
                                            "TRANSITION",
                                            "OVERLAY",
                                            "IMAGE",
                                            "CLIP",
                                        };
                                        preset = presets[RandomInt(0, presets.Count)];
                                        ConsoleOutput.WriteLine("Picked random preset: " + preset, Color.Gray);
                                    }
                                    else
                                    {
                                    */
                                    useEffect = true;
                                    //}
                                }
                                ConsoleOutput.WriteLine("Using preset: " + preset, Color.Gray);
                                if(!useEffect)
                                {
                                    switch(preset)
                                    {
                                        case "INTRO":
                                            string introPath = LibraryData.PickRandom(DefaultLibraryTypes.Intro, globalRandom);
                                            if(introPath != "" && bool.Parse(SaveData.saveValues["IntrosEnabled"]) && RandomInt(0, 101) < int.Parse(SaveData.saveValues["EffectChance"]))
                                            {
                                                progressText = "It's the approach... (" + (i + 1) + "/" + tennisCaption.captions.Count + ")";
                                                if (vidThreadWorker?.CancellationPending == true)
                                                    return;
                                                // delete the current video
                                                File.Delete(Path.Combine(Utilities.temporaryDirectory, "video" + i + "_tennis.mp4"));
                                                // copy the intro video
                                                Utilities.CopyVideo(introPath, Path.Combine(Utilities.temporaryDirectory, "video" + i + "_tennis.mp4"));
                                                // tennis caption
                                                float clipDuration = float.Parse(Utilities.GetLength(introPath));
                                                captionFile.captions.Add(new CaptionText(preset, CaptionFile.EncodeTime(offsetTime), CaptionFile.EncodeTime(offsetTime + ((int)(clipDuration * 1000)))));
                                                offsetTime += (int)(clipDuration * 1000);
                                                ConsoleOutput.WriteLine("Caption: " + preset + " (" + CaptionFile.EncodeTime(offsetTime) + ")", Color.Gray);
                                            }
                                            else
                                            {
                                                // tennis caption
                                                float clipDuration = float.Parse(Utilities.GetLength(Path.Combine(Utilities.temporaryDirectory, "video" + i + "_tennis.mp4")));
                                                captionFile.captions.Add(new CaptionText(preset, CaptionFile.EncodeTime(offsetTime), CaptionFile.EncodeTime(offsetTime + ((int)(clipDuration * 1000)))));
                                                offsetTime += (int)(clipDuration * 1000);
                                                ConsoleOutput.WriteLine("Caption: " + preset + " (" + CaptionFile.EncodeTime(offsetTime) + ")", Color.Gray);
                                            }
                                            break;
                                        case "OUTRO":
                                            string outroPath = LibraryData.PickRandom(DefaultLibraryTypes.Outro, globalRandom);
                                            if(outroPath != "" && bool.Parse(SaveData.saveValues["OutrosEnabled"]) && RandomInt(0, 101) < int.Parse(SaveData.saveValues["EffectChance"]))
                                            {
                                                progressText = "A lob... (" + (i + 1) + "/" + tennisCaption.captions.Count + ")";
                                                if (vidThreadWorker?.CancellationPending == true)
                                                    return;
                                                // delete the current video
                                                File.Delete(Path.Combine(Utilities.temporaryDirectory, "video" + i + "_tennis.mp4"));
                                                // copy the outro video
                                                Utilities.CopyVideo(outroPath, Path.Combine(Utilities.temporaryDirectory, "video" + i + "_tennis.mp4"));
                                                // tennis caption
                                                float clipDuration = float.Parse(Utilities.GetLength(outroPath));
                                                captionFile.captions.Add(new CaptionText(preset, CaptionFile.EncodeTime(offsetTime), CaptionFile.EncodeTime(offsetTime + ((int)(clipDuration * 1000)))));
                                                offsetTime += (int)(clipDuration * 1000);
                                                ConsoleOutput.WriteLine("Caption: " + preset + " (" + CaptionFile.EncodeTime(offsetTime) + ")", Color.Gray);
                                            }
                                            else
                                            {
                                                // tennis caption
                                                float clipDuration = float.Parse(Utilities.GetLength(Path.Combine(Utilities.temporaryDirectory, "video" + i + "_tennis.mp4")));
                                                captionFile.captions.Add(new CaptionText(preset, CaptionFile.EncodeTime(offsetTime), CaptionFile.EncodeTime(offsetTime + ((int)(clipDuration * 1000)))));
                                                offsetTime += (int)(clipDuration * 1000);
                                                ConsoleOutput.WriteLine("Caption: " + preset + " (" + CaptionFile.EncodeTime(offsetTime) + ")", Color.Gray);
                                            }
                                            break;
                                        case "TRANSITION":
                                            string transitionPath = LibraryData.PickRandom(DefaultLibraryTypes.Transition, globalRandom);
                                            if (transitionPath != "" && bool.Parse(SaveData.saveValues["TransitionsEnabled"]) && RandomInt(0, 101) < int.Parse(SaveData.saveValues["TransitionChance"]))
                                            {
                                                progressText = "A volley... (" + (i + 1) + "/" + tennisCaption.captions.Count + ")";
                                                if (vidThreadWorker?.CancellationPending == true)
                                                    return;
                                                // delete the current video
                                                File.Delete(Path.Combine(Utilities.temporaryDirectory, "video" + i + "_tennis.mp4"));
                                                // copy the transition video
                                                Utilities.CopyVideo(transitionPath, Path.Combine(Utilities.temporaryDirectory, "video" + i + "_tennis.mp4"));
                                                // tennis caption
                                                float clipDuration = float.Parse(Utilities.GetLength(transitionPath));
                                                captionFile.captions.Add(new CaptionText(preset, CaptionFile.EncodeTime(offsetTime), CaptionFile.EncodeTime(offsetTime + ((int)(clipDuration * 1000)))));
                                                offsetTime += (int)(clipDuration * 1000);
                                                ConsoleOutput.WriteLine("Caption: " + preset + " (" + CaptionFile.EncodeTime(offsetTime) + ")", Color.Gray);
                                            }
                                            else
                                            {
                                                // tennis caption
                                                float clipDuration = float.Parse(Utilities.GetLength(Path.Combine(Utilities.temporaryDirectory, "video" + i + "_tennis.mp4")));
                                                captionFile.captions.Add(new CaptionText(preset, CaptionFile.EncodeTime(offsetTime), CaptionFile.EncodeTime(offsetTime + ((int)(clipDuration * 1000)))));
                                                offsetTime += (int)(clipDuration * 1000);
                                                ConsoleOutput.WriteLine("Caption: " + preset + " (" + CaptionFile.EncodeTime(offsetTime) + ")", Color.Gray);
                                            }
                                            break;
                                        case "OVERLAY":
                                            string overlayPath = LibraryData.PickRandom(DefaultLibraryTypes.Overlay, globalRandom);
                                            if (overlayPath != "" && bool.Parse(SaveData.saveValues["OverlaysEnabled"]) && RandomInt(0, 101) < int.Parse(SaveData.saveValues["OverlayChance"]))
                                            {
                                                progressText = "A smash... (" + (i + 1) + "/" + tennisCaption.captions.Count + ")";
                                                if (vidThreadWorker?.CancellationPending == true)
                                                    return;
                                                // snip the overlay video
                                                float sourceLength = float.Parse(Utilities.GetLength(overlayPath), NumberStyles.Any, new CultureInfo("en-US"));
                                                float startOfClip = RandomFloat(0f, sourceLength - float.Parse(SaveData.saveValues["MinStreamDuration"]));
                                                float endOfClip = startOfClip + RandomFloat(float.Parse(SaveData.saveValues["MinStreamDuration"]), float.Parse(SaveData.saveValues["MaxStreamDuration"]));
                                                if (startOfClip < 0)
                                                    startOfClip = 0;
                                                if (endOfClip > sourceLength)
                                                    endOfClip = sourceLength;
                                                Utilities.SnipVideo(overlayPath, startOfClip, endOfClip, Path.Combine(Utilities.temporaryDirectory, "video" + i + "_tempoverlay.mp4"));
                                                Utilities.OverlayVideo(Path.Combine(Utilities.temporaryDirectory, "video" + i + "_tennis.mp4"), Path.Combine(Utilities.temporaryDirectory, "video" + i + "_tempoverlay.mp4"));
                                                // tennis caption
                                                float clipDuration = float.Parse(Utilities.GetLength(overlayPath));
                                                captionFile.captions.Add(new CaptionText(preset, CaptionFile.EncodeTime(offsetTime), CaptionFile.EncodeTime(offsetTime + ((int)(clipDuration * 1000)))));
                                                offsetTime += (int)(clipDuration * 1000);
                                                ConsoleOutput.WriteLine("Caption: " + preset + " (" + CaptionFile.EncodeTime(offsetTime) + ")", Color.Gray);
                                            }
                                            else
                                            {
                                                // tennis caption
                                                float clipDuration = float.Parse(Utilities.GetLength(Path.Combine(Utilities.temporaryDirectory, "video" + i + "_tennis.mp4")));
                                                captionFile.captions.Add(new CaptionText(preset, CaptionFile.EncodeTime(offsetTime), CaptionFile.EncodeTime(offsetTime + ((int)(clipDuration * 1000)))));
                                                offsetTime += (int)(clipDuration * 1000);
                                                ConsoleOutput.WriteLine("Caption: " + preset + " (" + CaptionFile.EncodeTime(offsetTime) + ")", Color.Gray);
                                            }
                                            break;
                                        case "IMAGE":
                                            string imagePath = LibraryData.PickRandom(DefaultLibraryTypes.Image, globalRandom);
                                            if (imagePath != "" && RandomInt(0, 101) < int.Parse(SaveData.saveValues["ImageChance"]))
                                            {
                                                progressText = "A jump... (" + (i + 1) + "/" + tennisCaption.captions.Count + ")";
                                                if (vidThreadWorker?.CancellationPending == true)
                                                    return;
                                                // make an image video
                                                float imageDuration = Utilities.ImageVideo(imagePath, Path.Combine(Utilities.temporaryDirectory, "video" + i + "_tennis.mp4"));
                                                if(imageDuration == -1)
                                                {
                                                    ConsoleOutput.WriteLine("Failed to create image video.", Color.Yellow);
                                                }
                                                // tennis caption
                                                captionFile.captions.Add(new CaptionText(preset, CaptionFile.EncodeTime(offsetTime), CaptionFile.EncodeTime(offsetTime + ((int)(imageDuration) * 1000))));
                                                offsetTime += (int)imageDuration * 1000;
                                                ConsoleOutput.WriteLine("Caption: " + preset + " (" + CaptionFile.EncodeTime(offsetTime) + ")", Color.Gray);
                                            }
                                            else
                                            {
                                                // tennis caption
                                                float clipDuration = float.Parse(Utilities.GetLength(Path.Combine(Utilities.temporaryDirectory, "video" + i + "_tennis.mp4")));
                                                captionFile.captions.Add(new CaptionText(preset, CaptionFile.EncodeTime(offsetTime), CaptionFile.EncodeTime(offsetTime + ((int)(clipDuration * 1000)))));
                                                offsetTime += (int)(clipDuration * 1000);
                                                ConsoleOutput.WriteLine("Caption: " + preset + " (" + CaptionFile.EncodeTime(offsetTime) + ")", Color.Gray);
                                            }
                                            break;
                                        case "CLIP":
                                            useClip = true;
                                            break;
                                        default:
                                            useEffect = true;
                                            break;
                                    }
                                }
                                if(useEffect)
                                {
                                    progressText = "Got a point... (" + (i + 1) + "/" + tennisCaption.captions.Count + ")";
                                    if (vidThreadWorker?.CancellationPending == true)
                                        return;
                                    // We rolled for an effect, let's pick one.
                                    PluginReturnValue effect = PluginHandler.PickRandom(globalRandom, Path.Combine(Utilities.temporaryDirectory, "video" + i + "_tennis.mp4"));
                                    if(effect.success)
                                    {
                                        preset = effect.pluginName.ToUpper();
                                        ConsoleOutput.WriteLine("Applied effect to clip " + i + ".", Color.LightGreen);
                                        // tennis caption
                                        float clipDuration = float.Parse(Utilities.GetLength(Path.Combine(Utilities.temporaryDirectory, "video" + i + "_tennis.mp4")));
                                        captionFile.captions.Add(new CaptionText(preset, CaptionFile.EncodeTime(offsetTime), CaptionFile.EncodeTime(offsetTime + ((int)(clipDuration * 1000)))));
                                        offsetTime += (int)(clipDuration * 1000);
                                        ConsoleOutput.WriteLine("Caption: " + preset + " (" + CaptionFile.EncodeTime(offsetTime) + ")", Color.Gray);
                                    }
                                    else
                                    {
                                        preset = "CLIP";
                                        useClip = true;
                                        useEffect = false;
                                    }
                                }
                                if(useClip)
                                {
                                    string sourceToPick = LibraryData.PickRandom(DefaultLibraryTypes.Material, globalRandom);
                                    if(sourceToPick != "" && RandomInt(0, 101) < int.Parse(SaveData.saveValues["EffectChance"]))
                                    {
                                        progressText = "Here's the baseline... (" + (i + 1) + "/" + tennisCaption.captions.Count + ")";
                                        if (vidThreadWorker?.CancellationPending == true)
                                            return;
                                        // Ensure that the start is not less than 0 and the end is not greater than the source length.
                                        float sourceLength = float.Parse(Utilities.GetLength(sourceToPick), NumberStyles.Any, new CultureInfo("en-US"));
                                        float startOfClip = RandomFloat(0f, sourceLength - float.Parse(SaveData.saveValues["MinStreamDuration"]));
                                        float endOfClip = startOfClip + RandomFloat(float.Parse(SaveData.saveValues["MinStreamDuration"]), float.Parse(SaveData.saveValues["MaxStreamDuration"]));
                                        if (startOfClip < 0)
                                            startOfClip = 0;
                                        if (endOfClip > sourceLength)
                                            endOfClip = sourceLength;
                                        Utilities.SnipVideo(sourceToPick, startOfClip, endOfClip, Path.Combine(Utilities.temporaryDirectory, "video" + i + "_tennis.mp4"));
                                        // tennis caption
                                        float clipDuration = endOfClip - startOfClip;
                                        captionFile.captions.Add(new CaptionText(preset, CaptionFile.EncodeTime(offsetTime), CaptionFile.EncodeTime(offsetTime + ((int)(clipDuration * 1000)))));
                                        offsetTime += (int)(clipDuration * 1000);
                                        ConsoleOutput.WriteLine("Caption: " + preset + " (" + CaptionFile.EncodeTime(offsetTime) + ")", Color.Gray);
                                    }
                                    else
                                    {
                                        // tennis caption
                                        float clipDuration = float.Parse(Utilities.GetLength(Path.Combine(Utilities.temporaryDirectory, "video" + i + "_tennis.mp4")));
                                        captionFile.captions.Add(new CaptionText(preset, CaptionFile.EncodeTime(offsetTime), CaptionFile.EncodeTime(offsetTime + ((int)(clipDuration * 1000)))));
                                        offsetTime += (int)(clipDuration * 1000);
                                        ConsoleOutput.WriteLine("Caption: " + preset + " (" + CaptionFile.EncodeTime(offsetTime) + ")", Color.Gray);
                                    }
                                }
                                // Speed up or slow down the video to the duration of the caption.
                                if (vidThreadWorker?.CancellationPending == true)
                                    return;
                                //Utilities.MatchTimeVideo(Path.Combine(Utilities.temporaryDirectory, "video" + i + "_tennis.mp4"), durationInSeconds, Path.Combine(Utilities.temporaryDirectory, "video" + i + ".mp4"));
                                Utilities.CopyVideo(Path.Combine(Utilities.temporaryDirectory, "video" + i + "_tennis.mp4"), Path.Combine(Utilities.temporaryDirectory, "video" + i + ".mp4"));
                                if (vidThreadWorker?.CancellationPending == true)
                                    return;
                            }
                        }
                    }
                }
                if(!usingTennis)
                {
                    for (int i = 0; i < maxClips; i++)
                    {
                        timeout = defaultTimeout;
                        CaptionText currentCaption = null;
                        float addedTime = 0;
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
                                float introLength = float.Parse(Utilities.GetLength(introPath), NumberStyles.Any, new CultureInfo("en-US"));
                                // tennis caption
                                currentCaption = Caption("INTRO", currentTime, currentTime + introLength);
                                addedTime = introLength;
                            }
                        }
                        if (vidThreadWorker?.CancellationPending == true)
                            return;
                        if(!intro)
                        {
                            bool rolledForOverlay = RandomInt(0, 101) < int.Parse(SaveData.saveValues["OverlayChance"]) && bool.Parse(SaveData.saveValues["OverlaysEnabled"]);
                            bool rolledForTransition = RandomInt(0, 101) < int.Parse(SaveData.saveValues["TransitionChance"]) && bool.Parse(SaveData.saveValues["TransitionsEnabled"]);
                            bool rolledForImage = RandomInt(0, 101) < int.Parse(SaveData.saveValues["ImageChance"]); string overlayPath = "";
                            progress = Convert.ToInt32(((float)i / (float)maxClips));
                            progressText = "Clipping... (" + (i + 1) + " of " + maxClips + ")";
                            string sourceToPick = LibraryData.PickRandom(DefaultLibraryTypes.Material, globalRandom);
                            float source = -1;
                            if(sourceToPick == "")
                            {
                                if(LibraryData.GetFileCount(DefaultLibraryTypes.Image) > 0)
                                {
                                    rolledForTransition = false;
                                    rolledForImage = true;
                                }
                                else
                                {
                                    ConsoleOutput.WriteLine("No material files found in library.", Color.Gray);
                                    progressText = "No material files found in library.";
                                    progressState = ProgressState.Failed;
                                    continue;
                                }
                            }
                            else
                            {
                                source = float.Parse(Utilities.GetLength(sourceToPick), NumberStyles.Any, new CultureInfo("en-US"));
                            }
                            string output = source.ToString("0.#########################", new CultureInfo("en-US"));
                            //ConsoleOutput.WriteLine(Utilities.GetLength(sourceToPick) + " -> " + output + " -> " + float.Parse(output, NumberStyles.Any, new CultureInfo("en-US")));
                            float outputDuration = float.Parse(output, NumberStyles.Any, new CultureInfo("en-US"));
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
                                float transitionDuration = float.Parse(Utilities.GetLength(transitionPath), NumberStyles.Any, new CultureInfo("en-US"));
                                currentCaption = Caption("TRANSITION", currentTime, currentTime + transitionDuration);
                                addedTime = transitionDuration;
                            }
                            else if(rolledForImage && LibraryData.GetFileCount(DefaultLibraryTypes.Image) > 0)
                            {
                                string imagePath = LibraryData.PickRandom(DefaultLibraryTypes.Image, globalRandom);
                                if(imagePath == "")
                                {
                                    ConsoleOutput.WriteLine("No images found in library.", Color.Yellow);
                                    continue;
                                }
                                progressText = "Filming... (" + (i + 1) + " of " + maxClips + ")";
                                float imageDuration = Utilities.ImageVideo(imagePath, Path.Combine(Utilities.temporaryDirectory, "video" + i + ".mp4"));
                                if(imageDuration == -1)
                                {
                                    ConsoleOutput.WriteLine("Failed to create image video.", Color.Yellow);
                                    continue;
                                }
                                currentCaption = Caption("IMAGE", currentTime, currentTime + imageDuration);
                                addedTime = imageDuration;
                            }
                            else
                            {
                                // No transition, just snip the video.
                                Utilities.SnipVideo(sourceToPick, startOfClip, endOfClip, Path.Combine(Utilities.temporaryDirectory, "video" + i + ".mp4"));
                                float clipDuration = float.Parse(Utilities.GetLength(Path.Combine(Utilities.temporaryDirectory, "video" + i + ".mp4")), NumberStyles.Any, new CultureInfo("en-US"));
                                currentCaption = Caption("CLIP", currentTime, currentTime + clipDuration);
                                addedTime = clipDuration;
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
                                float overlayDuration = float.Parse(Utilities.GetLength(overlayPath), NumberStyles.Any, new CultureInfo("en-US"));
                                float startOfOverlay = RandomFloat(0f, overlayDuration - float.Parse(SaveData.saveValues["MinStreamDuration"]));
                                float endOfOverlay = startOfOverlay + RandomFloat(float.Parse(SaveData.saveValues["MinStreamDuration"]), float.Parse(SaveData.saveValues["MaxStreamDuration"]));
                                Utilities.SnipVideo(overlayPath, startOfOverlay, endOfOverlay, Path.Combine(Utilities.temporaryDirectory, "video" + i + "_tempoverlay.mp4"));
                                Utilities.OverlayVideo(Path.Combine(Utilities.temporaryDirectory, "video" + i + ".mp4"), Path.Combine(Utilities.temporaryDirectory, "video" + i + "_tempoverlay.mp4"));
                                // The result is a video with an overlay at random points.
                                currentCaption = Caption("OVERLAY", currentTime, currentTime + (endOfOverlay - startOfOverlay));
                                addedTime = endOfOverlay - startOfOverlay;
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
                                            float effectDuration = float.Parse(Utilities.GetLength(Path.Combine(Utilities.temporaryDirectory, "video" + i + ".mp4")), NumberStyles.Any, new CultureInfo("en-US"));
                                            currentCaption = Caption(effect.pluginName.ToUpper(), currentTime, currentTime + effectDuration);
                                            addedTime = effectDuration;
                                        }
                                        ConsoleOutput.WriteLine(effect.success ? "Applied effect to " + (rolledForTransition ? "transition" : "clip") + " " + i + "." : "Failed to apply effect to " + (rolledForTransition ? "transition" : "clip") + " " + i + ".", effect.success ? Color.LightGreen : Color.Red);
                                    }
                                }
                            }
                        }
                        if(currentCaption != null)
                            captionFile.captions.Add(currentCaption);
                        currentTime += addedTime;
                    }
                    if (bool.Parse(SaveData.saveValues["OutrosEnabled"]))
                    {
                        string outroPath = LibraryData.PickRandom(DefaultLibraryTypes.Outro, globalRandom);
                        if(outroPath == "")
                        {
                            ConsoleOutput.WriteLine("No outros found in library.", Color.Yellow);
                            outroPath = "";
                        }
                        else
                        {
                            if (vidThreadWorker?.CancellationPending == true)
                                return;
                            maxClips++;
                            progressText = "Closing the film spool... (" + maxClips + " of " + maxClips + ")";
                            ConsoleOutput.WriteLine("Outro clip enabled, adding 1 to max clips. New max clips is " + maxClips + ".", Color.Gray);
                            ConsoleOutput.WriteLine("STARTING CLIP " + "video" + maxClips, Color.Gray);
                            Utilities.CopyVideo(outroPath, Path.Combine(Utilities.temporaryDirectory, "video" + maxClips + ".mp4"));
                            float outroDuration = float.Parse(Utilities.GetLength(outroPath), NumberStyles.Any, new CultureInfo("en-US"));
                            captionFile.captions.Add(Caption("OUTRO", currentTime, currentTime + outroDuration));
                            currentTime += outroDuration;
                            maxClips++;
                        }
                    }
                }
                if (vidThreadWorker?.CancellationPending == true)
                    return;
                // Concatenate all clips into one video.
                ConsoleOutput.WriteLine("Concatenating clips...", Color.LightGreen);
                progressText = "Concatenating clips...";
                progressState = ProgressState.Concatenating;
                Utilities.ConcatenateVideo(maxClips, tempOutput, captionFile);
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
            }
            catch(Exception ex)
            {
                progressState = ProgressState.Failed;
                failureReason = "Error: Press ~ to view console";
                progressText = failureReason;
                ConsoleOutput.WriteLine(ex.Message, Color.Red);
                ConsoleOutput.WriteLine(ex.StackTrace, Color.Transparent);
                ConsoleOutput.WriteLine("Is a gyan.dev build of FFmpeg installed?", Color.Yellow);
                ConsoleOutput.WriteLine("More information was printed to console.txt for troubleshooting.", Color.Yellow);
                CancelGeneration();
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
            timeoutWorker.RunWorkerAsync();
            vidThreadWorker.RunWorkerAsync();
            ConsoleOutput.WriteLine("Generation started.", Color.Green);
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
                    vidThreadWorker.ReportProgress(1);
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