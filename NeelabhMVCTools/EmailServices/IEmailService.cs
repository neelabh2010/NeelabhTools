using System.Net.Mail;

namespace NeelabhMVCTools.EmailServices
{
    public interface IEmailService
    {
        public bool InitSettings();
        public void SetSMTPHost(string smtpHost);
        public void SetSMTPPort(int smtpPort);
        public void SetPriority(MailPriority mailPriority);
        public void ResetReplyToAddressList();
        public void AddReplyToAddressList(string email);
        public void ResetCCToAddressList();
        public void AddCCToAddressList(string email);
        public void ResetBCCToAddressList();
        public void AddBCCToAddressList(string email);
        public ResultInfo SendEmail(string ReceiverEmail, string Subject, string HtmlMessage, string SenderName = "Support Team", string ReceiverName = "Member");
    }
}

