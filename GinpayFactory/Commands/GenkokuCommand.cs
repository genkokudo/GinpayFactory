namespace GinpayFactory
{
    [Command(PackageIds.GenkokuCommand)]
    internal sealed class GenkokuCommand : BaseCommand<GenkokuCommand>
    {
        protected override Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            return GenkokuWindow.ShowAsync();
        }
    }
}
