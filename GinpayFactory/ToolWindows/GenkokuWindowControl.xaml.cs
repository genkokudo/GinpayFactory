using GinpayFactory.ViewModels;
using System.Windows.Controls;

namespace GinpayFactory
{
    public partial class GenkokuWindowControl : UserControl
    {

        public GenkokuWindowControl(GenkokuWindowControlViewModel context)
        {
            InitializeComponent();
            DataContext = context;
        }
        
    }
}
