using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Vet_System.ViewModels;

namespace Vet_System.Services
{
    public static class AppServiceProvider
    {
        private static IServiceProvider? _services;

        public static IServiceProvider Services
        {
            get => _services ?? throw new InvalidOperationException("Service provider is not initialized");
            private set => _services = value;
        }

        public static void ConfigureServices(Window mainWindow)
        {
            var services = new ServiceCollection();

            // Ensure mainWindow.Content is a Frame
            if (mainWindow.Content == null)
            {
                mainWindow.Content = new Frame();
            }

            if (mainWindow.Content is not Frame frame)
            {
                throw new InvalidOperationException("Main frame not found");
            }

            // Register services
            services.AddSingleton<INavigationService>(sp => new NavigationService(frame));
            services.AddSingleton<DialogService>();
            services.AddSingleton<DatabaseService>();

            // Register ViewModels
            services.AddTransient<PatientsViewModel>();

            Services = services.BuildServiceProvider();
        }

        public static T GetRequiredService<T>() where T : class
        {
            return Services.GetRequiredService<T>()
                ?? throw new InvalidOperationException($"Service of type {typeof(T).Name} not found");
        }

        public static T? GetService<T>() where T : class
        {
            return Services.GetService<T>();
        }

        public static bool IsInitialized => _services != null;

        public static void Reset()
        {
            if (_services is IDisposable disposable)
            {
                disposable.Dispose();
            }
            _services = null;
        }
    }
}
