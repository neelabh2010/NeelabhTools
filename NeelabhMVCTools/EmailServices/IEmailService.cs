using System.Net.Mail;

namespace NeelabhMVCTools.EmailServices
{
    public interface IEmailService
    {
        public bool InitSettings();
        public void SetSMTPHost(string smtpHost);
        public void SetSMTPPort(int smtpPort);
        public void SetPriority(MailPriority mailPriority);
        public ResultInfo SendEmail(string ReceiverEmail, string Subject, string HtmlMessage, string SenderName = "Support Team", string ReceiverName = "Member");
    }
}

