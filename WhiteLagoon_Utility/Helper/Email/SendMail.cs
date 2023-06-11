using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon_Models.ViewModels;
using System.Data;
using System.Security.Cryptography.Xml;
using System.Data.Common;

namespace WhiteLagoon_Utility.Helper.Email
{
    public static class SendMail
    {
        public static bool AutoSignupMail(EmailData data)
        {
            string readFile = data.Template.ReadToEnd();
            string myString = readFile;
            myString = myString.Replace("$$UserName$$", data.To);

            //create the mail message 
            MailMessage mail = new MailMessage();

            //set the addresses 
            mail.From = new MailAddress(data.From); //IMPORTANT: This must be same as your smtp authentication address.
            mail.To.Add(data.To);

            //set the content 
            mail.Subject = data.Subject;
            //mail.Body = Body;
            mail.Body = myString.ToString();
            mail.IsBodyHtml = true;
            //send the message 
            SmtpClient smtp = new SmtpClient("mail.domain.in");

            //IMPORANT:  Your smtp login email MUST be same as your FROM address. 
            NetworkCredential Credentials = new NetworkCredential(data.From, data.Password);
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = Credentials;
            smtp.Port = 587;    //alternative port number is 8889
            smtp.EnableSsl = true;
            smtp.Send(mail);
            data.Template.Dispose();
            return true;
        }

        public static bool CommonMailFormat(EmailData data)
        {
            //create the mail message 
            MailMessage mail = new MailMessage();

            //set the addresses 
            mail.From = new MailAddress(data.From); //IMPORTANT: This must be same as your smtp authentication address.
            mail.To.Add(data.To);

            //set the content 
            mail.Subject = data.Subject;
            //mail.Body = Body;
            mail.Body = data.Body.ToString();
            mail.IsBodyHtml = true;
            //send the message 
            SmtpClient smtp = new SmtpClient("mail.domain.in");

            //IMPORANT:  Your smtp login email MUST be same as your FROM address. 
            NetworkCredential Credentials = new NetworkCredential(data.From, data.Password);
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = Credentials;
            smtp.Port = 25;    //alternative port number is 8889
            smtp.EnableSsl = false;
            smtp.Send(mail);
            return true;
        }
    }
}
