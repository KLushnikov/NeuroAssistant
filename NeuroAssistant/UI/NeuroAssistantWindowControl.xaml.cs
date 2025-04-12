using System.Windows;
using System.Windows.Controls;

namespace NeuroAssistant.UI
{
    /// <summary>
    /// Interaction logic for NeuroAssistantWindowControl.
    /// </summary>
    public partial class NeuroAssistantWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NeuroAssistantWindowControl"/> class.
        /// </summary>
        public NeuroAssistantWindowControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", ToString()),
                "NeuroAssistantWindow");
        }
    }
}