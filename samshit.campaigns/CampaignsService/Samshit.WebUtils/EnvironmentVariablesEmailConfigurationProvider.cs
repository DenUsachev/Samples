using System;
using System.Text;

namespace Samshit.WebUtils
{
    /// <summary>
    /// Builds the Email Service Configuration based on environment variables
    /// </summary>
    public class EnvironmentVariablesEmailConfigurationProvider : IEmailConfigurationProvider
    {
        /// <summary>
        /// Builds Email Service Configuration definition
        /// </summary>
        /// <returns></returns>
        public EmailServiceConfiguration GetConfiguration()
        {
            var smtpUser = Environment.GetEnvironmentVariable("BOT_SMTP_MAIL");
            var smtpPasswordRaw = Environment.GetEnvironmentVariable("BOT_SMTP_PASSWORD");
            var smtpHost = Environment.GetEnvironmentVariable("BOT_SMTP_URL");
            var smtpPort = ushort.Parse(Environment.GetEnvironmentVariable("BOT_SMTP_PORT") ?? "0");
            var smtpSecurity = Environment.GetEnvironmentVariable("BOT_SMTP_SECURITY");
            if (string.IsNullOrEmpty(smtpUser)
                || string.IsNullOrEmpty(smtpPasswordRaw)
                || string.IsNullOrEmpty(smtpHost)
                || smtpPort == default(ushort))
            {
                return null;
            }
            var smtpPasswordBytes = Convert.FromBase64String(smtpPasswordRaw);
            var smtpPassword = Encoding.UTF8.GetString(smtpPasswordBytes);
            return new EmailServiceConfiguration()
            {
                SmtpHost = smtpHost,
                SmtpPort = smtpPort,
                SmtpLogin = smtpUser,
                SmtpPassword = smtpPassword,
                SecurityTag = smtpSecurity
            };
        }
    }
}
