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
using Declarations;
using Declarations.Media;
using Declarations.Players;
using Implementation;
using LOGIC;
using Util;
using AxRDPCOMAPILib;
using System.Threading;

namespace GUI
{
    /// <summary>
    /// <para>Interaction logic for MainWindow.xaml</para>
    /// MainWindow that present the user to a remote desktop window, a videofeed and options to configure a Universal Robot.
    /// This window also handles different exception from the connections running in the background.
    /// </summary>
    public partial class MainWindow : Window
    {
        // Private declerations
        Logic logic;
        Connection connection;
        AxRDPViewer rmdView;
        InfoAndErrorWindow infoDlg;
        ProgressDialog progDialog;
        IVideoPlayer vPlayer;
        System.Windows.Forms.Panel p;
        bool noReconToGMT = true;
        bool noReconToUR = true;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        // Window handling
        /// <summary>Constructor of the MainWindow. </summary>
        public MainWindow()
        {
            InitializeComponent();
            logic = Logic.GetInstance();
            connection = logic.connection;

            Logic.OnException -= ShowException;
            Logic.OnException += ShowException;

            Connection.OnCurrentConfig -= Connection_OnCurrentConfig;
            Connection.OnCurrentConfig += Connection_OnCurrentConfig;

            Positioning.OnForce += Positioning_OnForce;
        }

        void Positioning_OnForce(ForceData force)
        {
            Dispatcher.BeginInvoke((Action)delegate
                {
                    sld_Force.Value = Convert.ToDouble(force.Zforce * 10);
                });

        }

        /// <summary>Window loaded event.</summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Unit selected = logic.GetSelectedUnit();                                                                    // Gets the selected unit to present informations in the window
            lb_ID.Content = selected.id;
            lb_Lokation.Content = selected.location;

            //ConnectRemoteDesktop();
            //ConnectVideoPlayer();

            connection.CheckConfigurations();                                                                           // Initialize the update of the configurations shown in the window
        }

