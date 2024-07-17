using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace NonsensicalVideoGenerator
{
    public class SimpleDateTime
    {
        public int Month { get; set; } = 0;
        public int Day { get; set; } = 0;
        public int Hour { get; set; } = 0;
        public int Minute { get; set; } = 0;
        public int Second { get; set; } = 0;
        public SimpleDateTime(int month = 1, int day = 1, int hour = 0, int minute = 0, int second = 0)
        {
            // Clamp values.
            Month = Math.Clamp(month, 1, 12);
            Day = Math.Clamp(day, 1, 31);
            Hour = Math.Clamp(hour, 0, 23);
            Minute = Math.Clamp(minute, 0, 59);
            Second = Math.Clamp(second, 0, 59);
        }
    }
    public class Holiday
    {
        public string InternalName { get; set; } = "";
        public string Name { get; set; } = "";
        public int Priority { get; set; } = 0;
        public bool Enabled { get; set; } = false;
        public Theme Theme { get; set; } = DefaultThemes.Nonsensical;
        public SimpleDateTime Start { get; set; } = new SimpleDateTime();
        public SimpleDateTime End { get; set; } = new SimpleDateTime();
        public bool CheckDate()
        {
            if(!Enabled)
                return false;
            DateTime now = DateTime.Now;
            DateTime start = new DateTime(now.Year, Start.Month, Start.Day, Start.Hour, Start.Minute, Start.Second);
            DateTime end = new DateTime(now.Year, End.Month, End.Day, End.Hour, End.Minute, End.Second);
            if(now >= start && now <= end)
                return true;
            return false;
        }
        public Holiday(string internalName = "", string name = "", int priority = 0, bool enabled = false, Theme theme = null, SimpleDateTime start = null, SimpleDateTime end = null)
        {
            InternalName = internalName;
            Name = name;
            Priority = priority;
            Enabled = enabled;
            Theme = theme;
            Start = start;
            End = end;
        }
    }
    public static class HolidayManager
    {
        public static Holiday? CurrentHoliday { get; set; } = null;
        public static List<Holiday> Holidays { get; set; } = new List<Holiday>()
        {
            new Holiday("anniversary", "Nonsensical Anniversary")
            {
                Enabled = true,
                Priority = 1,
                Theme = DefaultThemes.Anniversary,
                Start = new SimpleDateTime(7, 23, 0, 0, 0),
                End = new SimpleDateTime(7, 23, 23, 59, 59)
            },
            new Holiday("birthday", "KiwifruitDev's Birthday")
            {
                Enabled = true,
                Priority = 2,
                Theme = DefaultThemes.Birthday,
                Start = new SimpleDateTime(10, 14, 0, 0, 0),
                End = new SimpleDateTime(10, 14, 23, 59, 59)
            },
            new Holiday("halloween", "Halloween")
            {
                Enabled = true,
                Priority = 1,
                Theme = DefaultThemes.Spooky,
                Start = new SimpleDateTime(10, 15, 12, 0, 0),
                End = new SimpleDateTime(10, 31, 23, 59, 59)
            },
            new Holiday("christmas", "Christmas")
            {
                Enabled = true,
                Priority = 1,
                Theme = DefaultThemes.Holiday,
                Start = new SimpleDateTime(12, 1, 0, 0, 0),
                End = new SimpleDateTime(12, 25, 23, 59, 59)
            }
        };
        public static void SetHoliday(Holiday? holiday = null)
        {
            if(holiday == null)
            {
                CurrentHoliday = null;
                DefaultThemes.defaultTheme = DefaultThemes.Nonsensical;
                ThemeManager.activeTheme = DefaultThemes.Nonsensical;
                return;
            }
            ConsoleOutput.WriteLine($"Holiday: {holiday.Name}", Color.Yellow);
            CurrentHoliday = holiday;
            if(holiday.Theme != null)
            {
                DefaultThemes.defaultTheme = holiday.Theme;
                ThemeManager.activeTheme = holiday.Theme;
            }
        }
        public static void CheckHolidays()
        {
            // Respect priority.
            Holiday newHoliday = new();
            foreach(Holiday holiday in Holidays)
            {
                if(holiday.CheckDate())
                {
                    if(newHoliday.Priority < holiday.Priority)
                    {
                        newHoliday = holiday;
                    }
                }
            }
            if(newHoliday.Name != "")
            {
                SetHoliday(newHoliday);
            }
        }
    }
}
