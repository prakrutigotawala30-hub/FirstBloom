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
            var fromEmail =
                _configuration["EmailSettings:From"];

            var password =
                _configuration["EmailSettings:Password"];

            var smtpServer =
                _configuration["EmailSettings:SmtpServer"];

            var portString =
                _configuration["EmailSettings:Port"];

            if (string.IsNullOrWhiteSpace(fromEmail))
                throw new Exception("EmailSettings:From is missing.");

            if (string.IsNullOrWhiteSpace(password))
                throw new Exception("EmailSettings:Password is missing.");

            if (string.IsNullOrWhiteSpace(smtpServer))
                throw new Exception("EmailSettings:SmtpServer is missing.");

            if (!int.TryParse(portString, out int port))
                throw new Exception("EmailSettings:Port is invalid.");

            using var message = new MailMessage();

            message.From = new MailAddress(fromEmail);
            message.To.Add(toEmail);
            message.Subject = subject;
            message.Body = htmlMessage;
            message.IsBodyHtml = true;

            using var smtp = new SmtpClient(smtpServer, port);

            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential(
                fromEmail,
                password);

            await smtp.SendMailAsync(message);
        }
    }
}