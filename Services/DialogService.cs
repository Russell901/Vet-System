using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace Vet_System.Services
{
    public class DialogService
    {
        private readonly XamlRoot _xamlRoot;

        public DialogService(XamlRoot xamlRoot)
        {
            _xamlRoot = xamlRoot;
        }

        public async Task<bool> ShowConfirmationAsync(string title, string content)
        {
            var dialog = new ContentDialog
            {
                XamlRoot = _xamlRoot,
                Style = App.Current.Resources["CustomContentDialogStyle"] as Style,
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
            var dialog = new ContentDialog
            {
                XamlRoot = _xamlRoot,
                Style = App.Current.Resources["CustomContentDialogStyle"] as Style,
                Title = title,
                Content = content,
                CloseButtonText = "OK"
            };

            await dialog.ShowAsync();
        }
    }
}