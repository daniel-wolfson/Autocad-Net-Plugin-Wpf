using ID.Api.Enums;
using System;

namespace ID.Api.Models
{
    public class SendTemplateData : SendMessageData
    {
        public string AppLink { get; set; }
        public string TestLink { get; set; }
        public string Body { get; set; }
        public string Password { get; set; }

        public SendTemplateData() { }

        public SendTemplateData(int emailTemplate)
        {
            TemplateId = Enum.IsDefined(typeof(eXmlEmailTemplates), emailTemplate) ? emailTemplate : -1;

            if (TemplateId >= 0) IsValid();
        }

        public SendTemplateData(eXmlEmailTemplates emailTemplate, string name, string message) : this((int)emailTemplate)
        {
            Name = name;
            Message = message;
            Body = Build();
        }

        public SendTemplateData(int emailTemplate, string name, string password, string subject, string testLink, string appLink) : this(emailTemplate)
        {
            AppLink = appLink;
            TestLink = testLink;
            Subject = subject;
            Password = password;
            Name = name;
            Body = Build();
        }

        public string Build()
        {
            switch ((eXmlEmailTemplates)TemplateId)
            {
                case eXmlEmailTemplates.Aman:
                    Body = $"<div>" +
                           $"<div id='idNumber' class='idNumber'>Hi, {Name}</div>" +
                           $"<br />" +
                           $"<div id='psw' class='psw'>temp password: {Password}</div>" +
                           $"<br />" +
                           $"<div id='testLink' class='testLink'><a href='{AppLink}'>click me to open test!</<a>></div>" +
                           $"</div>";
                    break;
                case eXmlEmailTemplates.ServiceCenter:
                    Body = $"<div>" +
                           $"<div id='idNumber' class='idNumber'>Hi, </div>" +
                           $"<div id='idNumber' class='idNumber'>my name, {Name}</div>" +
                           $"<br />" +
                           $"<div id='psw' class='psw'>message: {Password}</div>" +
                           $"</div>";
                    break;
                default:
                    break;
            }
            return Body;
        }

        public override bool IsValid()
        {
            bool isValid;
            switch ((eXmlEmailTemplates)TemplateId)
            {
                case eXmlEmailTemplates.Aman:
                    isValid = !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(TestLink) && !string.IsNullOrEmpty(AppLink);
                    break;
                case eXmlEmailTemplates.ServiceCenter:
                    isValid = base.IsValid();
                    break;
                default:
                    isValid = false;
                    break;
            }
            // TODO: send log
            return isValid;
        }
    }

}

