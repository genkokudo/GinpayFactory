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
        private ITestService Test { get; }

        public GenkokuWindowControl(ITestService test)
        {
            Test = test;
            InitializeComponent();
        }

#pragma warning disable VSTHRD100 // Avoid async void methods
        private async void Button1_Click(object sender, RoutedEventArgs e)
#pragma warning restore VSTHRD100 // Avoid async void methods
        {
            try
            {
                var aaaa = Test.Test();

                await Task.Run(async () =>
                {
                    var general = await GinpayOption.GetLiveInstanceAsync();
                    var authKey = general.ApiKey;
                    
                    // DeepLの実装してみる
                    using var httpClient = new HttpClient();
                    using var request = new HttpRequestMessage(new HttpMethod("POST"), "https://api-free.deepl.com/v2/translate");
                    var contentList = new List<string>
                                {
                                    "auth_key=" + authKey,
                                    "text=Hello, world!",
                                    "target_lang=JA"
                                };
                    request.Content = new StringContent(string.Join("&", contentList));
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                    var response = await httpClient.SendAsync(request);
                    var resBodyStr = await response.Content.ReadAsStringAsync();
                    
                    // "{\"translations\":[{\"detected_source_language\":\"EN\",\"text\":\"ハロー、ワールド\"}]}"
                    var deserial = (JObject)JsonConvert.DeserializeObject(resBodyStr);
                    var result = deserial["translations"][0]["text"].ToString();
                    
                    // ログの出し方は？
                });
            }
            catch (Exception ex)
            {
                await ex.LogAsync();
            }
        }
        

    }
}
