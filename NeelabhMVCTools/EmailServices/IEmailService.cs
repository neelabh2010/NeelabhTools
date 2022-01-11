using System.Collections.Generic;
using System.Net.Mail;

namespace NeelabhMVCTools.EmailServices
{
    public interface IEmailService
    {
        public bool InitSettings();
        public void SetSMTPHost(string smtpHost);
        public void SetSMTPPort(int smtpPort);
        public void SetPriority(MailPriority mailPriority);
        public void ClearReplyToAddressList();
        public void AddReplyToAddressList(string email);
        public void ClearCCToAddressList();
        public void AddCCToAddressList(string email);
        public void ClearBCCToAddressList();
        public void AddBCCToAddressList(string email);




        public ResultInfo SendEmail(string ReceiverEmail, string Subject, string HtmlMessage, string SenderName = "Support Team", string ReceiverName = "Member");
    }
}

