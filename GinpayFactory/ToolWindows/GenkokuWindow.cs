﻿using Microsoft.VisualStudio.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace GinpayFactory
{
    /// <summary>
    /// Windowを作成するシェルクラス
    /// </summary>
    public class GenkokuWindow : BaseToolWindow<GenkokuWindow>
    {
        public override string GetTitle(int toolWindowId) => "謎の機能を持つWindow";

        public override Type PaneType => typeof(Pane);

        public override Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            return Task.FromResult<FrameworkElement>(new GenkokuWindowControl());
        }

        // ユニークなGUIDを付けること
        [Guid("7bcddffd-6eed-43e0-b219-dccd119f4916")]
        internal class Pane : ToolkitToolWindowPane
        {
            public Pane()
            {
                // イメージアイコンを設定する
                BitmapImageMoniker = KnownMonikers.ToolWindow;
            }
        }
    }
}