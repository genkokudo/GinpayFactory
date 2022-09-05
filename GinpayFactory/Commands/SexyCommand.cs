﻿using Community.VisualStudio.Toolkit;
using CommunityToolkit.Mvvm.DependencyInjection;
using GinpayFactory.Services;
using Microsoft.VisualStudio.Text;
using System.Text.RegularExpressions;

namespace GinpayFactory
{
    [Command(PackageIds.SexyCommand)]
    internal sealed class SexyCommand : BaseCommand<SexyCommand>
    {
        /// <summary>
        /// 選択範囲に対して、DeepLで日本語に翻訳して挿入する
        /// 英語のみ
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            // 各コマンドの最初に必要？
            await Package.JoinableTaskFactory.SwitchToMainThreadAsync();
            
            // 出力
            OutputWindowPane pane = await VS.Windows.CreateOutputWindowPaneAsync("Ginpay");

            // 選択中のコメントをDeepLにかけたい。
            // コメントの斜線を排除する。

            // 選択範囲があれば実行
            var docView = await VS.Documents.GetActiveDocumentViewAsync();
            if (docView == null) return;
            if (docView.TextView == null) return;
            var selection = docView.TextView.Selection;

            if (!selection.IsEmpty)
            {
                // 選択範囲がある場合
                // 選択文字列を取得
                var selectedText = selection.SelectedSpans[0].GetText();

                // 斜線と改行を排除
                await pane.WriteLineAsync("原文:");
                await pane.WriteLineAsync(selectedText);
                selectedText = CleanupComment(selectedText);
                await pane.WriteLineAsync("整形後原文:");
                await pane.WriteLineAsync(selectedText);

                // 2バイト文字が入ってないか
                if (!IsOneByteChar(selectedText))
                {
                    await pane.WriteLineAsync("2バイト文字が入っているようなので翻訳しません。");
                    return;
                }

                // DeepL
                var deepl = Ioc.Default.GetService<IDeeplService>();
                var result = await deepl.TranslateAsync(selectedText);
                if (result == null)
                {
                    await pane.WriteLineAsync("DeepL API呼び出しに失敗しました。");
                    return;
                }
                await pane.WriteLineAsync("翻訳:");
                await pane.WriteLineAsync(result);

                // 成功したら、カーソルの所に挿入
                docView.TextBuffer.Delete(selection.SelectedSpans[0].Span);
                docView.TextBuffer.Insert(selection.Start.Position, result);
            }
        }

        /// <summary>
        /// コメント文の斜線と改行を排除
        /// </summary>
        /// <returns></returns>
        private string CleanupComment(string comment)
        {
            var result = comment
                .Replace("/*", "")
                .Replace("*/", "")
                .Replace("/", "")
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Replace("\n", " ")
                ;

            // 連続したスペースの削除
            var regex = new Regex(@"\s+");
            regex.Replace(result, " ");

            return result;
        }

        // これダメだったら正規表現で取るしかないね。
        /// <summary>
        /// 1バイト文字で構成された文字列であるか判定
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private bool IsOneByteChar(string str)
        {
            byte[] byte_data = System.Text.Encoding.GetEncoding(932).GetBytes(str);
            if (byte_data.Length == str.Length)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
