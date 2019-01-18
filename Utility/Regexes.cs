using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using StatusBot.TypeReaders;

namespace StatusBot.Utility
{
    public static class ChronoRegex
    {
        private static Regex timeregex = new Regex(
            @"^(?:(?<years>\d{1,3}?(?:\.\d{1,3})?)y)?(?:(?<months>\d{1,3}?(?:\.\d{1,3})?)mo)?(?:(?<weeks>\d{1,3}?(?:\.\d{1,2})?)w)?(?:(?<days>\d{1,3}?(?:\.\d{1,2})?)d)?(?:(?<hours>\d{1,3}?(?:\.\d{1,2})?)h)?(?:(?<minutes>\d{1,3}?(?:\.\d)?)m)?(?:(?<seconds>\d{1,3}?)s)?$",
            RegexOptions.Compiled | RegexOptions.Multiline);
        
        public static ChronoString ParseChronoString(this string input)
        {
            Match M = timeregex.Match(input);
            if (M.Length == 0)
                throw new ArgumentException("Invalid time input format");
            var timevalues = new Dictionary<string, decimal>();

            foreach (var timename in timeregex.GetGroupNames())
            {
                if (timename == "0") continue;
                string decimalstr = M.Groups[timename].Value;
                Console.WriteLine($"{timename} {decimalstr}");
                if (!decimal.TryParse(decimalstr, out decimal value))
                {
                    timevalues[timename] = decimal.Zero;
                    continue;
                }
                Console.WriteLine($"============= Obtained decimal value: {value}");
                timevalues[timename] = (timename == "seconds" && value < decimal.One) ? decimal.Zero : value;
            }

            double days = (double)(timevalues["years"] * 365 + timevalues["months"] * 30 + timevalues["weeks"] * 7 + timevalues["days"]);
            double hours = (double)(timevalues["hours"]);
            double minutes = (double)(timevalues["minutes"]);
            double seconds = (double)(timevalues["seconds"]);

            TimeSpan TS = TimeSpan.FromDays(days).Add(TimeSpan.FromHours(hours)).Add(TimeSpan.FromMinutes(minutes)).Add(TimeSpan.FromSeconds(seconds));

            return new ChronoString
            {
                Input = input,
                TimeData = timevalues,
                Time = TS
            };
        }
    }
}
