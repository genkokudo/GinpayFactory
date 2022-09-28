using Community.VisualStudio.Toolkit;
using CommunityToolkit.Mvvm.DependencyInjection;
using GinpayFactory.Enums;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using Path = System.IO.Path;

namespace GinpayFactory.Services
{
    // サービス登録してる箇所がソリューション内で1つであることが前提。
    // 同一ソースでDI登録箇所が複数あり、片方コメントアウトされている場合までは面倒を見られない。ソースファイルを決定するまではコメントアウト考慮。

    public class DiOption
    {
        /// <summary>
        /// DIに使用しているライブラリ
        /// </summary>
        public DiLibrary DiLibrary { get; set; }

        /// <summary>
        /// DI登録を行っているservicesインスタンス名
        /// </summary>
        public string DiServicesName { get; set; }
    }

    /// <summary>
    /// プロジェクト内のソースに関する処理
    /// 値を記憶するのでSingleton登録すること
    /// </summary>
    public interface ISourceService
    {
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

        /// <summary>
        /// ソースコードからコメントを除外する。
        /// </summary>
        /// <param name="text">ソースコード</param>
        /// <returns>コメントが除外されたソースコード</returns>
        public string RemoveComments(string text);

        /// <summary>
        /// 行からインデントのスペースを取得する。
        /// インデントにTabを使用しているソースは想定しない。
        /// </summary>
        /// <param name="line">1行のテキスト</param>
        /// <returns></returns>
        public string GetIndentSpaces(string line);

        /// <summary>
        /// 1つのテキストから特定のキーワードを含む行を取り出す。
        /// 最初に見つかった行だけ取り出す。
        /// </summary>
        /// <param name="text">テキスト全部</param>
        /// <param name="keyword">探す文字列</param>
        /// <returns></returns>
        public string GetLineText(string text, string keyword);


        // ---------- ここから下はVS拡張依存 ----------
        /// <summary>
        /// 現在のソースが.csであることを確認する
        /// </summary>
        /// <returns>.csならtrue</returns>
        public Task<bool> CheckCurrentSourceIsCSharpAsync();

        /// <summary>
        /// 現在VSに表示しているソースのパスを取得する。
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

        /// <summary>
        /// ソリューション内から設定されているフレームワークでDI登録を行っている箇所を探す。
        /// 現在のソースからServiceクラスまたはIServiceインタフェースを探し、上記の場所に登録する。
        /// </summary>
        /// <param name="diSubmit">DI登録の種類</param>
        /// <returns></returns>
        public Task AddServiceAsync(DiSubmit diSubmit);
    }

    public class SourceService : ISourceService
    {
        /// <summary>
        /// ソリューション内のDIを行っているソースのPathを1つだけ記憶する
        /// コメントアウトしていても認識するので注意
        /// </summary>
        public string DiSourcePath { get; private set; }
        /// <summary>DI登録を行っているクラス</summary>
        public string DiClass { get; private set; }
        /// <summary>DI登録を行っているメソッド</summary>
        public string DiMethod { get; private set; }

        private IOptions<DiOption> Option { get; set; }
        private IRoslynService Roslyn { get; set; }

        public SourceService(IOptions<DiOption> option, IRoslynService roslyn)
        {
            Option = option;
            Roslyn = roslyn;
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
            // 現在の所CommunityToolkitのみ対応。
            foreach (var cs in csList)
            {
                var diClass = Roslyn.FindDiClass(cs);
                if (diClass.Item1 != null)
                {
                    DiSourcePath = cs;
                    DiClass = diClass.Item1;
                    DiMethod = diClass.Item2;
                    return cs;
                }
            }

            #region 没
            //// .csから、DI登録しているクラスを探す
            //// CommunityToolkitのみ対応。
            //foreach (var cs in csList)
            //{
            //    // コメントは除外
            //    var text = RemoveComments(File.ReadAllText(cs));
            //    if (text.Contains(Option.Value.DiLibrary.GetStringValue()))
            //    {
            //        // 見つかったら覚えておく
            //        return cs;
            //    }
            //}
            #endregion

            return null;
        }


