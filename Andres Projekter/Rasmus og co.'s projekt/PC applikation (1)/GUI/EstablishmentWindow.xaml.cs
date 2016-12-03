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
using System.Windows.Threading;

namespace GUI
{
    /// <summary>
    /// <para>Interaction logic for EtablishmentWindow.xaml</para>
    /// Allows the user to establish connection to a specific scanning unit.
    /// Is the unit not already in the list, this window can direct the user to ConnectionWindow to create a new connection. 
    /// </summary>
    public partial class EstablishmentWindow : Window
    {
        Logic logic;
        InfoAndErrorWindow errorDlg = null;
        InfoAndErrorWindow infoDlg = null;
        ProgressDialog progDialog;
        ConnectionWindow connWin;

        /// <summary>Create instance of the EstablishmentWindow.</summary>
        public EstablishmentWindow()
        {
            InitializeComponent();

            logic = Logic.GetInstance();
        }

        /// <summary>Window loaded event. Gets the scan units to show in the listbox.</summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ScanUnits scanUnits = logic.GetScanUnits();
            foreach (Unit u in scanUnits.Units)
            {
                lb_units.Items.Add(u.id);
            }
        }

        /// <summary>Selection changed event. Signals that a new object has been selected in the listbox.</summary>
        private void lb_units_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            logic.FindSelectedUnit(lb_units.SelectedItem);                                              // Inform logic that a new unit has been selected
            btn_establishConnection.IsEnabled = true;                                                   // Enable the establish connection button
        }

        /// <summary>Listbox lost focus event. Disables the establish connection button when focus is lost.</summary>
        private void lb_units_LostFocus(object sender, RoutedEventArgs e)
        {
            btn_establishConnection.IsEnabled = true;
        }

        /// <summary>Establish connection event. Opens a window for the user to confirm the establishment.</summary>
        private void btn_establishConnection_Click(object sender, RoutedEventArgs e)
        {
            if (infoDlg == null)
            {
                infoDlg = new InfoAndErrorWindow("Bekræftigelse","Vil du oprette forbindelse til " + lb_units.SelectedItem.ToString() + "?", "Ok", "Annuller");
                infoDlg.OnDialogFinished -= handleConnection;
                infoDlg.OnDialogFinished += handleConnection;
                infoDlg.Owner = this;
                infoDlg.ShowDialog();
            }
        }

        /// <summary>Handles the respons from the user.</summary>
        private void handleConnection(object sender, string e)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                if (e == "Ok")                                                                          // If the user confirms the establishment a progressbar is presented and a establish connection process is initialized
                {
                    progDialog = new ProgressDialog("Forbinder...");
                    progDialog.Owner = this;
                    this.IsEnabled = false;
                    progDialog.Show();

                    Logic.OnException -= new Logic.LogicExceptionEventHandler(ShowException);
                    Logic.OnException += new Logic.LogicExceptionEventHandler(ShowException);
                    Connection.OnConnectionCompleted -= new Connection.ConnectionCompletedEventHandler(ConnectionCompleted);
                    Connection.OnConnectionCompleted += new Connection.ConnectionCompletedEventHandler(ConnectionCompleted);

                    Thread connThread = new Thread(() =>                                                // Calls the establish connection in a thread so the progressbar isnt blocked
                    {
                        logic.EstablishConnection();
                    });
                    connThread.IsBackground = true;
                    connThread.Start();
                }

                else if (e == "Annuller")                                                               // The user cancels the establishment and the window is closed
                {
                    infoDlg.Close();
                    infoDlg = null;
                    this.IsEnabled = true;
                }
            });
        }

        /// <summary>Opens the MainWindow if connection is successful.</summary>
        private void ConnectionCompleted(bool result)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                if (result)
                {
                    Logic.OnException -= new Logic.LogicExceptionEventHandler(ShowException);           // Remove the listener from this window to prevent duplicate
                    MainWindow mainWin = new MainWindow();
                    mainWin.Show();
                    this.Close();
                    progDialog.Close();
                    infoDlg.Close();
                    infoDlg = null;
                }
            });
        }

        /// <summary>Add new unit click event. Opens a ConnectionWindow to add a new unit.</summary>
        private void btn_addNewUnit_Click(object sender, RoutedEventArgs e)
        {
            Logic.OnException -= new Logic.LogicExceptionEventHandler(ShowException);                                   // Remove listeners from this window to prevent duplicate
            Connection.OnConnectionCompleted -= new Connection.ConnectionCompletedEventHandler(ConnectionCompleted);
            connWin = new ConnectionWindow();
            connWin.Owner = this;
            if (connWin.ShowDialog().Value)
            {
                Unit newUnit = connWin.NewUnit;                                                                         // Get the new unit from the ConnectionWindow
                logic.SaveNewUnit(newUnit);                                                                             // Send the new unit to be saved
                lb_units.Items.Add(newUnit.id);                                                                         // Add the new unit to the listbox
            }
            connWin = null;
        }

        /// <summary>Open a new window to present the error to the user.</summary>
        /// <param name="error">Error message</param>
        /// <param name="source">Source of the error</param>
        private void ShowException(string error, string source)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                if (infoDlg != null)
                {
                    infoDlg.Close();
                    infoDlg = null;
                }

                progDialog.Close();
                this.IsEnabled = true;

                if (errorDlg == null)
                {
                    errorDlg = new InfoAndErrorWindow("Fejlmeddelelse", "Der kunne ikke oprettes forbindelse til " + source, "Ok");
                    errorDlg.OnDialogFinished -= infoDlg_Error_DialogFinished;
                    errorDlg.OnDialogFinished += infoDlg_Error_DialogFinished;
                    errorDlg.Owner = this;
                    errorDlg.ShowDialog();
                }
            });
        }

        /// <summary>DialogFinished event that closes the window.</summary>
        void infoDlg_Error_DialogFinished(object sender, string e)
        {
            errorDlg.Close();
            errorDlg = null;
        }
    }
}
