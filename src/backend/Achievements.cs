using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Steamworks;

namespace NonsensicalVideoGenerator
{
    public static class Achievements
    {
        public static List<string> awardedAchievements = new List<string>();
        public static void GetCurrentAchievements()
        {
            if(SteamManager.initialized)
            {
                for(int i = 0; i < SteamUserStats.GetNumAchievements(); i++)
                {
                    string name = SteamUserStats.GetAchievementName((uint)i);
                    SteamUserStats.GetAchievement(name, out bool achieved);
                    if(achieved)
                    {
                        awardedAchievements.Add(name);
                    }
                }
            }
        }
        public static void Award(string achievement)
        {
            if(Debug.debugModePermanent)
            {
                ConsoleOutput.WriteLine("Debug mode enabled, not awarding achievement: "+achievement, Color.LightBlue);
                return;
            }
            if(SteamManager.initialized)
            {
                if(!awardedAchievements.Contains(achievement))
                {
                    awardedAchievements.Add(achievement);
                    ConsoleOutput.WriteLine("Awarding achievement: "+achievement, Color.LightBlue);
                    SteamUserStats.SetAchievement(achievement);
                }
            }
        }
    }
}
