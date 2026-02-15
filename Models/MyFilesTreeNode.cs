using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace WocForC_.Models
{
    public class MyFilesTreeNode
    {
        public string _name { get; set; } = string.Empty;
        public string _path { get; set; } = string.Empty;
        public string _type { get; set; } = string.Empty;
        public StorageFile? _file { get; set; } = null;
        public StorageFolder? _folder { get; set; } = null;

        public ObservableCollection<MyFilesTreeNode> _children = new ObservableCollection<MyFilesTreeNode>();
        public MyFilesTreeNode? _parent { get; set; } = null;
        public bool _isFolder { get; set; } = false;
        public MyFilesTreeNode()
        {
            _children.Clear();
        }
        public MyFilesTreeNode(string name, string path, string type, bool isFolder)
        {
            _name = name;
            _path = path;
            _type = type;
            _isFolder = isFolder;
            _children.Clear();
        }
        public async Task RenameNode(string newName)
        {
            if (_isFolder && _folder != null)
            {
                await _folder.RenameAsync(newName);
                _name = newName;
            }
            else if (!_isFolder && _file != null)
            {
                await _file.RenameAsync(newName);
                _name = newName;
            }
        }
        public async Task CopyNodeAsync(StorageFolder destinationFolder)
        {
            if (_isFolder && _folder != null)
            {
                foreach (var child in _children)
                {
                    await child.CopyNodeAsync(destinationFolder);
                }
            }
            else if (!_isFolder && _file != null)
            {
                await _file.CopyAsync(destinationFolder);
            }
        }
        public async Task MoveNodeAsync(StorageFolder destinationFolder)
        {
            if (_isFolder && _folder != null)
            {
                foreach (var child in _children)
                {
                    await child.MoveNodeAsync(destinationFolder);
                }
            }
            else if (!_isFolder && _file != null)
            {
                await _file.MoveAsync(destinationFolder); // 或用带 NameCollisionOption 的重载
            }
        }
        public async Task DeleteNodeAsync()
        {
            if (_isFolder && _folder != null)
            {
                await _folder.DeleteAsync();
            }
            else if (!_isFolder && _file != null)
            {
                await _file.DeleteAsync();
            }
            if (_parent != null)
            {
                _parent._children.Remove(this);
            }
        }
        public void addParent(MyFilesTreeNode parent)
        {
            _parent = parent;
        }
    }
}
