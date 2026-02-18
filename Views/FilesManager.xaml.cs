using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.Windows.Storage.Pickers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using WocForC_.Models;
using WocForC_.Service;
using WocForC_.Views;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WocForC_.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>


    public sealed partial class FilesManager : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<MyFilesTreeNode>? DataSource { get; set; }

        private MyFilesTreeNode? _selectedNode;
        private MyFilesTreeNode? _addNode;
        private MyFilesTreeNode? _toNode;
        public MyFilesTreeNode? SelectedNode
        {
            get => _selectedNode;
            private set
            {
                if (ReferenceEquals(_selectedNode, value))
                    return;

                _selectedNode = value;
                OnPropertyChanged();
            }
        }
        public FilesManager()
        {
            InitializeComponent();
        }
        private async void NewManageButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                // disable the button to avoid double-clicking
                button.IsEnabled = false;

                var picker = new FolderPicker(button.XamlRoot.ContentIslandEnvironment.AppWindowId);

                picker.CommitButtonText = "选取文件夹";
                picker.SuggestedStartLocation = PickerLocationId.Desktop;
                picker.ViewMode = PickerViewMode.List;

                // Show the picker dialog window
                var folder = await picker.PickSingleFolderAsync();
                if (folder != null)
                {
                    FileDatas._choosedPath = folder.Path;
                    // Update the history with the selected folder
                    await Historys.AddHistory(folder.Path);
                    // Update the myFilesTreeData with the selected folder
                    await FileDatas.LoadFromPathAsync(folder.Path);
                }
                // re-enable the button
                button.IsEnabled = true;
            }
        }
        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(HistoryView));
        }
        private async void Refrech_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                // disable the button to avoid double-clicking
                button.IsEnabled = false;
                if (FileDatas._choosedPath != string.Empty)
                    await FileDatas.LoadFromPathAsync(FileDatas._choosedPath);
                // re-enable the button
                button.IsEnabled = true;
            }
        }
        private async void DeletePickedFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                // disable the button to avoid double-clicking
                button.IsEnabled = false;
                var picker = new FolderPicker(button.XamlRoot.ContentIslandEnvironment.AppWindowId);
                picker.CommitButtonText = "选取文件夹";
                picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                picker.ViewMode = PickerViewMode.List;
                // Show the picker dialog window
                var folder = await picker.PickSingleFolderAsync();
                if (folder != null)
                {
                    // Update the myFilesTreeData by removing the selected folder
                    await FileDatas.Clear(folder.Path);
                }
                // re-enable the button
                button.IsEnabled = true;
            }
        }
        private void FilesTreeView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs e)
        {
            SelectedNode = e.InvokedItem as MyFilesTreeNode;
        }
        private async void FilesTreeView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var treeViewItem = FindAncestor<TreeViewItem>(e.OriginalSource as DependencyObject);
            if (treeViewItem?.DataContext is MyFilesTreeNode node)
            {
                SelectedNode = node;
                if (SelectedNode._isFolder)
                {
                    var folder = await StorageFolder.GetFolderFromPathAsync(SelectedNode._path);
                    await Launcher.LaunchFolderAsync(folder);
                }
                else
                {
                    var file = await StorageFile.GetFileFromPathAsync(SelectedNode._path);
                    await Launcher.LaunchFileAsync(file);

                }
            }
            e.Handled = true;
        }
        private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T target) return target;
                current = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        private async void Open_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedNode is null) return;
            if (SelectedNode._isFolder)
            {
                var folder = await StorageFolder.GetFolderFromPathAsync(SelectedNode._path);
                await Launcher.LaunchFolderAsync(folder);
            }
            else
            {
                var file = await StorageFile.GetFileFromPathAsync(SelectedNode._path);
                await Launcher.LaunchFileAsync(file);
            }
        }
        private async void Rename_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedNode is null) return;
                // 弹出 ContentDialog 输入新名字，然后修改 SelectedNode.Name
                var input = new TextBox
                {
                    PlaceholderText = "请输入名称",
                    MinWidth = 320
                };
                var dialog = new ContentDialog
                {
                    XamlRoot = this.Content.XamlRoot,
                    Title = "重命名",
                    Content = input,
                    PrimaryButtonText = "重命名",
                    SecondaryButtonText = "取消",
                    DefaultButton = ContentDialogButton.Primary
                };

                dialog.PrimaryButtonClick += (s, e) =>
                {
                    if (string.IsNullOrWhiteSpace(input.Text))
                    {
                        e.Cancel = true;
                        input.Focus(FocusState.Programmatic);
                    }
                };

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    await SelectedNode.RenameNode(input.Text);
                }
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                string msg = ex.HResult switch
                {
                    unchecked((int)0x800700B7) => "目标文件已存在（同名冲突），请单击刷新键。",
                    unchecked((int)0x80070020) => "文件正在被占用（可能被资源管理器/预览窗格/杀毒或本程序占用）。",
                    unchecked((int)0x80070005) => "无权限/访问被拒绝。",
                    _ => $"移动失败：0x{ex.HResult:X8} {ex.Message}"
                };

                var dialog = new ContentDialog
                {
                    XamlRoot = this.Content.XamlRoot,
                    Title = "错误",
                    Content = msg,
                    PrimaryButtonText = "确认",
                    DefaultButton = ContentDialogButton.Primary
                };

                await dialog.ShowAsync();
                return;
            }
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedNode is null) return;
            if (SelectedNode._parent != null)
            {
                try
                {
                    await SelectedNode.DeleteNodeAsync();
                }
                catch (System.IO.FileNotFoundException ex)
                {
                    string msg = ex.HResult switch
                    {
                        unchecked((int)0x800700B7) => "目标文件删除，请单击刷新键。",
                        unchecked((int)0x80070020) => "文件正在被占用（可能被资源管理器/预览窗格/杀毒或本程序占用）。",
                        unchecked((int)0x80070005) => "无权限/访问被拒绝。",
                        _ => $"删除失败：0x{ex.HResult:X8} {ex.Message}"
                    };

                    var dialog = new ContentDialog
                    {
                        XamlRoot = this.Content.XamlRoot,
                        Title = "错误",
                        Content = msg,
                        PrimaryButtonText = "确认",
                        DefaultButton = ContentDialogButton.Primary
                    };

                    await dialog.ShowAsync();
                    return;
                }
            }
        }

        private async void Copy_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedNode is null) return;
            // 弹出选择文件夹对话框，调用 SelectedNode.CopyNodeAsync(destinationFolder);
            await ChooseFolderAsync(sender, e);
            if (_toNode is null) return;
            try
            {
                await SelectedNode.CopyNodeAsync(await StorageFolder.GetFolderFromPathAsync(_toNode._path));
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                string msg = ex.HResult switch
                {
                    unchecked((int)0x800700B7) => "目标文件已存在（同名冲突），请单击刷新键。",
                    unchecked((int)0x80070020) => "文件正在被占用（可能被资源管理器/预览窗格/杀毒或本程序占用）。",
                    unchecked((int)0x80070005) => "无权限/访问被拒绝。",
                    _ => $"移动失败：0x{ex.HResult:X8} {ex.Message}"
                };

                var dialog = new ContentDialog
                {
                    XamlRoot = this.Content.XamlRoot,
                    Title = "错误",
                    Content = msg,
                    PrimaryButtonText = "确认",
                    DefaultButton = ContentDialogButton.Primary
                };

                await dialog.ShowAsync();
                return;
            }

        }
        private async void Move_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedNode is null) return;
            // 弹出选择文件夹对话框，修改 SelectedNode 的路径
            await ChooseFolderAsync(sender, e);
            if (_toNode is null) return;
            try
            {
                await SelectedNode.MoveNodeAsync(await StorageFolder.GetFolderFromPathAsync(_toNode._path));
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                string msg = ex.HResult switch
                {
                    unchecked((int)0x800700B7) => "目标文件已存在（同名冲突），请单击刷新键。",
                    unchecked((int)0x80070020) => "文件正在被占用（可能被资源管理器/预览窗格/杀毒或本程序占用）。",
                    unchecked((int)0x80070005) => "无权限/访问被拒绝。",
                    _ => $"移动失败：0x{ex.HResult:X8} {ex.Message}"
                };

                var dialog = new ContentDialog
                {
                    XamlRoot = this.Content.XamlRoot,
                    Title = "错误",
                    Content = msg,
                    PrimaryButtonText = "确认",
                    DefaultButton = ContentDialogButton.Primary
                };

                await dialog.ShowAsync();
                return;
            }
        }

        private async void AddChild_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedNode is null) return;
            try
            {
                await ChooseFileAsync(sender, e);
                if (_addNode is null) return;
                SelectedNode._children.Add(_addNode);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                string msg = ex.HResult switch
                {
                    unchecked((int)0x800700B7) => "目标文件已存在（同名冲突），请单击刷新键。",
                    unchecked((int)0x80070020) => "文件正在被占用（可能被资源管理器/预览窗格/杀毒或本程序占用）。",
                    unchecked((int)0x80070005) => "无权限/访问被拒绝。",
                    _ => $"移动失败：0x{ex.HResult:X8} {ex.Message}"
                };

                var dialog = new ContentDialog
                {
                    XamlRoot = this.Content.XamlRoot,
                    Title = "错误",
                    Content = msg,
                    PrimaryButtonText = "确认",
                    DefaultButton = ContentDialogButton.Primary
                };

                await dialog.ShowAsync();
                return;
            }
        }
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        private async Task ChooseFileAsync(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker(((FrameworkElement)sender).XamlRoot.ContentIslandEnvironment.AppWindowId);

            picker.CommitButtonText = "选取文件";
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            picker.ViewMode = PickerViewMode.List;

            // Show the picker dialog window
            var file = await picker.PickSingleFileAsync();
            if (file is null) return;
            _addNode = new MyFilesTreeNode()
            {
                _path = file.Path,
                _isFolder = false
            };
            _addNode._parent = SelectedNode;
        }
        private async Task ChooseFolderAsync(object sender, RoutedEventArgs e)
        {
            var picker = new FolderPicker(((FrameworkElement)sender).XamlRoot.ContentIslandEnvironment.AppWindowId);

            picker.CommitButtonText = "选取文件夹";
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            picker.ViewMode = PickerViewMode.List;

            // Show the picker dialog window
            var folder = await picker.PickSingleFolderAsync();
            if (folder is null) return;
            _toNode = new MyFilesTreeNode()
            {
                _path = folder.Path,
                _isFolder = true
            };
        }
    }
}
