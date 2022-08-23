using System.ComponentModel;
using System.Runtime.InteropServices;

namespace GinpayFactory
{
    internal partial class OptionsProvider
    {
        [ComVisible(true)]
        public class GinpayOptionOptions : BaseOptionPage<GinpayOption> { }
    }

    public class GinpayOption : BaseOptionModel<GinpayOption>
    {
        [Category("ぎんぺーの設定")]
        [DisplayName("DeepL API Key")]
        [Description("DeepLのAPIキーを設定する。")]
        [DefaultValue("DeeplApiKey")]
        public string ApiKey { get; set; } = "DeeplApiKey";

        [Category("ぎんぺーの設定")]
        [DisplayName("Like")]
        [Description("好きな言葉を適当に設定。意味のない項目。")]
        [DefaultValue("いちご")]
        public string Like { get; set; } = "いちご";

        [Category("追加の設定項目")]
        [DisplayName("秘密の名前")]
        [Description("意味のない項目。")]
        [DefaultValue("ぎんぺー")]
        public string HandleName { get; set; } = "ぎんぺー";

        [Category("追加の設定項目")]
        [DisplayName("作者の事が好きか")]
        [Description("意味のない項目。お前は俺が好きか？")]
        [DefaultValue(true)]
        public bool IsLikeGinpay { get; set; } = true;
    }
}
