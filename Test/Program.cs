using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {

            /*
        public const string VERSION = "0.4.1.0";
        public const string NAME = "MineralExhaustionNotifier";
*/
            Dictionary<string, string> kv = new Dictionary<string, string>();

            string input = File.ReadAllText("../../../../MineralExhaustionNotifier/Version.cs");
            Regex pattern = new Regex("^\\s*public const string (?<key>\\w+)\\s+=\\s+\"(?<value>[\\w.]+)\";", RegexOptions.Multiline);
            MatchCollection matches = pattern.Matches(input);
            for (int i = 0; i< matches.Count; i++)
            {
                kv[matches[i].Groups["key"].Value] = matches[i].Groups["value"].Value;
            }



            Console.WriteLine("Hello World!");
        }
    }
}
