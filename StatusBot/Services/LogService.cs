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
        private static readonly string logfilepath = "./Logs/logfile.txt";

        public async Task Write(string text, ConsoleColor color = ConsoleColor.Gray, TimeAppend timeformat = TimeAppend.Long)
        {
            switch (timeformat)
            {
                case TimeAppend.Long:
                    text = TimeStamp(text, TimeAppend.Long); break;
                case TimeAppend.Short:
                    text = TimeStamp(text, TimeAppend.Short); break;
            }
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
            await File.AppendAllTextAsync(logfilepath, $"{text}\n");
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
