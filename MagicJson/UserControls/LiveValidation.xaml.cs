using System.Windows;
using MagicJson.Tools;

namespace MagicJson.UserControls
{
    /// <summary>
    /// Interaction logic for LiveValidation.xaml
    /// </summary>
    public partial class LiveValidation
    {
        public LiveValidation()
        {
            InitializeComponent();
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbJson.Text))
            {
                MessageBox.Show("JSON is missing.", "Wrong input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(tbSchema.Text))
            {
                MessageBox.Show("Schema is missing.", "Wrong input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = Utils.Validate(tbJson.Text, tbSchema.Text);
            var code = (new System.Text.ASCIIEncoding()).GetBytes(tbJson.Text);

            tbResult.Text = Utils.FormatResult(result, code);
        }
    }
}
