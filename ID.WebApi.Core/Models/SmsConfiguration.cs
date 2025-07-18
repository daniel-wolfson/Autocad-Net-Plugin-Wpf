using ID.Api.Interfaces;

namespace ID.Api.Models
{
    public class SmsConfiguration : ISmsConfiguration
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Company { get; set; }
        public string Sender { get; set; }
        public string AdminMobile { get; set; }
        public int CodeTimeExpiration { get; set; }
    }
}
