namespace Samshit.WebUtils
{
    public class EmailMessage : IEmailMessage
    {
        public string Body { get; set; }
        public string Subject { get; set; }
        public string From { get; set; }
        public string To { get; set; }
    }
}