        /// <summary>Window closing event.</summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //rmdView.Disconnect();                                                                                       // Disconnect to the remote desktop
            connection.CloseConnection();                                                                               // Close all the connections established
            Application.Current.Shutdown();
        }

        /// <summary>DialogFinished event that closes the window.</summary>
        void infoDlg_DialogFinished(object sender, string e)
        {
            infoDlg.Close();
            infoDlg = null;
        }

        /// <summary>Presents the exceptions based on the source of the exception.</summary>
        private void ShowException(string error, string source)
        {
            switch (source)
            {
                case "RMD":
                    ConnectionLostRD(error);
                    break;
                case "GMT":
                    if (noReconToGMT)
                        ConnectionLostGMT(error);
                    break;
                case "UR":
                    if (noReconToUR)
                        ConnectionLostUR(error);
                    break;
                case "MODBUS":
                    if (noReconToUR)
                        ConnectionLostUR(error);
                    break;
                case "URNOTRUNNING":
                    URNotRunning(error);
                    break;
                case "VF":
                    ConnectionLostVF(error);
                    break;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        // Remote desktop connection handler

        /// <summary>Creates the remote desktop viewer and connects to the server.</summary>
        private void ConnectRemoteDesktop()
        {
            rmdView = new AxRDPViewer();                                                                                // Creating instance of the Remote Desktop Viewer
            windowshost_RMT.Child = rmdView;                                                                            // Add the viewer to the windows forms host
            rmdView.OnConnectionTerminated += new _IRDPSessionEvents_OnConnectionTerminatedEventHandler(ConLostRD);     // Listener for connection terminated event from the viewer

            try
            {
                //rmdView.Connect(connection.GetInvitation(), "", "");                                                    // Connects to the remote desktop server based on the invitation received in the connection process
                rmdView.SmartSizing = IsEnabled;                                                                        // Size the remote desktop window to the viewers size
            }
            catch (Exception exc) { }
        }

        /// <summary>Opens a window showing the error.</summary>
        /// <param name="error">Error message.</param>
        private void ConnectionLostRD(string error)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                if (infoDlg == null)
                {
                    infoDlg = new InfoAndErrorWindow("Fejlmeddelelse", error, "Forsøg igen", "Annuller");
                    infoDlg.OnDialogFinished -= infoDlg_DialogFinished;
                    infoDlg.OnDialogFinished += infoDlg_DialogFinished;
                    infoDlg.OnDialogFinished -= handleRMD;
                    infoDlg.OnDialogFinished += handleRMD;
                    infoDlg.Owner = this;
                    infoDlg.ShowDialog();
                }
            });
        }

        /// <summary>Handles the respons from the user.</summary>
        /// <param name="e">Respons message.</param>
        private void handleRMD(object sender, string e)
        {
            if (e == "Forsøg igen")                                                                                     // Initiates a process to try and connect to the remote desktop again
            {
                Connection.OnConnectionRD -= Logic_OnConnectionRD;
                Connection.OnConnectionRD += Logic_OnConnectionRD;

                if (progDialog != null)
                    progDialog.Close();

                progDialog = new ProgressDialog("Forbinder til Fjernskrivebord...");                                    // Progressbar showing a process is running
                progDialog.Owner = this;
                progDialog.Show();

                connection.EstablishConnectionRD();                                                                     // Establish a new connection
            }
            else if (e == "Annuller")
            {

            }

        }

        /// <summary>Connects to the remote desktop server if connection was successful.</summary>
        /// <param name="result">Result of the connection.</param>
        void Logic_OnConnectionRD(bool result)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                progDialog.Close();
                progDialog = null;
                if (result)
                {
                    rmdView.Connect(connection.GetInvitation(), "", "");                                                // Reconnect to the remote desktop based on the new invitation
                }
            });
        }

        /// <summary>Initiates the connection lost process.</summary>
        private void ConLostRD(object sender, _IRDPSessionEvents_OnConnectionTerminatedEvent e)
        {
            ConnectionLostRD("Der er mistet forbindelse til fjernskrivebord.");
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        // GeoMagicTouch connection handler
        /// <summary>Opens a window showing the error.</summary>
        /// <param name="error">Error message.</param>
        private void ConnectionLostGMT(string error)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                if (progDialog != null)
                {
                    if (progDialog.IsActive)
                    {
                        progDialog.Close();                                                                             // If the progressbar is active from a previous attempt it will be closed
                        progDialog = null;
                    }
                }

                if (infoDlg == null)
                {
                    infoDlg = new InfoAndErrorWindow("Fejlmeddelelse", error, "Forsøg igen", "Annuller");
                    infoDlg.OnDialogFinished -= infoDlg_DialogFinished;
                    infoDlg.OnDialogFinished += infoDlg_DialogFinished;
                    infoDlg.OnDialogFinished -= handleGMT;
                    infoDlg.OnDialogFinished += handleGMT;
                    infoDlg.Owner = this;
                    infoDlg.ShowDialog();
                }
            });
        }

        /// <summary>Handles the respons from the user.</summary>
        /// <param name="e">Respons message.</param>
        private void handleGMT(object sender, string e)
        {
            if (e == "Forsøg igen")                                                                                     // Initiates a process to try and connect to the GeoMagicTouch again
            {
                Connection.OnConnectionGMT -= Logic_OnConnectionGMT;
                Connection.OnConnectionGMT += Logic_OnConnectionGMT;

                if (progDialog != null)
                    progDialog.Close();

                progDialog = new ProgressDialog("Forbinder til GMT...");                                                // Progressbar showing a process is running
                progDialog.Owner = this;
                progDialog.Show();

                Thread connThread = new Thread(() =>
                {
                    connection.EstablishConnectionGMT();                                                                // Establish a new connection
                });
                connThread.IsBackground = true;
                connThread.Start();
            }
            else if (e == "Annuller")
            {
                noReconToGMT = false;
            }
        }

        /// <summary>Closes the progressbar when connection result is received.</summary>
        /// <param name="result">Result of the connection.</param>
        void Logic_OnConnectionGMT(bool result)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                if (progDialog != null)
                {
                    if (progDialog.IsActive)
                    {
                        progDialog.Close();
                        progDialog = null;
                    }
                }
            });
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        // Universal Robot connection handler
        /// <summary>Opens a window showing the error.</summary>
        /// <param name="error">Error message.</param>
        private void ConnectionLostUR(string error)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                if (progDialog != null)
                {
                    if (progDialog.IsActive)
                    {
                        progDialog.Close();                                                                             // If the progressbar is active from a previous attempt it will be closed
                        progDialog = null;
                    }
                }

                if (infoDlg == null)
                {
                    infoDlg = new InfoAndErrorWindow("Fejlmeddelelse", error, "Forsøg igen", "Annuller");
                    infoDlg.OnDialogFinished -= infoDlg_DialogFinished;
                    infoDlg.OnDialogFinished += infoDlg_DialogFinished;
                    infoDlg.OnDialogFinished -= handleUR;
                    infoDlg.OnDialogFinished += handleUR;
                    infoDlg.Owner = this;
                    infoDlg.ShowDialog();
                }
            });
        }

        /// <summary>Handles the respons from the user.</summary>
        /// <param name="e">Respons message.</param>
        private void handleUR(object sender, string e)
        {
            if (e == "Forsøg igen")
            {
                Connection.OnConnectionUR -= Logic_OnConnectionUR;
                Connection.OnConnectionUR += Logic_OnConnectionUR;

                if (progDialog != null)
                    progDialog.Close();

                progDialog = new ProgressDialog("Forbinder til UR...");                                                // Progressbar showing a process is running
                progDialog.Owner = this;
                progDialog.Show();

                Thread connThread = new Thread(() =>
                {
                    connection.EstablishConnectionUR();                                                                // Establish a new connection to both the socket and modbus server
                    connection.EstablishConnectionModbus();
                });
                connThread.IsBackground = true;
                connThread.Start();
            }
            else if (e == "Annuller")
            {
                noReconToUR = false;
            }
        }

        /// <summary>Closes the progressbar when connection result is received.</summary>
        /// <param name="result">Result of the connection.</param>
        void Logic_OnConnectionUR(bool result)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                if (progDialog != null)
                {
                    if (progDialog.IsActive)
                    {
                        progDialog.Close();
                        progDialog = null;
                    }
                }
                if (result)
                    connection.CheckConfigurations();                                                                  // Rerun the update of the configuration data in the window
            });
        }

        /// <summary>Opens a window showing the error, when the robot is in the wrong mode for movement.</summary>
        /// <param name="error">Error message.</param>
        private void URNotRunning(string error)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                if (infoDlg == null)
                {
                    infoDlg = new InfoAndErrorWindow("Fejlmeddelelse", error, "Luk program");

                    infoDlg.OnDialogFinished -= infoDlg_DialogFinished;
                    infoDlg.OnDialogFinished += infoDlg_DialogFinished;
                    infoDlg.OnDialogFinished -= handleURNOTRUNNING;
                    infoDlg.OnDialogFinished += handleURNOTRUNNING;
                    infoDlg.Owner = this;
                    infoDlg.Show();

                    Thread NotRunning = new Thread(() =>                                                               // Wait until the robots modes fulfill the right conditions
                    {
                        while (true)
                        {
                            if (connection.robotMode == "ROBOT_MODE_RUNNING" && connection.safetyMode == "SAFETY_MODE_NORMAL")
                            {
                                break;
                            }
                        }
                        Dispatcher.BeginInvoke((Action)delegate
                        {
                            infoDlg.Close();
                            infoDlg = null;
                        });
                    });
                    NotRunning.IsBackground = true;
                    NotRunning.Start();
                }
            });
        }

        /// <summary>Closes the application.</summary>
        /// <param name="e">Respons message.</param>
        private void handleURNOTRUNNING(object sender, string e)
        {
            this.Close();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        // Videofeed connection handler
        /// <summary>Creates the videoviewer and gets the videofeed from the server.</summary>
        private void ConnectVideoPlayer()
        {
            p = new System.Windows.Forms.Panel();                                                                       // Instantiates a windows forms panel for the videoviewer
            p.BackColor = System.Drawing.Color.Black;
            videoFeedViewer.Child = p;                                                                                  // Adds the panel to the videoveiwer

            connection.GetVideoFeed(ref vPlayer);                                                                       // Connect to the video server
            vPlayer.WindowHandle = p.Handle;                                                                            // Binds the videoplayer to the panel
        }

        /// <summary>Opens a window showing the error.</summary>
        /// <param name="error">Error message.</param>
        private void ConnectionLostVF(string error)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                if (infoDlg == null)
                {
                    infoDlg = new InfoAndErrorWindow("Fejlmeddelelse", error, "Forsøg igen", "Annuller");
                    infoDlg.OnDialogFinished -= infoDlg_DialogFinished;
                    infoDlg.OnDialogFinished += infoDlg_DialogFinished;
                    infoDlg.OnDialogFinished -= handleVF;
                    infoDlg.OnDialogFinished += handleVF;
                    infoDlg.Owner = this;
                    infoDlg.ShowDialog();
                }
            });
        }

        /// <summary>Handles the respons from the user.</summary>
        /// <param name="e">Respons message.</param>
        private void handleVF(object sender, string e)
        {
            if (e == "Forsøg igen")
            {
                Connection.OnConnectionVF -= Logic_OnConnectionVF;
                Connection.OnConnectionVF += Logic_OnConnectionVF;

                if (progDialog != null)
                    progDialog.Close();

                progDialog = new ProgressDialog("Forbinder til videofeed...");                                          // Progressbar showing a process is running
                progDialog.Owner = this;
                progDialog.Show();

                connection.EstablishConnectionVF();                                                                     // Establish a new connection to the videofeed server
            }
            else if (e == "Annuller")
            {

            }
        }

        /// <summary>Closes the progressbar when connection result is received, and reconnects to the videofeed.</summary>
        /// <param name="result">Result of the connection.</param>
        private void Logic_OnConnectionVF(bool result)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                if (result)
                {
                    connection.GetVideoFeed(ref vPlayer);                                                                       // Connect to the video server
                    vPlayer.WindowHandle = p.Handle;                                                                            // Binds the videoplayer to the panel
                }
                if (progDialog != null)
                {
                    progDialog.Close();
                    progDialog = null;
                }
            });
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        // Configuration handler

        /// <summary>Save configurations button click event. Initiates the saving of configurations.</summary>
        private void btn_saveConfigurations_Click(object sender, RoutedEventArgs e)
        {
            ConfigureUR();
        }

        /// <summary>Creates an object of ConfigurationData and initiates the sending process.</summary>
        private void ConfigureUR()
        {
            ConfigurationData conf = new ConfigurationData();
            conf.Acceleration = Convert.ToDouble(tb_accConfig.Text.Replace(".", ","));
            conf.Speed = Convert.ToDouble(tb_speedConfig.Text.Replace(".", ","));
            conf.Payload = Convert.ToDouble(tb_payloadConfig.Text.Replace(".", ","));
            conf.OffsetZ = Convert.ToDouble(tb_tcpOffsetZConfig.Text.Replace(".", ","));

            Connection.OnConfigCompleted -= ConfigCompleted;
            Connection.OnConfigCompleted += ConfigCompleted;

            if (progDialog != null)
                progDialog.Close();

            progDialog = new ProgressDialog("Sender konfigurationer...");
            progDialog.Owner = this;
            progDialog.Show();

            connection.SetConfigurations(conf);
        }

        /// <summary>Presents the result of the configuration for the user.</summary>
        /// <param name="result">Result of the process.</param>
        private void ConfigCompleted(bool result)
        {
            Dispatcher.BeginInvoke((Action)async delegate
            {
                progDialog.Close();
                progDialog = null;
                if (result)                                                                                                         // Show a message that the process was successful
                {
                    lb_configConfirmation.Content = "Indstillingerne er gemt";
                    lb_configConfirmation.Visibility = System.Windows.Visibility.Visible;
                    lb_configConfirmation.Foreground = Brushes.Green;
                }
                else
                {                                                                                                                   // Open a window for the user to try again
                    if (infoDlg == null)
                    {
                        infoDlg = new InfoAndErrorWindow("Fejlmeddelelse", "Indstillingerne kunne ikke gemmes", "Forsøg igen", "Annuller");
                        infoDlg.OnDialogFinished += infoDlg_DialogFinished;
                        infoDlg.OnDialogFinished -= handleConfig;
                        infoDlg.OnDialogFinished += handleConfig;
                        infoDlg.Owner = this;
                        infoDlg.ShowDialog();
                    }
                }

                await Task.Delay(2000);
                lb_configConfirmation.Visibility = System.Windows.Visibility.Hidden;                                                // Hide the message again
            });
        }

        /// <summary>Handles the respons from the user.</summary>
        /// <param name="e">Respons message.</param>
        private void handleConfig(object sender, string e)
        {
            if (e == "Forsøg igen")
            {
                ConfigureUR();                                                                                                      // Try to send configurations again
            }
            else if (e == "Annuller")
            {
                lb_configConfirmation.Content = "Indstillingerne er ikke gemt";                                                     // Tell the user that the filled in configurations hasnt been saved
                lb_configConfirmation.Visibility = System.Windows.Visibility.Visible;
                lb_configConfirmation.Foreground = Brushes.Red;
            }
        }

        /// <summary>Updates the labels in the window with the configurations received.</summary>
        /// <param name="conf">Object containing the received configurations.</param>
        void Connection_OnCurrentConfig(ConfigurationData conf, ForceData force)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                lb_currentOffset.Content = conf.OffsetZ.ToString();
                lb_currentPayload.Content = conf.Payload.ToString();
                lb_currentAcc.Content = conf.Acceleration.ToString();
                lb_currentSpeed.Content = conf.Speed.ToString();
            });
        }
    }
}
