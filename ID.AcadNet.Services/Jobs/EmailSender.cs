using ID.Infrastructure;
using ID.Infrastructure.Interfaces;
using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;

namespace Intellidesk.AcadNet.Services.Jobs
{
    public class EmailSender : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            using (MailMessage message = new MailMessage("daniel.wolfson@hotmail.com", "daniel.wolfson@hotmail.com"))
            {
                IPluginSettings pluginSettings = Plugin.GetService<IPluginSettings>();

                if (pluginSettings.ReportIndex != 0 && DateTime.Now.Month == pluginSettings.ReportIndex)
                {
                    string productVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
                    message.Subject = $"Intellidesk plugin v{productVersion} for {pluginSettings.Name} 2017";
                    message.Body = $"User domain {Environment.UserDomainName}\nuser name: {Environment.UserName}\nuserId: {pluginSettings.UserId} ";

                    using (SmtpClient client = new SmtpClient
                    {
                        EnableSsl = true,
                        Host = "smtp.live.com",
                        Port = 25, // 465
                        Credentials = new NetworkCredential("daniel.wolfson@hotmail.com", "wolf1939")
                    })
                    {
                        client.Send(message);
                    }

                    pluginSettings.ReportIndex++;
                    if (pluginSettings.ReportIndex > 12) pluginSettings.ReportIndex = 1;
                    pluginSettings.Save();
                }
            }
            return Task.FromResult(true);
        }

        public static IEnumerable<string> GetIP4Addresses()
        {
            return Dns.GetHostAddresses(Dns.GetHostName())
                .Where(ipa => ipa.AddressFamily == AddressFamily.InterNetwork)
                .Select(x => x.ToString());
        }
    }
}