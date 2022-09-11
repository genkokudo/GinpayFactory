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
        private static string DiPath = null;    // TODO:これはシングルトンサービスに持たせないと他から呼べない。新しくサービスを作成すること。
        
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(DiPath))
            {
                return;
            }
            // DI
            var source = Ioc.Default.GetService<ISourceService>();

            // ソリューション内の.csファイルのフルパスを全て取得
            var csList = await source.GetSourcePathListAsync();

            // .csから、DI登録しているクラスを探す
            // CommunityToolkitのみ対応。
            foreach (var cs in csList)
            {
                var text = File.ReadAllText(cs);
                if (text.Contains("Ioc.Default.ConfigureServices"))     // TODO:EnumのSwitchにしてオプション化すること。Enumは表示と値を持たせること。
                {
                    // 見つかったら覚えておく
                    DiPath = cs;
                    break;
                }
            }
            

            
        }
    }
}
