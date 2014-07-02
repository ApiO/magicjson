using System.Windows;
using MagicJson.UserControls;

namespace MagicJson
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            tabLive.Content = new LiveValidation();
            tabSchema.Content = new SchemaValidation();

            tabLiveDoc.Content = new LiveDocumentation();
            tabFileDoc.Content = new FileDocumentation();

            tabJson.Content = new JsonValidation();
        }

        private void CloseClick(object sender, RoutedEventArgs routedEventArgs)
        {
            Close();
        }
    }
}
