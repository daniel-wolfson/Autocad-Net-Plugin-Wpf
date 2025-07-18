using ID.Api.Enums;
using ID.Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ID.Api.Interfaces
{
    public interface ISendService
    {
        Task<ObjectResult> SendEmailAsync(eXmlEmailTemplates xmlEmailTemplate, SendMessageData data);

        Task<ObjectResult> SendEmailAsync(eXmlEmailTemplates xmlEmailTemplate, List<PsygateCandidate> candidates);

        Task<ObjectResult> SendEmailSmtpAsync(string name, string subject, string addressFrom, string addressTo, string message);

        Task<ObjectResult> SendSmsAsync(string name, string mobile, string message);
    }
}
