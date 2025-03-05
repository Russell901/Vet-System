using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Vet_System.Services;
using Windows.UI.Popups;

namespace Vet_System
{
    public partial class App : Application
    {
        private Window m_window;
        public static Window MainWindow { get; private set; }
        private DatabaseService _databaseService;
        private DialogService _dialogService;

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            MainWindow = m_window;

            if (m_window.Content == null)
            {
                m_window.Content = new Frame();
            }

            m_window.Activate();

            m_window.DispatcherQueue.TryEnqueue(async () =>
            {
                await Task.Delay(100); // Give UI time to initialize
                await InitializeDatabaseAsync();
            });
        }


        private async Task InitializeDatabaseAsync()
        {
            try
            {
                var xamlRoot = (MainWindow.Content as FrameworkElement)?.XamlRoot;
                if (xamlRoot == null)
                {
                    throw new InvalidOperationException("Unable to initialize XamlRoot");
                }

                _dialogService = new DialogService(xamlRoot);
                _databaseService = new DatabaseService(xamlRoot);

                await _databaseService.InitializeAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database initialization failed: {ex.Message}");

                if (_dialogService != null)
                {
                    await _dialogService.ShowErrorAsync(
                        "Database Error",
                        $"Failed to initialize database: {ex.Message}"
                    );
                }
            }
        }


    }
}
