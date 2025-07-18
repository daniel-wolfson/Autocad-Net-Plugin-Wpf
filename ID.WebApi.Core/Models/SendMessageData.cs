using ID.Api.Enums;

namespace ID.Api.Models
{
    public class SendMessageData
    {
        public int TemplateId { get; set; }
        public string Subject { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }

        public SendMessageData() { }

        public virtual bool IsValid()
        {
            switch ((eXmlEmailTemplates)TemplateId)
            {
                case eXmlEmailTemplates.ServiceCenter:
                    return !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(Message);
                default:
                    return false;
            }
        }
    }

}

