using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinpayFactory.Enums
{
    /// <summary> 
    /// Enumに文字列を付加するための属性クラス 
    /// </summary> 
    public class StringValueAttribute : Attribute
    {
        /// <summary> 
        /// Enum内にstring型の値を持たせる 
        /// </summary> 
        public string StringValue { get; protected set; }

        /// <summary> 
        /// stringの値を初期化 
        /// </summary> 
        /// <param name="value"></param> 
        public StringValueAttribute(string value)
        {
            StringValue = value;
        }
    }

    /// <summary> 
    /// Enumに対して拡張メソッドを追加する 
    /// </summary> 
    public static class EnumExtension
    {
        /// <summary> 
        /// Enum値が渡されたらstringの値を取得する 
        /// これはEnum値にStringの値を設定したときのみ動作する 
        /// </summary> 
        /// <param name="value"></param> 
        /// <returns></returns> 
        public static string GetStringValue(this Enum value)
        {
            Type type = value.GetType();

            // この型のフィールド情報を取得 
            System.Reflection.FieldInfo fieldInfo = type.GetField(value.ToString());

            // 範囲外の値チェック 
            if (fieldInfo == null) return null;

            // フィールド情報から、目的の属性を探す 
            StringValueAttribute[] attribs = fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];

            // 属性があればその設定値を返却する 
            // 複数あっても最初のものだけ返却する 
            return attribs.Length > 0 ? attribs[0].StringValue : null;

        }
    }
}
