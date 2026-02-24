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
using WocForC_.ViewModel;
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

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
