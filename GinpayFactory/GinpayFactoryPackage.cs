global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using CommunityToolkit.Mvvm.DependencyInjection;
using GinpayFactory.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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

            // 設定項目の読み込み
            var general = await GinpayOption.GetLiveInstanceAsync();
            var deeplOption = new DeeplOption { ApiKey = general.ApiKey };
            var deeplOption2 = Options.Create(deeplOption);     // Servicesは使い回せるようにIOptionsにしたい。これでいい？

            GinpayOption.Saved += GinpayOption_Saved;

            // 作成したサービスをDIできるように登録する
            // appsettings.jsonではないので、AddOptionsは使用しない
            Ioc.Default.ConfigureServices(new ServiceCollection()
                        .AddTransient<IDeeplService, DeeplService>()
                        .AddTransient<GenkokuWindowControl>()
                        .AddSingleton(deeplOption2)
                        .BuildServiceProvider());

        }

        private void GinpayOption_Saved(GinpayOption obj)
        {
            Ioc.Default.GetService<IOptions<DeeplOption>>().Value.ApiKey = obj.ApiKey;  // 更新
        }
    }
}