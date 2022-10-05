using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GinpayFactory.Enums;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json.Linq;

namespace GinpayFactory.Services
{
    // 以下の条件を持った要素をDIを登録しているメソッドとする。
    // ・ServiceCollectionまたはIServiceCollectionをパラメータに持つメソッド
    // ・Ioc.Default.ConfigureServicesを呼んでいるメソッド
    // 今のところこれで十分なので、他の条件があれば必要に応じて随時追加

    /// <summary>
    /// DIを行っているクラスとメソッドを探すためのWalker
    /// </summary>
    public class DiWalker : SyntaxWalker
    {
        /// <summary>
        /// DIを行っているクラス名
        /// </summary>
        public string DiClassName { get; set; }

        /// <summary>
        /// DIを行っているメソッド名
        /// コンストラクタで行っている場合はクラス名と同じになる
        /// </summary>
        public string DiMethodName { get; set; }

        /// <summary>
        /// DI登録している所のソースコード
        /// </summary>
        public string DiSourceCode { get; set; }

        /// <summary>
        /// DI登録している所の記述範囲
        /// </summary>
        public TextSpan DiSourceSpan { get; set; }

        /// <summary>
        /// DI登録の形式
        /// </summary>
        public DiLibrary DiPattern { get; set; } = DiLibrary.HostedCommunityToolkit;

        /// <summary>
        /// DI登録している所のソースコードのType
        /// 要らないかも。
        /// </summary>
        public Type DiSourceType { get; set; }

        /// <summary>
        /// 各ノードを辿り、上に書かれた条件の要素を探してメソッド名とクラス名を記憶する
        /// </summary>
        /// <param name="node"></param>
        public override void Visit(SyntaxNode node)
        {
            if (node == null) return;

            // ServiceCollectionまたはIServiceCollectionをパラメータに持つメソッド（コンストラクタ）を探す

            if (node.GetType().Name == nameof(ConstructorDeclarationSyntax))
            {
                // コンストラクタの場合
                var constructor = node as ConstructorDeclarationSyntax;
                FindServiceCollectionParameter(constructor.ParameterList.Parameters, constructor.Parent as ClassDeclarationSyntax, constructor.Identifier.Text, constructor.Body.ToFullString(), constructor.FullSpan, node.GetType());
            }
            else if (node.GetType().Name == nameof(MethodDeclarationSyntax))
            {
                // メソッド定義の場合
                var method = node as MethodDeclarationSyntax;
                FindServiceCollectionParameter(method.ParameterList.Parameters, method.Parent as ClassDeclarationSyntax, method.Identifier.Text, method.Body.ToFullString(), method.FullSpan, node.GetType());
            }
            else if (node.GetType().Name == nameof(MemberAccessExpressionSyntax))
            {
                // Ioc.Default.ConfigureServicesを探す
                var member = node as MemberAccessExpressionSyntax;
                if (node.ToString().Trim() == "Ioc.Default.ConfigureServices")
                {
                    DiPattern = DiLibrary.CommunityToolkit;
                    SyntaxNode parent = member.Parent;
                    while (parent is not MethodDeclarationSyntax && parent is not ConstructorDeclarationSyntax || parent is null)
                    {
                        parent = parent.Parent;
                    }
                    if (parent is null)
                    {
                        // あり得ないはず
                    }
                    else if (parent.GetType().Name == nameof(ConstructorDeclarationSyntax))
                    {
                        // コンストラクタの場合
                        var con = parent as ConstructorDeclarationSyntax;
                        DiMethodName = DiClassName = con.Identifier.Text;
                        DiSourceCode = con.Body.ToFullString();
                        DiSourceSpan = con.FullSpan;
                        DiSourceType = con.GetType();
                    }
                    else if (parent.GetType().Name == nameof(MethodDeclarationSyntax))
                    {
                        // メソッド定義の場合
                        var mtd = parent as MethodDeclarationSyntax;
                        var cls = mtd.Parent as ClassDeclarationSyntax;
                        DiClassName = cls.Identifier.Text;
                        DiMethodName = mtd.Identifier.Text;
                        DiSourceCode = mtd.Body.ToFullString();
                        DiSourceSpan = mtd.FullSpan;
                        DiSourceType = mtd.GetType();
                    }
                }
            }

            base.Visit(node);
        }

        /// <summary>
        /// 引数にIServiceCollectionがあるかを確認する。
        /// あればそのメソッドをDI登録メソッドとして記憶する。
        /// </summary>
        /// <param name="Parameters"></param>
        /// <param name="classSyntax"></param>
        /// <param name="IdentifierText"></param>
        private void FindServiceCollectionParameter(SeparatedSyntaxList<ParameterSyntax> Parameters, ClassDeclarationSyntax classSyntax, string IdentifierText, string source, TextSpan span, Type type)
        {
            foreach (var parameter in Parameters)
            {
                // Type名を取得：クラスかインタフェースかは分からない
                // ソース解析だと名前しか分からないので、継承とかやっている場合は諦める。
                var typeName = parameter.Type.GetText().ToString().Trim();

                if (typeName == nameof(ServiceCollection) || typeName == nameof(IServiceCollection))
                {
                    DiClassName = classSyntax.Identifier.Text;
                    DiMethodName = IdentifierText;
                    DiSourceCode = source;
                    DiSourceSpan = span;
                    DiSourceType = type;
                }
            }
        }

        /// <summary>
        /// DIを行なっているクラスとメソッドを見つける
        /// なかったらnullを返す
        /// </summary>
        /// <param name="path">csファイルのパス</param>
        /// <returns>クラス名, メソッド名</returns>
        public MethodData FindDiClass(string path)
        {
            DiClassName = null;
            DiMethodName = null;

            // 対象のソースコードを読み込む
            var source = File.ReadAllText(path);

            // シンタックス ツリーに変換してルートのノードを取得する
            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            var rootNode = syntaxTree.GetRoot();

            Visit(rootNode);

            return DiClassName == null ? null : new MethodData
            {
                Path = path,
                ClassName = DiClassName,
                MethodName = DiMethodName,
                Span = DiSourceSpan,
                SourceCode = DiSourceCode,
                Type = DiSourceType
            };
        }
    }
}
