using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace NeelabhMVCTools.EmailServices
{
    public class EmailService : IEmailService
    {
        public string SenderEmail { get; set; }
        public string SenderPassword { get; set; }
        public string SMTPHost { get; set; }
        public int SMTPPort { get; set; }
        public MailPriority Priority { get; set; }

        public EmailService()
        {
            // default setting is for Gmail server --
            SMTPHost = "smtp.gmail.com";
            SMTPPort = 587;
            Priority = MailPriority.Normal;
        }

        private void SetSettings()
        {
            try
            {
                IConfigurationBuilder builder = new ConfigurationBuilder();
                builder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));
                var root = builder.Build();

                SMTPHost = root.GetSection("NT_EmailSettings").GetSection("SMTPHost").Value;
                SMTPPort = root.GetSection("NT_EmailSettings").GetSection("SMTPPort").Value.ToInt();
                SenderEmail = root.GetSection("NT_EmailSettings").GetSection("SenderEmail").Value;
                SenderPassword = root.GetSection("NT_EmailSettings").GetSection("SenderPassword").Value;
            }
            catch
            {

            }
        }

        public ResultInfo SendEmail(string ReceiverEmail, string Subject, string HtmlMessage, string SenderName = "Support Team", string ReceiverName = "Member")
        {
            // Sending Email --
            ResultInfo result = new ResultInfo();

            try
            {
                MailMessage mailMessage = new MailMessage
                {
                    From = new MailAddress(SenderEmail, SenderName),
                };

                mailMessage.To.Add(new MailAddress(ReceiverEmail, ReceiverName));
                mailMessage.Subject = Subject;
                mailMessage.Body = HtmlMessage;
                mailMessage.IsBodyHtml = true;
                mailMessage.Priority = Priority;

                using (SmtpClient smtp = new SmtpClient(SMTPHost, SMTPPort))
                {
                    smtp.Credentials = new NetworkCredential(SenderEmail, SenderPassword);
                    smtp.EnableSsl = true;
                    smtp.Send(mailMessage);
                }

                result.HasError = false;
                result.Message = "Mail sent successfully";
            }
            catch (Exception ex)
            {
                result.HasError = true;
                result.Message = ex.Message;
            }

            return result;
        }
    }
}
