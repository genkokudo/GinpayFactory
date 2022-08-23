global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using System.Runtime.InteropServices;
using System.Threading;

namespace GinpayFactory
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideToolWindow(typeof(GenkokuWindow.Pane), Style = VsDockStyle.Tabbed, Window = WindowGuids.SolutionExplorer)]
    [ProvideOptionPage(typeof(OptionsProvider.GinpayOptionOptions), "GinpayFactory", "GinpayOption", 0, 0, true, SupportsProfiles = true)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.GinpayFactoryString)]
    public sealed class GinpayFactoryPackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // [Command]を実装したCommandクラスに対して、自動的にInitializeAsyncを呼んでくれる。
            await this.RegisterCommandsAsync();

            // 作成したウィンドウを登録する
            this.RegisterToolWindows();
        }
    }
}