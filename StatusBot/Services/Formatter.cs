using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace StatusBot.Services
{
    public class Formatter
    {
        private static List<char> csv_specials;
        private static List<char> csv_operators;
        private static List<char> csv_allchars;
        public Formatter()
        {
            csv_specials = new List<char> { ',', '"', '\r', '\n' };
            csv_operators = new List<char> { '=', '+', '-', '@' };
            csv_allchars = new List<char>();
            csv_allchars.AddRange(csv_specials);
            csv_allchars.AddRange(csv_operators);
        }
        public string EscapeCSV(string rawstring)
        {
            if (rawstring.Any(c => csv_allchars.Contains(c)))
            {
                var cleanstring = new StringBuilder();
                cleanstring.Append('"');
                foreach (char c in rawstring)
                {
                    if (c == rawstring[0] && csv_operators.Any(o => o == c)) // If the first character is one of csv operator characters (used in formulas), add a tab character
                        cleanstring.Append('\t');
                    else if (c == '"')  // If the character is a double-quote, add an additional double quote
                        cleanstring.Append('"');
                    cleanstring.Append(c);
                }
                cleanstring.Append('"');
                return cleanstring.ToString();
            }
            else
                return rawstring;
        }
    }
}
