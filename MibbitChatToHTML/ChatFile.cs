using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
/// <summary>
/// Need to sunset most of this and make this abstract
/// </summary>
namespace MibbitChatToHTML
{
    class ChatFile
    {
        static int errorLineCount = 0;
        static int lineCount = 0;
        static bool isFileGood = true;
        static readonly string endParagraphTag = " </p>";
        //static readonly string regExPattern = @"^[a-zA-Z]+\s[a-zA-Z]+\s\—\s(0[1-9]|[12][0-9]|3[01])\/(0[1-9]|1[1,2])\/(19|20)\d{2}\s((1[0-2]|0?[1-9]):([0-5][0-9])\s([AaPp][Mm]))$";
        static readonly string regExPattern = @"^[a-zA-Z]+\s[a-zA-Z]+\s\—\s(1[0-2]|0[1-9])\/(0[1-9]|[12][0-9]|3[01])\/(19|20)\d{2}\s((1[0-2]|0?[1-9]):([0-5][0-9])\s([AaPp][Mm]))$";
        public static List<string> ProcessChatFile(string fileName, int TextFileType, MainWindow mw)
        {
            List<Tuple<int, string>> processedChatLines = new List<Tuple<int, string>>();
            List<string> justChatLines = new List<string>();
            Encoding fileEncoding = GetChatEncoding(fileName);

            //Fix word/line wrapped lines by joining them to previous
            List<string> allChatText = File.ReadAllLines(fileName, fileEncoding).ToList<string>();
            mw.TotalLinesText.Content = allChatText.Count.ToString();

            int formatKey = GetFormatKey(mw);
            lineCount = 0;
            errorLineCount = 0;
            isFileGood = true;
            if (isFileGood)
            {
                if (formatKey > 2)
                {
                    return ProcessDiscordChatLines(allChatText, formatKey, mw);
                }
                else
                {
                    return ProcessMibbitChatLines(allChatText, formatKey, mw);
                }
            }
            return justChatLines;
        }

        private static List<string> ProcessMibbitChatLines(List<string> allChatText, int formatKey, MainWindow currentWindow)
        {
            List<string> currentChatLines = new List<string>();
            foreach (string line in allChatText)
            {
                if (line.Length > 2 && isFileGood)
                {
                    string cleanLine = CleanUpMibbitFormatting(line, formatKey, currentWindow.mainNameDataTable);
                    Tuple<int, string> mibbitLine = new Tuple<int, string>(lineCount, cleanLine);
                    //currentChatLines.Add(mibbitLine);
                    currentChatLines.Add(cleanLine + "\r\n");
                }
                lineCount++;
            }
            currentWindow.FormattedLinesText.Content = currentChatLines.Count.ToString();
            return currentChatLines;
        }

        private static List<string> ProcessDiscordChatLines(List<string> allChatText, int formatKey, MainWindow currentWindow)
        {
            List<string> currentChatLines = new List<string>();
            //Discord Chat in HTML format
            if (formatKey == 3)
            {
                allChatText = PreProcessHTMLDiscordLine(allChatText, currentWindow);
            }
            currentChatLines = ProcessCleanedDiscordLine(allChatText, currentWindow);
            currentWindow.FormattedLinesText.Content = currentChatLines.Count.ToString();
            return currentChatLines;
        }

        private static List<string> PreProcessHTMLDiscordLine(List<string> allChatText, MainWindow currentWindow)
        {
            string combinedLines = string.Join("↵", allChatText);
            string trimmerLine = combinedLines;
            bool kickOut = false;
            List<string> formattedChatLines = new List<string>();
            Regex reg = new Regex(@"[A-Z]{1}[a-z]+\s[A-Z]{1}[a-z]+\↵\s\—\s\↵\d{1,2}\/\d{1,2}\/\d{4}\s(2[0-3]|[01]?[0-9]):([0-5]?[0-9])\s(PM|AM)");
            trimmerLine = combinedLines;
            while (!kickOut)
            {
                string nameDateBlock = string.Empty;

                Match match = reg.Match(trimmerLine);
                if (match.Index == 0 && match.Length > 0)
                {
                    string chatLine = string.Empty;
                    int nameLineLength = 0;
                    int chatLineLength = 0;
                    int returnCharacterCount = 0;
                    nameDateBlock = trimmerLine.Substring(match.Index, match.Length);
                    nameLineLength = nameDateBlock.Length;
                    formattedChatLines.Add(nameDateBlock);

                    trimmerLine = trimmerLine.Substring(nameLineLength, (trimmerLine.Length - nameLineLength));
                    trimmerLine = trimmerLine.Substring(1);
                    Match nextMatch = reg.Match(trimmerLine);
                    if (match.Success)
                    {
                        if (nextMatch.Success)
                        {
                            string preChatLine = trimmerLine.Substring(0, nextMatch.Index - 1);
                            chatLine = preChatLine;
                            chatLineLength = chatLine.Length - returnCharacterCount;
                        }
                        else
                        {
                            chatLine = trimmerLine;
                            kickOut = true;
                        }
                    }
                    formattedChatLines.Add(chatLine);
                    trimmerLine = trimmerLine.Substring((chatLineLength + returnCharacterCount + 1), trimmerLine.Length - chatLineLength - returnCharacterCount - 1);
                }
            }
            formattedChatLines = removeReturnCharacters(formattedChatLines);

            return formattedChatLines;
        }

