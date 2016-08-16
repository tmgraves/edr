using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net.Mail;
using System.Net;
using System.IO;

namespace EDR.Utilities
{
    public class MailHelper
    {
        private const int Timeout = 180000;
        //private readonly string _host;
        //private readonly int _port;
        //private readonly string _user;
        //private readonly string _pass;
        //private readonly bool _ssl;

        public string Sender { get; set; }
        public string Recipient { get; set; }
        public string RecipientCC { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string AttachmentFile { get; set; }

        //public MailHelper()
        //{
        //    ////MailServer - Represents the SMTP Server
        //    //_host = ConfigurationManager.AppSettings["MailServer"];
        //    ////Port- Represents the port number
        //    //_port = int.Parse(ConfigurationManager.AppSettings["Port"]);
        //    ////MailAuthUser and MailAuthPass - Used for Authentication for sending email
        //    //_user = ConfigurationManager.AppSettings["MailAuthUser"];
        //    //_pass = ConfigurationManager.AppSettings["MailAuthPass"];
        //    //_ssl = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSSL"]);
        //}

        public string Send()
        {
            try
            {
                var smtp = new SmtpClient();
                // We do not catch the error here... let it pass direct to the caller
                Attachment att = null;
                var message = new MailMessage("info@eatdancerepeat.com", Recipient, Subject, Body) { IsBodyHtml = true };
                if (RecipientCC != null)
                {
                    message.Bcc.Add(RecipientCC);
                }

                if (!String.IsNullOrEmpty(AttachmentFile))
                {
                    if (File.Exists(AttachmentFile))
                    {
                        att = new Attachment(AttachmentFile);
                        message.Attachments.Add(att);
                    }
                }

                //if (_user.Length > 0 && _pass.Length > 0)
                //{
                //    smtp.UseDefaultCredentials = false;
                //    smtp.Credentials = new NetworkCredential(_user, _pass);
                //    smtp.EnableSsl = _ssl;
                //}

                smtp.Send(message);

                if (att != null)
                    att.Dispose();
                message.Dispose();
                smtp.Dispose();

                return "Success";
            }

            catch (Exception ex)
            {
                return ex.Message;
            }
        }

    }
}
