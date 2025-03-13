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
            if(Cache.ContainsKey(video))
                return Cache[video];
            string cache = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "cache");
            if(!Directory.Exists(cache))
                Directory.CreateDirectory(cache);
            string videoFileExtension = Path.GetExtension(video);
            string cachePath = Path.Combine(cache, Path.GetRandomFileName() + videoFileExtension);
            File.Copy(video, cachePath);
            Cache.Add(video, cachePath);
            return cachePath;
        }
        public static void ClearCache()
        {
            foreach(KeyValuePair<string, string> entry in Cache)
                File.Delete(entry.Value);
            Cache.Clear();
        }
    }
}
