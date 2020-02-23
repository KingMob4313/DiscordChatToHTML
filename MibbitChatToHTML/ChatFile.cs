using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MibbitChatToHTML
{
    class ChatFile
    {
        public static List<string> ProcessChatFile(string fileName, MainWindow mw)
        {
            List<Tuple<int, string>> processedChatLines = new List<Tuple<int, string>>();
            List<string> justChatLines = new List<string>();
            Encoding fileEncoding = GetChatEncoding(fileName);

            //Fix word/line wrapped lines by joining them to previous
            List<string> allChatText = File.ReadAllLines(fileName, fileEncoding).ToList<string>();
            int formatKey = GetFormatKey(mw);

            int counter = 0;
            foreach (string line in allChatText)
            {

                if (line.Length > 2)
                {
                    string cleanLine = CleanUpMibbitFormatting(line, formatKey);
                    Tuple<int, string> tLine = new Tuple<int, string>(counter, cleanLine);
                    processedChatLines.Add(tLine);
                    justChatLines.Add(cleanLine + "\r\n");
                }
                counter++;
            }
            return justChatLines;
        }

        private static Encoding GetChatEncoding(string filename)
        {
            Encoding currentEncoding = null;
            using (var reader = new StreamReader(filename, Encoding.ASCII, true))
            {
                reader.Peek(); // you need this!
                currentEncoding = reader.CurrentEncoding;
            }
            return currentEncoding;

        }
        private static int GetFormatKey(MainWindow mw)
        {
            //Monstrously ill advised, but I'm feeling lazy
            if (mw.CBCleanedCheckBox.IsChecked == true)
            {
                return 1;
            }
            else if (mw.UnformattedCheckBox.IsChecked == true)
            {
                return 2;
            }
            else
            {
                return 3;
            }
        }

        private static string CleanUpMibbitFormatting(string line, int formatKey)
        {
            //<p style='color:#TEXTCOLORHERE;'><span style='font-weight: bold; color:#000000;'>NAME</span> Text </p>
            string cleanedLine = string.Empty;
            if (formatKey == 1)
            {
                cleanedLine = CBCleanedVersionLineFormat(line, cleanedLine);
            }
            else if (formatKey == 2)
            {
                cleanedLine = UnformattedLineFormat(line, cleanedLine);
            }
            else
            {
                throw new NotImplementedException();
            }

            return cleanedLine;

        }

        private static string UnformattedLineFormat(string line, string cleanedLine)
        {

            string trimmedLine = string.Empty;

            string pattern = @"^(2[0-3]|[01]?[0-9]):([0-5]?[0-9])\t";
            Regex reg = new Regex(pattern, RegexOptions.IgnoreCase);

            Match match = reg.Match(line);

            if (match.Success && line.Length > 2)
            {
                string ggg = line[6].ToString();
                if (ggg != "\t")
                {
                    string tempTrimmedLine = line.Substring(6, (line.Length - 6));

                    int nameTagStart = 0;
                    int nameTagEnd = tempTrimmedLine.IndexOf('\t', nameTagStart + 1);
                    string firstTemp = tempTrimmedLine.Replace(':', ' ');
                    string name = firstTemp.Substring((nameTagStart), (nameTagEnd - nameTagStart));
                    string post = tempTrimmedLine.Substring(nameTagEnd + 1);

                    name = AddNameTags(name);
                    post = CleanOddCharacters(post);
                    cleanedLine = name + post + " </p>";
                }
            }

            return cleanedLine;
        }

        private static string CBCleanedVersionLineFormat(string line, string cleanedLine)
        {
            if (line.StartsWith("*") && line.Length > 2)
            {
                int nameTagStart = line.IndexOf('*');
                int nameTagEnd = line.IndexOf('*', nameTagStart + 1);
                string firstTemp = line.Replace(':', ' ');
                string name = firstTemp.Substring((nameTagStart + 1), ((nameTagEnd - nameTagStart) - 2));
                string post = line.Substring(nameTagEnd + 1);

                name = AddNameTags(name);
                post = CleanOddCharacters(post);
                cleanedLine = name + post + " </p>";
            }

            return cleanedLine;
        }

        private static string CleanOddCharacters(string post)
        {
            post = post.Replace('*', '✳');
            post = post.Replace('~', '〰');
            if (post.Length > 0)
            {
                string afterDashCharacter = post.Substring((post.IndexOf('-') + 1), 1);
                if (!string.IsNullOrWhiteSpace(afterDashCharacter))
                {
                    post = post.Replace(" -", " 〰");
                    post = post.Replace("- ", "〰 ");
                }
            }

            return post;
        }

        private static string AddNameTags(string name)
        {
            if (name.Contains("Yara"))
            {
                name = "<p style='color:#666666;'><span style='font-weight: bold; color:#000000;'>" + name + ": " + "</span>";
            }
            else if (name.Contains("Damian") || name.Contains("BigBadWolf"))
            {
                name = "<p style='color:#800000;'><span style='font-weight: bold; color:#000000;'>" + "DamianStark" + ": " + "</span>";
            }
            else if (name.Contains("Tukov") || name.Contains("ST4313"))
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

            return name;
        }
    }
}
