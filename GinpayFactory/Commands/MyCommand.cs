using CommunityToolkit.Mvvm.DependencyInjection;
using GinpayFactory.Services;
using Microsoft.VisualStudio.Text;
using System.IO;
using System.Linq;

namespace GinpayFactory
{
    /// <summary>
    /// 現在のプロジェクト内から
    /// DIするサービスを登録するクラスを探す
    /// 単純な文字列検索のため、コメントアウトしていても認識してしまう。
    /// 1度見つけたら記憶するため、VS再起動するまで変更は受け付けない。→見つからなかったら再検索するように改善する？
    /// </summary>
    [Command(PackageIds.MyCommand)]
    internal sealed class MyCommand : BaseCommand<MyCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            // DI
            var source = Ioc.Default.GetService<ISourceService>();
            var roslyn = Ioc.Default.GetService<IRoslynService>();

            // ソリューション内の.csから、DI登録しているクラスを探す
            await source.UpdateDiSourcePathAsync();

            // 現在のソースが.csであることを確認する
            if (!await source.CheckCurrentSourceIsCSharpAsync())
            {
                // .csではない
                return;
            }

            // ドキュメントからサービス名か、インタフェース名を取得（Roslynかなあ…。）

            //docView.FilePath

            // 現在のソースがServiceまたはIServiceの場合、上記で見つけた箇所にAddTransientを追加する
            // .AddTransient<ISourceService, SourceService>()
            // (AddSingletonは別コマンド)

            // ".BuildServiceProvider()"という行の上に挿入。スペースの数もここと一緒にする。

        }
    }
}
