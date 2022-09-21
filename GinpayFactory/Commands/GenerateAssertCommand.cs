namespace GinpayFactory
{
    /// <summary>
    /// 指定したクラスのAssertコードを生成する。
    /// どうやって指定するんだろ。
    /// 生成したものはソースのどこに追加するんだろう。
    /// メッセージボックスからCtrl+Cって形が易しいかなあ。
    /// </summary>
    [Command(PackageIds.GenerateAssertCommand)]
    internal sealed class GenerateAssertCommand : BaseCommand<GenerateAssertCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await VS.MessageBox.ShowWarningAsync("GenerateAssertCommand", "Button clicked");
        }
    }
}