        private static List<string> removeReturnCharacters(List<string> formattedChatLines)
        {
            List<string> cleanedChatLines = new List<string>();
            foreach (string line in formattedChatLines)
            {
                cleanedChatLines.Add(line.Replace("↵", string.Empty));
            }
            return cleanedChatLines;
        }

        private static List<string> ProcessCleanedDiscordLine(List<string> allChatText, MainWindow currentWindow)
        {
            List<string> ProcessDiscordChatLines = new List<string>();

            //Discord Chat in native APP format
            foreach (string line in allChatText)
            {
                string processedLine = GatherFollowingLines(allChatText, lineCount);
                if (processedLine.Length > 2 && isFileGood)
                {
                    string cleanLine = UnformattedDiscordLineFormat(processedLine, currentWindow.mainNameDataTable);

                    if (cleanLine.Length > 0)
                    {
                        ProcessDiscordChatLines.Add(cleanLine + "\r\n");
                    }
                }
            }
            return ProcessDiscordChatLines;
        }

        private static string RemoveStrayDates(string chatLine)
        {
            Regex reg = new Regex(@"^\[((1[0-2]|0?[1-9]):([0-5][0-9])\s([AaPp][Mm]))\]", RegexOptions.None);
            Match match = reg.Match(chatLine);
            if (match.Success)
            {
                return string.Empty;
            }
            else
            {
                return chatLine;
            }
        }

        private static string GatherFollowingLines(List<string> lineList, int currentLineCount)
        {
            //List<string> preAssembledLines = new List<string>();
            StringBuilder assembledLine = new StringBuilder();
            int gatherLineCount = currentLineCount;
            string currentLine = string.Empty;
            bool haveHeaderLine = false;
            bool isPostDone = false;

            Regex reg = new Regex(regExPattern, RegexOptions.IgnoreCase);

            while (!isPostDone && gatherLineCount < lineList.Count)
            {
                currentLine = RemoveStrayDates(lineList[gatherLineCount]);

                //line with header
                Match match = reg.Match(currentLine);
                if (match.Success)
                {
                    if (!haveHeaderLine)
                    {
                        haveHeaderLine = true;
                        assembledLine.Append(currentLine + " █");
                    }
                    else
                    {
                        match = reg.Match(currentLine);
                        if (match.Success)
                        {
                            isPostDone = true;
                        }
                    }
                }
                else
                {
                    assembledLine.Append("\r\n" + currentLine);
                }

                if (!isPostDone)
                {
                    gatherLineCount++;
                }

            }
            lineCount = gatherLineCount;
            return assembledLine.ToString();
        }

        private static string ProcessFullFormatDiscordChat(List<string> allChatText, DataTable nameTable, ref int i)
        {
            string currentLine = string.Empty;
            List<string> currentLineList = new List<string>();

            //Add current line by AllChatText[i]
            string LineToProcess = allChatText[i].Length > 2 ? allChatText[i].ToString() : string.Empty;

            //Need to add a line-feed break processor
            if (CheckForHeaderLine(LineToProcess) && (i < allChatText.Count))
            {
                string composedLine = string.Empty;
                //Look ahead - Start at one
                i++;
                LineToProcess = AddNameTags(LineToProcess, nameTable);
                composedLine = JoinAndCleanFormattedLines(allChatText, i, LineToProcess, false) + "<br />";

                //Look head again
                i++;
                if (i < allChatText.Count)
                {
                    //Check if the next-next line has date and EM dash - if not join line.
                    while (!CheckForHeaderLine(allChatText[i]))
                    {
                        string tempLine = composedLine;
                        composedLine = JoinAndCleanFormattedLines(allChatText, i, tempLine, true);
                        i++;
                    }
                }
                string cleanedLine = CleanOddCharacters(composedLine);
                currentLine = (cleanedLine.Substring(0, (cleanedLine.Length - 6)) + endParagraphTag + "\r\n");
                //Adjust counter back
                i--;
            }
            return currentLine;
        }

