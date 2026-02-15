using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace WocForC_.Models
{
    internal class MyFilesTree
    {
        public MyFilesTree()
        {

        }
        public async Task<MyFilesTreeNode> BuildTreeAsync(string path)
        {
            StorageFolder _rootFolder = await StorageFolder.GetFolderFromPathAsync(path).AsTask();
            MyFilesTreeNode rootNode = new MyFilesTreeNode()
            {
                _name = _rootFolder.Name,
                _path = _rootFolder.Path,
                _type = "Folder",
                _folder = _rootFolder,
                _isFolder = true
            };
            await BuildTreeRecursiveAsync(rootNode, _rootFolder);
            return rootNode;
        }
        public async Task BuildTreeRecursiveAsync(MyFilesTreeNode currentNode, StorageFolder currentFolder)
        {
            var items = await currentFolder.GetItemsAsync();
            foreach (var item in items)
            {
                if (item is StorageFolder folder)
                {
                    MyFilesTreeNode folderNode = new MyFilesTreeNode
                    {
                        _name = folder.Name,
                        _path = folder.Path,
                        _type = "Folder",
                        _folder = folder,
                        _isFolder = true
                    };
                    folderNode.addParent(currentNode);
                    currentNode._children.Add(folderNode);
                    await BuildTreeRecursiveAsync(folderNode, folder);
                }
                else if (item is StorageFile file)
                {
                    MyFilesTreeNode fileNode = new MyFilesTreeNode
                    {
                        _name = file.Name,
                        _path = file.Path,
                        _type = file.FileType,
                        _file = file,
                        _isFolder = false
                    };
                    fileNode.addParent(currentNode);
                    currentNode._children.Add(fileNode);
                }
            }
        }
    }
}
