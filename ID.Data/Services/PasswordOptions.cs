namespace Intellidesk.Data.Services
{
    public class PasswordOptions
    {
        public PasswordOptions()
        {
        }

        public int RequiredLength { get; set; }
        public int RequiredUniqueChars { get; set; }
        public bool RequireDigit { get; set; }
        public bool RequireLowercase { get; set; }
        public bool RequireNonAlphanumeric { get; set; }
        public bool RequireUppercase { get; set; }
    }
}