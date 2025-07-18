namespace ID.Api.Interfaces
{
    public interface IEmailConfiguration
    {
        string AdminEmailAddressServiceCenter { get; set; }
        string AdminEmailAddressTo { get; }
        string AdminEmailAddressFrom { get; }
        int AdminTimeout { get; }

        string SmtpServer { get; }
        int SmtpPort { get; }
        string SmtpUsername { get; }
        string SmtpPassword { get; }

        string InfoMailServiceUrl { get; set; }
        string InfoMailUsername { get; set; }
        string InfoMailToken { get; set; }
        string InfoMailCampaignId { get; set; }
        string InfoMailCampaignName { get; set; }
        string InfoMailFromAddress { get; set; }
        string InfoMailFromName { get; set; }
        string InfoMailScheduledSendingTime { get; set; }
        int InfoMailSendingTimeInterval { get; set; }
    }
}
