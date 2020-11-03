using CashSchedulerWebServer.Notifications.Contracts;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Notifications
{
    public class Notificator : INotificator
    {
        private readonly IConfiguration Configuration;

        public Notificator(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        public async Task SendEmail(string address, NotificationTemplateType type, Dictionary<string, string> parameters)
        {
            var from = new MailAddress(Configuration["AppEmail:Address"], Configuration["AppEmail:Name"]);
            var to = new MailAddress(address);

            var templateDelegator = new NotificationDelegator();
            var template = templateDelegator.GetTemplate(type, parameters);
            
            var email = new MailMessage(from, to)
            {
                Subject = template.Subject,
                Body = template.Body,
                IsBodyHtml = true
            };

            var smtp = new SmtpClient(Configuration["AppEmail:SmtpClient:Host"], int.Parse(Configuration["AppEmail:SmtpClient:Port"]))
            {
                Credentials = new NetworkCredential(Configuration["AppEmail:Address"], Configuration["AppEmail:Password"]),
                EnableSsl = true
            };

            await smtp.SendMailAsync(email);
        }
    }
}
