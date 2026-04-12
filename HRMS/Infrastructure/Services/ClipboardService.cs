using HRMS.Core.Application.Interfaces;
using Microsoft.JSInterop;

namespace HRMS.Infrastructure.Services
{
    public class ClipboardService : IClipboardService
    {
        private readonly IJSRuntime _js;

        public ClipboardService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task CopyToClipboard(string text)
        {
            await _js.InvokeVoidAsync("navigator.clipboard.writeText", text);
        }
    }
}
