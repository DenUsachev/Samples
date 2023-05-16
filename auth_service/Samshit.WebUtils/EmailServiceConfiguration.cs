namespace Samshit.WebUtils
{
    public class EmailServiceConfiguration
    {
        private const string SSL_TOKEN = "SSL";

        public bool IsSslEnabled => SecurityTag == SSL_TOKEN ? true : false;
        public string SmtpHost { get; set; }
        public ushort SmtpPort { get; set; }
        public string SmtpLogin { get; set; }
        public string SmtpPassword { get; set; }
        public string SecurityTag { get; set; }
    }
}
