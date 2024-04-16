using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Shapes;
/// <summary>
/// Need to sunset most of this and make this abstract
/// </summary>
namespace ChatToHTML
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
            lineCount = 0; //EWW - I know I should be converting this away from static, but fml 
            List<Tuple<int, string>> processedChatLines = new List<Tuple<int, string>>();
            List<string> justChatLines = new List<string>();
            List<string> allChatText;
            Encoding fileEncoding = GetChatEncoding(fileName);
            try
            {
                allChatText = File.ReadAllLines(fileName, fileEncoding).ToList<string>();
                isFileGood = true;
            }
            catch
            {
                allChatText = new List<string>();
                isFileGood = false;
            }
            //Fix word/line wrapped lines by joining them to previous

            mw.TotalLinesText.Content = allChatText.Count.ToString();

            int formatKey = GetFormatKey(mw);

            if (isFileGood)
            {

                return ProcessDiscordChatLines(allChatText, formatKey, mw);
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
            List<string> preCurrentChatLines = new List<string>();
            List<string> currentChatLines = new List<string>();
            //Discord Chat in HTML format
            if (formatKey == 3)
            {
                preCurrentChatLines = PreProcessHTMLDiscordLine(allChatText, currentWindow);
            }
            currentChatLines = ProcessCleanedDiscordLine(preCurrentChatLines, currentWindow);
            currentWindow.FormattedLinesText.Content = currentChatLines.Count.ToString();

            List<string> finalChatLines = PostTools.PostCompositionCleanup(currentChatLines);
            return finalChatLines;
        }

        private static List<string> PreProcessHTMLDiscordLine(List<string> allChatText, MainWindow currentWindow)
        {
            string returnCharacter = "↵";
            List<string> formattedChatLines = new List<string>();
            foreach (string currentLine in allChatText)
            {
                string fixedLine = currentLine;
                if (currentLine.Length == 0)
                {
                    fixedLine = "⁂";
                }
                formattedChatLines.Add(fixedLine);
            }

            string combinedLines = string.Join(returnCharacter, formattedChatLines);
            bool kickOut = false;

            List<string> cleanedChatLines = new List<string>();
            Regex reg = new Regex(@"([A-Z]{1}[a-z]|[A-Z]{1}[A-Za-z]+)+\s([A-Z]{1}[a-z]+|[A-Z]{2})\↵\s\—\s\↵\d{1,2}\/\d{1,2}\/\d{4}\s(2[0-3]|[01]?[0-9]):([0-5]?[0-9])\s(PM|AM)");
            while (!kickOut)
            {
                string nameDateBlock = "⌀";

                Match match = reg.Match(combinedLines);

                //Found an issue: if post with no text (just an image) major issues

                if (match.Index == 0 && match.Length > 0)
                {
                    string chatLine = string.Empty;
                    int nameLineLength = 0;
                    int chatLineLength = 0;
                    int returnCharacterCount = 0;
                    nameDateBlock += combinedLines.Substring(match.Index, match.Length);
                    nameLineLength = nameDateBlock.Length;
                    cleanedChatLines.Add(nameDateBlock);

                    bool isEmptyPost = false;
                    combinedLines = combinedLines.Substring(nameLineLength, (combinedLines.Length - nameLineLength));
                    //combinedLines = combinedLines.Substring(1);
                    Match nextMatch = reg.Match(combinedLines);
                    if (match.Success)
                    {
                        if (nextMatch.Success)
                        {
                            if (nextMatch.Index != 0)
                            {
                                string preChatLine = combinedLines.Substring(0, nextMatch.Index - 1);
                                chatLine = preChatLine;
                                chatLineLength = chatLine.Length - returnCharacterCount;
                            }
                            else
                            {
                                chatLine = "❌✖EMPTY LINE✖❌";
                                chatLineLength = chatLine.Length - returnCharacterCount;
                                isEmptyPost = true;
                            }
                        }
                        else
                        {
                            chatLine = combinedLines;
                            kickOut = true;
                        }


                        if (!isEmptyPost)
                        {
                            combinedLines = combinedLines.Substring((chatLineLength + returnCharacterCount + 1), combinedLines.Length - chatLineLength - returnCharacterCount - 1);
                        }
                        cleanedChatLines.Add(chatLine.Replace((returnCharacter + returnCharacter + returnCharacter), "¶").Replace(returnCharacter, "¶"));
                    }
                }
            }
            cleanedChatLines = removeReturnCharacters(cleanedChatLines);

            return cleanedChatLines;
        }

        private static List<string> removeReturnCharacters(List<string> formattedChatLines)
        {
            string tempLine = string.Empty;
            List<string> cleanedChatLines = new List<string>();
            foreach (string line in formattedChatLines)
            {
                if (line.StartsWith("⌀"))
                {
                    cleanedChatLines.Add(line.Replace("↵", string.Empty).Replace("⌀", string.Empty));
                }
                else
                {
                    tempLine = line.Replace("↵↵↵", "⁂");
                    cleanedChatLines.Add(tempLine.Replace("↵", "¶"));
                }
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
                        cleanLine = cleanLine.Replace("⁂", "<br>\r\n");
                        cleanLine = cleanLine.Replace("¶", "<br>\r\n");
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
            if (mw.UnformattedCheckBox.IsChecked == true)
            {
                return 3;
            }
            else if (mw.UnformattedCheckBox.IsChecked != true)
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
                    post = PostTools.CleanOddCharacters(post);
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


        private static string UnformattedDiscordLineFormat(string line, DataTable nameDataTable)
        {
            string cleanedLine = string.Empty;
            int headerLength = line.IndexOf('█');
            if (headerLength > 0)
            {
                string headerLine = line.Substring(0, headerLength);
                string post = line.Substring((headerLength + 1), (line.Length - headerLength - 1));

                int nameLength = line.IndexOf('—');
                string name = headerLine.Substring(0, nameLength - 1);

                if (nameLength > 0)
                {
                    InCharacterPost currentInCharacterPost = new InCharacterPost();
                    currentInCharacterPost = currentInCharacterPost.GetInCharacterPost(name, post, nameDataTable);
                    cleanedLine = ConvertInCharacterPostToHTMLPostLine(currentInCharacterPost);
                    return cleanedLine;
                }
            }
            cleanedLine = "❌ - Line Error";
            return cleanedLine;
        }

        private static string ConvertInCharacterPostToHTMLPostLine(InCharacterPost intakePost)
        {
            string currentHTMLLine = "<p style='color:" +
                    intakePost.UserColor + "; font-family: " +
                    intakePost.CharacterFontInfo.FontFamily + " !important; font-size: " +
                    intakePost.CharacterFontInfo.FontSize + " !important; letter-spacing: " +
                    intakePost.CharacterFontInfo.LetterSpacing + " !important;' class='";

            currentHTMLLine += PostTools.RemoveWhitespace(intakePost.CharacterName) + "_Paragraph'>" +
                    "<span style='font-family: initial !important" +
                    "; font-weight: bold; color: " +
                    intakePost.HeaderColor + "; letter-spacing: initial !important;" +
                    " font-size: 14px !important;'" + " class='" +
                    PostTools.RemoveWhitespace(intakePost.CharacterName) +
                    "_NameBlock'>" +
                    intakePost.CharacterName + ": " + "</span>";

            currentHTMLLine += PostTools.CleanOddCharacters(intakePost.PostContent) + "</p>";

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
                post = PostTools.CleanOddCharacters(post);
                cleanedLine = name + post + endParagraphTag;
            }

            return cleanedLine;
        }

        private static string AddNameTags(string name, DataTable nameDataTable)
        {
            throw new NotImplementedException();
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
