using CommunityToolkit.Mvvm.DependencyInjection;
using GinpayFactory.Enums;
using GinpayFactory.Services;

namespace GinpayFactory
{
    /// <summary>
    /// 現在のソリューション内から、DIするサービスを登録するクラスを探す。
    /// 1度見つけたら記憶するため、VS再起動するまでそのクラス変更は受け付けない。
    /// 
    /// 表示中のソース内の一般クラスを全て
    /// その登録する場所にDI登録するソースを追加する。
    /// </summary>
    [Command(PackageIds.AddClassCommand)]
    internal sealed class AddClassCommand : BaseCommand<AddClassCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            // クラスをAddTransientするコマンド
            var source = Ioc.Default.GetService<ISourceService>();
            await source.AddServiceAsync(DiSubmit.GeneralClass);
        }
    }
}
