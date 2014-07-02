using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MagicJson.Data;
using MagicJson.Tools;

namespace MagicJson.UserControls
{
    /// <summary>
    /// Interaction logic for JsonValidation.xaml
    /// </summary>
    public partial class JsonValidation
    {
        private List<ValidationItem> _jsonDataItems;

        public JsonValidation()
        {
            InitializeComponent();
        }

        private void DgJsonFilesLoadingRow(object sender, DataGridRowEventArgs e)
        {
            var rowIndex = e.Row.GetIndex();

            if (!_jsonDataItems[rowIndex].HasResult()) return;

            switch (_jsonDataItems[rowIndex].Validation)
            {
                case Utils.ValidationState.Success:
                    e.Row.Foreground = new SolidColorBrush(Colors.Green);
                    break;
                case Utils.ValidationState.Error:
                    e.Row.Foreground = new SolidColorBrush(Colors.Red);
                    break;
            }
        }

        private void DataGridDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop, false) ? DragDropEffects.All : DragDropEffects.None;
        }

        private void DgJsonDragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            Utils.LoadFiles(ref dgJsonFiles, ref _jsonDataItems, files);
        }

        private void BtValidateJsonClick(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrWhiteSpace(tbSchemaFile.Text))
            {
                MessageBox.Show("Schema file path is missing.", "Wrong input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_jsonDataItems == null || _jsonDataItems.Count == 0)
            {
                MessageBox.Show("JSON files to validate are missing.", "Wrong input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var schema = File.ReadAllText(tbSchemaFile.Text);

            foreach (var dataItem in _jsonDataItems)
            {

                var json = File.ReadAllText(dataItem.File);

                var result = Utils.Validate(json, schema);
                dataItem.SetResult(result);
            }
            dgJsonFiles.ItemsSource = null;
            dgJsonFiles.ItemsSource = _jsonDataItems;
        }

        private void BtBrowseSchemaClick(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".schema",
                Filter = "JSON Schema documents (.schema)|*.schema",
            };
            var result = dlg.ShowDialog();
            if (result == true)
                tbSchemaFile.Text = dlg.FileName;
        }

        private void DgJsonFilesSelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            var dataItem = (ValidationItem)dgJsonFiles.SelectedItem;
            if (dataItem != null && dataItem.HasResult())
                tbJsonDetail.Text = dataItem.ToString();
        }

        private void BtAddJsonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".*",
                Filter = "file (.*)|*.*",
                Multiselect = true
            };
            var result = dlg.ShowDialog();
            if (result == true)
                Utils.LoadFiles(ref dgJsonFiles, ref _jsonDataItems, dlg.FileNames);
        }

        private void BtClearClick(object sender, RoutedEventArgs routedEventArgs)
        {
            dgJsonFiles.ItemsSource = null;
            _jsonDataItems.Clear();
            tbJsonDetail.Text = string.Empty;
        }

        private void DgJsonRowDblClick(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            Utils.OpenFile(((ValidationItem)dgJsonFiles.SelectedItem).File);
        }
    }
}
