namespace GinpayFactory.Enums
{
    /// <summary>
    /// 使用しているDIライブラリ
    /// 検出の際の文字列をStringValueとして定義する
    /// その文字列の上の行に"AddTransient"を追加する考え。
    /// </summary>
    public enum DiLibrary
    {
        /// <summary>CommunityToolkit</summary>
        [StringValue(".BuildServiceProvider()")] // "Ioc.Default.ConfigureServices"よりこっちの方がやりやすい
        CommunityToolkit,
    }
}
