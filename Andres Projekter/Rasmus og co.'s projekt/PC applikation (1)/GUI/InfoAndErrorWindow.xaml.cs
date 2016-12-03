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

namespace GUI
{
    /// <summary>
    /// <para>Interaction logic for InfoAndErrorWindow.xaml</para>
    /// Costumized dialog box with a text message that works similar to MessageBox to inform the user.
    /// The dialog box can have different buttons with costumized content depending on how the dialog is created.
    /// </summary>
    public partial class InfoAndErrorWindow : Window
    {
        /// <summary>DialogFinished event. This event is thrown when the dialog finishes.</summary>
        public event EventHandler<string> OnDialogFinished;

        /// <summary>Create instance of the InfoAndErrorWindow without parameters.</summary>
        public InfoAndErrorWindow()
        {
            InitializeComponent();
        }

        /// <summary>Create instance of the InfoAndErrorWindow with parameters. This constructor only shows one button.</summary>
        /// <param name="title">Title of the Window.</param>
        /// <param name="message">Message in the window.</param>
        /// <param name="confirmation">Button content.</param>
        public InfoAndErrorWindow(string title, string message, string confirmation)
        {
            InitializeComponent();
            lb_message.Text = message;
            Title = title;
            btn_confirmation.Content = confirmation;
            Panel_Btn.Children.Remove(btn_cancel);          // Remove the button from the window
        }

        /// <summary>Create instance of the InfoAndErrorWindow with parameters. This constructor shows two buttons.</summary>
        /// <param name="title">Title of the Window.</param>
        /// <param name="message">Message in the window.</param>
        /// <param name="confirmation">Button content.</param>
        /// <param name="cancel">Button content.</param>
        public InfoAndErrorWindow(string title, string message, string confirmation, string cancel)
        {
            InitializeComponent();
            lb_message.Text = message;
            Title = title;
            btn_confirmation.Content = confirmation;
            btn_cancel.Content = cancel;
        }

        /// <summary>Confirmation button click event.</summary>
        private void btn_confirmation_Click(object sender, RoutedEventArgs e)
        {
            btn_confirmation.IsEnabled = false;
            if(OnDialogFinished != null)
                OnDialogFinished(this, btn_confirmation.Content.ToString());
        }

        /// <summary>Cancel button click event.</summary>
        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            if (OnDialogFinished != null)
                OnDialogFinished(this, btn_cancel.Content.ToString());
        }
    }
}
