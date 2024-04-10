using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Microsoft.Win32;

namespace frogger
{
    public partial class MainWindow : Window
    {
        [DllImport("C:/Users/N/source/repos/frogger/x64/Debug/SHA256.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void ToSHA256(string FilePath, StringBuilder buffer);

        private string SelectedFile = "";
        private string SelectedDirectory = "";
        private bool running = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void File_Click(object sender, RoutedEventArgs e)
        {
            if (running == false)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "txt files (*.txt)|*.txt";

                if (ofd.ShowDialog() == true)
                {
                    SelectedFile = ofd.FileName;

                    if (this.InfoLabel.Content == "Please Select File!")
                    {
                        this.InfoLabel.Content = "";
                    }
                }
            }
        }

        private void Folder_Click(object sender, RoutedEventArgs e)
        {
            if (running == false)
            {
                OpenFolderDialog openFolderDialog = new OpenFolderDialog();

                if (openFolderDialog.ShowDialog() == true)
                {
                    SelectedDirectory = openFolderDialog.FolderName;

                    if (InfoLabel.Content == "Please Select Directory!")
                    {
                        this.InfoLabel.Content = "";
                    }
                }
            }
        }

        private void GenerateTreeViewItem(string txt, string hash, string status)
        {

            TreeViewItem treeViewItem = new TreeViewItem();
            treeViewItem.Header = txt;
            treeViewItem.FontFamily = new FontFamily("Cascadia Mono");
            treeViewItem.FontSize = 16;
            treeViewItem.Foreground = Brushes.White;
            treeViewItem.Background = new SolidColorBrush(Color.FromArgb(255, 10, 32, 47));
            treeViewItem.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 10, 32, 47));
            treeViewItem.IsExpanded = true;

            TextBox textBox = new TextBox();
            textBox.Text = "SHA256: " + hash;
            textBox.IsReadOnly = true;
            textBox.Background = new SolidColorBrush(Color.FromArgb(255, 10, 32, 47));
            textBox.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 10, 32, 47));
            textBox.Foreground = Brushes.White;

            TextBox textBox1 = new TextBox();
            textBox1.Text = "Status:    " + status;
            
            textBox1.IsReadOnly = true;
            textBox1.Background = new SolidColorBrush(Color.FromArgb(255, 10, 32, 47));
            textBox1.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 10, 32, 47));
            textBox1.Foreground = Brushes.White;


            this.treevieww.Items.Add(treeViewItem);
            treeViewItem.Items.Add(textBox);
            treeViewItem.Items.Add(textBox1);
        }

        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedDirectory != "")
            {
                treevieww.Items.Clear();

                List<string> stringList = new List<string>();
                
                DirectoryInfo Directory = new DirectoryInfo(SelectedDirectory);

                foreach(var file in Directory.GetFiles())
                {
                    string fileName = file.Name;
                    StringBuilder sb = new StringBuilder(64);
                    ToSHA256(SelectedDirectory + @"\" + fileName, sb);

                    stringList.Add(fileName + ":" + sb.ToString());
                    GenerateTreeViewItem(SelectedDirectory + @"\" + fileName, sb.ToString(), "GENERATED");
                }

                string[] strings = stringList.ToArray();

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "txt files (*.txt)|*.txt";
                saveFileDialog.Title = "Save Hash Values";
                saveFileDialog.ShowDialog();

                if(saveFileDialog.FileName != "")
                {
                    File.WriteAllLines(saveFileDialog.FileName, strings);
                }

                SelectedDirectory = "";
                SelectedFile = saveFileDialog.FileName;
            }
            else
            {
                this.InfoLabel.Content = "Please Select Directory!";
            }
        }

        private void Compare_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedDirectory != "" && SelectedFile != "")
            {
                treevieww.Items.Clear();

                DirectoryInfo Directory = new DirectoryInfo(SelectedDirectory);

                using (var sr = new StreamReader(SelectedFile))
                {

                    string line;
                    while((line = sr.ReadLine()) != null)
                    {
                        string[] strings = line.Split(':');

                        bool mismatch = true;
                        bool found = false;

                        string fileName = "";
                        StringBuilder sb = new StringBuilder(64);

                        foreach (var file in Directory.GetFiles())
                        {
                            fileName = file.Name;
                            if (fileName == strings[0])
                            {
                                found = true;

                                sb = new StringBuilder(64);
                                ToSHA256(SelectedDirectory + @"\" + fileName, sb);

                                if (sb.ToString() == strings[1])
                                {
                                    mismatch = false;
                                    break;
                                }
                            }
                        }

                        if (found)
                        {
                            if (mismatch == false)
                            {
                                GenerateTreeViewItem(SelectedDirectory + @"\" + strings[0], sb.ToString(), "OK");
                            }
                            else
                            {
                                GenerateTreeViewItem(SelectedDirectory + @"\" + strings[0], sb.ToString(), "MISMATCH");
                            }
                        }
                        else
                        {
                            GenerateTreeViewItem(SelectedDirectory + @"\" + strings[0], sb.ToString(), "FILE NOT FOUND");
                        }
                    }
                }

                SelectedDirectory = "";
                SelectedFile = "";
            }
            else if (SelectedDirectory == "" && SelectedFile == "")
            {
                this.InfoLabel.Content = "Please Select File And Directory!";
            }
            else if (SelectedFile == "")
            {
                this.InfoLabel.Content = "Please Select File!";
            }
            else
            {
                this.InfoLabel.Content = "Please Select Directory!";
            }
        }
    }
}