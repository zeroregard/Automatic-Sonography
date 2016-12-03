using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using Util;

namespace UR10TCPController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private double position_step = 0.001d;
        private double rotation_step = 0.001d;
        private bool doUpdatePoseTextBoxes = true;
        public MainWindow()
        {
            InitializeComponent();
            Data.OnCurrentURPoseReceived += new Data.CurrentURPoseReceived(UpdatePoseTextBoxes);
            Data.OnCurrentConfigurationReceived += new Data.CurrentConfigurationReceived(UpdateConfigurationControls);
            var data = Data.Instance;

        }

        private void UpdatePoseTextBoxes(URPose pose)
        {
            Dispatcher.Invoke(delegate
            {
                if (doUpdatePoseTextBoxes)
                {
                    textbox_pos_x.Text = pose.Xpose.ToString("N3", CultureInfo.InvariantCulture);
                    textbox_pos_y.Text = pose.Ypose.ToString("N3", CultureInfo.InvariantCulture);
                    textbox_pos_z.Text = pose.Zpose.ToString("N3", CultureInfo.InvariantCulture);

                    textbox_rot_x.Text = pose.RXpose.ToString("N3", CultureInfo.InvariantCulture);
                    textbox_rot_y.Text = pose.RYpose.ToString("N3", CultureInfo.InvariantCulture);
                    textbox_rot_z.Text = pose.RZpose.ToString("N3", CultureInfo.InvariantCulture);
                }
            });
        }

        private void UpdateConfigurationControls(ConfigurationData config)
        {
            Dispatcher.Invoke(delegate
            {
                txtBoxSpeed.Text = config.Speed.ToString();
                txtBoxAcceleration.Text = config.Acceleration.ToString();
            });
        }

        #region ClickMethodsPosition

        private void btn_pos_x_minus_Click(object sender, RoutedEventArgs e)
        {
            Logic.Instance.ChangePositionRotation(-position_step, 0, 0, 0, 0, 0);
        }

        private void btn_pos_x_plus_Click(object sender, RoutedEventArgs e)
        {
            Logic.Instance.ChangePositionRotation(position_step, 0, 0, 0, 0, 0);
        }

        private void btn_pos_y_minus_Click(object sender, RoutedEventArgs e)
        {
            Logic.Instance.ChangePositionRotation(0, -position_step, 0, 0, 0, 0);
        }

        private void btn_pos_y_plus_Click(object sender, RoutedEventArgs e)
        {
            Logic.Instance.ChangePositionRotation(0, position_step, 0, 0, 0, 0);
        }

        private void btn_pos_z_minus_Click(object sender, RoutedEventArgs e)
        {
            Logic.Instance.ChangePositionRotation(0, 0, -position_step, 0, 0, 0);
        }

        private void btn_pos_z_plus_Click(object sender, RoutedEventArgs e)
        {
            Logic.Instance.ChangePositionRotation(0, 0, position_step, 0, 0, 0);
        }

        #endregion

        #region ClickMethodsRotation

        private void btn_rot_x_minus_Click(object sender, RoutedEventArgs e)
        {
            Logic.Instance.ChangePositionRotation(0, 0, 0, -rotation_step, 0, 0);
        }

        private void btn_rot_x_plus_Click(object sender, RoutedEventArgs e)
        {
            Logic.Instance.ChangePositionRotation(0, 0, 0, rotation_step, 0, 0);
        }

        private void btn_rot_y_minus_Click(object sender, RoutedEventArgs e)
        {
            Logic.Instance.ChangePositionRotation(0, 0, 0, 0, -rotation_step, 0);
        }

        private void btn_rot_y_plus_Click(object sender, RoutedEventArgs e)
        {
            Logic.Instance.ChangePositionRotation(0, 0, 0, 0, rotation_step, 0);
        }

        private void btn_rot_z_minus_Click(object sender, RoutedEventArgs e)
        {
            Logic.Instance.ChangePositionRotation(0, 0, 0, 0, 0, -rotation_step);
        }

        private void btn_rot_z_plus_Click(object sender, RoutedEventArgs e)
        {
            Logic.Instance.ChangePositionRotation(0, 0, 0, 0, 0, rotation_step);
        }


        #endregion

        private void comboBox_position_step_DropDownClosed(object sender, EventArgs e)
        {
            string comboboxstring = comboBox_position_step.Text;
            if (!comboboxstring.Equals(""))
            {
                try
                {
                    position_step = double.Parse(comboboxstring, CultureInfo.InvariantCulture);
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception.Message);
                }
            }
        }

        private void comboBox_rotation_step_DropDownClosed(object sender, EventArgs e)
        {
            string comboboxstring = comboBox_rotation_step.Text;
            if (!comboboxstring.Equals(""))
            {
                try
                {
                    rotation_step = double.Parse(comboboxstring, CultureInfo.InvariantCulture);
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception.Message);
                }
            }
        }

        private void btnSendConfiguration_Click(object sender, RoutedEventArgs e)
        {
            ConfigurationData d = new ConfigurationData();
            d.Speed = double.Parse(txtBoxSpeed.Text, CultureInfo.InvariantCulture);
            d.Acceleration = double.Parse(txtBoxAcceleration.Text, CultureInfo.InvariantCulture);
            Data.Instance.SendConfiguration(d);
        }

        private void btnFetchConfiguration_Click(object sender, RoutedEventArgs e)
        {
            Data.Instance.FetchCurrentConfiguration();
        }

        private void btnSendTwoPoints_Click(object sender, RoutedEventArgs e)
        {
            Logic.Instance.SendPath();
        }

        private void btnSendPose_Click(object sender, RoutedEventArgs e)
        {
            double pos_x = double.Parse(textbox_pos_x.Text)/1000d;
            double pos_y = double.Parse(textbox_pos_y.Text)/1000d;
            double pos_z = double.Parse(textbox_pos_z.Text)/1000d;

            double rot_x = double.Parse(textbox_rot_x.Text)/1000d;
            double rot_y = double.Parse(textbox_rot_y.Text)/1000d;
            double rot_z = double.Parse(textbox_rot_z.Text)/1000d;
            URPose p = new URPose(pos_x, pos_y, pos_z, rot_x, rot_y, rot_z);
            Logic.Instance.SendEntirePose(p);
        }

        private void checkBoxUpdateLabels_Checked(object sender, RoutedEventArgs e)
        {
            handleCheckBox(true);
        }

        private void checkBoxUpdateLabels_Unchecked(object sender, RoutedEventArgs e)
        {
            handleCheckBox(false);
        }

        private void handleCheckBox(bool isChecked)
        {
            doUpdatePoseTextBoxes = isChecked;
        }
    }
}
