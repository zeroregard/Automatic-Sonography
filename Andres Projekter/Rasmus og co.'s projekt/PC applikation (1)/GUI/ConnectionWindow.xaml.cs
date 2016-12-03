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
using LOGIC;
using System.Threading;

namespace GUI
{
    /// <summary>
    /// <para>Interaction logic for ConnectionWindow.xaml</para>
    /// A window that allows the user to fill in informations about the scanning unit, and checks whether there is a connection.
    /// Is the connection confirmed the informations can be saved.
    /// </summary>
    public partial class ConnectionWindow : Window
    {
        Unit newUnit, testUnit;
        ProgressDialog progDialog;
        Thread testThread;
        TestConnection testConn;
        InfoAndErrorWindow infoDlg;

        /// <summary>Create instance of the ConnectionWindow.</summary>
        public ConnectionWindow()
        {
            InitializeComponent();
            lb_testResult.Visibility = System.Windows.Visibility.Hidden;
        }

        /// <summary>Click event for saving connection.</summary>
        private void btn_saveConnection_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        /// <summary>Get the new unit that needs to be saved.</summary>
        public Unit NewUnit
        {
            get { return newUnit; }
        }

        /// <summary>Click event for testing connection. Creates test unit based on the information typed in the window, and initalizes the test.</summary>
        private void btn_testConnection_Click(object sender, RoutedEventArgs e)
        {
            lb_testResult.Visibility = System.Windows.Visibility.Hidden;
            bool textBoxEmpty = String.IsNullOrWhiteSpace(tb_Id.Text) || String.IsNullOrWhiteSpace(tb_location.Text) || String.IsNullOrWhiteSpace(tb_urIpAddress.Text)
                || String.IsNullOrWhiteSpace(tb_rdIpAddress.Text) || String.IsNullOrWhiteSpace(tb_vfIpAddress.Text) || String.IsNullOrWhiteSpace(tb_projectName.Text)
                || String.IsNullOrWhiteSpace(tb_majorVersion.Text) || String.IsNullOrWhiteSpace(tb_minorVersion.Text) || String.IsNullOrWhiteSpace(tb_svnRevision.Text)
                || String.IsNullOrWhiteSpace(tb_buildDate.Text);                                                            // Checks that all fields have been filled in
            if (!textBoxEmpty)
            {
                progDialog = new ProgressDialog("Tester forbindelse...");
                progDialog.Owner = this;
                this.IsEnabled = false;
                progDialog.Show();                                                                                          // Shows the user that a progress is running

                testUnit = new Unit();                                                                                      // Creates unit with the filled in informations
                testUnit.id = tb_Id.Text;
                testUnit.location = tb_location.Text;
                testUnit.urIpAddress = tb_urIpAddress.Text;
                testUnit.rdIpAddress = tb_rdIpAddress.Text;
                testUnit.vfIpAddress = tb_vfIpAddress.Text;
                testUnit.projectName = tb_projectName.Text;
                testUnit.majorversion = Convert.ToInt32(tb_majorVersion.Text);
                testUnit.minorversion = Convert.ToInt32(tb_minorVersion.Text);
                testUnit.svnRevision = Convert.ToInt32(tb_svnRevision.Text);
                testUnit.buildDate = tb_buildDate.Text;

                Logic.OnException -= new Logic.LogicExceptionEventHandler(ShowException);                                   // Create listener for connection exception an test completion
                Logic.OnException += new Logic.LogicExceptionEventHandler(ShowException);
                TestConnection.OnTestCompleted -= new TestConnection.TestCompletedEventHandler(TestCompleted);
                TestConnection.OnTestCompleted += new TestConnection.TestCompletedEventHandler(TestCompleted);

                testThread = new Thread(() =>
                {
                    testConn = new TestConnection();
                    testConn.Test(testUnit);                                                                                // Initialize the test of the new unit
                });
                testThread.IsBackground = true;
                testThread.Name = "testThread";
                testThread.Start();
            }
            else
            {
                lb_testResult.Content = "Udfyld venligst alle felter";                                                      // Message to user if all fields have not been filled in
                lb_testResult.Foreground = Brushes.Red;
                lb_testResult.Visibility = System.Windows.Visibility.Visible;
            }
        }

        /// <summary>Opens a new window to show the error message to the user.</summary>
        private void ShowException(string error, string source)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                infoDlg = new InfoAndErrorWindow("Fejlmeddelelse", error, "OK");
                infoDlg.OnDialogFinished -= infoDlg_DialogFinished;
                infoDlg.OnDialogFinished += infoDlg_DialogFinished;
                infoDlg.Owner = this;
                infoDlg.ShowDialog();
            });
        }

        /// <summary>DialogFinished event that closes the window.</summary>
        private void infoDlg_DialogFinished(object sender, string e)
        {
            InfoAndErrorWindow dlg = sender as InfoAndErrorWindow;
            dlg.Close();
        }

        /// <summary>Handles the test result.</summary>
        private void TestCompleted(bool result)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                progDialog.Close();

                if (result)
                {
                    newUnit = testUnit;
                    btn_saveConnection.IsEnabled = true;
                    lb_testResult.Content = "Forbindelsen er GODKENDT";
                    lb_testResult.Foreground = Brushes.Green;
                }
                else
                {
                    btn_saveConnection.IsEnabled = false;
                    lb_testResult.Content = "Forbindelsen kunne IKKE godkendes";
                    lb_testResult.Foreground = Brushes.Red;
                }

                lb_testResult.Visibility = System.Windows.Visibility.Visible;
                this.IsEnabled = true;
            });
        }
    }
}
