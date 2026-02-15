using WocForC_.Models;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WocForC_.Service
{
    internal static class FileDatas
    {
        public static MyFilesTree MyFilesTreeData { get; } = new MyFilesTree();
        public static ObservableCollection<MyFilesTreeNode> TreeItems { get; } = new();
        public static MyFilesTreeNode? RootNode { get; private set; }

        public static string _choosedPath = string.Empty;
        public static async Task LoadFromPathAsync(string path)
        {
            RootNode = await MyFilesTreeData.BuildTreeAsync(path);

            TreeItems.Clear();
            foreach (var child in RootNode._children)
                TreeItems.Add(child);
        }
        public static async Task Clear(string path)
        {
            RootNode = null;
            TreeItems.Clear();
        }
    }
}