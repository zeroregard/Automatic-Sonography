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
using Util;
namespace FullRotationProgramWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Util.Misc m;
        //Thread socketThread, geoThread;
        URPose URpose;
        Logic logik;

        public MainWindow()
        {
            InitializeComponent();
            logik = new Logic();
            //m = new Util.Misc();

            AnalyzeURData.OnActualURPose += new AnalyzeURData.ActualURPoseEventHandler(updateActualURPoseLabels);
        }

        private void updateActualURJointLabels(URPose aPose)
        {
            Dispatcher.Invoke(delegate
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
            double x = double.Parse(tbx.Text);
            double y = double.Parse(tby.Text);
            double z = double.Parse(tbz.Text);

            double rx = double.Parse(tbRx.Text);
            double ry = double.Parse(tbRy.Text);
            double rz = double.Parse(tbRz.Text);

            logik.testURPose(x, y, z, rx, ry, rz);
        }

        private void updateActualURPoseLabels(URPose a)
        {
            URpose = a;
            Dispatcher.Invoke(delegate
            {
                lbx.Content = Math.Round(a.Xpose, 3);
                lby.Content = Math.Round(a.Ypose, 3);
                lbz.Content = Math.Round(a.Zpose, 3);
                lbRx.Content = Math.Round(a.RXpose, 3);
                lbRy.Content = Math.Round(a.RYpose, 3);
                lbRz.Content = Math.Round(a.RZpose, 3);
            });
        }
    }
}
