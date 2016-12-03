using System;
using System.Windows;
using Microsoft.Kinect;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KinectConnectionTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor _sensor;
        MultiSourceFrameReader _reader;
        public MainWindow()
        {
            InitializeComponent();
            InitSensor();
            InitReader();
        }

        /// <summary>
        /// Initialize sensor, logs if sensor's missing
        /// </summary>
        private void InitSensor()
        {
            _sensor = KinectSensor.GetDefault();
            if (_sensor == null)
            {
                Debug.WriteLine("Sensor not found");
            }
            else
            {
                _sensor.Open();
            }
        }

        /// <summary>
        /// Initialize reader, show which FrameSourceTypes we want to view. Hook up the Reader function so that it gets called every frame.
        /// </summary>
        private void InitReader()
        {
            _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Depth);
            _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
        }


        /// <summary>
        /// Open the color, depth and infrared frames in a single method
        /// </summary>
        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            // Get a reference to the multi-frame
            var reference = e.FrameReference.AcquireFrame();
            using (var frame = reference.DepthFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    image.Source = Extensions.ToBitmap(frame);
                }
            }

        }

    }
}
