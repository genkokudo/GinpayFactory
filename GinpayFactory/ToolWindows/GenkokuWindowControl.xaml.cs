using GinpayFactory.Services;
using Microsoft.VisualStudio.OLE.Interop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static System.Net.Mime.MediaTypeNames;

namespace GinpayFactory
{
    public partial class GenkokuWindowControl : UserControl
    {
        private IDeeplService Deepl { get; }

        public GenkokuWindowControl(IDeeplService deepl)
        {
            Deepl = deepl;
            InitializeComponent();
        }

#pragma warning disable VSTHRD100 // Avoid async void methods
        private async void Button1_Click(object sender, RoutedEventArgs e)
#pragma warning restore VSTHRD100 // Avoid async void methods
        {
            try
            {
                await Task.Run(async () =>
                {
                    var test = await Deepl.TranslateAsync("");
                });
            }
            catch (Exception ex)
            {
                await ex.LogAsync();
            }
        }
        

    }
}
