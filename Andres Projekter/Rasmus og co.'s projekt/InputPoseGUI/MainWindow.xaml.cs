using System;
using System.Collections.Generic;
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

namespace InputPoseGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DATA.SendCommandsToUR command;
        Util.Misc m;
        Point currentPos;
        Thread socketThread, forceThread;
        bool moving;
        public MainWindow()
        {
            InitializeComponent();
            command = new DATA.SendCommandsToUR();
            m = new Util.Misc();

            socketThread = new Thread(new ThreadStart(DATA.SocketListener.RunSocketListener));
            socketThread.Start();
            forceThread = new Thread(new ThreadStart(DATA.ModbusComm.ReadInpRegDTO));
            forceThread.Start();

            Util.Misc.OnActualPose += new Util.Misc.ActualURPoseEventHandler(updateActualURPoseLabels);
            Util.Misc.OnActualForce += new Util.Misc.ActualForceEventHandler(updateActualForceLabels);
            DATA.TCP_server.gmPoseEvent += new DATA.TCP_server.GMposeEvent(updateGMposeLabels);

            MouseMove += new MouseEventHandler(MainWindow_MouseMove);
            MouseDown += new MouseButtonEventHandler(MainWindow_MouseDown);
            MouseUp += new MouseButtonEventHandler(MainWindow_MouseUp);
        }

        private void MainWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            moving = false;
            ReleaseMouseCapture();
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CaptureMouse();
            moving = true;
        }

        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            currentPos = Mouse.GetPosition(canvas);
            xPosTB.Text = Convert.ToString(currentPos.X);
            yPosTB.Text = Convert.ToString(currentPos.Y);

            if (moving)
            {
                if (currentPos.X <= 600 && currentPos.X >= 0 && currentPos.Y <= 400 && currentPos.Y >= 0)
                {
                    string x = Convert.ToString(0.2 + (currentPos.X / 1000));
                    //x = x.Replace(",", ".");
                    string y = "-" + Convert.ToString(0.2 + (currentPos.Y / 1000));
                    //y = y.Replace(",", ".");

                    //command.SendPose(x, y, "0.4", "0", "3.14", "0");
                    command.sendPoseModbus(x, "-0,4", "0,4", "0", "3,14", "0");
                    //Thread.Sleep(500);
                }
            }
        }

        private void btnSendPose_Click(object sender, RoutedEventArgs e)
        {
            //command.SendPose(tbx.Text, tby.Text, tbz.Text, tbRx.Text, tbRy.Text, tbRz.Text);
            command.sendPoseModbus(tbx.Text, tby.Text, tbz.Text, tbRx.Text, tbRy.Text, tbRz.Text);
        }

        private void updateActualURPoseLabels(Util.URPose a)
        {
            Dispatcher.Invoke((Action)delegate
            {
                lbx.Content = Math.Round(a.Xpose, 3);
                lby.Content = Math.Round(a.Ypose, 3);
                lbz.Content = Math.Round(a.Zpose, 3);
                lbRx.Content = Math.Round(a.RXpose, 3);
                lbRy.Content = Math.Round(a.RYpose, 3);
                lbRz.Content = Math.Round(a.RZpose, 3);
            });
        }

        private void updateActualForceLabels(Util.ForceData f)
        {
            Dispatcher.Invoke((Action)delegate
            {
                lbxForce.Content = Math.Round(f.Xforce, 3);
                lbyForce.Content = Math.Round(f.Yforce, 3);
                lbzForce.Content = Math.Round(f.Zforce, 3);
                lbRxForce.Content = Math.Round(f.RXforce, 3);
                lbRyForce.Content = Math.Round(f.RYforce, 3);
                lbRzForce.Content = Math.Round(f.RZforce, 3);
            });
        }

        private void updateGMposeLabels(Util.GeoMagicPose gmPose)
        {
            Dispatcher.Invoke((Action)delegate
            {
                lbxGMPose.Content = Math.Round(gmPose.Xpose, 3);
                lbyGMPose.Content = Math.Round(gmPose.Ypose, 3);
                lbzGMPose.Content = Math.Round(gmPose.Zpose, 3);
                lbRxGMPose.Content = Math.Round(gmPose.RXpose, 3);
                lbRyGMPose.Content = Math.Round(gmPose.RYpose, 3);
                lbRzGMPose.Content = Math.Round(gmPose.RZpose, 3);
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            command.ShutDownUR();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            command.SendPopUp(tbMessage.Text, "Popup");
        }
    }
}
