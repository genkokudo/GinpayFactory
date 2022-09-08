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
        /// <returns>フルパスのリスト</returns>
        public Task<List<string>> GetSourcePathListAsync();
    }

    public class SourceService : ISourceService
    {
        public async Task<List<string>> GetSourcePathListAsync()
        {
            var result = new List<string>();

            //// ソリューション内のすべてのプロジェクトを取得する
            //var projects = await VS.Solutions.GetAllProjectsAsync();
            //var project = projects.First();
            //foreach (var child in project.Children)
            //{
            //    if (child.Type == SolutionItemType.PhysicalFile)
            //    {
            //        // これでファイルが取れる。
            //        Console.WriteLine(child.FullPath);
            //    }
            //    if (child.Type == SolutionItemType.PhysicalFolder)
            //    {
            //    }
            //}

            return result;
        }
    }
}
