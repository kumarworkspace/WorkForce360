using HRMS.Core.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace HRMS.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string? _smtpServer;
    private readonly int _smtpPort;
    private readonly string? _smtpUsername;
    private readonly string? _smtpPassword;
    private readonly string? _fromEmail;
    private readonly string? _fromName;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        _smtpServer = _configuration["EmailSettings:SmtpServer"];
        _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
        _smtpUsername = _configuration["EmailSettings:SmtpUsername"];
        _smtpPassword = _configuration["EmailSettings:SmtpPassword"];
        _fromEmail = _configuration["EmailSettings:FromEmail"];
        _fromName = _configuration["EmailSettings:FromName"] ?? "WorkForce360";
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
    {
        try
        {
            if (string.IsNullOrEmpty(_smtpServer) || string.IsNullOrEmpty(_fromEmail))
            {
                _logger.LogWarning("Email settings not configured. Email will not be sent.");
                return false;
            }

            using var client = new SmtpClient(_smtpServer, _smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_fromEmail, _fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            message.To.Add(toEmail);

            await client.SendMailAsync(message);
            
            _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {ToEmail}", toEmail);
            return false;
        }
    }

    public async Task<bool> SendLeaveRequestSubmittedEmailAsync(string toEmail, string staffName, string requestType, DateTime fromDate, DateTime toDate, decimal? days, decimal? hours)
    {
        var subject = $"{requestType} Request Submitted";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2 style='color: #1a237e;'>Leave Request Submitted</h2>
                <p>Dear {staffName},</p>
                <p>Your {requestType.ToLower()} request has been submitted successfully.</p>
                <table style='border-collapse: collapse; margin: 20px 0;'>
                    <tr><td style='padding: 8px; font-weight: bold;'>Request Type:</td><td style='padding: 8px;'>{requestType}</td></tr>
                    <tr><td style='padding: 8px; font-weight: bold;'>From Date:</td><td style='padding: 8px;'>{fromDate:MMM dd, yyyy}</td></tr>
                    <tr><td style='padding: 8px; font-weight: bold;'>To Date:</td><td style='padding: 8px;'>{toDate:MMM dd, yyyy}</td></tr>
                    {(days.HasValue ? $"<tr><td style='padding: 8px; font-weight: bold;'>Total Days:</td><td style='padding: 8px;'>{days.Value:F2}</td></tr>" : "")}
                    {(hours.HasValue ? $"<tr><td style='padding: 8px; font-weight: bold;'>Total Hours:</td><td style='padding: 8px;'>{hours.Value:F2}</td></tr>" : "")}
                </table>
                <p>Your request is pending approval. You will be notified once it is reviewed.</p>
                <p>Best regards,<br/>WorkForce360</p>
            </body>
            </html>";

        return await SendEmailAsync(toEmail, subject, body);
    }

    public async Task<bool> SendLeaveRequestApprovedEmailAsync(string toEmail, string staffName, string requestType, DateTime fromDate, DateTime toDate, string approverName, string? comments)
    {
        var subject = $"{requestType} Request Approved";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2 style='color: #2e7d32;'>Leave Request Approved</h2>
                <p>Dear {staffName},</p>
                <p>Your {requestType.ToLower()} request has been approved by {approverName}.</p>
                <table style='border-collapse: collapse; margin: 20px 0;'>
                    <tr><td style='padding: 8px; font-weight: bold;'>Request Type:</td><td style='padding: 8px;'>{requestType}</td></tr>
                    <tr><td style='padding: 8px; font-weight: bold;'>From Date:</td><td style='padding: 8px;'>{fromDate:MMM dd, yyyy}</td></tr>
                    <tr><td style='padding: 8px; font-weight: bold;'>To Date:</td><td style='padding: 8px;'>{toDate:MMM dd, yyyy}</td></tr>
                    <tr><td style='padding: 8px; font-weight: bold;'>Approved By:</td><td style='padding: 8px;'>{approverName}</td></tr>
                </table>
                {(string.IsNullOrEmpty(comments) ? "" : $"<p><strong>Comments:</strong> {comments}</p>")}
                <p>Best regards,<br/>WorkForce360</p>
            </body>
            </html>";

        return await SendEmailAsync(toEmail, subject, body);
    }

    public async Task<bool> SendLeaveRequestRejectedEmailAsync(string toEmail, string staffName, string requestType, DateTime fromDate, DateTime toDate, string approverName, string? comments)
    {
        var subject = $"{requestType} Request Rejected";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2 style='color: #c62828;'>Leave Request Rejected</h2>
                <p>Dear {staffName},</p>
                <p>Your {requestType.ToLower()} request has been rejected by {approverName}.</p>
                <table style='border-collapse: collapse; margin: 20px 0;'>
                    <tr><td style='padding: 8px; font-weight: bold;'>Request Type:</td><td style='padding: 8px;'>{requestType}</td></tr>
                    <tr><td style='padding: 8px; font-weight: bold;'>From Date:</td><td style='padding: 8px;'>{fromDate:MMM dd, yyyy}</td></tr>
                    <tr><td style='padding: 8px; font-weight: bold;'>To Date:</td><td style='padding: 8px;'>{toDate:MMM dd, yyyy}</td></tr>
                    <tr><td style='padding: 8px; font-weight: bold;'>Rejected By:</td><td style='padding: 8px;'>{approverName}</td></tr>
                </table>
                {(string.IsNullOrEmpty(comments) ? "" : $"<p><strong>Comments:</strong> {comments}</p>")}
                <p>Please contact your manager or HR for more information.</p>
                <p>Best regards,<br/>WorkForce360</p>
            </body>
            </html>";

        return await SendEmailAsync(toEmail, subject, body);
    }

    public async Task<bool> SendLeaveRequestPendingHRApprovalEmailAsync(string toEmail, string staffName, string requestType, DateTime fromDate, DateTime toDate)
    {
        var subject = $"{requestType} Request Pending HR Approval";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2 style='color: #f57c00;'>Leave Request Pending HR Approval</h2>
                <p>Dear HR Team,</p>
                <p>A {requestType.ToLower()} request from {staffName} is pending your approval.</p>
                <table style='border-collapse: collapse; margin: 20px 0;'>
                    <tr><td style='padding: 8px; font-weight: bold;'>Staff Name:</td><td style='padding: 8px;'>{staffName}</td></tr>
                    <tr><td style='padding: 8px; font-weight: bold;'>Request Type:</td><td style='padding: 8px;'>{requestType}</td></tr>
                    <tr><td style='padding: 8px; font-weight: bold;'>From Date:</td><td style='padding: 8px;'>{fromDate:MMM dd, yyyy}</td></tr>
                    <tr><td style='padding: 8px; font-weight: bold;'>To Date:</td><td style='padding: 8px;'>{toDate:MMM dd, yyyy}</td></tr>
                </table>
                <p>Please review and approve/reject the request in the HRMS system.</p>
                <p>Best regards,<br/>WorkForce360</p>
            </body>
            </html>";

        return await SendEmailAsync(toEmail, subject, body);
    }
}





