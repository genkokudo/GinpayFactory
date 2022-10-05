using CommunityToolkit.Mvvm.DependencyInjection;
using GinpayFactory.Services;
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

            // TODO:csが出来たとして、どうやって既存コードと置換するのか？
            // Delete,Insertはできるので、あとはDeleteする範囲を取得できればよい。それはRoslynで取れるはず。

            //    // 成功したら、カーソルの所に挿入
            //    var docView = await VS.Documents.GetActiveDocumentViewAsync();
            //    var selection = docView.TextView.Selection;
            //    docView.TextBuffer.Delete(selection.SelectedSpans[0].Span);
            //    docView.TextBuffer.Insert(selection.Start.Position, result);


            // TODO:正規表現で登録しているサービス一覧が取れないかな？
            // ・Roslynで登録してる部分を取得（メソッド作っちゃえ）
            // ・このtextに対してパターンマッチをかける。
            // ・元の登録コードを再生成できるように、登録の種類・順序・サービス名を控える。



            #region 取り敢えず置いといて
            //// DI
            //var assert = Ioc.Default.GetService<IAssertService>();

            //Assembly assem = Assembly.GetExecutingAssembly();
            ////Assembly assem2 = Assembly.GetExecutingAssembly();
            ////Assembly assem3 = Assembly.GetExecutingAssembly();

            //// TODO:現在表示中のアセンブリ名を取得
            //// TODO:現在表示中のクラス名を指定
            ////var targetObject = assem.CreateInstance("StudyRoslyn.Sample");

            //await VS.MessageBox.ShowWarningAsync($"{assem.FullName}", "Button clicked");
            ////await VS.MessageBox.ShowWarningAsync($"{assem2.FullName}", "Button clicked");
            ////await VS.MessageBox.ShowWarningAsync($"{assem3.FullName}", "Button clicked");

            ////Console.WriteLine(new AssertService().MakeAssert(targetObject, nameof(targetObject)));

            ////if (!string.IsNullOrWhiteSpace(result))
            ////{
            ////    // 成功したら、カーソルの所に挿入
            ////    var docView = await VS.Documents.GetActiveDocumentViewAsync();
            ////    var selection = docView.TextView.Selection;
            ////    docView.TextBuffer.Delete(selection.SelectedSpans[0].Span);
            ////    docView.TextBuffer.Insert(selection.Start.Position, result);
            ////}
            #endregion
        }
    }
}
