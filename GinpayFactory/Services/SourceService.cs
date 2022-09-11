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

// 機能とか対応言語増やすならここ
// https://www.deepl.com/docs-api
namespace GinpayFactory.Services
{
    /// <summary>
    /// プロジェクト内のソースに関する処理
    /// </summary>
    public interface ISourceService
    {
        // サービス登録してる箇所がソリューション内で1つであることが前提。
        // もし複数ある想定なら？
        // →現在編集中のプロジェクトを取って、その中で探す
        // →全部列挙して、その中から対象をユーザに選択させる

        /// <summary>
        /// ソリューション内の.csファイルのフルパスを全て取得する。
        /// </summary>
        /// <param name="excludeObj">objフォルダに生成されたソースを除外する</param>
        /// <returns>フルパスのリスト</returns>
        public Task<List<string>> GetSourcePathListAsync(bool excludeObj = true);
    }

    public class SourceService : ISourceService
    {
        public async Task<List<string>> GetSourcePathListAsync(bool excludeObj)
        {
            var result = new List<string>();

            // ソリューション内のすべてのプロジェクトを取得する
            var projects = await VS.Solutions.GetAllProjectsAsync();
            foreach (var project in projects)
            {
                var files = Directory.GetFiles(Path.GetDirectoryName(project.FullPath), "*.cs", SearchOption.AllDirectories);
                if (excludeObj)
                {
                    var objDir = Path.Combine(Path.GetDirectoryName(project.FullPath), "obj");
                    var objFiles = Directory.GetFiles(objDir, "*.cs", SearchOption.AllDirectories);
                    files = files.Except(objFiles).ToArray();
                }
                result.AddRange(files);

            }

            return result;
        }

    }
}
