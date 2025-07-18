namespace ID.Api.Interfaces
{
    public interface ISmsConfiguration
    {
        string Password { get; set; }
        string UserName { get; set; }
        string Company { get; set; }
        string Sender { get; set; }
        string AdminMobile { get; set; }
        int CodeTimeExpiration { get; set; }
    }
}