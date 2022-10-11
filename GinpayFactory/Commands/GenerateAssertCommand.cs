using CommunityToolkit.Mvvm.DependencyInjection;
using GinpayFactory.Services;
using Microsoft.VisualStudio.Shell.Interop;
using System.Linq;
using System.Reflection;

namespace GinpayFactory
{
    // TODO:結論から言って実現不可能。

    /// <summary>
    /// 現在表示中のクラスのAssertコードを生成する。
    /// TypeクラスのCreateInstanceメソッド
    /// </summary>
    [Command(PackageIds.GenerateAssertCommand)]
    internal sealed class GenerateAssertCommand : BaseCommand<GenerateAssertCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            // 各コマンドの最初に必要
            await Package.JoinableTaskFactory.SwitchToMainThreadAsync();

            // DI
            var source = Ioc.Default.GetService<ISourceService>();

            // Roslynで登録してる部分を取得
            var diSource = await source.SeekOrGetDiSourceAsync();
            if (diSource == null)
            {
                await VS.MessageBox.ShowAsync(
                    "情報",
                    "DI登録を行っているソースが見つかりませんでした。",
                    OLEMSGICON.OLEMSGICON_INFO, 
                    OLEMSGBUTTON.OLEMSGBUTTON_OK
                );
                return;
            }
            // このtextに対してパターンマッチをかけ、登録されているサービス一覧を作成するメソッドを作る。（他のクラスにDIする時にも使う。）
            var serviceNameList = source.GetServiceNameList(diSource.SourceCode);

            await VS.MessageBox.ShowAsync(
                "見つかったServiceのリスト",
                string.Join("\r\n", serviceNameList),
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK
            );

            // 現在のカーソル位置のクラスがどれかを判別して、そのクラスのソースコード（というかノード）を取得。
            // TODO:右クリックした時点のファイル（というかノード）とカーソル位置は控えておくこと。クラス取得時とソース差し替え時に使う。どこに控える？
            // →控えるのはノードだけで良くない？
            var docView = await VS.Documents.GetActiveDocumentViewAsync();
            var position = docView.TextView.Caret.Position.BufferPosition;

            //await source.AddAndReplaceInjectionAsync(serviceNameList);

            await GenkokuWindow.ShowAsync();





            // TODO:csが出来たとして、どうやって既存コードと置換するのか？
            // Delete,Insertはできるので、あとはDeleteする範囲を取得できればよい。

            //    // 成功したら、カーソルの所に挿入
            //    var docView = await VS.Documents.GetActiveDocumentViewAsync();
            //    var selection = docView.TextView.Selection;
            //    docView.TextBuffer.Delete(selection.SelectedSpans[0].Span);
            //    docView.TextBuffer.Insert(selection.Start.Position, result);


        }
    }
}
