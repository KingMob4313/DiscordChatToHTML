using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;

namespace MibbitChatToHTML
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Tuple<int, string>> annotatedChatLines = null;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            annotatedChatLines = new List<Tuple<int, string>>();
            List<string> justChatLines = new List<string>();

            OpenFileDialog OFD = new OpenFileDialog();
            OFD.Filter = "Text|chat*.txt|All|*.*";
            OFD.FileName = "chat";
            bool? result = OFD.ShowDialog();

            if (result == true)
            {
                var currentFileName = OFD.FileName;
                FileNameTextBox.Text = currentFileName;

                justChatLines = ChatFile.ProcessChatFile(OFD.FileName, this);

            }
            string allChat = StreamOutLines(justChatLines);
            ChatTextBox.Text = allChat;
        }

        private string StreamOutLines(List<string> justChatLines)
        {
            string chat = string.Empty;
            foreach (string line in justChatLines)
            {
                chat = chat + line;
            }
            return chat;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
