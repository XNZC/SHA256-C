using System.Diagnostics;
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

namespace frogger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("C:/Users/N/source/repos/frogger/x64/Debug/SHA256.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void ToSHA256(string FilePath, StringBuilder buffer);

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
            this.ProgressBar.Value += 50;
            
            StringBuilder sb = new StringBuilder(64);
            ToSHA256("C:/Users/N/source/repos/test/files/pic1.jpg", sb);
            this.Label00.Content = sb.ToString();
            Debug.WriteLine(sb.ToString());

            ToSHA256("C:/Users/N/source/repos/test/files/pic2.jpg", sb);
            this.Label01.Content = sb.ToString();
            Debug.WriteLine(sb.ToString());
        }
    }
}