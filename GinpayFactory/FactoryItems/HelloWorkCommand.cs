using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace GinpayFactory.FactoryItems
{
    /// <summary>
    /// ツールメニューからHelloWorkを選んだ時のコマンドハンドラー
    /// </summary>
    internal sealed class HelloWorkCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("7935a59d-f684-408d-a9f5-6c4b87412552");

        /// <summary>
        /// このコマンドを提供するパッケージVS Package。NULLではありません。
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// <see cref="HelloWorkCommand"/> クラスの新しいインスタンスを初期化します。
        /// メニューのコマンドハンドラを追加（コマンドは、コマンドテーブルファイルに存在する必要があります）
        /// </summary>
        /// <param name="package">オーナーズパッケージ、NULLではありません。</param>
        /// <param name="commandService">コマンドを追加するコマンドサービス（NULL不可）</param>
        private HelloWorkCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// コマンドのインスタンスを取得します。
        /// </summary>
        public static HelloWorkCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// オーナーパッケージからサービス提供者を取得する。
        /// </summary>
        private IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return package;
            }
        }

        /// <summary>
        /// コマンドのシングルトンインスタンスを初期化します。
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // メインスレッドに切り替える - HelloWorkCommandのコンストラクタ内のAddCommandの呼び出しは、UIスレッドを必要とします。
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new HelloWorkCommand(package, commandService);
        }

        /// <summary>
        /// この関数は、メニュー項目がクリックされたときにコマンドを実行するために使用されるコールバックです。
        /// OleMenuCommandServiceサービスとMenuCommandクラスを使って、どのようにメニュー項目とこの関数が関連付けられているかは、コンストラクタを参照してください。
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            // UIスレッドで呼び出されているかどうかを判断し、そうでない場合はCOMException(RPC_E_WRONG_THREAD)を投げる。
            ThreadHelper.ThrowIfNotOnUIThread();

            // タイトルと表示メッセージ
            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            string title = "HelloWork!!";

            // メッセージボックスを表示する
            VsShellUtilities.ShowMessageBox(
                package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
