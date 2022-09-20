using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinpayFactory.Enums
{
    // Prism.Iocだと、Optionはこんな感じ。
    // IContainerRegistry containerRegistry
    // containerRegistry.RegisterInstance(typeof(IOptions<ParameterOptions>), Options.Create(new ParameterOptions { RegionName = RegionNames.ContentRegion }));

    // CommunityToolkitだと、同じくOptions.Createでインスタンスを作って、AddSingletonで追加。
    // または、こんな感じでやる。 AddOptions<MyOptions>().Configure<T1, T2, ...>((o, t1, t2, ...)=>{o.????});

    /// <summary>
    /// DI登録の種類
    /// {1}には、IHost版の場合に"services"を入れる
    /// </summary>
    public enum DiSubmit
    {
        /// <summary>
        /// Transient
        /// </summary>
        [StringValue("{1}.AddTransient<I{0}, {0}>(){2}")]
        Transient = 1,
        /// <summary>
        /// Singleton
        /// </summary>
        [StringValue("{1}.AddSingleton<I{0}, {0}>(){2}")]
        Singleton = 2,
        /// <summary>
        /// Option
        /// パターンが多岐に渡るため実装しない。
        /// やる場合はOptions.CreateしたオブジェクトをAddSingletonすること。
        /// </summary>
        [StringValue("")]
        Option = 3,
        /// <summary>
        /// その他一般的なクラスを登録
        /// WPFのViewやViewModelなど。
        /// </summary>
        [StringValue("{1}.AddTransient<{0}>(){2}")]
        GeneralClass = 4
    }

}
