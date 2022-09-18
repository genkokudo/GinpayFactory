using GinpayFactory.Enums;
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
        /// <returns>.csならtrue</returns>
        public Task<bool> CheckCurrentSourceIsCSharpAsync();

        /// <summary>
        /// 現在のソースのパスを取得する。
        /// </summary>
        /// <returns>現在のソースのパス</returns>
        public Task<string> GetActiveDocumentFilePathAsync();

        /// <summary>
        /// 記憶しているDI登録処理を行っているクラスから、
        /// DI登録処理を行っている箇所を探して、登録する
        /// </summary>
        /// <param name="di">DI登録の種類</param>
        /// <param name="serviceName">サービス名</param>
        /// <returns></returns>
        public Task SeekAndInsertDiAsync(DiSubmit di, string serviceName);

        // DI登録処理を行っているクラス一覧を取得
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

        public async Task<string> GetActiveDocumentFilePathAsync()
        {
            var currentSource = await VS.Documents.GetActiveDocumentViewAsync();
            if (currentSource == null) return null;
            return currentSource.FilePath;
        }

        public async Task<bool> CheckCurrentSourceIsCSharpAsync()
        {
            var filePath = await GetActiveDocumentFilePathAsync();
            if (filePath == null) return false;
            var ex = Path.GetExtension(filePath);
            if (ex != ".cs") return false;
            return true;
        }
        
        public async Task SeekAndInsertDiAsync(DiSubmit di, string serviceName)
        {
            await UpdateDiSourcePathAsync();

            // DIしている場所を探す（DIライブラリ別）
            // 実際にVSで開いたらいいのかな？
            var view = await VS.Documents.OpenAsync(DiSourcePath);

            // 何文字目からが登録処理か
            var text = File.ReadAllText(DiSourcePath);
            var position = text.IndexOf(Option.Value.DiLibrary.GetStringValue());

            // 入れてみる
            view.TextBuffer.Insert(position, string.Format(di.GetStringValue(), serviceName));

            // TODO:結果
            //hogeIoc.Default.ConfigureServices(new ServiceCollection()

        }
    }

    /// <summary>
    /// DI登録の種類
    /// </summary>
    public enum DiSubmit
    {
        /// <summary>
        /// Transient
        /// </summary>
        [StringValue(".AddTransient<I{0}, {0}>()")]
        Transient = 1,
        /// <summary>
        /// Singleton
        /// </summary>
        [StringValue(".AddSingleton<I{0}, {0}>()")]
        Singleton = 2,
        /// <summary>
        /// Option
        /// </summary>
        [StringValue("")]
        Option = 3,
        /// <summary>
        /// その他一般的なオブジェクトを登録
        /// </summary>
        [StringValue("")]
        GeneralObject = 4
    }
}
