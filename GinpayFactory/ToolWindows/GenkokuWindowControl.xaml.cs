using System.Windows;
using System.Windows.Controls;

namespace GinpayFactory
{
    public partial class GenkokuWindowControl : UserControl
    {
        public GenkokuWindowControl()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            VS.MessageBox.Show("GenkokuWindowControl", "Button clicked");
        }
    }
}
