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
            await GenkokuWindow.ShowAsync();


        }
    }
}
