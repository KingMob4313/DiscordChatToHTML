using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;

namespace MibbitChatToHTML
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static public string lineFromDialog = string.Empty;
        List<Tuple<int, string>> annotatedChatLines = null;
        public MainWindow()
        {
            InitializeComponent();
            LoadInfo();

        }

        private void LoadInfo()
        {
            List<string> CurrentTextFileTypes = new List<string>();
            CurrentTextFileTypes.Add("Mibbit IRC");
            CurrentTextFileTypes.Add("Discord");
            TextFileTypeComboBox.ItemsSource = CurrentTextFileTypes;
            TextFileTypeComboBox.SelectedIndex = 1; //Default to Discord
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            annotatedChatLines = new List<Tuple<int, string>>();
            List<string> justChatLines = new List<string>();

            OpenFileDialog OFD = new OpenFileDialog();
            OFD.Filter = "Text|*.txt|All|*.*";
            OFD.FileName = "chat";
            bool? result = OFD.ShowDialog();

            if (result == true)
            {
                var currentFileName = OFD.FileName;
                FileNameTextBox.Text = currentFileName;
                justChatLines = ChatFile.ProcessChatFile(OFD.FileName, TextFileTypeComboBox.SelectedIndex, this);
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
            Clipboard.Clear();
            Clipboard.SetData(DataFormats.UnicodeText, ChatTextBox.Text);
            MessageBox.Show("Data Copied.");
        }

        private void CBCleanedCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UnformattedCheckBox.IsChecked = false;
        }

        private void UnformattedCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CBCleanedCheckBox.IsChecked = false;
        }

        private void NameControlButton_Click(object sender, RoutedEventArgs e)
        {
            NameControl dialog = new NameControl();
            DataSet dataSet = new DataSet();
            DataTable XMLDataTable = new DataTable();
            dataSet.ReadXml("NamesAliasFile.xml");
            XMLDataTable = dataSet.Tables[0];

            dialog.NameDataGrid.ItemsSource = XMLDataTable.AsDataView();
            dialog.NameDataGrid.AutoGenerateColumns = true;
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                try
                {
                    XMLDataTable.WriteXml("NamesAliasFile.xml");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            //a.Any(a => b.Contains(a))
        }
    }
}