        private static string JoinAndCleanFormattedLines(List<string> allChatText, int i, string currentLine, bool needsLineFeed)
        {
            string composedLine;
            string nextLine = allChatText[i];
            if (needsLineFeed)
            {
                composedLine = currentLine.Replace("\r\n", " ") + " <br />" + (nextLine.Length > 2 ? nextLine.Replace("\r\n", " ") + "<br />" : string.Empty);
            }
            else
            {
                composedLine = currentLine.Replace("\r\n", " ") + " " + nextLine.Replace("\r\n", " ") + " ";
            }
            return composedLine;
        }

        private static bool CheckForHeaderLine(string line)
        {
            return line.Contains("—") && (line.Contains(@"/" + DateTime.Now.Year.ToString()) || line.Contains("Yesterday") || line.Contains("Today"));
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
            //1 = mibbit + formatted
            if (mw.UnformattedCheckBox.IsChecked == false && mw.TextFileTypeComboBox.SelectedIndex == 0)
            {
                return 1;
            }
            //2 = mibbit + unformatted
            else if (mw.UnformattedCheckBox.IsChecked == true && mw.TextFileTypeComboBox.SelectedIndex == 0)
            {
                return 2;
            }
            //3 = discord + Mirc style
            else if (mw.UnformattedCheckBox.IsChecked == true && mw.TextFileTypeComboBox.SelectedIndex == 1)
            {
                return 3;
            }
            else if (mw.UnformattedCheckBox.IsChecked == false && mw.TextFileTypeComboBox.SelectedIndex == 1)
            {
                return 4;
            }
            else
            { return 0; }
        }

        private static string CleanUpMibbitFormatting(string line, int formatKey, DataTable nameDataTable)
        {
            //<p style='color:#TEXTCOLORHERE;'><span style='font-weight: bold; color:#000000;'>NAME</span> Text </p>
            string cleanedLine = string.Empty;
            if (formatKey == 2)
            {
                cleanedLine = UnformattedLineFormat(line, cleanedLine, nameDataTable);

            }
            else if (formatKey == 1)
            {
                cleanedLine = CBCleanedVersionLineFormat(line, cleanedLine, nameDataTable);
            }
            else
            {
                throw new NotImplementedException();
            }

            return cleanedLine;

        }

