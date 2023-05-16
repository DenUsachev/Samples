using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;

namespace Samshit.WebUtils
{
    public class EmailService : IEmailService
    {
        private readonly bool _securityTag;
        private readonly string _smtpHost;
        private readonly ushort _smtpPort;
        private readonly string _smtpLogin;
        private readonly string _smtpPassword;

        public EmailService(EmailServiceConfiguration configuration)
        {
            _smtpHost = configuration.SmtpHost;
            _smtpPort = configuration.SmtpPort;
            _smtpLogin = configuration.SmtpLogin;
            _smtpPassword = configuration.SmtpPassword;
            _securityTag = configuration.IsSslEnabled;
        }

        public async Task<bool> SendMessageAsync(IEmailMessage message)
        {
            var sendResult = false;
            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync($"{_smtpHost}", _smtpPort, _securityTag);
                    await client.AuthenticateAsync(_smtpLogin, _smtpPassword);
                    if (client.IsAuthenticated)
                    {
                        var msg = new MimeMessage();
                        var addrFrom = MailboxAddress.Parse(message.From);
                        var addrTo = MailboxAddress.Parse(message.To);
                        msg.From.Add(addrFrom);
                        msg.To.Add(addrTo);
                        message.Subject = message.Subject;
                        var bodyBuilder = new BodyBuilder {TextBody = message.Body};
                        msg.Body = bodyBuilder.ToMessageBody();
                        await client.SendAsync(msg);
                        await client.DisconnectAsync(true);
                        sendResult = true;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to send e-mail: {0}", e.Message);
                    sendResult = false;
                }
            }

            return sendResult;
        }
    }
}