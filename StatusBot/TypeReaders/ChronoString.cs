using System;
using System.Collections.Generic;
using System.Text;

namespace StatusBot.TypeReaders
{
    public class ChronoString
    {
        public string Input { get; set; }
        public Dictionary<string, decimal> TimeData { get; set; }
        public TimeSpan Time { get; set; }
    }
}
