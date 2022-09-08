using Microsoft.VisualStudio.Text;
using System.Linq;

namespace GinpayFactory
{
    [Command(PackageIds.MyCommand)]
    internal sealed class MyCommand : BaseCommand<MyCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            // アクティブなテキスト ビューを取得する
            DocumentView docView = await VS.Documents.GetActiveDocumentViewAsync();
            if (docView?.TextView == null) return; //not a text window
            SnapshotPoint position = docView.TextView.Caret.Position.BufferPosition;
            docView.TextBuffer?.Insert(position, "some text"); // Inserts text at the caret

            //// 含まれているファイルからプロジェクトを取得する
            //string fileName = "c:\\file\\in\\project.txt";
            //PhysicalFile item = await PhysicalFile.FromFileAsync(fileName);
            //Project project = item.ContainingProject;

            // ソリューション内のすべてのプロジェクトを取得する
            var projects = await VS.Solutions.GetAllProjectsAsync();
            var project = projects.First();
            foreach (var child in project.Children)
            {
                if (child.Type == SolutionItemType.PhysicalFile)
                {
                    // これでファイルが取れる。
                    Console.WriteLine(child.FullPath);
                }
                if (child.Type == SolutionItemType.PhysicalFolder)
                {
                }
            }
            
            var references = project.References;        // 謎。Nugetの参照とか取れるわけではない。
            var reference = references.FirstOrDefault();

            // プロジェクトは取れるけど、名前の取得とか、ファイルの追加とかぐらいしか出来ないっぽい。

            Console.WriteLine();
        }
    }
}
