namespace NeelabhMVCTools.EmailServices
{
    public interface IEmailService
    {
        public ResultInfo SendEmail(string ReceiverEmail, string Subject, string HtmlMessage, string SenderName = "Support Team", string ReceiverName = "Member");
    }
}

