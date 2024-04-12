using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Policy;
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
using static System.Net.WebRequestMethods;

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

        private void ChangeProgress(float val)
        {
            ProgressBar.Value = val;
        }

        private async void GenerateSHA256Hashes(List<string> files, List<string> fileNames, bool generate)
        {

            if (generate == true)
            {
                StreamWriter outputfile = new StreamWriter(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "SHA256_Values.txt"));

                float totalFiles = files.Count;
                float currentFile = 0;

                for (int i = 0; i < files.Count; i++)
                {
                    StringBuilder sb = new StringBuilder(64);

                    await Task.Run(() => ToSHA256(files[i], sb));

                    GenerateTreeViewItem(files[i], sb.ToString(), "GENERATED");
                    outputfile.WriteLine(fileNames[i] + ":" + sb.ToString());

                    currentFile++;
                    this.InfoLabel.Content = "Running! " + "(" + currentFile + "/" + totalFiles + ")";

                    ChangeProgress((currentFile / totalFiles) * 100);
                }

                outputfile.Close();
                this.InfoLabel.Content = "Saved to: '" + System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "SHA256_Values.txt") + "'";
                ChangeProgress(0);
            }
            else
            {
                DirectoryInfo Directory = new DirectoryInfo(SelectedDirectory);
                for (int i = 0; i < files.Count; i++)
                {
                    bool mismatch = true;
                    bool found = false;

                    float totalFiles = files.Count;
                    float currentFile = i+1;

                    StringBuilder sb = new StringBuilder(64);
                    foreach (var file in Directory.GetFiles())
                    {

                        if (file.Name == files[i])
                        {
                            found = true;
                            await Task.Run(() => ToSHA256(SelectedDirectory + @"\" + files[i], sb));

                            if (sb.ToString() == fileNames[i]) // hashes
                            {
                                mismatch = false;
                                break;
                            }
                        }
                    }

                    if (found == true)
                    {
                        if (mismatch == false)
                        {
                            GenerateTreeViewItem(SelectedDirectory + @"\" + files[i], sb.ToString(), "OK");
                        }
                        else
                        {
                            GenerateTreeViewItem(SelectedDirectory + @"\" + files[i], sb.ToString(), "MISMATCH");
                        }
                    }
                    else
                    {
                        GenerateTreeViewItem(SelectedDirectory + @"\" + files[i], sb.ToString(), "FILE NOT FOUND");
                    }

                    this.InfoLabel.Content = "Running! " + "(" + currentFile + "/" + totalFiles + ")";
                    ChangeProgress((currentFile / totalFiles) * 100);
                    
                }

                this.InfoLabel.Content = "Finished";
            }

            SelectedDirectory = "";
            SelectedFile = "";
            running = false;
        }

        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedDirectory != "" && running == false)
            {
                running = true;
                this.InfoLabel.Content = "Running!";
                treevieww.Items.Clear();

                DirectoryInfo Directory = new DirectoryInfo(SelectedDirectory);
                List<string> files = new List<string>();
                List<string> fileNames = new List<string>();

                foreach(var file in Directory.GetFiles())
                {
                    string fileName = file.Name;
                    string fileLocation = (SelectedDirectory + @"\" + fileName);

                    files.Add(fileLocation);
                    fileNames.Add(fileName);
                }

                GenerateSHA256Hashes(files, fileNames, true);
            }
            else if (running == true)
            {
                this.InfoLabel.Content = "Process Is Currently Running!";
            }
            else
            {
                this.InfoLabel.Content = "Please Select Directory!";
            }
        }

        private void Compare_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedDirectory != "" && SelectedFile != "" && running == false)
            {
                running = true;
                treevieww.Items.Clear();

                List<string> files = new List<string>();
                List<string> hashes = new List<string>();

                using (var sr = new StreamReader(SelectedFile))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] strings = line.Split(':');
                        files.Add(strings[0]);
                        hashes.Add(strings[1]);
                    }                    
                }

                GenerateSHA256Hashes(files, hashes, false);
            }
            else if (running == true)
            {
                this.InfoLabel.Content = "Process Is Currently Running!";
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