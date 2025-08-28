using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace NonsensicalVideoGenerator
{
    public class SimpleDateTime
    {
        private static int OffsetPST = -8;
        private static int OffsetPDT = -7;
        private static SimpleDateTime? FirstSundayNovember = null;
        private static SimpleDateTime? SecondSundayMarch = null;
        public int Month { get; set; } = 0;
        public int Day { get; set; } = 0;
        public int Hour { get; set; } = 0;
        public int Minute { get; set; } = 0;
        public int Second { get; set; } = 0;
        public int Offset { get; set; } = 0;
        public SimpleDateTime(int month = 1, int day = 1, int hour = 0, int minute = 0, int second = 0, int offset = 0, bool skipDST = false)
        {
            // Clamp values.
            Month = Math.Clamp(month, 1, 12);
            Day = Math.Clamp(day, 1, 31);
            Hour = Math.Clamp(hour, 0, 23);
            Minute = Math.Clamp(minute, 0, 59);
            Second = Math.Clamp(second, 0, 59);
            if(offset != 0)
            {
                Offset = OffsetPST;
                skipDST = true;
            }
            if(!skipDST)
            {
                // Check to see if daylight savings date needs to be calculated.
                if(FirstSundayNovember == null || SecondSundayMarch == null)
                {
                    // Get calendar year so we can determine the first sundays of november and march.
                    int year = Global.currentYear;
                    // Get the first sunday of november.
                    DateTime firstSundayNovemberDateTime = new DateTime(year, 11, 1);
                    while(firstSundayNovemberDateTime.DayOfWeek != DayOfWeek.Sunday)
                        firstSundayNovemberDateTime = firstSundayNovemberDateTime.AddDays(1);
                    // Get the second sunday of march.
                    DateTime secondSundayMarchDateTime = new DateTime(year, 3, 1);
                    while(secondSundayMarchDateTime.DayOfWeek != DayOfWeek.Sunday)
                        secondSundayMarchDateTime = secondSundayMarchDateTime.AddDays(1);
                    secondSundayMarchDateTime = secondSundayMarchDateTime.AddDays(7);
                    // Convert to SimpleDateTime.
                    FirstSundayNovember = new SimpleDateTime(firstSundayNovemberDateTime.Month, firstSundayNovemberDateTime.Day, 0, 0, 0, 0, true);
                    SecondSundayMarch = new SimpleDateTime(secondSundayMarchDateTime.Month, secondSundayMarchDateTime.Day, 0, 0, 0, 0, true);
                }
                // Check to see if this month/day is after the first sunday of november or march.
                if(month == FirstSundayNovember.Month)
                {
                    if(day < FirstSundayNovember.Day)
                        Offset = OffsetPDT;
                }
                else if(month == SecondSundayMarch.Month)
                {
                    if(day >= SecondSundayMarch.Day)
                        Offset = OffsetPDT;
                }
                else if(month > SecondSundayMarch.Month && month < FirstSundayNovember.Month)
                {
                    Offset = OffsetPDT;
                }
            }
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
        public int StartYear { get; set; } = 0;
        public string StatusText { get; set; } = "";
        public bool CheckDate()
        {
            if(!Enabled)
                return false;
            DateTime now = DateTime.UtcNow;
            DateTime start = new DateTime(now.Year, Start.Month, Start.Day, Start.Hour, Start.Minute, Start.Second);
            DateTime end = new DateTime(now.Year, End.Month, End.Day, End.Hour, End.Minute, End.Second);
            // add offset
            SimpleDateTime nowSimple = new SimpleDateTime(now.Month, now.Day, now.Hour, now.Minute, now.Second);
            now = now.AddHours(nowSimple.Offset);
            if(now >= start && now <= end)
                return true;
            return false;
        }
        public string GetStatusText()
        {
            if(StatusText == "")
                return L.T(0, "Content:StatusWelcome");
            // ignore StartYear if it's not set
            if(StartYear == 0)
                return L.T(0, StatusText);
            // get years since StartYear to display in status text
            int years = Global.currentYear - StartYear;
            // st, nd, rd, th
            bool st = years % 10 == 1 && years % 100 != 11;
            bool nd = years % 10 == 2 && years % 100 != 12;
            bool rd = years % 10 == 3 && years % 100 != 13;
            string suffix = st ? "St" : nd ? "Nd" : rd ? "Rd" : "Th";
            string yearsLocalized = L.T(0, "Holidays:OrdinalSuffix" + suffix.ToString(CultureInfo.InvariantCulture), years.ToString(CultureInfo.InvariantCulture));
            // combine status text with years
            return L.T(0, StatusText, yearsLocalized);
        }
        public Holiday(string internalName = "", string name = "", int priority = 0, bool enabled = false, Theme? theme = null, SimpleDateTime? start = null, SimpleDateTime? end = null, string statusText = "")
        {
            InternalName = internalName;
            Name = name;
            Priority = priority;
            Enabled = enabled;
            Theme = theme ?? DefaultThemes.Nonsensical;
            Start = start ?? new SimpleDateTime();
            End = end ?? new SimpleDateTime();
            StatusText = statusText;
        }
    }
    public static class HolidayManager
    {
        public static Holiday? CurrentHoliday { get; set; } = null;
        public static List<Holiday> Holidays { get; set; } = new List<Holiday>()
        {
            new Holiday("newyearseve", "New Year's Eve")
            {
                Enabled = true,
                Priority = 1,
                Theme = DefaultThemes.Nonsensical,
                Start = new SimpleDateTime(12, 31, 0, 0, 0),
                End = new SimpleDateTime(12, 31, 23, 59, 59),
                StatusText = "Holidays:StatusNewYears"
            },
            new Holiday("anniversary", "Nonsensical Anniversary")
            {
                Enabled = true,
                Priority = 1,
                Theme = DefaultThemes.Anniversary,
                Start = new SimpleDateTime(7, 23, 0, 0, 0),
                End = new SimpleDateTime(7, 23, 23, 59, 59),
                StartYear = 2023,
                StatusText = "Holidays:StatusAnniversary"
            },
            new Holiday("halloween", "Halloween")
            {
                Enabled = true,
                Priority = 1,
                Theme = DefaultThemes.Spooky,
                Start = new SimpleDateTime(10, 15, 12, 0, 0),
                End = new SimpleDateTime(10, 31, 23, 59, 59),
                StatusText = "Holidays:StatusHalloween"
            },
            new Holiday("christmas", "Christmas")
            {
                Enabled = true,
                Priority = 1,
                Theme = DefaultThemes.Holiday,
                Start = new SimpleDateTime(12, 1, 0, 0, 0),
                End = new SimpleDateTime(12, 25, 23, 59, 59),
                StatusText = "Holidays:StatusChristmas"
            },
            new Holiday("newyearsday", "New Year's Day")
            {
                Enabled = true,
                Priority = 1,
                Theme = DefaultThemes.Nonsensical,
                Start = new SimpleDateTime(1, 1, 0, 0, 0),
                End = new SimpleDateTime(1, 1, 23, 59, 59),
                StatusText = "Holidays:StatusNewYears"
            }
        };
        public static void SetHoliday(Holiday? holiday = null)
        {
            if(SaveData.saveValues["DisableHolidays"] == "true")
                holiday = null;
            CurrentHoliday = holiday;
            ConsoleOutput.WriteLine($"Holiday: {(holiday != null ? holiday.Name : "None")}", Color.Yellow);
            DefaultThemes.defaultTheme = holiday != null ? holiday.Theme : DefaultThemes.Nonsensical;
            ThemeManager.activeTheme = holiday != null ? holiday.Theme : DefaultThemes.Nonsensical;
            Global.generator.progressText = holiday != null ? holiday.GetStatusText() : L.T(0, "Content:StatusWelcome");
        }
        public static void CheckHolidays()
        {
            // Check for holiday command line parameter.
            if(Global.parameters.Contains("-holiday"))
            {
                int index = Global.parameters.IndexOf("-holiday");
                if(index + 1 < Global.parameters.Count)
                {
                    string holiday = Global.parameters[index + 1];
                    foreach(Holiday h in Holidays)
                    {
                        if(h.InternalName == holiday.ToLower())
                        {
                            SetHoliday(h);
                            return;
                        }
                    }
                }
            }
            // Respect priority.
            Holiday? newHoliday = new();
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
            if(newHoliday.Name == "")
                newHoliday = null;
            SetHoliday(newHoliday);
        }
    }
}
