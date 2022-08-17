﻿using Microsoft.VisualStudio.Text;

namespace GinpayFactory
{
    [Command(PackageIds.SexyCommand)]
    internal sealed class SexyCommand : BaseCommand<SexyCommand>
    {
        /// <summary>
        /// 現在のエディタに対して、GUIDを採番して挿入する
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            // 各コマンドの最初に必要？
            await Package.JoinableTaskFactory.SwitchToMainThreadAsync();

            // 現在編集中のドキュメントを取得
            var docView = await VS.Documents.GetActiveDocumentViewAsync();
            if (docView?.TextView == null) return;

            // ドキュメントから現在のカーソル位置を取得
            var position = docView.TextView.Caret.Position.BufferPosition;

            // 対象のカーソル位置に文字列を挿入
            docView.TextBuffer?.Insert(position, Guid.NewGuid().ToString());
        }
    }
}
