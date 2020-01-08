using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MibbitChatToHTML
{
    class ChatFile
    {
        public static List<string> ProcessChatFile(string fileName, MainWindow mw)
        {
            List<Tuple<int, string>> processedChatLines = new List<Tuple<int, string>>();
            List<string> justChatLines = new List<string>();

            //Fix word/line wrapped lines by joining them to previous
            List<string> allChatText = File.ReadAllLines(fileName, Encoding.GetEncoding(1252)).ToList<string>();

            int counter = 0;
            foreach (string line in allChatText)
            {
                
                if (line.Length > 2)
                {
                    string cleanLine = CleanUpMibbitFormatting(line);
                    Tuple<int, string> tLine = new Tuple<int, string>(counter, cleanLine);
                    processedChatLines.Add(tLine);
                    justChatLines.Add(cleanLine + "\r\n");
                }
                counter++;
            }
            return justChatLines;
        }
        private static string CleanUpMibbitFormatting(string line)
        {
            //<p style='color:#TEXTCOLORHERE;'><span style='font-weight: bold; color:#000000;'>NAME</span> Text </p>
            string cleanedLine = string.Empty;
            if(line.StartsWith("*") && line.Length > 2)
            {
                int nameTagStart = line.IndexOf('*');
                int nameTagEnd = line.IndexOf('*',nameTagStart + 1);
                string firstTemp = line.Replace(':', ' ');
                string name = firstTemp.Substring((nameTagStart + 1), ((nameTagEnd - nameTagStart) - 2));
                string post = line.Substring(nameTagEnd + 1);
                if(name.Contains("Yara"))
                {
                    name = "<p style='color:#666666;'><span style='font-weight: bold; color:#000000;'>" + name + ": " + "</span>";
                }
                else if(name.Contains("Damian"))
                {
                    name = "<p style='color:#800000;'><span style='font-weight: bold; color:#000000;'>" + name + ": " + "</span>";
                }
                else if (name.Contains("Tukov"))
                {
                    name = "<p style='color:#110481;'><span style='font-weight: bold; color:#000000;'>" + name + ": " + "</span>";
                }
                else if (name.Contains("Guyli"))
                {
                    name = "<p style='color:#5200CC;'><span style='font-weight: bold; color:#000000;'>" + name + ": " + "</span>";
                }
                else
                {
                    name = "<p style='color:#000000;'><span style='font-weight: bold; color:#000000;'>" + name + ": " + "</span>";
                }
                post = post.Replace('*', '✳');
                post = post.Replace('~', '〰');
                cleanedLine = name + post + " </p>";
            }
            return cleanedLine;

        }
    }
}
