using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace StatusBot.Services
{
    public enum TimeAppend { Short, Long }

    public class LogService
    {
        private static readonly string normallogpath = "./Logs/logfile.txt";
        private static readonly string errorlogpath = "./Logs/errorlog.txt";

        public async Task Write(string text, ConsoleColor color = ConsoleColor.Gray, TimeAppend timeformat = TimeAppend.Long)
        {
            await Log(text, color, timeformat, normallogpath);
        }

        public async Task WriteError(string text, ConsoleColor color = ConsoleColor.Red, TimeAppend timeformat = TimeAppend.Long)
        {
            await Log(text, color, timeformat, errorlogpath);
        }

        private async Task Log(string text, ConsoleColor color, TimeAppend timeformat, string logpath)
        {
            text = TimeStamp(text, timeformat);
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
            await File.AppendAllTextAsync(logpath, $"{text}\n");
        }

        public string TimeStamp(string text, TimeAppend timeformat = TimeAppend.Long)
        {
            DateTime now = DateTime.Now;
            if (timeformat == TimeAppend.Long)
                return $"{now.ToShortDateString()} {now.ToLongTimeString()} {text}";
            else
                return $"{now.ToShortDateString()} {text}";
        }
    }
}
