using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace WocForC_.Service
{
    internal static class Historys
    {
        static List<StorageFolder> _history = new List<StorageFolder>();
        public static async Task AddHistory(string newManagePath)
        {
            _history.Add(await StorageFolder.GetFolderFromPathAsync(newManagePath));
        }
        public static async Task ClearHistory()
        {
            _history.Clear();
        }
        public static async Task<List<StorageFolder>> GetHistory()
        {
            return _history;
        }
    }
}
