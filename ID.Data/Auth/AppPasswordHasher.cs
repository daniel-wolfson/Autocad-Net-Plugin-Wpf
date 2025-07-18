using Microsoft.AspNet.Identity;

namespace Intellidesk.Data.Auth
{
    public class AppPasswordHasher : PasswordHasher
    {
        public override string HashPassword(string password)
        {
            return base.HashPassword(password);
        }

        public override PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            //return PasswordVerificationResult.Failed;
            return PasswordVerificationResult.SuccessRehashNeeded;
        }
    }

}