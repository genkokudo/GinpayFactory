namespace GinpayFactory.Enums
{
    /// <summary>
    /// 使用しているDIライブラリ
    /// 検出の際の文字列をStringValueとして定義する
    /// </summary>
    public enum DiLibrary
    {
        /// <summary>CommunityToolkit</summary>
        [StringValue("Ioc.Default.ConfigureServices")]
        CommunityToolkit,
    }
}
