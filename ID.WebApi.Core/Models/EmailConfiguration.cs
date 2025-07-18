using ID.Api.Interfaces;

namespace ID.Api.Models
{
    public class EmailConfiguration : IEmailConfiguration
    {
        public string AdminEmailAddressTo { get; set; }
        public string AdminEmailAddressFrom { get; set; }
        public string AdminEmailAddressServiceCenter { get; set; }
        public int AdminTimeout { get; set; }

        public string SmtpServer { get; set; } // https://outlook.office.com/owa/?path=/options/popandimap
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }

        public string InfoMailServiceUrl { get; set; }
        public string InfoMailUsername { get; set; }
        public string InfoMailToken { get; set; }
        public string InfoMailCampaignId { get; set; }
        public string InfoMailCampaignName { get; set; }
        public string InfoMailFromAddress { get; set; }
        public string InfoMailFromName { get; set; }
        public string InfoMailScheduledSendingTime { get; set; }
        public int InfoMailSendingTimeInterval { get; set; }
    }
}
