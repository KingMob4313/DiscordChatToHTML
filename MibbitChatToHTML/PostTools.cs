using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace ChatToHTML
{
    public static class PostTools
    {
        static Random random = new Random();

        //Get scene hex code and make sure it's not the same forward and backward
        public static string GetRandomHexNumber(int digits)
        {
            bool isAnagram = true;
            string result = string.Empty;
            //redo the random if it's an anagram
            while (isAnagram)
            {
                byte[] buffer = new byte[digits / 2];
                random.NextBytes(buffer);
                result = String.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
                string reversedResult = PostTools.ReverseString(result);
                isAnagram = result == reversedResult;
            }

            if (!(digits % 2 == 0))
            {
                result = result + random.Next(16).ToString("X");
            }
            return result;
        }

        public static IEnumerable<string> GraphemeClusters(this string s)
        {
            var enumerator = StringInfo.GetTextElementEnumerator(s);
            while (enumerator.MoveNext())
            {
                yield return (string)enumerator.Current;
            }
        }

        public static string ReverseGraphemeClusters(this string s)
        {
            return string.Join("", s.GraphemeClusters().Reverse().ToArray());
        }

        public static string ReverseString(string stringInput)
        {
            //Don't use this with unicode characters with accents
            string reversedString;

            char[] charArray = stringInput.ToCharArray();
            Array.Reverse(charArray);
            reversedString = new string(charArray);

            return reversedString;
        }

        public static string CleanOddCharacters(string post)
        {
            string tempString = string.Empty;

            //Ugly AF - but functional
            tempString = CharacterReplacer(post, "*", "✳");
            tempString = CharacterReplacer(tempString, "~", "〰");
            tempString = CharacterReplacer(tempString, "”", "\"");
            tempString = CharacterReplacer(tempString, "“", "\"");
            //tempString = CharacterReplacer(tempString, "’", "'");
            //tempString = CharacterReplacer(tempString, "‘", "'");
            tempString = CharacterReplacer(tempString, "…", "... ");
            tempString = CharacterReplacer(tempString, "...", "... ");
            tempString = CharacterReplacer(tempString, "))", " )) ");
            tempString = CharacterReplacer(tempString, "_", string.Empty);
            tempString = CharacterReplacer(tempString, ".\"", ". \"");
            tempString = CharacterReplacer(tempString, "<br /> <br /> <br />", "<br /> <br />");
            tempString = CharacterReplacer(tempString, "\r\n\r\n", "<br />");

            Regex sPeriodSpace = new Regex(@"[a-zA-Z0-9À-ž][\.\,\!][a-zA-Z0-9À-ž]|[\.\,\!][\'\""][a-zA-Z0-9À-ž]");
            Match match = sPeriodSpace.Match(tempString);
            if (match.Success)
            {
                string firstHalf = tempString.Substring(0, match.Index + 2);
                string secondHalf = tempString.Substring(match.Index + 2, tempString.Length -  (match.Index + 2));
                tempString = firstHalf + "  " + secondHalf;
            }
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

        private static readonly Regex sWhitespace = new Regex(@"\s+");
        public static string RemoveWhitespace(string input)
        {
            return sWhitespace.Replace(input, string.Empty);
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

        internal static string ConvertIntColorToHexColor(System.Windows.Media.Color thisColor)
        {
            string redValue = thisColor.R.ToString("X").PadLeft(2, '0');
            string greenValue = thisColor.G.ToString("X").PadLeft(2, '0');
            string blueValue = thisColor.B.ToString("X").PadLeft(2, '0');

            return ("#" + redValue + greenValue + blueValue);
        }
    }
}
