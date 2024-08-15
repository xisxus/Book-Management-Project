//namespace project.Controllers;

//using Microsoft.AspNetCore.Identity;
//using Microsoft.Extensions.Options;
//using project.Models;
//using System.Net;
//using System.Net.Mail;
//using System.Threading.Tasks;

//public class SmtpEmailSender : IEmailSender
//{
//    private readonly SmtpSettings _smtpSettings;

//    public SmtpEmailSender(IOptions<SmtpSettings> smtpSettings)
//    {
//        _smtpSettings = smtpSettings.Value;
//    }

//    public async Task SendEmailAsync(string email, string subject, string message)
//    {
//        var client = new SmtpClient(_smtpSettings.Server, _smtpSettings.Port)
//        {
//            Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
//            EnableSsl = true
//        };

//        var mailMessage = new MailMessage
//        {
//            From = new MailAddress(_smtpSettings.SenderEmail, _smtpSettings.SenderName),
//            Subject = subject,
//            Body = message,
//            IsBodyHtml = true,
//        };

//        mailMessage.To.Add(email);

//        await client.SendMailAsync(mailMessage);
//    }
//}
