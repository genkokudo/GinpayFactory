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
using Community.VisualStudio.Toolkit;
using System.IO;
using System.Windows.Shapes;
using Microsoft.VisualStudio.Package;
using Path = System.IO.Path;
using GinpayFactory.Enums;

namespace GinpayFactory.Services
{
    /// <summary>
    /// ソース解析関係の処理を定義する
    /// </summary>
    public interface IRoslynService
    {
        // IOptionの登録は？

        /// <summary>
        /// .csソースからサービスか、インタフェースを探し
        /// そのサービス名を取得する。
        /// "～～Service"というネーミング以外は無効とする。
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>サービス名</returns>
        public string GetServiceClassName(string filePath);
    }

    public class RoslynService : IRoslynService
    {
        public string GetServiceClassName(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}
