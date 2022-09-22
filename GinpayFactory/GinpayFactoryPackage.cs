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

            // 設定項目を読み込む
            // Servicesを使い回せるようにIOptionsにする
            var general = await GinpayOption.GetLiveInstanceAsync();
            var deeplOption = Options.Create(new DeeplOption
            {
                ApiKey = general.ApiKey
            });
            var diOption = Options.Create(new DiOption
            {
                DiLibrary = general.DiLibrary,
                DiServicesName = general.DiServicesName
            });

            // 作成したサービスをDIできるように登録する
            // appsettings.jsonではないので、AddOptionsは使用しない
            Ioc.Default.ConfigureServices(new ServiceCollection()
                    .AddTransient<IDeeplService, DeeplService>()
                    .AddSingleton<ISourceService, SourceService>()
                    .AddTransient<IRoslynService, RoslynService>()
                    .AddTransient<GenkokuWindowControl>()
                    .AddSingleton(deeplOption)
                    .AddSingleton(diOption)
                    .BuildServiceProvider()
            );

            // VSの設定変更時にイベント処理で反映させる
            GinpayOption.Saved += (GinpayOption obj) =>
            {
                Ioc.Default.GetService<IOptions<DeeplOption>>().Value.ApiKey = obj.ApiKey;
                Ioc.Default.GetService<IOptions<DiOption>>().Value.DiLibrary = obj.DiLibrary;
                Ioc.Default.GetService<IOptions<DiOption>>().Value.DiServicesName = obj.DiServicesName;
            };
        }

    }
}