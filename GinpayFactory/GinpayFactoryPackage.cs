using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace GinpayFactory
{
    /// <summary>
    /// このアセンブリが公開するパッケージを実装するクラスです。
    /// </summary>
    /// <remarks>
    /// <para>
    /// Visual Studio でクラスが有効なパッケージとみなされるための最小要件は、
    /// IVsPackage インターフェースを実装し、それ自体をシェルに登録することです。
    /// このパッケージは、Managed Package Framework(MPF)で定義されたヘルパークラスを使用しています。
    /// IVsPackageインターフェースの実装を提供するPackageクラスから派生し、
    /// フレームワークで定義された登録属性を使用して自分自身とそのコンポーネントをシェルに登録します。
    /// これらの属性は、pkgdef作成ユーティリティに.pkgdefファイルにどのデータを格納すれば良いかを指示します。
    /// </para>
    /// <para>
    /// VSに読み込まれるためには、.vsixmanifestファイルのAsset Type="Microsoft.VisualStudio.VsPackage" ...&gt; でパッケージが参照される必要があります。
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuidString)]
    public sealed class GinpayFactoryPackage : AsyncPackage
    {
        /// <summary>
        /// GinpayFactoryPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "6bc4622b-ca50-42d7-9ab8-80f6764254d9";

        #region Package Members

        /// <summary>
        /// パッケージの初期化; このメソッドはパッケージが設置された直後に呼び出されるので、VisualStudioが提供するサービスに依存するすべての初期化コードを置くことができる場所です。
        /// </summary>
        /// <param name="cancellationToken">VSのシャットダウン時に発生する初期化キャンセルを監視するためのキャンセルトークン。</param>
        /// <param name="progress">進行状況の更新を行うプロバイダ。</param>
        /// <returns>パッケージ初期化の非同期作業を表すタスク、またはタスクがない場合は既に完了したタスク。このメソッドからNULLを返してはいけない。</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // 非同期で初期化する場合、この時点で現在のスレッドはバックグラウンドスレッドである可能性があります。
            // UIスレッドに切り替わった後、UIスレッドを必要とする初期化処理を行う。
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
        }

        #endregion
    }
}
