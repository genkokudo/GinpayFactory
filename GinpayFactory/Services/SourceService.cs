using Community.VisualStudio.Toolkit;
using CommunityToolkit.Mvvm.DependencyInjection;
using GinpayFactory.Enums;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Shapes;
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

        /// <summary>
        /// DI登録のソースが未取得の場合、検索して取得する
        /// 既にある場合、そのcsから再取得する
        /// </summary>
        /// <returns></returns>
        public Task<MethodData> SeekOrGetDiSourceAsync();

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
        /// DI登録処理を行っている箇所を探して、指定したサービスを登録する
        /// </summary>
        /// <param name="di">DI登録の種類</param>
        /// <param name="serviceName">サービス名</param>
        /// <returns></returns>
        public Task SeekAndInsertDiAsync(DiSubmit di, string serviceName);

        /// <summary>
        /// ソリューション内から設定されているフレームワークでDI登録を行っている箇所を探す。
        /// 現在表示中のソースからServiceクラスまたはIServiceインタフェースを探し、上記の場所に登録する。
        /// </summary>
        /// <param name="diSubmit">DI登録の種類</param>
        /// <returns></returns>
        public Task AddServiceAsync(DiSubmit diSubmit);

        /// <summary>
        /// 現在のソリューションでDIされているサービスの一覧を検出し、その一覧を取得する。
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<string>> GetServiceNameListAsync();

        /// <summary>
        /// 引数のソースコードに対し
        /// 正規表現によってDIされているサービスの一覧を検出し、その一覧を取得する。
        /// </summary>
        /// <param name="source">ソースコード</param>
        /// <returns>サービスの一覧</returns>
        public IEnumerable<string> GetServiceNameList(string source);

        // TODO:じっそうすること
        // 現在のカーソル位置のクラスがどれかを判別して、そのクラスのソースコードを取得する
        // Roslynからクラス一覧とそのSpanを取る
        // サービス名をインタフェースとしてDIしたソースを取得し、差し替える。
        public Task AddAndReplaceInjectionAsync(IEnumerable<string> serviceNames);
    }

    public class SourceService : ISourceService
    {
        /// <summary>DI登録を行っているメソッドとファイル名情報</summary>
        public MethodData DiMethodData { get; private set; }

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

        public async Task<MethodData> SeekOrGetDiSourceAsync()
        {
            // 1度ソースを見つけている場合は、検索は行わずソースコード再取得のみ
            DiMethodData = DiMethodData == null ? await SeekDiSourceAllAsync() : GetDiSource(DiMethodData.Path);
            return DiMethodData;
        }

        // 対象csからDI登録のソースを取得する
        private MethodData GetDiSource(string csPath)
        {
            // .csから、DI登録しているクラスを探す
            // 現在の所CommunityToolkitのみ対応。
            var diClass = Roslyn.FindDiMethod(csPath);
            if (diClass != null)
            {
                return diClass;
            }
            return null;
        }

        // 全てのcsからDI登録のソースを探して取得する
        private async Task<MethodData> SeekDiSourceAllAsync()
        {
            // ソリューション内の.csファイルのフルパスを全て取得
            var csList = await GetSourcePathListAsync();

            // 全ての.csから、DI登録しているクラスを探す
            foreach (var cs in csList)
            {
                var di = GetDiSource(cs);
                if (di != null)
                {
                    return di;
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
            // DIしている場所を探す（DIライブラリ別）
            var diData = await SeekOrGetDiSourceAsync();

            // 実際にVSでその場所を表示する
            var view = await VS.Documents.OpenAsync(diData.Path);

            // 何文字目からが登録処理か
            var text = File.ReadAllText(diData.Path);
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

        public async Task AddServiceAsync(DiSubmit diSubmit)
        {
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

        public IEnumerable<string> GetServiceNameList(string source)
        {
            var list = new List<string>();

            // "<"と">"に囲まれている文字列を検索
            var terms = "<(.+)>";
            // 条件に合った文字列を全部拾う
            var r = new Regex(terms, RegexOptions.Multiline);
            var mc = r.Matches(source);

            foreach (var item in mc)
            {
                var services = item.ToString().Trim('<').Trim('>').Replace(" ", string.Empty).Split(',');
                list.AddRange(services);
            }

            // 重複は除外する
            // 末尾が"Service"ではないものは除外する
            var result = list.Distinct();
            result = result.Where(x => x.EndsWith("Service"));

            // 先頭から"I"除いた文字列が他と重複した場合、それはインタフェースとして除外する
            var removeList = new List<string>();
            foreach (var item in result)
            {
                if (item.StartsWith("I"))
                {
                    var serviceName = item.Substring(1);
                    if (result.Contains(serviceName))
                    {
                        removeList.Add(item);
                    }
                }
            }
            result = result.Where(x => !removeList.Contains(x));

            return result;
        }

        public async Task<IEnumerable<string>> GetServiceNameListAsync()
        {
            var source = await SeekOrGetDiSourceAsync();
            if (source == null)
            {
                return null;
            }
            return GetServiceNameList(source.SourceCode);
        }

        public async Task AddAndReplaceInjectionAsync(IEnumerable<string> serviceNames)
        {
            // 表示中のソースからクラスとSpanを取得
            var classes = Roslyn.GetAllClasses(await GetActiveDocumentFilePathAsync());
            // 現在のカーソル位置のクラスがどれかを判別して、そのクラスのソースコードを取得する
            var docView = await VS.Documents.GetActiveDocumentViewAsync();
            var position = docView.TextView.Caret.Position.BufferPosition;      // TODO:これ、右クリックした時点の値とファイルじゃないと意味ないのでは？↑も。

            // サービス名をインタフェースとしてDIしたソースを取得し、差し替える。
            //Roslyn.AddInjection(string source, IEnumerable<string> serviceNames);
        }
    }
}
