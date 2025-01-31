using Microsoft.UI.Xaml;

namespace Hinagiku
{
    public partial class App : Application
    {
        private Window mainWindow {get; set;}
        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            mainWindow = new MainWindow();
            mainWindow.Activate();
        }
    }
}
