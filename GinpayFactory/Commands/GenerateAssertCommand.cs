using CommunityToolkit.Mvvm.DependencyInjection;
using GinpayFactory.Services;
using Microsoft.VisualStudio.Shell.Interop;
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
            // ・Roslynで登録してる部分を取得
            var diSource = await source.SeekOrGetDiSourceAsync();
            if (diSource == null)
            {
                await VS.MessageBox.ShowAsync(
                    "情報",
                    "DI登録を行っているソースが見つかりませんでした。",
                    OLEMSGICON.OLEMSGICON_INFO, 
                    OLEMSGBUTTON.OLEMSGBUTTON_OK
                );
            }
            // ・このtextに対してパターンマッチをかけ、登録されているサービス一覧を作成するメソッドを作る。（他のクラスにDIする時にも使う。）

            // 登録ソース生成はメソッド、型引数、引数、呼び出し順序を控えればいける？無茶？
            // やっぱ無茶感が強いので、挿入だけにしておくか…。想定していないメソッドが入るだけで復元できなくなっちゃう。
            // 上記のサービス一覧にあるものを登録しようとすると警告を出して処理中断。
            // ・元の登録コードを再生成できるように、登録の種類・順序・サービス名を控える。





            // TODO:csが出来たとして、どうやって既存コードと置換するのか？
            // Delete,Insertはできるので、あとはDeleteする範囲を取得できればよい。

            //    // 成功したら、カーソルの所に挿入
            //    var docView = await VS.Documents.GetActiveDocumentViewAsync();
            //    var selection = docView.TextView.Selection;
            //    docView.TextBuffer.Delete(selection.SelectedSpans[0].Span);
            //    docView.TextBuffer.Insert(selection.Start.Position, result);


            // TODO:正規表現で登録しているサービス一覧が取れないかな？

        }
    }
}
