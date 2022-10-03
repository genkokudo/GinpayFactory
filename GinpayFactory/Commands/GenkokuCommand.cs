namespace GinpayFactory
{
    /// <summary>
    /// WPFのWindowを開いてみるだけ。
    /// 特に何もできない。
    /// </summary>
    [Command(PackageIds.GenkokuCommand)]
    internal sealed class GenkokuCommand : BaseCommand<GenkokuCommand>
    {
        protected override Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            return GenkokuWindow.ShowAsync();
        }
    }
}
