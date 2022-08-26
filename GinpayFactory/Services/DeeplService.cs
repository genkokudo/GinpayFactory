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

// 機能とか対応言語増やすならここ
// https://www.deepl.com/docs-api
namespace GinpayFactory.Services
{
    public class DeeplOption
    {
        /// <summary>
        /// DeepL FreeのAPIキー
        /// </summary>
        public string ApiKey { get; set; }
    }

    /// <summary>
    /// DeepLサービスに英語を投げて、翻訳結果を受け取るサービス
    /// </summary>
    public interface IDeeplService
    {
        /// <summary>
        /// 英語から日本語に翻訳するAPIを実行する。
        /// </summary>
        /// <param name="englishText">英語</param>
        /// <returns>失敗したらnull</returns>
        public Task<string> TranslateAsync(string englishText);
    }

    public class DeeplService : IDeeplService
    {
        public IOptions<DeeplOption> Option { get; set; }

        public DeeplService(IOptions<DeeplOption> option)
        {
            Option = option;
        }

        public async Task<string> TranslateAsync(string englishText)
        {
            try
            {
                // DeepLの実装
                using var httpClient = new HttpClient();
                using var request = new HttpRequestMessage(new HttpMethod("POST"), "https://api-free.deepl.com/v2/translate");
                var contentList = new List<string>
                                    {
                                        "auth_key=" + Option.Value.ApiKey,
                                        "text=" + englishText,
                                        "target_lang=JA"
                                    };
                request.Content = new StringContent(string.Join("&", contentList));
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                var response = await httpClient.SendAsync(request);
                var resBodyStr = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(resBodyStr))
                {
                    return null;
                }

                // "{\"translations\":[{\"detected_source_language\":\"EN\",\"text\":\"ハロー、ワールド\"}]}"
                var deserial = (JObject)JsonConvert.DeserializeObject(resBodyStr);
                return deserial["translations"][0]["text"].ToString();
            }
            catch (Exception)
            {
                // 失敗したらnull
                return null;
            }
        }
    }
}
