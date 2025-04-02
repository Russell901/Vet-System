using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace Vet_System.Services
{
    public class DialogService
    {
        private XamlRoot _xamlRoot;

        public DialogService(XamlRoot xamlRoot)
        {
            UpdateXamlRoot(xamlRoot);
        }

        public void UpdateXamlRoot(XamlRoot xamlRoot)
        {
            if (xamlRoot != null)
            {
                _xamlRoot = xamlRoot ?? throw new ArgumentException(nameof(xamlRoot));
            }
        }

        public async Task<bool> ShowConfirmationAsync(string title, string content)
        {
            if (_xamlRoot == null)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Cannot show confirmation dialog - XamlRoot is null");
                return false;
            }

            var dialog = new ContentDialog
            {
                XamlRoot = _xamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = title,
                Content = content,
                PrimaryButtonText = "Yes",
                SecondaryButtonText = "No",
                DefaultButton = ContentDialogButton.Primary
            };

            var result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }

        public async Task ShowErrorAsync(string title, string content)
        {
            if (_xamlRoot == null)
            {
                System.Diagnostics.Debug.WriteLine($"Error Dialog (XamlRoot null): {title} - {content}");
                return;
            }

            var dialog = new ContentDialog
            {
                XamlRoot = _xamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = title,
                Content = content,
                CloseButtonText = "OK"
            };

            await dialog.ShowAsync();
        }

    }
}