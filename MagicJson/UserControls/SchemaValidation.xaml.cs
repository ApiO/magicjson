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
    /// Interaction logic for SchemaValidation.xaml
    /// </summary>
    public partial class SchemaValidation
    {
        private List<ValidationItem> _schemaDataItems; 

        public SchemaValidation()
        {
            InitializeComponent();
        }

        private void DgSchemaFilesSelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            var dataItem = (ValidationItem)dgSchemaFiles.SelectedItem;
            if (dataItem != null && dataItem.HasResult())
                tbSchemaDetail.Text = dataItem.ToString();
        }

        private void DgSchemaFilesLoadingRow(object sender, DataGridRowEventArgs e)
        {
            var rowIndex = e.Row.GetIndex();

            if (!_schemaDataItems[rowIndex].HasResult()) return;

            switch (_schemaDataItems[rowIndex].Validation)
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

        private void DgSchemaDragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            Utils.LoadFiles(ref dgSchemaFiles, ref _schemaDataItems, files);
        }

        private void BtClearSchemaClick(object sender, RoutedEventArgs e)
        {
            dgSchemaFiles.ItemsSource = null;
            _schemaDataItems.Clear();
            tbSchemaDetail.Text = string.Empty;
        }

        private void BtValidateSchemaClick(object sender, RoutedEventArgs e)
        {
            if (_schemaDataItems == null || _schemaDataItems.Count == 0)
            {
                MessageBox.Show("Schema files to validate are missing.", "Wrong input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var schema = System.Text.Encoding.UTF8.GetString(Properties.Resources.RFC_Schema_3);

            foreach (var dataItem in _schemaDataItems)
            {
                var json = File.ReadAllText(dataItem.File);

                var result = Utils.Validate(json, schema);
                dataItem.SetResult(result);
            }
            dgSchemaFiles.ItemsSource = null;
            dgSchemaFiles.ItemsSource = _schemaDataItems;
        }

        private void BtAddSchemaClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".schema",
                Filter = "JSON Schema documents (.schema)|*.schema",
                Multiselect = true
            };
            var result = dlg.ShowDialog();
            if (result == true)
                Utils.LoadFiles(ref dgSchemaFiles, ref _schemaDataItems, dlg.FileNames);
        }

        private void DgSchemaRowDblClick(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            Utils.OpenFile(((ValidationItem)dgSchemaFiles.SelectedItem).File);
        }
    }
}