        public string RemoveComments(string text)
        {
            // コメントを除外したソースにする。
            // 文字列に"//"とか"/*"とか入ってるのは知らない…。

            // 優先順は"*/", "//", "/*"のようだ。
            var re = @"(@(?:""[^""]*"")+|""(?:[^""\n\\]+|\\.)*""|'(?:[^'\n\\]+|\\.)*')|//.*|/\*(?s:.*?)\*/";
            return Regex.Replace(text, re, "$1");
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
            var keyword = Option.Value.DiLibrary.GetStringValue();
            var position = text.IndexOf(keyword);

            // スペースがあれば揃える
            var targetLine = GetLineText(text, keyword);
            var spaces = GetIndentSpaces(targetLine);

            // 入れてみる
            var service = Option.Value.DiLibrary == DiLibrary.CommunityToolkit ? string.Empty : Option.Value.DiServicesName;
            var semicolon = Option.Value.DiLibrary == DiLibrary.CommunityToolkit ? string.Empty : ";";
            view.TextBuffer.Insert(position, $"{string.Format(di.GetStringValue(), serviceName, service, semicolon)}\r\n{spaces}");
            
        }

        public string GetIndentSpaces(string line)
        {
            var count = 0;
            foreach (var item in line)
            {
                if (item != ' ')
                {
                    break;
                }
                count++;
            }
            return new string(' ', count);
        }

        public string GetLineText(string text, string keyword)
        {
            var lines = text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
            foreach (var line in lines)
            {
                if (line.Contains(keyword))
                {
                    return line;
                }
            }
            return null;
        }

        /// <summary>
        /// ソリューション内から設定されているフレームワークでDI登録を行っている箇所を探す。
        /// 現在のソースからServiceクラスまたはIServiceインタフェースを探し、上記の場所に登録する。
        /// </summary>
        /// <param name="diSubmit">DI登録の種類</param>
        /// <returns></returns>
        public async Task AddServiceAsync(DiSubmit diSubmit)
        {
            // ソリューション内の.csから、DI登録しているクラスを探す
            await UpdateDiSourcePathAsync();

            // 現在のソースが.csであることを確認する
            if (!await CheckCurrentSourceIsCSharpAsync())
            {
                // .csではない
                await VS.MessageBox.ShowAsync("情報", "この処理は.csのみ有効です。", OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK);
                return;
            }

            // 一般クラス登録
            if (diSubmit == DiSubmit.GeneralClass)
            {
                var classes = Roslyn.GetAllClassNames(await GetActiveDocumentFilePathAsync());
                await SubmitAsync(diSubmit, classes);
                return;
            }
            else
            {
                // サービス登録
                var services = Roslyn.GetServiceClassNames(await GetActiveDocumentFilePathAsync());
                if (services.Count() == 0)
                {
                    await VS.MessageBox.ShowAsync("情報", "Serviceが見つかりませんでした。", OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK);
                    return;
                }

                // ソースに登録処理を追加する
                await SubmitAsync(diSubmit, services);
            }
        }

        /// <summary>
        /// 取得したクラス名全てについて、DI登録するかダイアログで確認する。
        /// Yesを選択した場合は登録する。
        /// </summary>
        /// <param name="diSubmit">登録種類</param>
        /// <param name="names">クラス名</param>
        /// <returns></returns>
        private async Task SubmitAsync(DiSubmit diSubmit, IEnumerable<string> names)
        {
            foreach (var name in names)
            {
                var result = await VS.MessageBox.ShowAsync($"{diSubmit}登録", $"{name}をDI登録しますか？", OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_YESNO);
                if (result == Microsoft.VisualStudio.VSConstants.MessageBoxResult.IDNO)
                {
                    continue;
                }
                await SeekAndInsertDiAsync(diSubmit, name);
            }
        }
    }
}
