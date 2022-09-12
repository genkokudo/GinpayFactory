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

// 機能とか対応言語増やすならここ
// https://www.deepl.com/docs-api
namespace GinpayFactory.Services
{
    public class DiOption
    {
        /// <summary>
        /// DIに使用しているライブラリ
        /// </summary>
        public DiLibrary DiLibrary { get; set; }
    }

    /// <summary>
    /// プロジェクト内のソースに関する処理
    /// 値を記憶するのでSingleton登録すること
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

        /// <summary>
        /// 現在のソリューション内から、
        /// DI登録処理を行っているクラスを探して記憶する
        /// </summary>
        /// <returns></returns>
        public Task UpdateDiSourcePathAsync();
    }

    public class SourceService : ISourceService
    {
        /// <summary>
        /// ソリューション内のDIを行っているソースのPathを1つだけ記憶する
        /// コメントアウトしていても認識するので注意
        /// </summary>
        public string DiSourcePath { get; private set; }

        public IOptions<DiOption> Option { get; set; }

        public SourceService(IOptions<DiOption> option)
        {
            Option = option;
        }

        public async Task<List<string>> GetSourcePathListAsync(bool excludeObj = true)
        {
            var result = new List<string>();

            // ソリューション内のすべてのプロジェクトを取得する
            var projects = await VS.Solutions.GetAllProjectsAsync();
            foreach (var project in projects)
            {
                var files = Directory.GetFiles(Path.GetDirectoryName(project.FullPath), "*.cs", SearchOption.AllDirectories);
                if (excludeObj)
                {
                    // objフォルダは除外する
                    var objDir = Path.Combine(Path.GetDirectoryName(project.FullPath), "obj");
                    var objFiles = Directory.GetFiles(objDir, "*.cs", SearchOption.AllDirectories);
                    files = files.Except(objFiles).ToArray();
                }
                result.AddRange(files);

            }

            return result;
        }

        public async Task UpdateDiSourcePathAsync()
        {
            if (!string.IsNullOrWhiteSpace(DiSourcePath))
            {
                return;
            }

            DiSourcePath = await SeekDiSourceAsync();
        }

        private async Task<string> SeekDiSourceAsync()
        {
            // ソリューション内の.csファイルのフルパスを全て取得
            var csList = await GetSourcePathListAsync();

            // .csから、DI登録しているクラスを探す
            // CommunityToolkitのみ対応。
            foreach (var cs in csList)
            {
                var text = File.ReadAllText(cs);
                if (text.Contains(Option.Value.DiLibrary.GetStringValue()))
                {
                    // 見つかったら覚えておく
                    return cs;
                }
            }

            return null;
        }
    }
}
