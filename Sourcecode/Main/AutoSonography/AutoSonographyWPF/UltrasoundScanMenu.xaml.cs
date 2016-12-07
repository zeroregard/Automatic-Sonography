using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using RoboLibrary;
using DataStructures;
namespace AutoSonographyWPF
{
    /// <summary>
    /// Interaction logic for UltrasoundScanMenu.xaml
    /// </summary>
    public partial class UltrasoundScanMenu : Window
    {
        RoboMaster master;
        MainWindow mainWindowInstance;
        private bool isClosed;

        bool paused = false;
        public UltrasoundScanMenu()
        {
            InitializeComponent();
            RoboMaster.PathCompletionEvent += UpdateCompletion;
        }

        public void UpdateCompletion(object sender, EventArgs e)
        {
            if (!isClosed)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (textBoxCompletionValue != null && master != null)
                        textBoxCompletionValue.Text = master.CompletionPercentage.ToString("n0");
                });
            }
        }

        public void Initialize(RoboMaster robomaster, List<URPose> path, MainWindow mainWindow)
        {
            isClosed = false;
            master = robomaster;
            master.ProcessPath(path);
            mainWindowInstance = mainWindow;
        }

        private Color PauseColor = Color.FromArgb(255, 255, 220, 128);
        private Color ResumeColor = Color.FromArgb(255, 140, 255, 128);
        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            master.PauseScanning();
            if (paused)
            {
                paused = false;
                btnPause.Content = "Pause";
                btnPause.Background = new SolidColorBrush(PauseColor);
            }
            else
            {
                paused = true;
                btnPause.Content = "Resume";
                btnPause.Background = new SolidColorBrush(ResumeColor);
            }
        }


        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            master.StopScanning();
            isClosed = true;
            mainWindowInstance.Show();
        }
    }
}
