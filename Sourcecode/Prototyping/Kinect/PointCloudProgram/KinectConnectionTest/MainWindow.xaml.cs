using System;
using System.Windows;
using Microsoft.Kinect;
using Microsoft.Kinect.Fusion;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Threading.Tasks;

namespace KinectConnectionTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool CaptureCurrentImage = false;
        KinectFusionizer fusionizer;
        //KinectFusionHelper helper;
        public MainWindow()
        {
            InitializeComponent();
            fusionizer = new KinectFusionizer();
            //helper = new KinectFusionHelper();
        }


        private void btnCapture_Click(object sender, RoutedEventArgs e)
        {
            fusionizer.CaptureCurrent = true;
        }
    }
}
