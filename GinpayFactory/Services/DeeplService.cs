using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace GinpayFactory.Services
{
    public class DeeplOption
    {
        /// <summary>
        /// DeepL FreeのAPIキー
        /// </summary>
        public string ApiKey { get; set; }
    }

    public interface IDeeplService
    {
        public Task<string> TestAsync();
    }

    public class DeeplService : IDeeplService
    {
        public IOptions<DeeplOption> Option { get; set; }

        public DeeplService(IOptions<DeeplOption> option)
        {
            Option = option;
        }

        public async Task<string> TestAsync()
        {
            // DeepLの実装してみる
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("POST"), "https://api-free.deepl.com/v2/translate");
            var contentList = new List<string>
                                {
                                    "auth_key=" + Option.Value.ApiKey,
                                    "text=Hello, world!",
                                    "target_lang=JA"
                                };
            request.Content = new StringContent(string.Join("&", contentList));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

            var response = await httpClient.SendAsync(request);
            var resBodyStr = await response.Content.ReadAsStringAsync();

            // "{\"translations\":[{\"detected_source_language\":\"EN\",\"text\":\"ハロー、ワールド\"}]}"
            var deserial = (JObject)JsonConvert.DeserializeObject(resBodyStr);
            return deserial["translations"][0]["text"].ToString();
        }
    }
}
