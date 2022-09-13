﻿using GinpayFactory.Enums;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Path = System.IO.Path;

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

        // VS拡張依存
        /// <summary>
        /// 現在のソースが.csであることを確認する
        /// </summary>
        /// <returns></returns>
        public Task<bool> CheckCurrentSourceIsCSharpAsync();
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

        public async Task<bool> CheckCurrentSourceIsCSharpAsync()
        {
            var docView = await VS.Documents.GetActiveDocumentViewAsync();
            if (docView == null) return false;
            var ex = Path.GetExtension(docView.FilePath);
            if (ex != ".cs") return false;
            return true;
        }
    }
}
