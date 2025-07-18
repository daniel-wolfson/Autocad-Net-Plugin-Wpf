using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace ID.Infrastructure.Helpers
{
    public partial class Util
    {
        /// <summary> read token by objectType and claimType /// </summary>
        public static T ReadToken<T>(string token, string key, string claimType = ClaimTypes.UserData)
        {
            var isTokenValid = Util.VerifyToken(token, key);
            if (!isTokenValid)
            {
                return default;
            }

            var handler = new JwtSecurityTokenHandler();

            ////string authHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            ////token = token.Replace("Bearer ", "");
            ////var jsonToken = handler.ReadToken(token);
            ////var payload = token.Split(".")[1];
            ////var t = handler.ReadJwtToken(token);

            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            if (claimType == null && typeof(T).Name.Contains("AppUser"))
            {
                claimType = ClaimTypes.UserData;
            }

            var value = jwtToken.Claims.First(claim => claim.Type == claimType).Value;

            if (claimType == ClaimTypes.UserData)
            {
                value = Util.DecryptText(value, key);
            }

            T data = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value);
            return data;
        }

        /// <summary> create token /// </summary>
        public static string CreateToken(string userData, string key, List<Claim> customClaims = null)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);
            //var userData = Newtonsoft.Json.JsonConvert.SerializeObject(user);

            var encryptedUserData = Util.EncryptText(userData, key);
            var identityClaims = new List<Claim>() { new Claim(ClaimTypes.UserData, encryptedUserData) };

            customClaims?.ToList().ForEach(x =>
                identityClaims.Add(new Claim(x.Type, x.Value))
            );

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(identityClaims),
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.Now.AddHours(24),
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenObj = tokenHandler.CreateToken(tokenDescriptor);
            var tokenResult = tokenHandler.WriteToken(tokenObj);

            return tokenResult;
        }

        /// <summary> verify token by TokenValidationParameters /// </summary>
        public static bool VerifyToken(string token, string key)
        {
            var securityKey = Encoding.UTF8.GetBytes(key);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(securityKey),
                ValidateIssuer = false,
                ValidateAudience = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken validatedToken;

            try
            {
                tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
            }
            catch (SecurityTokenException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }

            return validatedToken != null;
        }

        public static string CreateToken(string userName, string key, double dayLifetimeCount)
        {
            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityKey = Encoding.ASCII.GetBytes(key);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, userName),
                }),
                Expires = DateTime.UtcNow.AddDays(dayLifetimeCount),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(securityKey), SecurityAlgorithms.HmacSha256Signature),
            };
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static ClaimsIdentity GetClaimsIdentity(object userdata)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.UserData, Newtonsoft.Json.JsonConvert.SerializeObject(userdata)));

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            return claimsIdentity;
        }
    }
}
