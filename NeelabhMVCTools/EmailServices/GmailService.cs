using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace NeelabhMVCTools.EmailServices
{
    public class GmailService : IEmailService
    {
        public string SenderEmail { get; set; }
        public string SenderPassword { get; set; }
        public string SMTPHost { get; set; }
        public int SMTPPort { get; set; }
        public MailPriority Priority { get; set; }

        public GmailService()
        {
            // default setting is for Gmail server --
            SMTPHost = "smtp.gmail.com";
            SMTPPort = 587;
            Priority = MailPriority.Normal;
        }

        public bool InitSettings()
        {
            try
            {
                IConfigurationBuilder builder = new ConfigurationBuilder();
                builder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));
                var root = builder.Build();

                // set below setting in the appsetttings.json file --

                SMTPHost = root.GetSection("NT_EmailSettings").GetSection("GmailSettings").GetSection("SmtpHost").Value;
                SMTPPort = root.GetSection("NT_EmailSettings").GetSection("GmailSettings").GetSection("SmtpPort").Value.ToInt();
                SenderEmail = root.GetSection("NT_EmailSettings").GetSection("GmailSettings").GetSection("SenderEmail").Value;
                SenderPassword = root.GetSection("NT_EmailSettings").GetSection("GmailSettings").GetSection("SenderPassword").Value;

                // example format in appsettings.json file --
                    //"NT_EmailSettings": {
                    //    "GmailSettings": {
                    //        "SmtpHost": "smtp.gmail.com",
                    //        "SmtpPort": "587",
                    //        "SenderEmail": "senderemail@gmail.com",
                    //        "SenderPassword": "passwordofsender"
                    //    }
                    //}
            }
            catch
            {
                return false;
            }

            return true;
        }

        public void SetSMTPHost(string smtpHost)
        {
            SMTPHost = smtpHost;    
        }

        public void SetSMTPPort(int smtpPort)
        {
            SMTPPort = smtpPort;
        }

        public void SetPriority(MailPriority mailPriority = MailPriority.Normal)
        {
            Priority = mailPriority;
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
