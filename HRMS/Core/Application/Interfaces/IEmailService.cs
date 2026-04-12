namespace HRMS.Core.Application.Interfaces;

public interface IEmailService
{
    Task<bool> SendLeaveRequestSubmittedEmailAsync(string toEmail, string staffName, string requestType, DateTime fromDate, DateTime toDate, decimal? days, decimal? hours);
    Task<bool> SendLeaveRequestApprovedEmailAsync(string toEmail, string staffName, string requestType, DateTime fromDate, DateTime toDate, string approverName, string? comments);
    Task<bool> SendLeaveRequestRejectedEmailAsync(string toEmail, string staffName, string requestType, DateTime fromDate, DateTime toDate, string approverName, string? comments);
    Task<bool> SendLeaveRequestPendingHRApprovalEmailAsync(string toEmail, string staffName, string requestType, DateTime fromDate, DateTime toDate);
    Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
}





