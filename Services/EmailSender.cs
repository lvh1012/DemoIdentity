using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;

namespace DemoIdentity.Services
{
    // service gửi mail confirm
    public class EmailSender : IEmailSender
    {
        private readonly Dictionary<string, string> conf;

        public EmailSender()
        {
            conf = new Dictionary<string, string>
            {
                { "Mail", "x@gmail.com" },
                { "DisplayName", "Le Viet Hoang" },
                { "Password", "x" },
                { "Host", "smtp-mail.outlook.com" },
                { "Port", "587 " },
            };
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var email = new MimeMessage();
            email.Sender = new MailboxAddress(conf["DisplayName"], conf["Mail"]);
            email.From.Add(new MailboxAddress(conf["DisplayName"], conf["Mail"]));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder();
            builder.HtmlBody = message;
            email.Body = builder.ToMessageBody();

            using var smtp = new MailKit.Net.Smtp.SmtpClient();

            try
            {
                smtp.Connect(conf["Host"], int.Parse(conf["Port"]), SecureSocketOptions.StartTls);
                smtp.Authenticate(conf["Mail"], conf["Password"]);
                await smtp.SendAsync(email);
            }
            catch (Exception)
            {

            }

            smtp.Disconnect(true);
        }
    }
}