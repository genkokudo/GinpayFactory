using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

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
        
        public IEnumerable<string> GetServiceClassNames(string path)
        {
            var result = new List<string>();

            // 1ファイルごとに解析しているので、別ファイルに定義したものは拾えない事に注意
            var syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(path));
            var compilation = CSharpCompilation.Create("sample", new SyntaxTree[] { syntaxTree }, references);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var nodes = syntaxTree.GetRoot().DescendantNodes();

            // ノード群からクラスに関する構文情報群を取得
            var classSyntaxArray = nodes.OfType<ClassDeclarationSyntax>();
            foreach (var syntax in classSyntaxArray)
            {
                var name = semanticModel.GetDeclaredSymbol(syntax).Name;
                if (name.EndsWith("Service"))
                {
                    result.Add($"{name}");
                }
            }

            // ノード群からインタフェースに関する構文情報群を取得
            var interfaceSyntaxArray = nodes.OfType<InterfaceDeclarationSyntax>();
            foreach (var syntax in interfaceSyntaxArray)
            {
                var name = semanticModel.GetDeclaredSymbol(syntax).Name;
                // "先頭のIは取る"
                name = name.StartsWith("I") ? name.Remove(0, 1) : name;
                if (name.EndsWith("Service"))
                {
                    result.Add($"{name}");
                }
            }

            return result.Distinct();   // 重複は除外
        }
    }
}
