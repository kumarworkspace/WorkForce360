namespace HRMS.Core.Application.Interfaces
{
    public interface IClipboardService
    {
        Task CopyToClipboard(string text);
    }
}
