using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace FullRotationProgramWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Util.Misc m;
        //Thread socketThread, geoThread;
        Util.URPose URpose;
        Logic logik;

        public MainWindow()
        {
            InitializeComponent();
            logik = new Logic();
            m = new Util.Misc();

            Util.Misc.OnActualPose += new Util.Misc.ActualURPoseEventHandler(updateActualURPoseLabels);
            Util.Misc.OnActualJoint += new Util.Misc.ActualURJointEventHandler(updateActualURJointLabels);
            GeoMagicTouch.OnGeoPose += new GeoMagicTouch.GeoMagicPoseEventHandler(updateGMposeLabels);
        }

        private void updateActualURJointLabels(Util.URPose aPose)
        {
            Dispatcher.Invoke((Action)delegate
            {
                lbx.Content = Math.Round(aPose.Xpose, 3);
                lby.Content = Math.Round(aPose.Ypose, 3);
                lbz.Content = Math.Round(aPose.Zpose, 3);
                lbRx.Content = Math.Round(aPose.RXpose, 3);
                lbRy.Content = Math.Round(aPose.RYpose, 3);
                lbRz.Content = Math.Round(aPose.RZpose, 3);
            });
        }

        private void btnSendPose_Click(object sender, RoutedEventArgs e)
        {
        }

        private void updateActualURPoseLabels(Util.URPose a)
        {
            //URpose = a;
            //Dispatcher.Invoke((Action)delegate
            //{
            //    lbx.Content = Math.Round(a.Xpose, 3);
            //    lby.Content = Math.Round(a.Ypose, 3);
            //    lbz.Content = Math.Round(a.Zpose, 3);
            //    lbRx.Content = Math.Round(a.RXpose, 3);
            //    lbRy.Content = Math.Round(a.RYpose, 3);
            //    lbRz.Content = Math.Round(a.RZpose, 3);
            //});
        }

        private void updateGMposeLabels(Util.GeoMagicPose gmPose)
        {
            //Dispatcher.Invoke((Action)delegate
            Dispatcher.BeginInvoke((Action)delegate
            {
                lbxGMPose.Content = Math.Round(gmPose.Xpose, 3);
                lbyGMPose.Content = Math.Round(gmPose.Ypose, 3);
                lbzGMPose.Content = Math.Round(gmPose.Zpose, 3);
                lbRxGMPose.Content = Math.Round(gmPose.RXpose, 3);
                lbRyGMPose.Content = Math.Round(gmPose.RYpose, 3);
                lbRzGMPose.Content = Math.Round(gmPose.RZpose, 3);
                //moveUR(gmPose);
            });
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
        }
    }
}
