using CommunityToolkit.Mvvm.DependencyInjection;
using GinpayFactory.Services;
using GinpayFactory.ViewModels;
using Microsoft.VisualStudio.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace GinpayFactory
{
    /// <summary>
    /// Windowを作成するシェルクラス
    /// </summary>
    public class GenkokuWindow : BaseToolWindow<GenkokuWindow>
    {
        public override string GetTitle(int toolWindowId) => "クラスにサービスを挿入する";

        public override Type PaneType => typeof(Pane);

        public override Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            return Task.FromResult<FrameworkElement>(new GenkokuWindowControl(Ioc.Default.GetService<GenkokuWindowControlViewModel>()));
        }

        // ユニークなGUIDを付けること
        [Guid("7bcddffd-6eed-43e0-b219-dccd119f4916")]
        internal class Pane : ToolkitToolWindowPane
        {
            public Pane()
            {
                // イメージアイコンを設定する
                BitmapImageMoniker = KnownMonikers.ToolWindow;
            }
        }
    }
}
