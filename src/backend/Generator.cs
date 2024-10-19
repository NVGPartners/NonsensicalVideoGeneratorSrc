using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using Steamworks;
using Microsoft.Xna.Framework.Media;

#if MONOGAME
using Microsoft.Xna.Framework;
#else
using System.Drawing;
#endif

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
    public class Clip
    {
        public string name = "";
        public bool rolledForOverlay = false;
        public bool rolledForTransition = false;
        public bool intro = false;
        public Clip(string name, bool rolledForOverlay = false, bool rolledForTransition = false, bool intro = false)
        {
            this.name = name;
            this.rolledForOverlay = rolledForOverlay;
            this.rolledForTransition = rolledForTransition;
            this.intro = intro;
        }
    }
    public enum RollReason
    {
        NoRoll,
        TransitionEffectChance100Percent,
        EffectChance100Percent,
        TransitionEffect,
        Effect,
    }
    public class Generator
    {
        public static string temporaryDirectory = @".\temp";
        public Random globalRandom = new Random();
        public BackgroundWorker? vidThreadWorker { get; set; }
        public float progress { get; set; } = 0;
        public ProgressState progressState { get; set; } = ProgressState.Idle;
        public string progressText { get; set; } = "";
        public string failureReason { get; set; } = "";
        public bool generatorActive = false;
        public bool forceConcatenate = false;
        public BackgroundWorker? timeoutWorker { get; set; }
        public BackgroundWorker? killWorker { get; set; }
        public int timeout = 0;
        public string tempOutput = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "library", "video", "renders", "temp.mp4");
        public static string oldExportParams = "-c:v libx264 -crf 18 -preset veryfast -ar 32000 -shortest -fflags +genpts";
        public static string betterExportParams = "-vcodec libx264 -crf 28 -preset ultrafast -ac 2 -c:a aac -b:a 160k -ar 44100 -map_metadata -1 -reset_timestamps 1 -shortest -fflags +genpts";
        public bool audioSync = true;
        public static string exportParams = betterExportParams;
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
            failureReason = L.T(0, "Generate:StatusCancelled");
            progressText = failureReason;
            ConsoleOutput.WriteLine("Generation cancelled.", Color.Red);
            if(forceConcatenate)
            {
                // Count videos under Path.Combine(temporaryDirectory, "0.mp4")
                Regex regex = new Regex(@"(\d+)\.mp4");
                List<Clip> clips = new();
                foreach (string file in Directory.GetFiles(temporaryDirectory))
                {
                    Match match = regex.Match(file);
                    if (match.Success)
                    {
                        clips.Add(new Clip(match.Groups[1].Value + ".mp4"));
                    }
                }
                if(clips.Count > 0)
                {
                    for(int i = 0; i < clips.Count; i++)
                    {
                        ApplyEffects(clips[i], i, clips.Count);
                    }
                    ConsoleOutput.WriteLine("Concatenating clips...", Color.LightGreen);
                    progressText = L.T(0, "Generate:StatusConcateClips");
                    progressState = ProgressState.Concatenating;
                    FFprobe_CombineVideo(clips, tempOutput);
                    bool finished = true;
                    // Save to library if it exists.
                    if (File.Exists(tempOutput))
                    {
                        ConsoleOutput.WriteLine("Saving to library...", Color.LightGreen);
                        LibraryFile libraryFile = new LibraryFile(Global.videoTitle, tempOutput, DefaultLibraryTypes.Render);
                        progressText = L.T(0, "Generate:StatusLibrarySave");
                        if(LibraryData.Load(libraryFile) == null)
                        {
                            ConsoleOutput.WriteLine("Failed to save to library.", Color.Red);
                            progressText = L.T(0, "Generate:StatusFailLibrarySave");
                            progressState = ProgressState.Failed;
                            failureReason = L.T(0, "Generate:StatusFailLibrarySave");
                            finished = false;
                        }
                        else
                        {
#if MONOGAME
                            if (bool.Parse(SaveData.saveValues["PlayAutomatically"]))
                                FramePlayer.PlayMedia(libraryFile);
#endif
                        }
                    }
                    else
                    {
                        ConsoleOutput.WriteLine("Concatenation failed.", Color.Red);
                        progressText = L.T(0, "Generate:StatusFailConcatenation");
                        progressState = ProgressState.Failed;
                        failureReason = L.T(0, "Generate:StatusFailConcatenation");
                        finished = false;
                    }
                    if(finished)
                    {
                        progressText = L.T(0, "Generate:StatusCompleted");
                        progressState = ProgressState.Completed;
                        generatorActive = false;
                        // award achievements
                        if (SteamManager.initialized && Global.canAchieve)
                        {
                            List<string> achievements = new()
                            {
                                "ACHIEVEMENT_FIRST_RENDER",
                            };
                            if(Global.usedWorkshopPlugin)
                            {
                                Global.usedWorkshopPlugin = false;
                                achievements.Add("ACHIEVEMENT_WORKSHOP_USAGE");
                            }
                            if(Global.rolledForOverlay)
                            {
                                Global.rolledForOverlay = false;
                                achievements.Add("ACHIEVEMENT_CHROMA_KEY");
                            }
                            if(Global.usedAllEffectChance)
                            {
                                Global.usedAllEffectChance = false;
                                achievements.Add("ACHIEVEMENT_ALL_EFFECTS");
                            }
                            if(Global.usedDifferentOutro)
                            {
                                Global.usedDifferentOutro = false;
                                achievements.Add("ACHIEVEMENT_OUTRO_OVERRIDE");
                            }
                            foreach(string achievement in achievements)
                            {
                                Achievements.Award(achievement);
                            }
                        }
                        SaveData.saveValues["TotalVideosRendered"] = (int.Parse(SaveData.saveValues["TotalVideosRendered"], CultureInfo.InvariantCulture) + 1).ToString(CultureInfo.InvariantCulture);
                        SaveData.Save();
                        GlobalContent.GetSound("RenderComplete").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                        /*
                        // Open the video in the default video player if the user has that option enabled.
                        if (bool.Parse(SaveData.saveValues["PlayAutomatically"]))
                        {
                            ProcessStartInfo startInfo = new()
                            {
                                FileName = tempOutput,
                                UseShellExecute = true
                            };
                            Process.Start(startInfo);
                        }
                        */
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
            while (bool.Parse(SaveData.saveValues["EnableTimeOut"]))
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
        public void ApplyEffects(Clip thisClip, int i, int maxClips)
        {
            try
            {
                if(thisClip.intro)
                    return;
                if(!thisClip.rolledForTransition || bool.Parse(SaveData.saveValues["TransitionEffects"]))
                {
                    int numberOfPlugins = PluginHandler.GetPluginCount(true);
                    if(numberOfPlugins > 0)
                    {
                        // Roll for effect
                        RollReason rollReason = RollReason.NoRoll;
                        int roll = RandomInt(0, 100);
                        if(thisClip.rolledForTransition && int.Parse(SaveData.saveValues["TransitionEffectChance"], CultureInfo.InvariantCulture) == 100)
                            rollReason = RollReason.TransitionEffectChance100Percent;
                        else if(!thisClip.rolledForTransition && int.Parse(SaveData.saveValues["EffectChance"], CultureInfo.InvariantCulture) == 100)
                            rollReason = RollReason.EffectChance100Percent;
                        else if(thisClip.rolledForTransition && roll < int.Parse(SaveData.saveValues["TransitionEffectChance"], CultureInfo.InvariantCulture))
                            rollReason = RollReason.TransitionEffect;
                        else if(!thisClip.rolledForTransition && roll < int.Parse(SaveData.saveValues["EffectChance"], CultureInfo.InvariantCulture))
                            rollReason = RollReason.Effect;
                        switch(rollReason)
                        {
                            case RollReason.TransitionEffectChance100Percent:
                                ConsoleOutput.WriteLine("Clip " + i + ": Transition effect chance is 100%", Color.Gray);
                                break;
                            case RollReason.EffectChance100Percent:
                                ConsoleOutput.WriteLine("Clip " + i + ": Effect chance is 100%", Color.Gray);
                                break;
                            case RollReason.TransitionEffect:
                                ConsoleOutput.WriteLine("Clip " + i + ": Rolled for transition effect.", Color.Gray);
                                break;
                            case RollReason.Effect:
                                ConsoleOutput.WriteLine("Clip " + i + ": Rolled for effect.", Color.Gray);
                                break;
                            default:
                                ConsoleOutput.WriteLine("Clip " + i + ": No effect rolled.", Color.Gray);
                                break;
                        }
                        if(rollReason != RollReason.NoRoll)
                        {
                            progressText = L.T(0, thisClip.rolledForTransition ? "Generate:StatusApplyTransitionEffect" : "Generate:StatusApplyEffect", (i + 1).ToString(CultureInfo.InvariantCulture), maxClips.ToString(CultureInfo.InvariantCulture));
                            // We rolled for an effect, let's pick one.
                            PluginReturnValue effect = PluginHandler.PickRandom(globalRandom, Path.Combine(temporaryDirectory, thisClip.name));
                            if(effect.success)
                            {
                                if(int.Parse(SaveData.saveValues["EffectChance"], CultureInfo.InvariantCulture) >= 100)
                                {
                                    Global.usedAllEffectChance = true;
                                }
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
                                        string isVideo = "";
                                        ffprobeProcess.OutputDataReceived += (sender, e) =>
                                        {
                                            isVideo += e.Data;
                                        };
                                        ffprobeProcess.Start();
                                        ffprobeProcess.BeginOutputReadLine();
                                        ffprobeProcess.WaitForExit();
                                        if (isVideo != "" && isVideo != "N/A")
                                        {
                                            foundOutput = true;
                                        }
                                        break;
                                    }
                                }
                                if(foundOutput)
                                {
                                    // Delete existing videoi.mp4
                                    if(File.Exists(Path.Combine(temporaryDirectory, thisClip.name)))
                                        File.Delete(Path.Combine(temporaryDirectory, thisClip.name));
                                    try
                                    {
                                        File.Move(effect.jobFolder + "output.mp4", Path.Combine(temporaryDirectory, thisClip.name));
                                    }
                                    catch(Exception ex)
                                    {
                                        ConsoleOutput.WriteLine("Failed to move output.mp4 to " + thisClip.name +": " + ex.Message, Color.Red);
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
                            ConsoleOutput.WriteLine(effect.success ? "Applied "+effect.pluginName+" to " + (thisClip.rolledForTransition ? "transition" : "clip") + " " + i + "." : "Failed to apply "+effect.pluginName+" to " + (thisClip.rolledForTransition ? "transition" : "clip") + " " + i + ".", effect.success ? Color.LightGreen : Color.Red);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Achievements.Award("ACHIEVEMENT_LUA_ERROR");
                ConsoleOutput.WriteLine(ex.Message, Color.Red);
                ConsoleOutput.WriteLine("Failed to apply effect to clip " + i + ".", Color.Red);
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

            DiscordRPC.UpdatePresence();

            // Load library.
            progressText = L.T(0, "Generate:StatusLibraryParse");
            LibraryData.Load();

            int maxClips = int.Parse(SaveData.saveValues["MaxClipCount"], CultureInfo.InvariantCulture);

            // Check to ensure that the source pool is not empty.
            if(LibraryData.GetFileCount(DefaultLibraryTypes.Material) == 0 && maxClips > 0)
            {
                ConsoleOutput.WriteLine("No material files found in library.", Color.Red);
                failureReason = L.T(0, "Generate:StatusFailLibraryEmpty");
                progressText = failureReason;
                CancelGeneration();
                return;
            }

            // Set global random with seed.
            if(Global.randomSeed != 0 && !Global.parameters.Contains("-seed"))
            {
                progressText = L.T(0, "Generate:StatusSeed");
                int seed = DateTime.UtcNow.Millisecond;
                // Convert ProjectTitle to int seed
                /*
                string seedString = Global.videoTitle;
                int seed = 0;
                foreach (char c in seedString)
                {
                    seed += (int)c;
                }
                */
                ConsoleOutput.WriteLine("Seed: " + seed.ToString(CultureInfo.InvariantCulture), Color.Gray);
                Global.randomSeed = seed;
                globalRandom = new Random(seed);
            }
            
            ConsoleOutput.WriteLine("Max clips: " + maxClips.ToString(CultureInfo.InvariantCulture), Color.Gray);

            // Clean up previous temporary files.
            progressText = L.T(0, "Generate:StatusCleanUp");
            CleanUp();

            if (vidThreadWorker?.CancellationPending == true)
                return;

            // Make sure the temporary directory exists.
            Directory.CreateDirectory(temporaryDirectory);

            progressText = L.T(0, "Generate:StatusStarting");
            progressState = ProgressState.Rendering;
            List<Clip> clips = new();
            try
            {
                for (int i = 0; i < maxClips; i++)
                {
                    try
                    {
                        Clip thisClip = new Clip(
                            i + ".mp4",
                            LibraryData.GetFileCount(DefaultLibraryTypes.Overlay) > 0 && RandomInt(0, 100) < int.Parse(SaveData.saveValues["OverlayChance"], CultureInfo.InvariantCulture),
                            LibraryData.GetFileCount(DefaultLibraryTypes.Transition) > 0 && RandomInt(0, 100) < int.Parse(SaveData.saveValues["TransitionChance"], CultureInfo.InvariantCulture),
                            false
                        );
                        timeout = int.Parse(SaveData.saveValues["TimeOut"], CultureInfo.InvariantCulture);
                        if (vidThreadWorker?.CancellationPending == true)
                            return;
                        ConsoleOutput.WriteLine("Starting clip " + (i + 1) + "/" + maxClips + "...", Color.Gray);
                        progressText = L.T(0, "Generate:StatusClipStart", (i + 1).ToString(CultureInfo.InvariantCulture), maxClips.ToString(CultureInfo.InvariantCulture));
                        if (i == 0 && bool.Parse(SaveData.saveValues["IntrosEnabled"]))
                        {
                            // Add the intro.
                            string introPath = LibraryData.PickRandom(DefaultLibraryTypes.Intro, globalRandom);
                            if(introPath != "")
                            {
                                thisClip.intro = true;
                                maxClips++;
                                ConsoleOutput.WriteLine("Intro clip enabled, adding 1 to max clips. New max clips is " + maxClips + ".", Color.Gray);
                                progress = Convert.ToInt32(i / maxClips, CultureInfo.InvariantCulture);
                                progressText = L.T(0, "Generate:StatusApplyIntro", (i + 1).ToString(CultureInfo.InvariantCulture), maxClips.ToString(CultureInfo.InvariantCulture));
                                FFprobe_EncodeVideo(introPath, Path.Combine(temporaryDirectory, thisClip.name));
                            }
                        }
                        if (vidThreadWorker?.CancellationPending == true)
                            return;
                        if(!thisClip.intro)
                        {
                            string overlayPath = "";
                            progress = Convert.ToInt32(i / maxClips, CultureInfo.InvariantCulture);
                            ConsoleOutput.WriteLine("Clipping... (" + (i + 1) + "/" + maxClips + ")", Color.Gray);
                            progressText = L.T(0, "Generate:StatusClipping", (i + 1).ToString(CultureInfo.InvariantCulture), maxClips.ToString(CultureInfo.InvariantCulture));
                            string sourceToPick = LibraryData.PickRandom(DefaultLibraryTypes.Material, globalRandom);
                            float source = -1;
                            if(sourceToPick == "")
                            {
                                ConsoleOutput.WriteLine("No material files found in library.", Color.Gray);
                                progressText = L.T(0, "Generate:StatusFailLibraryEmpty");
                                progressState = ProgressState.Failed;
                                continue;
                            }
                            else
                            {
                                source = float.Parse(FFprobe_Length(sourceToPick), CultureInfo.InvariantCulture);
                            }
                            ConsoleOutput.WriteLine("Length: " + source, Color.Gray);
                            string output = source.ToString(CultureInfo.InvariantCulture);
                            //ConsoleOutput.WriteLine(FFprobe_Length(sourceToPick) + " -> " + output + " -> " + float.Parse(output, CultureInfo.InvariantCulture));
                            float outputDuration = float.Parse(output, CultureInfo.InvariantCulture);
                            float startOfClip = RandomFloat(0f, outputDuration - float.Parse(SaveData.saveValues["MinStreamDuration"], CultureInfo.InvariantCulture));
                            float endOfClip = startOfClip + RandomFloat(float.Parse(SaveData.saveValues["MinStreamDuration"], CultureInfo.InvariantCulture), float.Parse(SaveData.saveValues["MaxStreamDuration"], CultureInfo.InvariantCulture));
                            // Ensure that the start is not less than 0 and the end is not greater than the source length.
                            if (startOfClip < 0)
                                startOfClip = 0;
                            if (endOfClip > outputDuration)
                                endOfClip = outputDuration;
                            if (vidThreadWorker?.CancellationPending == true)
                                return;
                            // Add an overlay to the video, if rolled for.
                            if(thisClip.rolledForOverlay)
                            {
                                // Get random overlay.
                                overlayPath = LibraryData.PickRandom(DefaultLibraryTypes.Overlay, globalRandom);
                                if(overlayPath == "")
                                {
                                    thisClip.rolledForOverlay = false;
                                }
                            }
                            if(sourceToPick != "")
                                ConsoleOutput.WriteLine(Path.GetFileName(sourceToPick) + " ("+i+") - " + (endOfClip - startOfClip) + " (" + startOfClip + " to " + endOfClip + ")", Color.Gray);
                            // Insert transition if rolled, ensure that there is a transition as well.
                            //bool alreadySnipped = false;
                            if (!thisClip.rolledForOverlay && thisClip.rolledForTransition && LibraryData.GetFileCount(DefaultLibraryTypes.Transition) > 0)
                            {
                                string transitionPath = LibraryData.PickRandom(DefaultLibraryTypes.Transition, globalRandom);
                                if(transitionPath != "")
                                {
                                    progressText = L.T(0, "Generate:StatusApplyTransition", (i + 1).ToString(CultureInfo.InvariantCulture), maxClips.ToString(CultureInfo.InvariantCulture));
                                    ConsoleOutput.WriteLine("Transitioning...", Color.Gray);
                                    FFprobe_EncodeVideo(transitionPath, Path.Combine(temporaryDirectory, thisClip.name));
                                }
                                else
                                {
                                    thisClip.rolledForTransition = false;
                                }
                            }
                            else
                            {
                                // No transition, just snip the video.
                                if(thisClip.rolledForOverlay && bool.Parse(SaveData.saveValues["PlayOverlayInFull"]))
                                {
                                    // Snip video to overlay length at startOfClip (override endOfClip)
                                    float overlayDuration = float.Parse(FFprobe_Length(overlayPath), CultureInfo.InvariantCulture);
                                    endOfClip = startOfClip + overlayDuration;
                                    FFprobe_ClipVideo(sourceToPick, startOfClip, endOfClip, Path.Combine(temporaryDirectory, thisClip.name));
                                }
                                else
                                {
                                    FFprobe_ClipVideo(sourceToPick, startOfClip, endOfClip, Path.Combine(temporaryDirectory, thisClip.name));
                                }
                                SaveData.saveValues["TotalClipsTrimmed"] = (int.Parse(SaveData.saveValues["TotalClipsTrimmed"], CultureInfo.InvariantCulture) + 1).ToString(CultureInfo.InvariantCulture);
                                SaveData.Save();
                                ConsoleOutput.WriteLine("Snipped video.", Color.Gray);
                            }
                            if (vidThreadWorker?.CancellationPending == true)
                                return;
                            // Parse overlay if rolled.
                            if(thisClip.rolledForOverlay)
                            {
                                if(overlayPath != null)
                                {
                                    progressText = L.T(0, "Generate:StatusApplyOverlay", (i + 1).ToString(CultureInfo.InvariantCulture), maxClips.ToString(CultureInfo.InvariantCulture));
                                    ConsoleOutput.WriteLine("Rolled for overlay, adding overlay to clip " + i + ".", Color.Gray);
                                    // Get base name of overlay
                                    string overlayName = Path.GetFileNameWithoutExtension(overlayPath);
                                    // Interpret __color_00FF00, __similarity_030, and __blend_020 if they're specified in any order
                                    string chromaKeyColor = "00FF00";
                                    string chromaKeySimilarity = "0.3";
                                    string chromaKeyBlend = "0.2";
                                    bool overlayChanged = false;
                                    string[] overlayNameSplit = overlayName.Split("__");
                                    foreach (string s in overlayNameSplit)
                                    {
                                        if (s.Contains("color_"))
                                        {
                                            string hexCode = s.Substring(s.IndexOf("color_") + 6, 6);
                                            // Check if it's valid hexedecimal
                                            if (hexCode.Length == 6 && Regex.IsMatch(hexCode, @"\A\b[0-9a-fA-F]+\b\Z"))
                                                chromaKeyColor = hexCode;
                                            overlayChanged = true;
                                        }
                                        if (s.Contains("similarity_"))
                                        {
                                            string similarity = s.Substring(s.IndexOf("similarity_") + 11, 3);
                                            // Convert from 000 to 0.00
                                            if (similarity.Length == 3 && Regex.IsMatch(similarity, @"\A\b[0-9]+\b\Z"))
                                                chromaKeySimilarity = (float.Parse(similarity, CultureInfo.InvariantCulture) / 100).ToString(CultureInfo.InvariantCulture);
                                            // Trim leading zero at 4th character
                                            if (chromaKeySimilarity.Length == 4 && chromaKeySimilarity[2] == '0' && chromaKeySimilarity[3] == '0')
                                                chromaKeySimilarity = chromaKeySimilarity.Substring(0, 2);
                                            overlayChanged = true;
                                        }
                                        if (s.Contains("blend_"))
                                        {
                                            string blend = s.Substring(s.IndexOf("blend_") + 6, 3);
                                            // Convert from 000 to 0.00
                                            if (blend.Length == 3 && Regex.IsMatch(blend, @"\A\b[0-9]+\b\Z"))
                                                chromaKeyBlend = (float.Parse(blend, CultureInfo.InvariantCulture) / 100).ToString(CultureInfo.InvariantCulture);
                                            // Trim leading zero at 4th character
                                            if (chromaKeyBlend.Length == 4 && chromaKeyBlend[2] == '0' && chromaKeyBlend[3] == '0')
                                                chromaKeyBlend = chromaKeyBlend.Substring(0, 2);
                                            overlayChanged = true;
                                        }
                                    }
                                    // If the overlay has changed, print out the new values
                                    if (overlayChanged)
                                        ConsoleOutput.WriteLine("Overlay has chroma key values: color=" + chromaKeyColor + ", similarity=" + chromaKeySimilarity + ", blend=" + chromaKeyBlend, Color.Gray);
                                    // We snip the clip here in case it was a transition
                                    if(!bool.Parse(SaveData.saveValues["PlayOverlayInFull"]))
                                    {
                                        // Now we'll snip the overlay with another random duration.
                                        float overlayDuration = float.Parse(FFprobe_Length(overlayPath), CultureInfo.InvariantCulture);
                                        float startOfOverlay = RandomFloat(0f, overlayDuration - float.Parse(SaveData.saveValues["MinStreamDuration"], CultureInfo.InvariantCulture));
                                        float endOfOverlay = startOfOverlay + RandomFloat(float.Parse(SaveData.saveValues["MinStreamDuration"], CultureInfo.InvariantCulture), float.Parse(SaveData.saveValues["MaxStreamDuration"], CultureInfo.InvariantCulture));
                                        // Make sure the start is not less than 0 and the end is not greater than the overlay length.
                                        if (startOfOverlay < 0)
                                            startOfOverlay = 0;
                                        if (endOfOverlay > overlayDuration)
                                            endOfOverlay = overlayDuration;
                                        FFprobe_ClipVideo(overlayPath, startOfOverlay, endOfOverlay, Path.Combine(temporaryDirectory, i + "_tempoverlay.mp4"));
                                        OverlayVideo(Path.Combine(temporaryDirectory, thisClip.name), Path.Combine(temporaryDirectory, i + "_tempoverlay.mp4"), chromaKeyColor, chromaKeySimilarity, chromaKeyBlend);
                                    }
                                    else
                                    {
                                        // Overlay video on top of clip.
                                        OverlayVideo(Path.Combine(temporaryDirectory, thisClip.name), overlayPath);
                                    }
                                }
                            }
                            if (vidThreadWorker?.CancellationPending == true)
                                return;
                        }
                        clips.Add(thisClip);
                    }
                    catch
                    {
                        ConsoleOutput.WriteLine("Failed to generate clip " + (i + 1) +".", Color.Red);
                    }
                }
                if (vidThreadWorker?.CancellationPending == true)
                    return;
                // Finished, throw to get into catch block.
                throw new Exception("Finished");
            }
            catch(Exception ex2)
            {
                // not finished
                if (ex2.Message != "Finished")
                {
                    ConsoleOutput.WriteLine(ex2.Message, Color.Red);
                    // print line number
                    if(ex2.StackTrace != null)
                    {
                        ConsoleOutput.WriteLine(ex2.StackTrace, Color.Red);
                    }
                }
                try
                {
                    if (vidThreadWorker?.CancellationPending == true)
                        return;
                    for(int i = 0; i < clips.Count; i++)
                    {
                        if (vidThreadWorker?.CancellationPending == true)
                            return;
                        ApplyEffects(clips[i], i, clips.Count);
                    }
                    if (vidThreadWorker?.CancellationPending == true)
                        return;
                    // Concatenate all clips into one video.
                    ConsoleOutput.WriteLine("Concatenating clips...", Color.LightGreen);
                    progressText = L.T(0, "Generate:StatusConcateClips");
                    progressState = ProgressState.Concatenating;
                    FFprobe_CombineVideo(clips, tempOutput);
                    if (vidThreadWorker?.CancellationPending == true)
                        return;
                    bool finished = true;
                    // Try to call post-render effects.
                    int numberOfPlugins = PluginHandler.GetPluginCount(true);
                    if(numberOfPlugins > 0)
                    {
                        progressText = L.T(0, "Generate:StatusApplyPostRenderEffect");
                        // We rolled for an effect, let's pick one.
                        PluginReturnValue effect = PluginHandler.PickRandom(globalRandom, tempOutput, AddonType.PostRenderEffect);
                        if(effect.success)
                        {
                            // Check if effect job path contains output.mp4, if so, plugin was indeed successful.
                            // so replace tempOutput with output.mp4
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
                                    string isVideo = "";
                                    ffprobeProcess.OutputDataReceived += (sender, e) =>
                                    {
                                        isVideo += e.Data;
                                    };
                                    ffprobeProcess.Start();
                                    ffprobeProcess.BeginOutputReadLine();
                                    ffprobeProcess.WaitForExit();
                                    if (isVideo != "" && isVideo != "N/A")
                                    {
                                        foundOutput = true;
                                    }
                                    break;
                                }
                            }
                            if(foundOutput)
                            {
                                // Delete existing tempOutput
                                if(File.Exists(tempOutput))
                                    File.Delete(tempOutput);
                                try
                                {
                                    File.Move(effect.jobFolder + "output.mp4", tempOutput);
                                }
                                catch(Exception ex)
                                {
                                    ConsoleOutput.WriteLine("Failed to move output.mp4 to " + tempOutput +": " + ex.Message, Color.Red);
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
                        if(effect.pluginName != "")
                            ConsoleOutput.WriteLine(effect.success ? "Applied "+effect.pluginName+" to render." : "Failed to apply "+effect.pluginName+" to render.", effect.success ? Color.LightGreen : Color.Red);
                    }
                    // Save to library if it exists.
                    if (File.Exists(tempOutput))
                    {
                        ConsoleOutput.WriteLine("Saving to library...", Color.LightGreen);
                        LibraryFile libraryFile = new LibraryFile(Global.videoTitle, tempOutput, DefaultLibraryTypes.Render);
                        progressText = L.T(0, "Generate:StatusLibrarySave");
                        if(LibraryData.Load(libraryFile) == null)
                        {
                            ConsoleOutput.WriteLine("Failed to save to library.", Color.Red);
                            progressText = L.T(0, "Generate:StatusFailLibrarySave");
                            progressState = ProgressState.Failed;
                            failureReason = L.T(0, "Generate:StatusFailLibrarySave");
                            finished = false;
                            CancelGeneration();
                        }
                        else
                        {
#if MONOGAME
                            if (bool.Parse(SaveData.saveValues["PlayAutomatically"]))
                                FramePlayer.PlayMedia(libraryFile);
#endif
                        }
                    }
                    else
                    {
                        ConsoleOutput.WriteLine("Concatenation failed.", Color.Red);
                        progressText = L.T(0, "Generate:StatusFailConcatenation");
                        progressState = ProgressState.Failed;
                        failureReason = L.T(0, "Generate:StatusFailConcatenation");
                        finished = false;
                        CancelGeneration();
                    }
                    if(finished)
                    {
                        if (vidThreadWorker?.CancellationPending == true)
                            return;
                        progressText =  L.T(0, "Generate:StatusCompleted");
                        progressState = ProgressState.Completed;
                        generatorActive = false;
                        if(vidThreadWorker != null)
                            vidThreadWorker.ReportProgress(100);
                        if(timeoutWorker != null)
                            timeoutWorker.CancelAsync();
                        // Open the video in the default video player if the user has that option enabled.
                        /*
                        if (bool.Parse(SaveData.saveValues["PlayAutomatically"]))
                        {
                            ProcessStartInfo startInfo = new()
                            {
                                FileName = tempOutput,
                                UseShellExecute = true
                            };
                            Process.Start(startInfo);
                        }
                        */
                    }
                    else
                    {
                        throw new Exception("Failed to save to library.");
                    }
                }
                catch(Exception ex)
                {
                    progressState = ProgressState.Failed;
                    failureReason = L.T(0, "Generate:StatusFailGeneric");
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
            // Delete all paths in calledMedia with LibraryData.Unload()
            if(bool.Parse(SaveData.saveValues["DeleteClipsAfterMaxUniqueClips"])
                || bool.Parse(SaveData.saveValues["DisableClipsAfterMaxUniqueClips"]))
            {
                for (int i = 0; i < LibraryData.calledMedia.Count; i++)
                {
                    if(bool.Parse(SaveData.saveValues["DeleteClipsAfterMaxUniqueClips"]))
                        LibraryData.Unload(LibraryData.calledMedia[i]);
                    else
                        LibraryData.SetEnabled(LibraryData.calledMedia[i], false);
                }
            }
            LibraryData.calledMedia.Clear();
            Global.usedWorkshopPlugin = false;
            Global.rolledForOverlay = false;
            Global.usedAllEffectChance = false;
#if MONOGAME
            FramePlayer.Stop();
#endif
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
                timeout = int.Parse(SaveData.saveValues["TimeOut"], CultureInfo.InvariantCulture);
                tempOutput = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "library", "video", "renders", Global.videoTitle + ".mp4");
                timeoutWorker.RunWorkerAsync();
                vidThreadWorker.RunWorkerAsync();
                ConsoleOutput.WriteLine("Generation started.", Color.Green);
                // Print out current save data
                ConsoleOutput.WriteLine("Save data:", Color.Transparent);
                ConsoleOutput.WriteLine("{", Color.Transparent);
                foreach(KeyValuePair<string, string> kvp in SaveData.saveValues)
                {
                    ConsoleOutput.WriteLine("  \"" + kvp.Key + "\": \"" + kvp.Value + "\"", Color.Transparent);
                }
                ConsoleOutput.WriteLine("}", Color.Transparent);
            }
            catch(Exception ex)
            {
                progressState = ProgressState.Failed;
                failureReason = L.T(0, "Generate:StatusFailGeneric");
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
                        failureReason = L.T(0, "Generate:StatusCancelling");
                        progressText = failureReason;
                        ConsoleOutput.WriteLine("Generation cancelling...", Color.Yellow);
                        StartKillThread();
                    }
                    else
                    {
                        failureReason = L.T(0, "Generate:StatusCancelled");
                        progressText = failureReason;
                        ConsoleOutput.WriteLine("Generation cancelled.", Color.Red);
                    }
                }
                else
                {
                    try
                    {
                        vidThreadWorker.ReportProgress(1);
                        // Play error sting (music)
                        //UserInterface.instance.music = 1;
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
        public static string FFprobe_Length(string file)
        {
            try
            {
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = Global.useSystemFFprobe ? "ffprobe" : @".\ffprobe.exe";
                startInfo.Arguments = "-i \"" + file
                        + "\" -show_entries format=duration"
                        + " -v quiet"
                        + " -of csv=\"p=0\"";
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";
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
                Global.generator.failureReason = L.T(0, "Generate:StatusFailLength");
                Global.generator.progressText = Global.generator.failureReason;
                Global.generator.CancelGeneration();
                return "0";
            }
        }
        public static void FFprobe_ClipVideo(string video, double startTime, double endTime, string output)
        {
            FFprobe_ClipVideo(video, startTime.ToString(CultureInfo.InvariantCulture), endTime.ToString(CultureInfo.InvariantCulture), output);
        }
        public static void FFprobe_ClipVideo(string video, string startTime, string endTime, string output)
        {
            try
            {
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = Global.useSystemFFmpeg ? "ffmpeg" : @".\ffmpeg.exe";
                startInfo.Arguments = "-i \"" + video
                        + "\" -ss " + startTime
                        + " -to " + endTime
                        + (SaveData.saveValues["ConstrainAspectRatio"] == "true" ? " -vf scale=" + SaveData.saveValues["VideoWidth"] + "x" + SaveData.saveValues["VideoHeight"] + ",setsar=1:1,fps=fps=30" : " -vf \"scale=(iw*sar)*min(" + SaveData.saveValues["VideoWidth"] + "/(iw*sar)\\," + SaveData.saveValues["VideoHeight"] + "/ih):ih*min(" + SaveData.saveValues["VideoWidth"] + "/(iw*sar)\\," + SaveData.saveValues["VideoHeight"] + "/ih),pad=" + SaveData.saveValues["VideoWidth"] + ":" + SaveData.saveValues["VideoHeight"] + ":(" + SaveData.saveValues["VideoWidth"] + "-iw*min(" + SaveData.saveValues["VideoWidth"] + "/iw\\," + SaveData.saveValues["VideoHeight"] + "/ih))/2:(" + SaveData.saveValues["VideoHeight"] + "-ih*min(" + SaveData.saveValues["VideoWidth"] + "/iw\\," + SaveData.saveValues["VideoHeight"] + "/ih))/2,setsar=1:1,fps=fps=30\"")
                        + (Global.generator.audioSync == true ? " -af aresample=async=1000" : "")
                        + " "+exportParams
                        + " -y"
                        + " \"" + output + "\"";
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardError = true;
                startInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";
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
                Global.generator.failureReason = L.T(0, "Generate:StatusFailClipping");
                Global.generator.progressText = Global.generator.failureReason;
                Global.generator.CancelGeneration();
            }
        }

        // Check if a video has audio
        public static bool HasAudio(string video)
        {
            try
            {
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = Global.useSystemFFprobe ? "ffprobe" : @".\ffprobe.exe";
                startInfo.Arguments = "-i \"" + video
                        + "\" -show_streams"
                        + " -v quiet"
                        + " -of csv=\"p=0\"";
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";
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
                Global.generator.failureReason = L.T(0, "Generate:StatusFailCheckAudio");
                Global.generator.progressText = Global.generator.failureReason;
                Global.generator.CancelGeneration();
                return false;
            }
        }

        public static void FFprobe_EncodeVideo(string video, string output)
        {
            try
            {
                // If the video has no audio, add silence to it
                bool noAudio = HasAudio(video);
                string appendNoAudio = "";
                if(!noAudio)
                {
                    ConsoleOutput.WriteLine("Video has no audio, adding silence...", Color.Gray);
                    // Get video length
                    string length = FFprobe_Length(video);
                    appendNoAudio = "-f lavfi -i anullsrc=channel_layout=mono:sample_rate=44100 -t " + length + " ";
                }
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = Global.useSystemFFmpeg ? "ffmpeg" : @".\ffmpeg.exe";
                startInfo.Arguments = "-i \"" + video + "\" " + appendNoAudio
                        + (SaveData.saveValues["ConstrainAspectRatio"] == "true" ? " -vf scale=" + SaveData.saveValues["VideoWidth"] + "x" + SaveData.saveValues["VideoHeight"] + ",setsar=1:1,fps=fps=30" : " -vf \"scale=(iw*sar)*min(" + SaveData.saveValues["VideoWidth"] + "/(iw*sar)\\," + SaveData.saveValues["VideoHeight"] + "/ih):ih*min(" + SaveData.saveValues["VideoWidth"] + "/(iw*sar)\\," + SaveData.saveValues["VideoHeight"] + "/ih),pad=" + SaveData.saveValues["VideoWidth"] + ":" + SaveData.saveValues["VideoHeight"] + ":(" + SaveData.saveValues["VideoWidth"] + "-iw*min(" + SaveData.saveValues["VideoWidth"] + "/iw\\," + SaveData.saveValues["VideoHeight"] + "/ih))/2:(" + SaveData.saveValues["VideoHeight"] + "-ih*min(" + SaveData.saveValues["VideoWidth"] + "/iw\\," + SaveData.saveValues["VideoHeight"] + "/ih))/2,setsar=1:1,fps=fps=30\"")
                        + (Global.generator.audioSync == true ? " -af aresample=async=1000" : "")
                        + " "+exportParams
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
                Global.generator.failureReason = L.T(0, "Generate:StatusFailCopying");
                Global.generator.progressText = Global.generator.failureReason;
                Global.generator.CancelGeneration();
            }
        }
        public static void FFprobe_Error(object sender, DataReceivedEventArgs e)
        {
            if(e.Data != null)
            {
                ConsoleOutput.WriteLine(e.Data, Color.Transparent);
                // Conversion failed?
                if (e.Data.Contains("Conversion failed!"))
                {
                    // We don't want to try to concatenate again
                    ConsoleOutput.WriteLine("Fatal error while concatenating videos.");
                    Global.generator.failureReason = L.T(0, "Generate:StatusFailConcatenate");
                    Global.generator.progressText = Global.generator.failureReason;
                    Global.generator.CancelGeneration();
                }
            }
        }
        public static void FFprobe_CombineVideo(List<Clip> clips, string ou)
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
                // Add outro if enabled
                if (bool.Parse(SaveData.saveValues["OutrosEnabled"]))
                {
                    string outroPath = LibraryData.PickRandom(DefaultLibraryTypes.Outro, Global.generator.globalRandom);
                    if(outroPath == "")
                    {
                        ConsoleOutput.WriteLine("No outros found in library.", Color.Yellow);
                    }
                    else
                    {
                        Clip thisClip = new Clip(clips.Count + ".mp4");
                        if(!outroPath.Contains("defaultoutro.mp4"))
                            Global.usedDifferentOutro = true;
                        Global.generator.progressText = L.T(0, "Generate:StatusApplyOutro");
                        ConsoleOutput.WriteLine("Outro clip enabled, adding 1 to max clips.", Color.Gray);
                        FFprobe_EncodeVideo(outroPath, Path.Combine(temporaryDirectory, thisClip.name));
                        clips.Add(thisClip);
                    }
                }

                // Re-encode all files in temporary directory that start with video to video0, video1, etc.
                int i2 = 0;
                ConsoleOutput.WriteLine("Re-encoding...", Color.Gray);
                for (int i = 0; i < clips.Count; i++)
                {
                    Clip thisClip = clips[i];
                    if (File.Exists(Path.Combine(temporaryDirectory, thisClip.name)))
                    {
                        Global.generator.progressText = L.T(0, "Generate:StatusEncode", (i + 1).ToString(CultureInfo.InvariantCulture), clips.Count.ToString(CultureInfo.InvariantCulture));
                        // Run ffmpeg to re-encode the video and add audio if it doesn't have any
                        string appendNoAudio = "";
                        if (!HasAudio(Path.Combine(temporaryDirectory, thisClip.name)))
                        {
                            ConsoleOutput.WriteLine("Video has no audio, adding silence...", Color.Gray);
                            // Get video length
                            string length = FFprobe_Length(Path.Combine(temporaryDirectory, thisClip.name));
                            appendNoAudio = "-f lavfi -i anullsrc=channel_layout=mono:sample_rate=44100 -t " + length + " ";
                        }
                        Process process2 = new Process();
                        ProcessStartInfo startInfo2 = new ProcessStartInfo();
                        startInfo2.WindowStyle = ProcessWindowStyle.Hidden;
                        startInfo2.FileName = Global.useSystemFFmpeg ? "ffmpeg" : @".\ffmpeg.exe";
                        startInfo2.Arguments = "-i \"" + Path.Combine(temporaryDirectory, thisClip.name) + "\" " + appendNoAudio
                                + (Global.generator.audioSync == true ? " -af \"aresample=async=1000\"" : "")
                                + (SaveData.saveValues["ConstrainAspectRatio"] == "true" ? " -vf scale=" + SaveData.saveValues["VideoWidth"] + "x" + SaveData.saveValues["VideoHeight"] + ",setsar=1:1,fps=fps=30" : " -vf \"scale=(iw*sar)*min(" + SaveData.saveValues["VideoWidth"] + "/(iw*sar)\\," + SaveData.saveValues["VideoHeight"] + "/ih):ih*min(" + SaveData.saveValues["VideoWidth"] + "/(iw*sar)\\," + SaveData.saveValues["VideoHeight"] + "/ih),pad=" + SaveData.saveValues["VideoWidth"] + ":" + SaveData.saveValues["VideoHeight"] + ":(" + SaveData.saveValues["VideoWidth"] + "-iw*min(" + SaveData.saveValues["VideoWidth"] + "/iw\\," + SaveData.saveValues["VideoHeight"] + "/ih))/2:(" + SaveData.saveValues["VideoHeight"] + "-ih*min(" + SaveData.saveValues["VideoWidth"] + "/iw\\," + SaveData.saveValues["VideoHeight"] + "/ih))/2,setsar=1:1,fps=fps=30\"")
                                + " "+exportParams
                                + " -y"
                                + " \"" + Path.Combine(temporaryDirectory, "concat" + i2 + ".mp4") + "\"";
                        startInfo2.UseShellExecute = false;
                        startInfo2.RedirectStandardError = true;
                        startInfo2.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";
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

                List<Clip> clips2 = new List<Clip>();
                ConsoleOutput.WriteLine("Checking for validity...", Color.Gray);
                for (int i = 0; i < i2; i++)
                {
                    Global.generator.progressText = L.T(0, "Generate:StatusCheckValidity", (i + 1).ToString(CultureInfo.InvariantCulture), i2.ToString(CultureInfo.InvariantCulture));
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
                    string isVideo = "";
                    ffprobeProcess.OutputDataReceived += (sender, e) =>
                    {
                        isVideo += e.Data;
                    };
                    ffprobeProcess.Start();
                    ffprobeProcess.BeginOutputReadLine();
                    ffprobeProcess.WaitForExit();
                    if(isVideo != "" && isVideo != "N/A")
                    {
                        clips2.Add(new Clip("concat" + i + ".mp4"));
                    }
                    else
                    {
                        ConsoleOutput.WriteLine("File " + Path.Combine(temporaryDirectory, "concat" + i + ".mp4") + " is not a valid video file, skipping.", Color.Yellow);
                    }
                }

                ConsoleOutput.WriteLine("Concatenating...", Color.Gray);
                Global.generator.progressText = L.T(0, "Generate:StatusConcateClips");
                ConcatenateVideos(clips2, 0, ou);
            }
            catch(Exception ex2)
            {
                ConsoleOutput.WriteLine(ex2.Message);
                ConsoleOutput.WriteLine("Fatal error while concatenating videos.");
                Global.generator.failureReason = L.T(0, "Generate:StatusFailConcatenate");
                Global.generator.progressText = Global.generator.failureReason;
                Global.generator.CancelGeneration();
            }
        }
        // Concats 50 clips at once.
        // Then, concats each iteration of 50 clips into output
        public static void ConcatenateVideos(List<Clip> clips, int iteration, string output)
        {
            ConsoleOutput.WriteLine("Concatenating... (" + (iteration+1) + ")", Color.Gray);
            Global.generator.progressText = L.T(0, "Generate:StatusConcatenating", (iteration+1).ToString(CultureInfo.InvariantCulture));
            // Up to 50 clips at once
            int max = 50;
            if (clips.Count < max)
                max = clips.Count;
            // Create a list of clips to concatenate
            List<Clip> clips2 = new List<Clip>();
            for (int i = 0; i < max; i++)
            {
                clips2.Add(clips[i]);
            }
            // Concatenate them
            string concat = "";
            for (int i = 0; i < clips2.Count; i++)
            {
                concat += "-i \"" + Path.Combine(temporaryDirectory, clips2[i].name) + "\" ";
            }
            // filter_complex
            concat += "-filter_complex \"";
            for (int i = 0; i < clips2.Count; i++)
            {
                concat += "[" + i + ":v:0][" + i + ":a:0]";
            }
            concat += "concat=n=" + clips2.Count + ":v=1:a=1[outv][outa];"
                    + "[outv]scale=" + SaveData.saveValues["VideoWidth"] + "x" + SaveData.saveValues["VideoHeight"] + ",setsar=1:1,fps=fps=30[outv]"
                    + (Global.generator.audioSync == true ? ";[outa]aresample=async=1000[outa]" : "")
                    + "\""
                    + " -map \"[outv]\""
                    + " -map \"[outa]\""
                    + " "+exportParams
                    + " -y";
            // Run ffmpeg to concatenate them
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = Global.useSystemFFmpeg ? "ffmpeg" : @".\ffmpeg.exe";
            startInfo.Arguments = concat + " \"" + Path.Combine(temporaryDirectory, "iteration" + iteration + ".mp4") + "\"";
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardError = true;
            startInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";
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
            // Keep all files
            // Delete the clips from the list
            for (int i = 0; i < max; i++)
            {
                clips.RemoveAt(0);
            }
            // If there are more than 0 clips left, run this function again
            if (clips.Count > 0)
            {
                ConcatenateVideos(clips, iteration + 1, output);
            }
            else
            {
                // Concatenation is done, concat all iterations into output
                ConsoleOutput.WriteLine("Concatenating iterations...", Color.Gray);
                Global.generator.progressText = L.T(0, "Generate:StatusConcateIterations");
                // Create a list of clips to concatenate
                List<Clip> clips3 = new List<Clip>();
                for (int i = 0; i <= iteration; i++)
                {
                    clips3.Add(new Clip("iteration" + i + ".mp4"));
                }
                // Concatenate them
                string concat2 = "";
                for (int i = 0; i < clips3.Count; i++)
                {
                    concat2 += "-i \"" + Path.Combine(temporaryDirectory, clips3[i].name) + "\" ";
                }
                // filter_complex
                concat2 += "-filter_complex \"";
                for (int i = 0; i < clips3.Count; i++)
                {
                    concat2 += "[" + i + ":v:0][" + i + ":a:0]";
                }
                concat2 += "concat=n=" + clips3.Count + ":v=1:a=1[outv][outa];"
                        + "[outv]scale=" + SaveData.saveValues["VideoWidth"] + "x" + SaveData.saveValues["VideoHeight"] + ",setsar=1:1,fps=fps=30[outv]"
                        + (Global.generator.audioSync == true ? ";[outa]aresample=async=1000[outa]" : "")
                        + "\""
                        + " -map \"[outv]\""
                        + " -map \"[outa]\""
                        + " "+exportParams
                        + " -y";
                // Run ffmpeg to concatenate them
                Process process2 = new Process();
                ProcessStartInfo startInfo2 = new ProcessStartInfo();
                startInfo2.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo2.FileName = Global.useSystemFFmpeg ? "ffmpeg" : @".\ffmpeg.exe";
                startInfo2.Arguments = concat2 + " \"" + output + "\"";
                startInfo2.UseShellExecute = false;
                startInfo2.RedirectStandardError = true;
                startInfo2.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";
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
                // Delete all paths in calledMedia with LibraryData.Unload()
                if(bool.Parse(SaveData.saveValues["DeleteClipsAfterMaxUniqueClips"])
                    || bool.Parse(SaveData.saveValues["DisableClipsAfterMaxUniqueClips"]))
                {
                    for (int i = 0; i < LibraryData.calledMedia.Count; i++)
                    {
                        if(bool.Parse(SaveData.saveValues["DeleteClipsAfterMaxUniqueClips"]))
                            LibraryData.Unload(LibraryData.calledMedia[i]);
                        else
                            LibraryData.SetEnabled(LibraryData.calledMedia[i], false);
                    }
                }
                LibraryData.calledMedia.Clear();
            }
        }
        public static void OverlayVideo(string video, string overlay, string chromaKeyColor = "00FF00", string chromaKeySimilarity = "0.3", string chromaKeyBlend = "0.2")
        {
            try
            {
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                string overlayed_video = video.Replace(".mp4", "_chromakey.mp4");
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = Global.useSystemFFmpeg ? "ffmpeg" : @".\ffmpeg.exe";
                startInfo.Arguments = "-i \"" + video
                        + "\" -i \"" + overlay
                        + "\" -filter_complex \""
                        + "[1:v]colorkey=0x" + chromaKeyColor + ":" + chromaKeySimilarity + ":" + chromaKeyBlend + ",scale=" + SaveData.saveValues["VideoWidth"] + "x" + SaveData.saveValues["VideoHeight"] + ",setsar=1:1,fps=fps=30[outv];"
                        + "[0:v][outv]overlay=shortest=1[finalv];"
                        + "[0:a][1:a]amix=inputs=2:duration=shortest" + (Global.generator.audioSync == true ? ",aresample=async=1000" : "") + "[outa]"
                        + "\""
                        + " -map \"[finalv]\""
                        + " -map \"[outa]\""
                        + " "+exportParams
                        + " -y \"" + overlayed_video + "\"";
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardError = true;
                startInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";
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
                Global.generator.progressText = L.T(0, "Generate:StatusFailOverlay");
            }
        }
        public void CleanUp()
        {
            if (Directory.Exists(temporaryDirectory))
            {
                try
                {
                    Directory.Delete(temporaryDirectory, true);
                    ConsoleOutput.WriteLine("Temporary directory deleted.", Color.Gray);
                }
                catch
                {
                    ConsoleOutput.WriteLine("Temporary directory could not be deleted.", Color.Red);
                }
            }
        }
        public static Process GenerateThumbnail(string media, string output, LibraryRootType rootType = LibraryRootType.Video, int width = 29, int height = 23)
        {
            int texScale = int.Parse(SaveData.saveValues["VideoPlaybackScale"], CultureInfo.InvariantCulture);
            int texWidth = width * texScale;
            int texHeight = height * texScale;
            Process process = new Process();
            try
            {
                Dictionary<LibraryRootType, string> rootTypeFilters = new()
                {
                    {LibraryRootType.Video, "-filter_complex \"[0:v]thumbnail,scale=" + texWidth + ":" + texHeight + "[thumb]\" -map \"[thumb]\" -frames:v 1"},
                    {LibraryRootType.Audio, "-filter_complex \"[0:a]aformat=channel_layouts=mono,showwavespic=s=" + texWidth + "x" + texHeight + ":colors=white[thumb]\" -map \"[thumb]\" -frames:v 1"},
                    {LibraryRootType.Image, ""},
                };
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = Global.useSystemFFmpeg ? "ffmpeg" : @".\ffmpeg.exe",
                    Arguments =  "-i \"" + media + "\" " + rootTypeFilters[rootType] + " -y \"" + output + "\"",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".",
                    CreateNoWindow = true
                };
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
            catch
            {
                // Do nothing
            }
            return process;
        }
    }
}
