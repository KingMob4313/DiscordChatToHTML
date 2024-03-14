using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MibbitChatToHTML
{
    internal class InCharacterPost
    {
        private string userColor;
        private string headerColor;

        public string CharacterName { get; set; }
        public string PostContent { get; set; }
        public DateTimeOffset PostDate { get; set; }
        public ulong PostId { get; set; }
        public string UserColor { get => userColor; set => userColor = value; }

        public string HeaderColor { get => headerColor; set => headerColor = value; }

        public FontInfo CharacterFontInfo { get; set; }
        public string DiscordUserName { get; set; }

        public InCharacterPost()
        {
            string xmlFontFamily = string.Empty;
            string xmlLetterSpacing = string.Empty;
            string xmlFontSize = string.Empty;

            CharacterFontInfo = new FontInfo(xmlFontFamily, xmlLetterSpacing, xmlFontSize);
        }

        public InCharacterPost(string postLineText, DataTable xmlNameTable)
        {
            string xmlFontFamily = string.Empty;
            string xmlLetterSpacing = string.Empty;
            string xmlFontSize = string.Empty;

            CharacterFontInfo = new FontInfo(xmlFontFamily, xmlLetterSpacing, xmlFontSize);
        }

        public InCharacterPost GetInCharacterPost(string aliasName, string postText, DataTable xmlNameTable)
        {
            InCharacterPost inCharacterPost = new InCharacterPost();

            string nameInPost = GetNameFromPostLine(aliasName);

            List<string> nameList = xmlNameTable.AsEnumerable().Select(x => x[0].ToString()).ToList();
            int nameIndex = nameList.FindIndex(s => nameInPost.Contains(s));

            inCharacterPost.CharacterName = xmlNameTable.Rows[nameIndex][1].ToString();
            inCharacterPost.UserColor = xmlNameTable.Rows[nameIndex][2].ToString();
            inCharacterPost.CharacterFontInfo.FontFamily = xmlNameTable.Rows[nameIndex][3].ToString();
            inCharacterPost.CharacterFontInfo.FontSize = xmlNameTable.Rows[nameIndex][4].ToString();
            inCharacterPost.CharacterFontInfo.LetterSpacing = xmlNameTable.Rows[nameIndex][5].ToString();

            inCharacterPost.headerColor = SetHeaderColor(xmlNameTable.Rows[nameIndex][2].ToString());

            inCharacterPost.PostContent = GetPostContentFromPostLine(postText);

            return inCharacterPost;
        }

        //This gets the character name from the alias name from the input - DiscordName or whatever
        private string GetNameFromPostLine(string aliasNameText)
        {
            string characterName = aliasNameText;

            return characterName;
        }

        private string GetPostContentFromPostLine(string postText)
        {
            string characterPost = postText;

            return characterPost;
        }

        private string SetHeaderColor(string characterPostColor)
        {
            string thisColor = "#000000";
            Color inputColor = new Color();

            try
            {
                //The idea is that I want to keep the most essential color while darkening the rest
                int redValue = Convert.ToInt32(("0x0000" + characterPostColor.Substring(1, 2) + ""), 16);
                int greenValue = Convert.ToInt32(("0x0000" + characterPostColor.Substring(3, 2) + ""), 16);
                int blueValue = Convert.ToInt32(("0x0000" + characterPostColor.Substring(5, 2) + ""), 16);

                int maxRedValue = Math.Max(redValue, Math.Max(greenValue, blueValue));
                int maxGreenValue = Math.Max(greenValue, Math.Max(blueValue, redValue));
                int maxBlueValue = Math.Max(blueValue, Math.Max(redValue, greenValue));

                redValue = redValue == maxRedValue ? (maxRedValue * 2) / 3 : redValue / 3;
                greenValue = greenValue == maxGreenValue ? (maxGreenValue * 2) / 3 : greenValue / 3;
                blueValue = blueValue == maxBlueValue ? (maxBlueValue * 2) / 3 : blueValue / 3;

                inputColor = Color.FromRgb(Convert.ToByte(redValue), Convert.ToByte(greenValue), Convert.ToByte(blueValue));
                return ("#" + inputColor.ToString().Substring(3));
            }
            catch
            {
                return thisColor;
            }
        }



        internal class FontInfo
        {
            private string fontFamily;
            private string letterSpacing;
            private string fontSize;
            //todo: get from json
            private static string fontSeperatorCharacter = ("⌀");
            private static string letterSpacingSeperatorCharacter = ("§");
            private static string spaceReplacementCharacter = ("_");
            //public string FontFamily { get => GetFontFamily(fontFamily); set => fontFamily = GetFontFamily(value); }
            //public string LetterSpacing { get => GetLetterSpacing(letterSpacing); set => letterSpacing = GetLetterSpacing(value); }


            public string FontFamily { get => fontFamily; set => fontFamily = value; }
            public string LetterSpacing { get => letterSpacing; set => letterSpacing = value; }
            public string FontSize { get => fontSize; set => fontSize = value; }

            public FontInfo(string xmlFontFamily, string xmlLetterSpacing, string fontSize)
            {
                FontFamily = xmlFontFamily;
                LetterSpacing = xmlLetterSpacing;
                FontSize = fontSize;
            }
        }
    }
}
