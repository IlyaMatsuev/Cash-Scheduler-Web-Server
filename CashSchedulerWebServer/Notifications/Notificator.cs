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
        private IConfiguration Configuration { get; }

        public Notificator(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        public Task SendEmail(string address, NotificationTemplateType type, Dictionary<string, string> parameters)
        {
            var templateDelegator = new NotificationDelegator();
            var template = templateDelegator.GetTemplate(type, parameters);

            return SendEmail(address, template);
        }

        public async Task SendEmail(string address, NotificationTemplate template)
        {
            var from = new MailAddress(Configuration["App:Email:Address"], Configuration["App:Email:Name"]);
            var to = new MailAddress(address);

            var email = new MailMessage(from, to)
            {
                Subject = template.Subject,
                Body = template.Body,
                IsBodyHtml = true
            };

            var smtp = new SmtpClient(Configuration["App:Email:SMTP:Host"], int.Parse(Configuration["App:Email:SMTP:Port"]))
            {
                Credentials = new NetworkCredential(Configuration["App:Email:Address"], Configuration["App:Email:Password"]),
                EnableSsl = true
            };

            await smtp.SendMailAsync(email);
        }
    }
}
