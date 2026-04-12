using HRMS.Components;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace HRMS.Core.Application.Services
{
    public interface IMessageBoxService
    {
        Task ShowAsync(string title, string message, string icon = null, string confirmText = "OK", Func<Task> onConfirm = null);
        Task ShowTwoButtonsAsync(string title, string message, string icon = null,
                                  string confirmText = "Save", string cancelText = "Cancel",
                                  Func<Task> onConfirm = null, Func<Task> onCancel = null);

       

    }

    public class MessageBoxService : IMessageBoxService
    {
        private readonly IDialogService _dialogService;

        public MessageBoxService(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        public async Task ShowAsync(string title, string message, string icon = null, string confirmText = "OK", Func<Task> onConfirm = null)
        {
            var parameters = new DialogParameters
            {
                ["Title"] = title,
                ["Message"] = message,
                ["Icon"] = icon,
                ["ConfirmButtonText"] = confirmText,
                ["OnConfirm"] = EventCallback.Factory.Create(this, onConfirm)
            };

            var options = new DialogOptions { CloseButton = false, MaxWidth = MaxWidth.Small, FullWidth = true , NoHeader = true };

            var dialog = _dialogService.Show<MessageBox>(title, parameters, options);
            await dialog.Result;
        }

        public async Task ShowTwoButtonsAsync(string title, string message, string icon = null,
                                              string confirmText = "Save", string cancelText = "Cancel",
                                              Func<Task> onConfirm = null, Func<Task> onCancel = null)
        {
            var parameters = new DialogParameters
            {
                ["Title"] = title,
                ["Message"] = message,
                ["Icon"] = icon,
                ["ConfirmButtonText"] = confirmText,
                ["CancelButtonText"] = cancelText,
                ["OnConfirm"] = EventCallback.Factory.Create(this, onConfirm),
                ["OnCancel"] = EventCallback.Factory.Create(this, onCancel)
            };

            var options = new DialogOptions { CloseButton = false, MaxWidth = MaxWidth.Small, FullWidth = true, NoHeader=true };

            var dialog = _dialogService.Show<MessageBox>(title, parameters, options);
            await dialog.Result;
        }
    }
}
