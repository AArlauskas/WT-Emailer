using emailer_backend.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace emailer_backend
{
    public class Emailer
    {
        public static async Task SendEmailsToUSA(int id, string email, string password, string message, string topic)
        {
            EmailerEntities db = new EmailerEntities();
            var root = HttpContext.Current.Server.MapPath("~/Uploads/" + id + "/");
            var emails = File.ReadAllLines("~/emails.txt");
            var fileRoutes = Directory.GetFiles(root).ToList();
            int counter = 0;

            var mail = new MailMessage();
            SmtpClient config = new SmtpClient("smtp.gmail.com");
            config.Port = 587;
            config.EnableSsl = true;
            config.Credentials = new NetworkCredential(email, password);
            mail.From = new MailAddress(email);
            mail.Subject = topic;
            mail.Body = message;
            mail.IsBodyHtml = true;
            foreach (var route in fileRoutes)
            {
                var attachment = new Attachment(route);
                mail.Attachments.Add(attachment);
            }

            foreach(var line in emails)
            {
                if (counter == 5)
                {
                    counter = 0;
                    Thread.Sleep(90000000);
                }
                try
                {
                    mail.To.Add(new MailAddress(line));
                    config.Send(mail);
                    db.Users.Find(id).SentCount++;
                    await db.SaveChangesAsync();
                    System.Threading.Thread.Sleep(3000);
                }
                catch (Exception e)
                {
                    System.Threading.Thread.Sleep(180000);
                    counter++;
                }
                finally
                {
                    mail.To.Clear();
                }
            }
            db.Users.Find(id).Status = "Done";
            await db.SaveChangesAsync();
        }
    }
}