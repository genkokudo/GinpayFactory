namespace GinpayFactory.Enums
{
    /// <summary>
    /// 使用しているDIライブラリ
    /// 検出の際の文字列をStringValueとして定義する
    /// その文字列の上の行に"AddTransient"を追加する考え。
    /// 
    /// Roslynでメソッドチェーンを処理するのが困難なため、この方法にする。
    /// </summary>
    public enum DiLibrary
    {
        /// <summary>
        /// CommunityToolkit
        /// Ioc.Default.ConfigureServicesを使ってDI登録するパターン
        /// </summary>
        [StringValue(".BuildServiceProvider()")] // "Ioc.Default.ConfigureServices"よりこっちの方がやりやすい
        CommunityToolkit = 1,
        /// <summary>
        /// IHost版
        /// CommunityToolkit
        /// </summary>
        [StringValue("services.AddTransient")]  // 結構乱暴な決め打ち
        HostedCommunityToolkit = 2,
    }
}
