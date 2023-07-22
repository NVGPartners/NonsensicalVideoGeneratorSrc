using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended.Tweening;

namespace NonsensicalVideoGenerator
{
    public static class FramePlayer
    {
        public static int currentFrame = 0;
        public static List<Texture2D> frames = new();
        public static double timeStarted = 0;
        public static int fps = 30;
        public static bool playing = false;
        public static bool audioPlaying = false;
        public static SoundEffectInstance audio;
        public static bool processing = false;
        public static double startedProcessing = 0;
        public static string currentPath = "";
        public static BackgroundWorker worker = new();
        public static BackgroundWorker progressWorker = new();
        public static BackgroundWorker countWorker = new();
        public static BackgroundWorker audioConvertWorker = new();
        public static int count = 0;
        public static double audioLength = 0;
        public static double currentAudioTime = 0;
        public static bool canPlayBgMusic = true;
        private static void ExtractFramesAndAudio()
        {
            try
            {
                if(worker.CancellationPending || !processing)
                    return;
                try
                {
                    // Delete all files in the directory
                    foreach (string file2 in Directory.GetFiles(".\\temp\\extracted\\frames"))
                    {
                        if(worker.CancellationPending || !processing)
                            return;
                        File.Delete(file2);
                    }
                    // Delete audio file if it exists
                    if (File.Exists(".\\temp\\extracted\\audio.wav"))
                    {
                        File.Delete(".\\temp\\extracted\\audio.wav");
                    }
                }
                catch
                {
                }
                if(worker.CancellationPending || !processing)
                    return;
                int w = GlobalGraphics.Scale(104);
                int h = GlobalGraphics.Scale(82);
                ProcessStartInfo startInfo = new()
                {
                    FileName = "ffmpeg",
                    // 100x78 letterboxed
                    // fastest method
                    Arguments = "-i \""+currentPath+"\" -vf scale=" + w + ":" + h + ":force_original_aspect_ratio=decrease,pad=" + w + ":" + h + ":(ow-iw)/2:(oh-ih)/2 -r " + fps + " -y .\\temp\\extracted\\frames\\%d.bmp .\\temp\\extracted\\audio.wav",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                Process process = new()
                {
                    StartInfo = startInfo
                };
                process.ErrorDataReceived += (sender, args) => {
                    if (args.Data != null)
                        ConsoleOutput.WriteLine(args.Data, Color.Transparent);
                };
                process.Start();
                if(worker.CancellationPending || !processing)
                    return;
                process.BeginErrorReadLine();
                if(worker.CancellationPending || !processing)
                    return;
                process.WaitForExit();
                if(worker.CancellationPending || !processing)
                    return;
                // Load frames
                frames.Clear();
                if(worker.CancellationPending || !processing)
                    return;
                int curcount = Directory.GetFiles(".\\temp\\extracted\\frames").Length;
                count = curcount;
                for (int i = 1; i <= curcount; i++)
                {
                    if(worker.CancellationPending || !processing)
                        return;
                    Global.generatorFactory.progressText = "Loading frame " + i + "/" + curcount + "...";
                    FileStream frameFile = File.OpenRead($".\\temp\\extracted\\frames\\{i}.bmp");
                    frames.Add(Texture2D.FromStream(UserInterface.instance.GraphicsDevice, frameFile));
                    frameFile.Close();
                }
                // Load audio
                FileStream audioFile = File.OpenRead(".\\temp\\extracted\\audio.wav");
                SoundEffect snd = SoundEffect.FromStream(audioFile);
                audio = snd.CreateInstance();
                ScreenManager.PushNavigation("Video");
                ScreenManager.GetScreen<VideoScreen>("Video")?.Show();
                audioFile.Close();
                if(worker.CancellationPending || !processing)
                    return;
            }
            catch(Exception e)
            {
                ConsoleOutput.WriteLine($"Failed to extract frames and audio.");
                ConsoleOutput.WriteLine(e.Message);
                Stop();
                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                Global.generatorFactory.progressText = "Failed to play media.";
            }
            processing = false;
        }
        // Progress thread (checks directory size)
        public static void ProgressThread()
        {
            int dircount = 0;
            while(processing && !progressWorker.CancellationPending && dircount < count)
            {
                // Check directory size
                dircount = Directory.GetFiles(".\\temp\\extracted\\frames").Length;
                if(dircount > 0)
                {
                    Global.generatorFactory.progressText = "Extracting frames... (" + dircount + "/" + count + ")";
                }
                else
                {
                    Global.generatorFactory.progressText = "Extracting frames... (0/" + count + ")";
                }
                System.Threading.Thread.Sleep(100);
            }
        }
        public static void CountThread()
        {
            // Get frame count with ffprobe
            try
            {
                if(countWorker.CancellationPending || !processing)
                    return;
                ProcessStartInfo startInfo = new()
                {
                    FileName = "ffprobe",
                    Arguments = "-v error -count_frames -select_streams v:0 -show_entries stream=nb_read_frames -of default=nokey=1:noprint_wrappers=1 \"" + currentPath + "\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                Process process = new()
                {
                    StartInfo = startInfo
                };
                process.OutputDataReceived += (sender, args) => {
                    if(args.Data != null)
                    {
                        count = int.Parse(args.Data);
                        ConsoleOutput.WriteLine(args.Data, Color.Transparent);
                    }
                };
                if(countWorker.CancellationPending || !processing)
                    return;
                process.Start();
                if(countWorker.CancellationPending || !processing)
                    return;
                process.BeginOutputReadLine();
                if(countWorker.CancellationPending || !processing)
                    return;
                process.WaitForExit();
            }
            catch(Exception e)
            {
                ConsoleOutput.WriteLine($"Failed to get frame count.");
                ConsoleOutput.WriteLine(e.Message);
            }
        }
        public static bool Stop()
        {
            try
            {
                if(countWorker.IsBusy || progressWorker.IsBusy || worker.IsBusy)
                {
                    // Stop workers
                    audioConvertWorker.CancelAsync();
                    countWorker.CancelAsync();
                    progressWorker.CancelAsync();
                    worker.CancelAsync();
                    return false;
                }
                else
                {
                    processing = false;
                }
                audioPlaying = false;
                audioLength = 0;
                currentAudioTime = 0;
                playing = false;
                count = 0;
                currentFrame = 0;
                currentPath = "";
                timeStarted = 0;
                processing = false;
                startedProcessing = 0;
                canPlayBgMusic = true;
                // Unload frames
                foreach(Texture2D frame in frames)
                {
                    frame.Dispose();
                }
                frames.Clear();
                // Unload audio
                if(audio != null)
                {
                    audio.Stop();
                    audio.Dispose();
                    audio = null;
                    ScreenManager.GetScreen<VideoScreen>("Video")?.Hide();
                    Global.generatorFactory.progressText = "Stopped playback.";
                }
            }
            catch(Exception e)
            {
                ConsoleOutput.WriteLine($"Failed to stop playback.");
                ConsoleOutput.WriteLine(e.Message);
                return false;
            }
            return true;
        }
        public static void AudioConvertThread()
        {
            try
            {
                if(audioConvertWorker.CancellationPending || !processing)
                    return;
                ProcessStartInfo startInfo = new()
                {
                    FileName = "ffmpeg",
                    Arguments = "-i \"" + currentPath + "\" -vn -acodec pcm_s16le -ar 44100 -ac 2 -f wav -y .\\temp\\extracted\\audio.wav",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                Process process = new()
                {
                    StartInfo = startInfo
                };
                process.ErrorDataReceived += (sender, args) => {
                    if(args.Data != null)
                        ConsoleOutput.WriteLine(args.Data, Color.Transparent);
                };
                if(audioConvertWorker.CancellationPending || !processing)
                    return;
                process.Start();
                if(audioConvertWorker.CancellationPending || !processing)
                    return;
                process.BeginErrorReadLine();
                if(audioConvertWorker.CancellationPending || !processing)
                    return;
                process.WaitForExit();
                // Unload audio
                if(audio != null)
                {
                    audio.Stop();
                    audio.Dispose();
                    audio = null;
                    ScreenManager.GetScreen<VideoScreen>("Video")?.Hide();
                }
                FileStream audioFile = File.OpenRead(".\\temp\\extracted\\audio.wav");
                SoundEffect snd = SoundEffect.FromStream(audioFile);
                audio = snd.CreateInstance();
                audioFile.Close();
                currentAudioTime = 0;
                audioLength = snd.Duration.TotalMilliseconds;
                canPlayBgMusic = false;
                audio.Play();
                audioPlaying = true;
                Global.generatorFactory.progressText = "Playing media...";
            }
            catch(Exception e)
            {
                ConsoleOutput.WriteLine($"Failed to convert audio.");
                ConsoleOutput.WriteLine(e.Message);
            }
            processing = false;
        }
        public static void PlayMedia(LibraryFile file)
        {
            // If file is identical to current file, loop
            if(currentPath == file.Path)
            {
                if(audio != null)
                {
                    if(!audioPlaying && frames.Count == 0)
                    {
                        audio.Play();
                        Global.generatorFactory.progressText = "Playing media...";
                        FramePlayer.canPlayBgMusic = false;
                        audioPlaying = true;
                    }
                    else
                    {
                        audio.Stop();
                        currentAudioTime = 0;
                        audioPlaying = false;
                        FramePlayer.canPlayBgMusic = true;
                        Global.generatorFactory.progressText = "Stopped playback.";
                    }
                }
                playing = false;
                return;
            }
            if(Stop())
            {
                Global.generatorFactory.progressText = "Loading media...";
                currentPath = file.Path;
                // Extract frames and audio
                if(!processing)
                {
                    if(file.Type.RootType == LibraryRootType.Video)
                    {
                        // Run ffmpeg to extract frames and audio to .\temp\extracted\frames\* and .\temp\extracted\audio.wav
                        // Create the directory if it doesn't exist
                        if (!Directory.Exists(".\\temp\\extracted\\frames"))
                        {
                            Directory.CreateDirectory(".\\temp\\extracted\\frames");
                        }
                        currentAudioTime = 0;
                        audioLength = 0;
                        processing = true;
                        startedProcessing = 0;
                        worker = new();
                        worker.WorkerSupportsCancellation = true;
                        worker.DoWork += (sender, args) => ExtractFramesAndAudio();
                        worker.RunWorkerAsync();
                        countWorker = new();
                        countWorker.WorkerSupportsCancellation = true;
                        countWorker.DoWork += (sender, args) => CountThread();
                        countWorker.RunWorkerCompleted += (sender, args) => {
                            progressWorker = new();
                            progressWorker.WorkerSupportsCancellation = true;
                            progressWorker.DoWork += (sender, args) => ProgressThread();
                            progressWorker.RunWorkerAsync();
                        };
                        countWorker.RunWorkerAsync();
                    }
                    else
                    {
                        try
                        {
                            // Create the directory if it doesn't exist
                            if (!Directory.Exists(".\\temp\\extracted"))
                            {
                                Directory.CreateDirectory(".\\temp\\extracted");
                            }
                            currentAudioTime = 0;
                            audioLength = 0;
                            processing = true;
                            startedProcessing = 0;
                            audioConvertWorker = new();
                            audioConvertWorker.WorkerSupportsCancellation = true;
                            audioConvertWorker.DoWork += (sender, args) => AudioConvertThread();
                            audioConvertWorker.RunWorkerAsync();
                        }
                        catch(Exception e)
                        {
                            ConsoleOutput.WriteLine($"Failed to play media.");
                            ConsoleOutput.WriteLine(e.Message);
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                            Global.generatorFactory.progressText = "Failed to play media.";
                        }
                    }
                }
            }
            else
            {
                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"]) / 100f, 0f, 0f);
                Global.generatorFactory.progressText = "Failed to stop playback.";
            }
        }
        public static void Update(GameTime gameTime)
        {
            if(audioLength > 0 && currentAudioTime < audioLength && audioPlaying)
            {
                currentAudioTime += gameTime.ElapsedGameTime.TotalMilliseconds;
                if(currentAudioTime > audioLength)
                {
                    Stop();
                    audioPlaying = false;
                }
            }
            if(frames.Count > 0 && !playing && audio != null && !processing)
            {
                Global.generatorFactory.progressText = "Playing media...";
                timeStarted = gameTime.TotalGameTime.TotalSeconds;
                canPlayBgMusic = false;
                audio.Play();
                audio.Volume = int.Parse(SaveData.saveValues["VideoVolume"]) / 100f;
                currentFrame = 0;
                playing = true;
            }
            else if(processing && startedProcessing == 0)
            {
                startedProcessing = gameTime.TotalGameTime.TotalSeconds;
            }
            if(playing)
            {
                if(currentFrame >= 0 && currentFrame < frames.Count)
                {
                    // Update frame
                    currentFrame = (int)((gameTime.TotalGameTime.TotalSeconds - timeStarted) * fps);
                    if(currentFrame >= frames.Count)
                    {
                        if(audio != null)
                            audio.Stop();
                        playing = false;
                    }
                }
                // Update volume on the fly
                if(audio != null)
                {
                    audio.Volume = int.Parse(SaveData.saveValues["VideoVolume"]) / 100f;
                }
            }
        }
    }
}
