using System;
using System.Windows;
using MagicJson.Tools;
using Newtonsoft.Json.Linq;

namespace MagicJson.UserControls
{
    /// <summary>
    /// Interaction logic for LiveDocumentation.xaml
    /// </summary>
    public partial class LiveDocumentation
    {
        public LiveDocumentation()
        {
            InitializeComponent();
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(tbJsonDetail.Text))
            {
                MessageBox.Show("JSON files to validate are missing.", "Wrong input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var type = GetExtractType();

            if(type == null)
            {
                MessageBox.Show("No extract format selected.", "Wrong input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var jObject = JObject.Parse(tbJsonDetail.Text);
                tbJsonResult.Text = Documentation.Generate(jObject, (Documentation.Type) type);
            }
            catch (Exception exception)
            {
                tbJsonResult.Text = Utils.FormatedException(exception);
            }
        }

        private Documentation.Type? GetExtractType()
        {
            if(rbHtml.IsChecked != null && (bool) rbHtml.IsChecked)
                return Documentation.Type.HTML;
            if (rbWiki.IsChecked != null && (bool) rbWiki.IsChecked)
                return Documentation.Type.Wiki;
            if (rbRedmine.IsChecked != null && (bool)rbRedmine.IsChecked)
                return Documentation.Type.Redmine;
            return null;
        }
    }
}
