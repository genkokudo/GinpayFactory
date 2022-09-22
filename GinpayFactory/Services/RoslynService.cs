using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Shapes;

namespace GinpayFactory.Services
{
    /// <summary>
    /// ソース解析関係の処理を定義する
    /// </summary>
    public interface IRoslynService
    {
        // IOptionの登録は？

        /// <summary>
        /// .csソースからサービスか、インタフェースを探し
        /// そのサービス名を取得する。
        /// "～～Service"というネーミング以外は無効とする。
        /// </summary>
        /// <param name="path"></param>
        /// <returns>見つかったサービス名全て</returns>
        public IEnumerable<string> GetServiceClassNames(string path);

        /// <summary>
        /// ソース内のクラス名を全て取得する
        /// </summary>
        /// <param name="path"></param>
        /// <returns>見つかったクラス名全て</returns>
        public IEnumerable<string> GetAllClassNames(string path);
    }

    public class RoslynService : IRoslynService
    {
        // 解析用コンパイラで参照するdll
        // ぶっちゃけよく分かってない。おまじない。
        // 多分、コンパイルエラーが出たらtypeof(クラス名).Assembly.Locationみたいな感じでdll参照増やしていく感じだと思う。
        static readonly PortableExecutableReference[] references = new[]{
            // microlib.dll
            // intは内部的にはSystem.Int32を利用している。
            // メタリファレンスは何も指定しないとSystem.Int32等がインポートされていない。
            // コンパイルエラーを回避するため、objectクラスが属するアセンブリをメタリファレンスに指定しておく。
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            // System.dll
            MetadataReference.CreateFromFile(typeof(ObservableCollection<>).Assembly.Location),
            // System.Core.dll
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
        };

        // 1ファイルごとに解析しているので、別ファイルに定義したものは拾えない事に注意
        /// <summary>
        /// ソース解析に必要なものをまとめたクラス
        /// </summary>
        private class CSharpAnalysis
        {
            public SyntaxTree SyntaxTree { get; set; }
            public CSharpCompilation Compilation { get; set; }
            public SemanticModel SemanticModel { get; set; }
            public IEnumerable<SyntaxNode> Nodes { get; set; }

            /// <summary>
            /// 解析する.csのフルパス
            /// </summary>
            /// <param name="fullpath"></param>
            public CSharpAnalysis(string fullpath)
            {
                SyntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(fullpath));
                Compilation = CSharpCompilation.Create("sample", new SyntaxTree[] { SyntaxTree }, references);
                SemanticModel = Compilation.GetSemanticModel(SyntaxTree);
                Nodes = SyntaxTree.GetRoot().DescendantNodes();
            }
        }

        /// <summary>
        /// ソース内のクラス名をすべて取得
        /// </summary>
        /// <param name="analysis">ソース解析情報</param>
        /// <returns>ソース内の全てのクラス名</returns>
        private IEnumerable<string> GetClassNames(CSharpAnalysis analysis)
        {
            var result = new List<string>();

            // ノード群からクラスに関する構文情報群を取得
            var classSyntaxArray = analysis.Nodes.OfType<ClassDeclarationSyntax>();
            foreach (var syntax in classSyntaxArray)
            {
                var name = analysis.SemanticModel.GetDeclaredSymbol(syntax).Name;
                result.Add($"{name}");
            }

            return result.Distinct();   // 重複は除外
        }

        /// <summary>
        /// ソース内のインタフェース名をすべて取得
        /// </summary>
        /// <param name="analysis">ソース解析情報</param>
        /// <returns>ソース内の全てのインタフェース名</returns>
        private IEnumerable<string> GetInterfaceNames(CSharpAnalysis analysis)
        {
            var result = new List<string>();

            // ノード群からインタフェースに関する構文情報群を取得
            var interfaceSyntaxArray = analysis.Nodes.OfType<InterfaceDeclarationSyntax>();
            foreach (var syntax in interfaceSyntaxArray)
            {
                var name = analysis.SemanticModel.GetDeclaredSymbol(syntax).Name;
                // "先頭のIは取る"
                name = name.StartsWith("I") ? name.Remove(0, 1) : name;
                if (name.EndsWith("Service"))
                {
                    result.Add($"{name}");
                }
            }

            return result.Distinct();   // 重複は除外
        }

        public IEnumerable<string> GetServiceClassNames(string path)
        {
            var result = new List<string>();

            // ソース解析
            var analysis = new CSharpAnalysis(path);

            // クラス名を全て取得
            // 末尾がServiceでないものは除外
            result.AddRange(GetClassNames(analysis).Where(x => x.EndsWith("Service")));

            // インタフェース名を全て取得
            // 末尾がServiceでないものは除外
            // 先頭の"I"は取る
            result.AddRange(GetInterfaceNames(analysis).Select(x => x.StartsWith("I") ? x.Remove(0, 1) : x).Where(x => x.EndsWith("Service")));

            return result.Distinct();   // 重複は除外
        }

        public IEnumerable<string> GetAllClassNames(string path)
        {
            return GetClassNames(new CSharpAnalysis(path));
        }
    }
}
