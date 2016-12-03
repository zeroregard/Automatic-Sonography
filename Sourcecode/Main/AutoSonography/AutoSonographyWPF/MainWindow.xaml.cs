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
using System.Windows.Navigation;
using System.Windows.Shapes;
using RoboLibrary;
using ComputerVisionLibrary;
using DataStructures;
using CalculationLibrary;
using Microsoft.Kinect;
namespace AutoSonographyWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RoboMaster robomaster;
        private CVMesh currentMesh;
        private CalculationMaster calcmaster;
        public MainWindow()
        {
            InitializeComponent();
            ModBus.NoConnectionEvent += NoRobotConnection;
            robomaster = new RoboMaster();
            robomaster.SetStandardPose();
            calcmaster = new CalculationMaster();
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            robomaster?.SetStandardPose();
            Application.Current.Shutdown();
        }


        private void btn3DScan_Click(object sender, RoutedEventArgs e)
        {
            _3DScanMenu menu = new _3DScanMenu();
            menu.Initialize(new ComputerVisionMaster(), this);
            menu.Show();
            this.Hide();
        }

        public void MeshScanComplete(CVMesh mesh)
        {
            this.Show();
            currentMesh = mesh;
            btnUltraSoundScan.IsEnabled = true;
            btnConvertAndSave.IsEnabled = true;
        }

        private void btnUltraSoundScan_Click(object sender, RoutedEventArgs e)
        {
            List<URPose> path = calcmaster.FindPath(currentMesh);
            if (robomaster == null)
                return;
            UltrasoundScanMenu menu = new UltrasoundScanMenu();
            menu.Initialize(robomaster, path, this);
            menu.Show();
            this.Hide();
        }

        private void btnConvertAndSave_Click(object sender, RoutedEventArgs e)
        {
            string location = Environment.CurrentDirectory;
            //PLYHandler.Output(calcmaster.calibrator.ConvertToRobospace(currentMesh).Vertices, location);
            PLYHandler.Output(/*calcmaster.calibrator.ConvertToRobospace(*/currentMesh/*)*/, ref location, false);
        }

        public void NoRobotConnection(object sender, EventArgs e)
        {
            MessageBox.Show(ModBus.NoConnectionError, "UR10 isn't connected", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public void NoKinectConnection()
        {
            
        }
    }
}
