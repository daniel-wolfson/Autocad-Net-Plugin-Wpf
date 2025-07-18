using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace ID.Infrastructure.Helpers
{
    /// <summary> Generation numbers/strings </summary>
    public partial class Util
    {
        public static string CreateGuid()
        {
            Guid g = Guid.NewGuid();
            string guid = g.ToString();
            return guid.Replace("-", "");
        }

        public static string GenerateRandomDigits(int requiredLength, int requiredUniqueDigits = 3)
        {
            PasswordOptions options = new PasswordOptions()
            {
                RequiredLength = requiredLength,
                RequiredUniqueChars = requiredUniqueDigits,
                RequireDigit = true,
                RequireLowercase = false,
                RequireNonAlphanumeric = false,
                RequireUppercase = false
            };

            return GenerateRandomSymbols(options);
        }

        public static string GenerateRandomSymbols(PasswordOptions opts = null)
        {
            if (opts == null)
            {
                opts = new PasswordOptions()
                {
                    RequiredLength = 8,
                    RequiredUniqueChars = 3,
                    RequireDigit = true,
                    RequireLowercase = true,
                    RequireNonAlphanumeric = false,
                    RequireUppercase = true
                };
            };

            string[] randomChars = new[] {
                "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
                "abcdefghijkmnopqrstuvwxyz",    // lowercase
                "0123456789"                    // digits
                //"!@$?_-"                      // non-alphanumeric
            };

            Random rand = new Random(Environment.TickCount);
            Thread.Sleep(20);
            List<char> chars = new List<char>();

            if (opts.RequireUppercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);

            if (opts.RequireLowercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);

            if (opts.RequireDigit)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);

            if (opts.RequireNonAlphanumeric)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[3][rand.Next(0, randomChars[3].Length)]);

            int randomCharsIndex = 0;
            if (opts.RequireDigit && !opts.RequireNonAlphanumeric && !opts.RequireUppercase && !opts.RequireLowercase)
            {
                randomCharsIndex = 2;
            }

            for (int i = chars.Count; i < opts.RequiredLength
                || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
            {
                if (randomCharsIndex == 0)
                    randomCharsIndex = rand.Next(0, randomChars.Length);

                string rcs = randomChars[randomCharsIndex];

                chars.Insert(rand.Next(0, chars.Count), rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }

        //public string GeneratePasswordHashCrypto(int length = 8)
        //{
        //    RNGCryptoServiceProvider cryptRNG = new RNGCryptoServiceProvider();
        //    byte[] tokenBuffer = new byte[length];
        //    cryptRNG.GetBytes(tokenBuffer);
        //    return Convert.ToBase64String(tokenBuffer);
        //}

        public static void GeneratePasswordHash(string password, string salt, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            using (var hmac = new HMACSHA512()) //Encoding.UTF8.GetBytes(salt))
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        public static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
            //if (storedHash.Length != 64)
            //    throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
            //if (storedSalt.Length != 128)
            //    throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");
            using (var hmac = new HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }
            return true;
        }
    }
}
