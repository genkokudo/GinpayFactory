﻿using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Shapes;

namespace GinpayFactory.Services
{
    /// <summary>
    /// ソース解析関係の処理を定義する
    /// </summary>
    public interface IRoslynService
    {
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

        /// <summary>
        /// ソース内でDI登録されているクラスとメソッドを探す。
        /// その部分のSpanとソースコードも取得する。
        /// </summary>
        /// <param name="path"></param>
        /// <returns>無ければnull</returns>
        public MethodData FindDiMethod(string path);

        /// <summary>
        /// ソース内のクラスとそのSpanを全て取得する
        /// </summary>
        /// <param name="path"></param>
        /// <returns>見つかったクラス名とSpan、それ以外のフィールドはnull</returns>
        public IEnumerable<MethodData> GetAllClasses(string path);

        // TODO:実装すること
        /// <summary>
        /// クラスのソースコードに指定したサービスをDIしたソースコードを生成する。
        /// サービス名の頭に"I"を追加したインタフェースとしてDIされる。
        /// </summary>
        /// <param name="serviceNames">DIするサービス名</param>
        /// <returns>DI後のソースコード</returns>
        public string AddInjection(string source, IEnumerable<string> serviceNames);
    }

    /// <summary>
    /// Roslynで探し出したクラス内のメソッドについての情報
    /// </summary>
    public class MethodData
    {
        /// <summary>ソースのパス</summary>
        public string Path { get; set; }
        /// <summary>クラス名</summary>
        public string ClassName { get; set; }
        /// <summary>メソッド名</summary>
        public string MethodName { get; set; }

        /// <summary>記述範囲</summary>
        public TextSpan Span { get; set; }
        /// <summary>ソースコード</summary>
        public string SourceCode { get; set; }
        /// <summary>ソースコードのType</summary>
        public Type Type { get; set; }
    }

    public class RoslynService : IRoslynService
    {
        // 解析用コンパイラで参照するdll
        // ぶっちゃけよく分かってない。おまじない。
        // 多分、コンパイルエラーが出たらtypeof(クラス名).Assembly.Locationみたいな感じでdll参照増やしていく感じだと思う。
        private static readonly PortableExecutableReference[] references = new[]{
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

        /// <summary>
        /// DIを探すクラス
        /// </summary>
        private static DiWalker DiWalker { get; set; }

        // 1ファイルごとに解析しているので
        // そのファイル内で参照しているクラスの内、別ファイルに定義したもの等は拾えない事に注意
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

        public MethodData FindDiMethod(string path)
        {
            // 無ければ作成
            DiWalker ??= new DiWalker();

            var classmethod = DiWalker.FindDiClass(path);
            return classmethod;
        }

        public string AddInjection(string source, IEnumerable<string> serviceNames)
        {
            // クラスのソースコードに指定したサービスをDIしたソースコードを生成する。
            // サービス名の頭に"I"を追加したインタフェースとしてDIされる。
            throw new NotImplementedException();
        }

        public IEnumerable<MethodData> GetAllClasses(string path)
        {
            var result = new List<MethodData>();
            var analysis = new CSharpAnalysis(path);

            // ノード群からクラスに関する構文情報群を取得
            var classSyntaxArray = analysis.Nodes.OfType<ClassDeclarationSyntax>();
            foreach (var syntax in classSyntaxArray)
            {
                var name = analysis.SemanticModel.GetDeclaredSymbol(syntax).Name;
                result.Add(new MethodData
                {
                    ClassName = name,
                    Span = syntax.Span,
                    SourceCode = syntax.ToString()
                });
            }
            return result;
        }
    }
}
