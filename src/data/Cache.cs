using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace NonsensicalVideoGenerator
{
    // Dirty workaround for the fact that MonoGame.Extended2.VideoPlayback doesn't allow file unlocking
    public static class VideoCache
    {
        private static Dictionary<string, string> Cache = new Dictionary<string, string>();
        public static string GetCachePath(string video)
        {
            // Generate a UUID based on the video file name and last write time
            // This ensures that the video is fingerprinted and would account for any changes to the video file
            string videoUniqueKey = Path.GetFileName(video) + "_" + File.GetLastWriteTime(video).ToString("yyyyMMddHHmmss");
            // Check if the video is already in the cache
            if(Cache.ContainsKey(videoUniqueKey))
                return Cache[videoUniqueKey];
            // If the video is not in the cache, copy it to the cache directory and return the path
            string cache = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "cache");
            if(!Directory.Exists(cache))
                Directory.CreateDirectory(cache);
            string videoFileExtension = Path.GetExtension(video);
            string cachePath = Path.Combine(cache, Path.GetRandomFileName() + videoFileExtension);
            try
            {
                File.Copy(video, cachePath);
            }
            catch
            {
                // We have to give up copying, as the resulting file was locked by the video player
            }
            Cache.Add(videoUniqueKey, cachePath);
            return cachePath;
        }
        public static void ClearCache()
        {
            // Loop through all videos inside the cache path directory and delete them
            string cache = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "cache");
            if(Directory.Exists(cache))
            {
                foreach(string file in Directory.GetFiles(cache))
                {
                    File.Delete(file);
                }
            }
            // Clear the cache dictionary
            Cache.Clear();
        }
    }
}