        private static string UnformattedLineFormat(string line, string cleanedLine, DataTable nameDataTable)
        {
            //Right now "Word breaks things. Need to clean up when there is a quote and then not a space
            string trimmedLine = string.Empty;
            int timeStampOffset = 9;
            string pattern = @"^(2[0-3]|[01]?[0-9]):([0-5]?[0-9])\s{4}";
            Regex reg = new Regex(pattern, RegexOptions.IgnoreCase);

            Match match = reg.Match(line);

            if (match.Success && line.Length > 2)
            {
                string ggg = line[timeStampOffset].ToString();
                if (ggg != "\t")
                {
                    string tempTrimmedLine = line.Substring(timeStampOffset, (line.Length - timeStampOffset));

                    int nameTagStart = 0;
                    int nameTagEnd = tempTrimmedLine.IndexOf('\t', nameTagStart + 1);
                    string firstTemp = tempTrimmedLine.Replace(':', ' ');
                    string name = firstTemp.Substring((nameTagStart), (nameTagEnd - nameTagStart));
                    string post = tempTrimmedLine.Substring(nameTagEnd + 1);

                    name = AddNameTags(name, nameDataTable);
                    post = CleanOddCharacters(post);
                    tempTrimmedLine = FormattingOddityCatcher(tempTrimmedLine);
                    cleanedLine = name + post + endParagraphTag;
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

        private static string TrimmedDiscordLineFormat(string line, DataTable nameDataTable)
        {
            string pattern = @"^[A-Z]{1}[a-z]+\s[A-Z]{1}[a-z]+\n\s\—\s\n\d{1,2}\/\d{1,2}\/\d{4}\s(2[0-3]|[01]?[0-9]):([0-5]?[0-9])\s(PM|AM)";

            string cleanedLine = string.Empty;
            Regex reg = new Regex(pattern, RegexOptions.IgnoreCase);

            Match match = reg.Match(line);
            int xyz = match.Length;
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

                    name = AddNameTags(name, nameDataTable);
                    post = CleanOddCharacters(post);
                    tempTrimmedLine = FormattingOddityCatcher(tempTrimmedLine);
                    cleanedLine = name + post + endParagraphTag;
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

        private static string UnformattedDiscordLineFormat(string line, DataTable nameDataTable)
        {
            string cleanedLine = string.Empty;
            int headerLength = line.IndexOf('█');

            string headerLine = line.Substring(0, headerLength);
            string post = line.Substring((headerLength + 1), (line.Length - headerLength - 1));

            int nameLength = line.IndexOf('—');
            string name = headerLine.Substring(0, nameLength - 1);

            if (nameLength > 0)
            {
                InCharacterPost currentInCharacterPost = new InCharacterPost();
                currentInCharacterPost = currentInCharacterPost.GetInCharacterPost(name, post, nameDataTable);
                cleanedLine = ConvertInCharacterPostToHTMLPostLine(currentInCharacterPost);
            }
            else
            {
                cleanedLine = "❌ - Line Error";
            }
            return cleanedLine;
        }

        private static string ConvertInCharacterPostToHTMLPostLine(InCharacterPost intakePost)
        {
            string currentHTMLLine = "<p style='color:" +
            intakePost.UserColor + "; font-family: " +
            intakePost.CharacterFontInfo.FontFamily + "; letter-spacing: " +
            intakePost.CharacterFontInfo.LetterSpacing + ";' class='" +

            PostTools.RemoveWhitespace(intakePost.CharacterName) + "_Paragraph'>" +
            "<span style='font-weight: bold; color: " +
            intakePost.HeaderColor + ";'" + " class='" +
            PostTools.RemoveWhitespace(intakePost.CharacterName) +
            "_NameBlock'>" +
            intakePost.CharacterName + ": " + "</span>";

            currentHTMLLine += intakePost.PostContent + "</p>";

            return currentHTMLLine;
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

        //Sunsetted, not sure

        //private static string CBCleanedVersionLineFormat(string line, string cleanedLine, DataTable nameDataTable)
        //{
        //    if (line.StartsWith("*") && line.Length > 2)
        //    {
        //        int nameTagStart = line.IndexOf('*');
        //        int nameTagEnd = line.IndexOf('*', nameTagStart + 1);
        //        string firstTemp = line.Replace(':', ' ');
        //        string name = firstTemp.Substring((nameTagStart + 1), ((nameTagEnd - nameTagStart) - 2));
        //        string post = line.Substring(nameTagEnd + 1);

        //        name = AddNameTags(name, nameDataTable);
        //        post = CleanOddCharacters(post);
        //        cleanedLine = name + post + endParagraphTag;
        //    }

        //    return cleanedLine;
        //}

        private static string CBCleanedVersionLineFormat(string line, string cleanedLine, DataTable nameDataTable)
        {
            string pattern = @"^(2[0-3]|[01]?[0-9]):([0-5]?[0-9])\t";
            Regex reg = new Regex(pattern, RegexOptions.IgnoreCase);

            Match match = reg.Match(line);

            if (match.Success && line.Length > 2 && !line.Contains("\t***"))
            {
                int nameTagStart = 6;
                int nameTagEnd = line.IndexOf('\t', nameTagStart);
                string firstTemp = line;
                string name = firstTemp.Substring((nameTagStart), ((nameTagEnd - nameTagStart)));
                string post = line.Substring(nameTagEnd + 1);

                name = AddNameTags(name, nameDataTable);
                post = CleanOddCharacters(post);
                cleanedLine = name + post + endParagraphTag;
            }

            return cleanedLine;
        }

        private static string AddNameTags(string name, DataTable nameDataTable)
        {
            throw new NotImplementedException();
        }

        private static string CleanOddCharacters(string post)
        {
            string tempString = string.Empty;

            //Ugly AF - but functional
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
            tempString = CharacterReplacer(tempString, "<br /> <br /> <br />", "<br /> <br />");

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
        private InCharacterPost GetCharacterPostInfo(string thisPost, DataTable nameDataTable)
        {
            InCharacterPost thisPostLine = new InCharacterPost(thisPost, nameDataTable);
            return thisPostLine;
        }
    }
}
