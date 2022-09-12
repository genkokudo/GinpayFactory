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

            // ソリューション内の.csから、DI登録しているクラスを探す
            // CommunityToolkitのみ対応。
            await source.UpdateDiSourcePathAsync();
            
        }
    }
}
