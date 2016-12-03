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
    /// <para>Interaction logic for ProgressDialog.xaml</para>
    /// Dialog that shows a costumized describing text and a running progressbar.
    /// </summary>
    public partial class ProgressDialog : Window
    {
        /// <summary>Shows a running progressbar and presents a text.</summary>
        /// <param name="text">Text describing the progress.</param>
        public ProgressDialog(string text)
        {
            InitializeComponent();
            lb_text.Text = text;
        }
    }
}
