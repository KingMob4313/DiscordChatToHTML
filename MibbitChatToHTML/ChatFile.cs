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
        static int errorLineCount = 0;
        static int lineCount = 0;
        static bool isFileGood = true;

        public static List<string> ProcessChatFile(string fileName, int TextFileType, MainWindow mw)
        {
            List<Tuple<int, string>> processedChatLines = new List<Tuple<int, string>>();
            List<string> justChatLines = new List<string>();
            Encoding fileEncoding = GetChatEncoding(fileName);

            //Fix word/line wrapped lines by joining them to previous
            List<string> allChatText = File.ReadAllLines(fileName, fileEncoding).ToList<string>();

            int formatKey = GetFormatKey(mw);
            lineCount = 0;
            errorLineCount = 0;
            isFileGood = true;

            //Lol this is hilariously ignorant - needs a refactor
            if (TextFileType == 1 && formatKey == 3)
            {
                //Combine split lines by discord
                List<string> discordLines = new List<string>();
                discordLines = CombineDiscordFormattedLines(allChatText);
                allChatText.Clear();
                allChatText.AddRange(discordLines);
                return allChatText;
            }

            foreach (string line in allChatText)
            {
                if (isFileGood)
                {
                    if (line.Length > 2)
                    {
                        if (TextFileType == 0)
                        {
                            string cleanLine = CleanUpMibbitFormatting(line, formatKey);
                            Tuple<int, string> mibbitLine = new Tuple<int, string>(lineCount, cleanLine);
                            processedChatLines.Add(mibbitLine);
                            justChatLines.Add(cleanLine + "\r\n");
                        }
                        else
                        {
                            if (formatKey == 2)
                            {
                                string cleanLine = CleanUpDiscordFormatting(line, formatKey);
                                Tuple<int, string> discoLine = new Tuple<int, string>(lineCount, cleanLine);
                                processedChatLines.Add(discoLine);
                                justChatLines.Add(cleanLine + "\r\n");
                            } 
                        }
                    }
                    else
                    { }
                    lineCount++;
                }
                else
                {
                    justChatLines.Clear();
                    justChatLines.Add("WRONG FILE FORMAT TRY A DIFFERENT FORMAT!");
                    return justChatLines;
                }
            }
            return justChatLines;
        }

        private static List<string> CombineDiscordFormattedLines(List<string> allChatText)
        {
            List<string> currentLineList = new List<string>();
            string composedLine = string.Empty;
            for (int i = 0; i < allChatText.Count; i++)
            {
                //Get current line
                string currentLine = allChatText[i];
                //See if current line has a header if so...
                if (CheckForHeaderLine(currentLine) && (i < allChatText.Count))
                {
                    //Look ahead - Start at one
                    i++;
                    currentLine = AddNameTags(currentLine);
                    composedLine = JoinAndCleanFormattedLines(allChatText, i, currentLine);
                    i++;
                    if (i < allChatText.Count)
                    {


                        //Check if the next-next line has date and EM dash - if not join line.
                        while (!CheckForHeaderLine(allChatText[i]))
                        {
                            string tempLine = composedLine;
                            composedLine = JoinAndCleanFormattedLines(allChatText, i, tempLine);
                            i++;

                        }
                    }
                    currentLineList.Add(CleanOddCharacters(composedLine) + "</p>\r\n");
                    //Adjust counter back
                    i--;
                }
            }
            return currentLineList;
        }

        private static string JoinAndCleanFormattedLines(List<string> allChatText, int i, string currentLine)
        {
            string composedLine;
            string nextLine = allChatText[i];
            composedLine = currentLine + " " + nextLine.Replace("\r\n", " ");
            return composedLine;
        }

        private static bool CheckForHeaderLine(string line)
        {
            return line.Contains("—") && line.Contains(@"/2021");
        }

        private static Encoding GetChatEncoding(string filename)
        {
            Encoding currentEncoding = null;
            using (var reader = new StreamReader(filename, Encoding.UTF8, true))
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

        private static string CleanUpDiscordFormatting(string line, int formatKey)
        {
            string cleanedLine = string.Empty;

            if (formatKey == 1)
            {
                cleanedLine = FullFormattedDiscordLineFormat(line, cleanedLine);
            }
            else if (formatKey == 2)
            {
                cleanedLine = UnformattedDiscordLineFormat(line, cleanedLine);
            }
            return cleanedLine;
        }

        private static string FullFormattedDiscordLineFormat(string line, string cleanedLine)
        {
            //string pattern = @"^\[([0-3]|[01]?[0-9]):([0-5]?[0-9])\s(PM|AM)\]\s";
            //Regex reg = new Regex(pattern, RegexOptions.IgnoreCase);

            //Match match = reg.Match(line);
            //int spacer = match.Length;

            //if (match.Success && line.Length > 2)
            //{
            //    string ggg = line[spacer].ToString();
            //    if (ggg != "\t")
            //    {
            //        string tempTrimmedLine = line.Substring(spacer, (line.Length - spacer));

            //        int nameTagStart = 0;
            //        int nameTagEnd = tempTrimmedLine.IndexOf(':', nameTagStart + 1);
            //        string firstTemp = tempTrimmedLine.Replace(':', ' ');
            //        string name = firstTemp.Substring((nameTagStart), (nameTagEnd - nameTagStart));
            //        string post = tempTrimmedLine.Substring(nameTagEnd + 1);

            //        name = AddNameTags(name);
            //        post = CleanOddCharacters(post);
            //        tempTrimmedLine = FormattingOddityCatcher(tempTrimmedLine);
            //        cleanedLine = name + post + " </p>";
            //    }
            //    else
            //    {
            //        cleanedLine = UserLogEntryHandler(line);
            //    }
            //}
            //else
            //{
            //    if (!match.Success)
            //    {
            //        cleanedLine = "NO MATCH - MISSING LINE";
            //    }
            //}
            return cleanedLine;
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
            //Right now "Word breaks things. Need to clean up when there is a quote and then not a space
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
                    tempTrimmedLine = FormattingOddityCatcher(tempTrimmedLine);
                    cleanedLine = name + post + " </p>";
                }
                else
                {
                    cleanedLine = UserLogEntryHandler(line);
                }

            }
            else
            {
                //ADD ERROR COUNT KICK OUT AND ADD POPUP IF TOO MANY
                if (!match.Success)
                {
                    cleanedLine = "NO MATCH - MISSING LINE";
                    errorLineCount++;
                    if (errorLineCount > 3 && errorLineCount / lineCount > 0.3)
                    {
                        isFileGood = false;

                    }
                }
            }

            if (isFileGood)
            {
                int quoteCounter = 0;
                foreach (char lineChar in cleanedLine)
                {
                    if (lineChar.ToString() == "\"")
                    {
                        quoteCounter++;
                    }
                }
                if ((quoteCounter % 2) == 1)
                {
                    CorrectionControl dialog = new CorrectionControl();
                    dialog.TextToCorrectTextBox.Text = cleanedLine;
                    //dialog.OriginalChatText.Text = tempHtmlLines.ToString();

                    dialog.ShowDialog();
                    if (dialog.DialogResult.HasValue && dialog.DialogResult.Value)
                    {
                        cleanedLine = dialog.TextToCorrectTextBox.Text;
                    }
                }

                return cleanedLine;
            }
            else
            {
                return "WRONG FILE FORMAT TRY A DIFFERENT FORMAT!";
            }
        }

        private static string UnformattedDiscordLineFormat(string line, string cleanedLine)
        {
            string pattern = @"^\[([0-3]|[01]?[0-9]):([0-5]?[0-9])\s(PM|AM)\]\s";
            Regex reg = new Regex(pattern, RegexOptions.IgnoreCase);

            Match match = reg.Match(line);
            int spacer = match.Length;

            if (match.Success && line.Length > 2)
            {
                string ggg = line[spacer].ToString();
                if (ggg != "\t")
                {
                    string tempTrimmedLine = line.Substring(spacer, (line.Length - spacer));

                    int nameTagStart = 0;
                    int nameTagEnd = tempTrimmedLine.IndexOf(':', nameTagStart + 1);
                    string firstTemp = tempTrimmedLine.Replace(':', ' ');
                    string name = firstTemp.Substring((nameTagStart), (nameTagEnd - nameTagStart));
                    string post = tempTrimmedLine.Substring(nameTagEnd + 1);

                    name = AddNameTags(name);
                    post = CleanOddCharacters(post);
                    tempTrimmedLine = FormattingOddityCatcher(tempTrimmedLine);
                    cleanedLine = name + post + " </p>";
                }
                else
                {
                    cleanedLine = UserLogEntryHandler(line);
                }
            }
            else
            {
                if (!match.Success)
                {
                    cleanedLine = "NO MATCH - MISSING LINE";
                }
            }
            return cleanedLine;
        }

        private static string UserLogEntryHandler(string line)
        {
            string cleanedLine;
            if (line.Contains("mibbit.com Online IRC Client"))
            {
                cleanedLine = "USER QUIT";
            }
            else if (line.ToLower().Contains("joined") && line.ToLower().Contains("thirdsofthewheel"))
            {
                cleanedLine = "USER JOINED";
            }
            else
            {
                cleanedLine = "MISSING LINE";
            }

            return cleanedLine;
        }

        private static string FormattingOddityCatcher(string tempTrimmedLine)
        {
            //string cleanedLine = string.Empty;
            //int characterCount = tempTrimmedLine.Count();
            //for (int i = 0; i < characterCount; i++)
            //{
            //    if (i < characterCount - 2)
            //    {
            //        string watcher = string.Empty;
            //        string watcher2 = string.Empty; 
            //        if (i > 1)
            //        {
            //            watcher = tempTrimmedLine.Substring(i - 1, 3);
            //            watcher2 = tempTrimmedLine.Substring(i - 1, 2);
            //        }
            //        if (tempTrimmedLine.Substring(i, 1) == "\"" && (tempTrimmedLine.Substring(i - 1, 2) != " \"") && (tempTrimmedLine.Substring(i + 1, 1) != " "))
            //        {
            //            cleanedLine += tempTrimmedLine.Substring(i, 1) + "  ";
            //            //May need to adjust this
            //            i = i + 1;
            //        }
            //        else
            //        {
            //            cleanedLine += tempTrimmedLine.Substring(i, 1);
            //        }
            //    }
            //    else
            //    {
            //        cleanedLine += tempTrimmedLine.Substring(i, 1);
            //    }
            //}
            //return cleanedLine;

            return tempTrimmedLine;
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
            string tempString = string.Empty;
            tempString = CharacterReplacer(post, "*", "✳");
            tempString = CharacterReplacer(tempString, "~", "〰");
            tempString = CharacterReplacer(tempString, "”", "\"");
            tempString = CharacterReplacer(tempString, "“", "\"");
            tempString = CharacterReplacer(tempString, "’", "'");
            tempString = CharacterReplacer(tempString, "…", "... ");
            tempString = CharacterReplacer(tempString, "...", "... ");
            tempString = CharacterReplacer(tempString, "))", " )) ");
            tempString = CharacterReplacer(tempString, "_", string.Empty);
            tempString = CharacterReplacer(tempString, ".\"", ". \"");

            if (tempString.Length > 0)
            {
                if ((tempString.IndexOf('-') + 1) != tempString.Length)
                {
                    string afterDashCharacter = tempString.Substring((tempString.IndexOf('-') + 1), 1);
                    if (!string.IsNullOrWhiteSpace(afterDashCharacter))
                    {
                        tempString = CharacterReplacer(tempString, " -", " 〰");
                        tempString = CharacterReplacer(tempString, "- ", "〰 ");
                    }
                }
            }

            return tempString;
        }

        private static string CharacterReplacer(string post, string badCharacter, string goodCharacter)
        {
            string tempString = string.Empty;

            if (post.Contains(badCharacter))
            {
                tempString = post.Replace(badCharacter, goodCharacter).ToString();
                return tempString;
            }
            else
            {
                tempString = (post).ToString();
                return tempString;
            }
        }

        private static string AddNameTags(string name)
        {
            if (name.Contains("Yara"))
            {
                name = "<p style='color:#666666;'><span style='font-weight: bold; color:#000000;'>" + "Yara Sherred" + ": " + "</span>";

            }
            else if (name.Contains("Damian") || name.Contains("BigBadWolf") || name.ToLower().Contains("blacksmithst"))
            {
                if (name.Contains("BigBadWolf") || name.Contains("Damian"))
                {
                    name = "<p style='color:#800000;'><span style='font-weight: bold; color:#000000;'>" + "DamianStark" + ": " + "</span>";
                }
                else
                {
                    name = "<p style='color:#800000;'><span style='font-weight: bold; color:#000000;'>" + "BlackSmithST" + ": " + "</span>";
                }
            }
            else if (name.Contains("Tukov") || name.Contains("ST4313"))
            {
                if (name.Contains("Tukov"))
                {
                    name = "<p style='color:#110481;'><span style='font-weight: bold; color:#000000; font-family: Lucida Console, Monaco, monospace; letter - spacing: 0.07em;'>" + "Kai Tukov" + ": " + "</span>";
                }
                else
                {
                    name = "<p style='color:#110481;'><span style='font-weight: bold; color:#000000; font-family: Lucida Console, Monaco, monospace; letter - spacing: 0.07em;'>" + "ST4313" + ": " + "</span>";
                }

            }
            else if (name.Contains("Guyli") || name.Contains("The Minx"))
            {
                name = "<p style='color:#5200CC;'><span style='font-weight: bold; color:#000000;'>" + "Guylian" + ": " + "</span>";
            }
            else
            {
                name = "<p style='color:#000000;'><span style='font-weight: bold; color:#000000;'>" + name + ": " + "</span>";
            }

            return name;
        }
    }
}
