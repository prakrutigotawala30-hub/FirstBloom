using System.Net;
using System.Net.Mail;

namespace FirstBloom.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(
            string toEmail,
            string subject,
            string htmlMessage)
        {
            var senderEmail =
                _configuration["EmailSettings:SenderEmail"];

            var senderPassword =
                _configuration["EmailSettings:Password"];

            var senderName =
                _configuration["EmailSettings:SenderName"];

            var host =
                _configuration["EmailSettings:Host"];

            var port =
                int.Parse(
                    _configuration["EmailSettings:Port"] ?? "587");

            using var message = new MailMessage();

            message.From = new MailAddress(
                senderEmail!,
                senderName);

            message.To.Add(toEmail);
            message.Subject = subject;
            message.Body = htmlMessage;
            message.IsBodyHtml = true;

            using var smtp = new SmtpClient(host, port);

            smtp.EnableSsl = true;

            smtp.Credentials = new NetworkCredential(
                senderEmail,
                senderPassword);

            await smtp.SendMailAsync(message);
        }
    }
}