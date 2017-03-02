using System.Net.Mail;

namespace ExigoService
{
    public class SendEmailRequest
    {
        public SendEmailRequest()
        {
            this.IsHtml = true;
        }

        public SMTPConfiguration SMTPConfiguration { get; set; }

        public bool IsHtml { get; set; }
        public MailPriority Priority { get; set; }

        public string[] To { get; set; }
        public string[] ReplyTo { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
