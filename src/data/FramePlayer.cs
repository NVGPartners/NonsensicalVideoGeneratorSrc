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
using System.Globalization;

namespace NonsensicalVideoGenerator
{
    public static class FramePlayer
    {
        public static int currentFrame = 0;
#if MONOGAME
        public static List<Texture2D> frames = new();
        public static Texture2D? audioFrame;
#else
        public static List<Image> frames = new();
#endif
        public static double timeStarted = 0;
        public static int fps = 30;
        public static bool playing = false;
        public static bool audioPlaying = false;
#if MONOGAME
        public static SoundEffectInstance? audio;
#else
        public static SoundPlayer audio;
#endif
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
                int scale =  int.Parse(SaveData.saveValues["VideoPlaybackScale"], CultureInfo.InvariantCulture);
                int w = 100 * scale;
                int h = 78 * scale;
                ProcessStartInfo startInfo = new()
                {
                    FileName = "ffmpeg",
                    // 100x78 letterboxed
                    // fastest method
                    Arguments = "-i \""+currentPath+"\" -vf scale=" + w + ":" + h + ":force_original_aspect_ratio=decrease,pad=" + w + ":" + h + ":(ow-iw)/2:(oh-ih)/2 -r " + fps + " -y .\\temp\\extracted\\frames\\%d.bmp",
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
                // Extract audio
                startInfo = new()
                {
                    FileName = "ffmpeg",
                    Arguments = "-i \"" + currentPath + "\" -vn -acodec pcm_s16le -ar 44100 -ac 2 -f wav -y .\\temp\\extracted\\audio.wav",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                process = new()
                {
                    StartInfo = startInfo
                };
                process.ErrorDataReceived += (sender, args) => {
                    if(args.Data != null)
                        ConsoleOutput.WriteLine(args.Data, Color.Transparent);
                };
                if(worker.CancellationPending || !processing)
                    return;
                process.Start();
                if(worker.CancellationPending || !processing)
                    return;
                process.BeginErrorReadLine();
                if(worker.CancellationPending || !processing)
                    return;
                process.WaitForExit();
                // Write blank audio if it doesn't exist
                if(!File.Exists(".\\temp\\extracted\\audio.wav"))
                {
                    ConsoleOutput.WriteLine("Failed to extract audio, using blank audio.", Color.Red);
                    startInfo = new()
                    {
                        FileName = "ffmpeg",
                        Arguments = "-f lavfi -i anullsrc=channel_layout=stereo:sample_rate=44100 -t 1 -acodec pcm_s16le -ar 44100 -ac 2 -f wav -y .\\temp\\extracted\\audio.wav",
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };
                    process = new()
                    {
                        StartInfo = startInfo
                    };
                    process.ErrorDataReceived += (sender, args) => {
                        if(args.Data != null)
                            ConsoleOutput.WriteLine(args.Data, Color.Transparent);
                    };
                    if(worker.CancellationPending || !processing)
                        return;
                    process.Start();
                    if(worker.CancellationPending || !processing)
                        return;
                    process.BeginErrorReadLine();
                    if(worker.CancellationPending || !processing)
                        return;
                    process.WaitForExit();
                }
                // Load frames
                audioFrame = null;
                frames.Clear();
                if(worker.CancellationPending || !processing)
                    return;
                int curcount = Directory.GetFiles(".\\temp\\extracted\\frames").Length;
                count = curcount;
                for (int i = 1; i <= curcount; i++)
                {
                    if(worker.CancellationPending || !processing)
                        return;
                    Global.generator.progressText = L.T(0, "Video:StatusLoadFrame", i.ToString(CultureInfo.InvariantCulture), curcount.ToString(CultureInfo.InvariantCulture));
                    FileStream frameFile = File.OpenRead($".\\temp\\extracted\\frames\\{i}.bmp");
#if MONOGAME
                    if(UserInterface.instance != null)
                        frames.Add(Texture2D.FromStream(UserInterface.instance.GraphicsDevice, frameFile));
#else
                    frames.Add(Image.FromStream(frameFile));
#endif
                    frameFile.Close();
                }
                // Load audio
                FileStream audioFile = File.OpenRead(".\\temp\\extracted\\audio.wav");
#if MONOGAME
                SoundEffect snd = SoundEffect.FromStream(audioFile);
                audio = snd.CreateInstance();
                //ScreenManager.PushNavigation("Video");
                //ScreenManager.GetScreen<VideoScreen>("Video")?.Show();
#else
                audio = new SoundPlayer(audioFile);
#endif
                audioFile.Close();
                if(worker.CancellationPending || !processing)
                    return;
            }
            catch(Exception e)
            {
                ConsoleOutput.WriteLine($"Failed to extract frames and audio.");
                ConsoleOutput.WriteLine(e.Message);
                Stop();
                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                Global.generator.progressText = L.T(0, "Video:StatusFailPlay");
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
                    Global.generator.progressText = L.T(0, "Video:StatusExtractFrames", dircount.ToString(CultureInfo.InvariantCulture), count.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    Global.generator.progressText = L.T(0, "Video:StatusExtractFrames", "0", count.ToString(CultureInfo.InvariantCulture));
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
                        count = int.Parse(args.Data, CultureInfo.InvariantCulture);
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
#if MONOGAME
                foreach(Texture2D frame in frames)
                {
                    frame.Dispose();
                }
#else
                foreach(Image frame in frames)
                {
                    frame.Dispose();
                }
#endif
                frames.Clear();
                // Unload audio
                if(audio != null)
                {
                    audio.Stop();
                    audio.Dispose();
                    audio = null;
#if MONOGAME
                    //ScreenManager.GetScreen<VideoScreen>("Video")?.Hide();
#endif
                    Global.generator.progressText = L.T(0, "Video:StatusStop");
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
                if(!File.Exists(".\\temp\\extracted\\audio.wav"))
                {
                    throw new Exception("Failed to convert audio.");
                }
                // Unload audio
                if(audio != null)
                {
                    audio.Stop();
                    audio.Dispose();
                    audio = null;
#if MONOGAME
                    //ScreenManager.GetScreen<VideoScreen>("Video")?.Hide();
#endif
                }
                FileStream audioFile = File.OpenRead(".\\temp\\extracted\\audio.wav");
#if MONOGAME
                SoundEffect snd = SoundEffect.FromStream(audioFile);
                audio = snd.CreateInstance();
#else
                audio = new SoundPlayer(audioFile);
#endif
                audioFile.Close();
                currentAudioTime = 0;
#if MONOGAME
                audioLength = snd.Duration.TotalMilliseconds;
#else
                // Get length of audio with ffprobe
                startInfo = new()
                {
                    FileName = "ffprobe",
                    Arguments = "-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"" + currentPath + "\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                process = new()
                {
                    StartInfo = startInfo
                };
                process.OutputDataReceived += (sender, args) => {
                    if(args.Data != null)
                    {
                        audioLength = double.Parse(args.Data, CultureInfo.InvariantCulture) * 1000;
                        ConsoleOutput.WriteLine(args.Data, Color.Transparent);
                    }
                };
                if(audioConvertWorker.CancellationPending || !processing)
                    return;
                process.Start();
                if(audioConvertWorker.CancellationPending || !processing)
                    return;
                process.BeginOutputReadLine();
                if(audioConvertWorker.CancellationPending || !processing)
                    return;
                process.WaitForExit();
#endif
                canPlayBgMusic = false;
                Process waveProcess = Generator.GenerateThumbnail(currentPath, ".\\temp\\extracted\\audio.bmp", LibraryRootType.Audio, 100, 78);
                // Defer until the process is finished
                while(!waveProcess.HasExited)
                {
                    System.Threading.Thread.Sleep(100);
                }
                audioFrame = null;
                FileStream audioFrameFile = File.OpenRead(".\\temp\\extracted\\audio.bmp");
                audioFrame = Texture2D.FromStream(UserInterface.instance.GraphicsDevice, audioFrameFile);
                audioFrameFile.Close();
                if(SaveData.saveValues["UseExternalVideoPlayer"] == "false")
                {
                    audio.Volume = float.Parse(SaveData.saveValues["VideoVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f;
                    audio.Play();
                    //ScreenManager.PushNavigation("Video");
                    //ScreenManager.GetScreen<VideoScreen>("Video")?.Show();
                    audioPlaying = true;
                    Global.generator.progressText = L.T(0, "Video:StatusPlay");
                }
                else
                {
                    audio.Stop();
                    audioPlaying = false;
                }
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
            if(SaveData.saveValues["UseExternalVideoPlayer"] == "true")
            {
                try
                {
                    ProcessStartInfo startInfo = new()
                    {
                        FileName = file.Path,
                        UseShellExecute = true
                    };
                    Process process = new()
                    {
                        StartInfo = startInfo
                    };
                    process.Start();
                }
                catch(Exception e)
                {
                    ConsoleOutput.WriteLine($"Failed to open external video player.", Color.Red);
                    ConsoleOutput.WriteLine(e.Message, Color.Red);
                    GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                    Global.generator.progressText = L.T(0, "Video:StatusFailPlay");
                }
                return;
            }
            // If file is identical to current file, loop
            if(currentPath == file.Path)
            {
                if(audio != null)
                {
                    if(!audioPlaying && frames.Count == 0)
                    {
                        audio.Volume = float.Parse(SaveData.saveValues["VideoVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f;
                        audio.Play();
                        Global.generator.progressText = L.T(0, "Video:StatusPlay");
                        canPlayBgMusic = false;
                        audioPlaying = true;
                    }
                    else
                    {
                        audio.Stop();
                        currentAudioTime = 0;
                        audioPlaying = false;
                        canPlayBgMusic = true;
                        Global.generator.progressText = L.T(0, "Video:StatusStop");
                    }
                }
                playing = false;
                return;
            }
            if(Stop())
            {
                Global.generator.progressText = L.T(0, "Video:StatusLoad");
                currentPath = file.Path ?? "";
                // Extract frames and audio
                if(!processing)
                {
                    if(file.Type != null && file.Type.RootType == LibraryRootType.Video)
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
                            if(file.Type == null)
                                throw new Exception("Invalid file type???");
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
                            GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                            Global.generator.progressText = L.T(0, "Video:StatusFailPlay");
                        }
                    }
                }
            }
            else
            {
                GlobalContent.GetSound("Error").Play(int.Parse(SaveData.saveValues["SoundEffectVolume"], CultureInfo.InvariantCulture) / 100f, 0f, 0f);
                Global.generator.progressText = L.T(0, "Video:StatusFailStop");
            }
        }
        public static void Update(GameTime gameTime)
        {
            if(SaveData.saveValues["UseExternalVideoPlayer"] == "false")
            {
                if(audioLength > 0 && currentAudioTime < audioLength && audioPlaying)
                {
                    currentAudioTime += gameTime.ElapsedGameTime.TotalMilliseconds;
                    if(currentAudioTime > audioLength)
                    {
                        audio.Stop();
                        currentAudioTime = 0;
                        audioPlaying = false;
                        canPlayBgMusic = true;
                        Global.generator.progressText = L.T(0, "Video:StatusStop");
                    }
                }
                if(frames.Count > 0 && !playing && audio != null && !processing)
                {
                    Global.generator.progressText = L.T(0, "Video:StatusPlay");
                    timeStarted = gameTime.TotalGameTime.TotalSeconds;
                    canPlayBgMusic = false;
                    audio.Volume = float.Parse(SaveData.saveValues["VideoVolume"], System.Globalization.CultureInfo.InvariantCulture) / 100f;
                    audio.Play();
                    audio.Volume = int.Parse(SaveData.saveValues["VideoVolume"], CultureInfo.InvariantCulture) / 100f;
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
                        audio.Volume = int.Parse(SaveData.saveValues["VideoVolume"], CultureInfo.InvariantCulture) / 100f;
                    }
                }
            }
        }
    }
}
