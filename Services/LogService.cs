using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.Versioning;

namespace StatusBot.Services
{
    public enum TimeAppend { Long, Short }

    public class LogService
    {
        public LogService()
        {
            Console.WriteLine("LogService initialized");
        }
        private readonly string logdir = Path.GetFullPath("./Logs/");
        private readonly string d_format = "yyyy-MM-dd";
        private readonly string t_format = "HH:mm:ss";
        public async Task WriteAsync(string text, ConsoleColor color = ConsoleColor.Gray, TimeAppend append = TimeAppend.Long)
        {
            text = TimeStamp(text, append);
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
            string date_now = DateTime.Now.ToLocalTime().ToString(d_format);
            string fname = $"log_{date_now}.txt";
            string logpath = Path.Join(new string[] { logdir, fname });
            await File.AppendAllTextAsync(logpath, $"{text}\n");
        }

        public async Task WriteErrorAsync(string text, ConsoleColor color = ConsoleColor.Red, TimeAppend append = TimeAppend.Long)
        {
            text = TimeStamp(text, append);
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
            string date_now = DateTime.Now.ToLocalTime().ToString(d_format);
            string fname = $"log_{date_now}.txt";
            string logpath = Path.Join(new string[] { logdir, fname });
            await File.AppendAllTextAsync(logpath, $"{text}\n");
        }

        private string TimeStamp(string text, TimeAppend append)
        {
            DateTime now = DateTime.Now.ToLocalTime();
            if (append == TimeAppend.Long)
                return $"{now.ToString(d_format)} {now.ToString(t_format)} {text}";
            else
                return $"{now.ToString(d_format)} {text}";
        }
    }
}
