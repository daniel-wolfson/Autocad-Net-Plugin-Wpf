using AspNet.Identity.PostgreSQL;
using ID.Infrastructure;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Helpers;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Intellidesk.Data.Models.Securiry;
using Microsoft.AspNet.Identity;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Intellidesk.Data.Services
{
    public class UserService : BaseService, IAdminService<IdentityUserDetails>, IDisposable
    {
        private readonly IAuthOptions _authOptions;
        private readonly IPluginSettings _pluginSettings;
        private string _licenseFullPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Intellidesk", "license.txt");

        public UserService(IAuthOptions authOptions, IPluginSettings pluginSettings)
        {
            _authOptions = authOptions;
            _pluginSettings = pluginSettings;
        }

        public async Task<IdentityUserDetails> AuthenticateWithUserManager(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return default;

            IdentityUserDetails user = UserManager.FindAsync(username, password).Result;

            // check if username exists
            if (user == null)
                return default;

            // check if password is correct
            if (!await UserManager.CheckPasswordAsync(user, password))
                return default;
            //if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordHash))
            //    return null;

            return user;
        }

        public async Task<string> AuthenticateAsync(string email, string password)
        {
            string tokenResult = null;

            //using (var client = new GeneralHttpClient(ApiServiceNames.DalApi, null))
            //{
            //    Util.GeneratePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);
            //    MultipartFormDataContent form = new MultipartFormDataContent();
            //    form.Add(new StringContent(email), "Email");
            //    form.Add(new StringContent(Convert.ToBase64String(passwordHash)), "PasswordHash");
            //    //var userParams = new LoginModel { Username = username, Email = "daniel.wolfson@hotmail.com", PasswordHash = password, Role = 1 };
            //    var tokenResponse = await client.ExecuteAsync<object>(HttpMethod.Post, "Account/GetToken", form, CustomMediaTypeNames.FormData);
            //}

            var passwordHash = Util.EncryptPassword(password, _authOptions.SALT);
            var appConfig = Plugin.GetService<IAppConfig>();

            MultipartFormDataContent formContent = new MultipartFormDataContent
            {
                { new StringContent(email), "Email" },
                { new StringContent(passwordHash), "PasswordHash" }
            };

            using (var client = new HttpClient())
            {
                var host = appConfig.Endpoints.FirstOrDefault(x => x.Key == ApiServiceNames.DalApi.ToString()).Value;
                client.BaseAddress = new Uri($"{host}/api/{ApiServiceNames.DalApi}/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(CustomMediaTypeNames.XWwwFormUrlencoded));

                HttpResponseMessage tokenResponse = await client.PostAsync("Account/GetToken", formContent);
                if (tokenResponse.IsSuccessStatusCode)
                {
                    var token = await tokenResponse.Content.ReadAsAsync<string>(new[] { new JsonMediaTypeFormatter() });
                    if (!string.IsNullOrEmpty(token) && Util.VerifyToken(token, _authOptions.KEY))
                    {
                        tokenResult = token;
                        Plugin.Logger.Information($"{nameof(CreateTokenAsync)} Token issued"); //{token}
                    }
                    else
                    {
                        tokenResult = null;
                        Plugin.Logger.Error($"{nameof(CreateTokenAsync)} Token Error");
                    }
                    return tokenResult;
                }
            }

            return tokenResult;
        }

        public async Task<string> CreateTokenAsync()
        {
            string tokenResult;
            using (var client = new HttpClient())
            {
                try
                {
                    //_logger.Info($"{nameof(Candidates)} method httpClient.GetAsync rerquest...");
                    //var json = JsonConvert.SerializeObject(userParams);
                    //var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
                    //httpClient.BaseAddress = new Uri(baseUri);
                    //httpClient.DefaultRequestHeaders.Add("token", token);
                    //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "Authorization");

                    var baseAddress = $"{_pluginSettings.MapitApiHost}{_pluginSettings.MapitApiEndPoint}/Account/GetToken";
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

                    //Creating form content
                    var formContent = new FormUrlEncodedContent(new[]
                        {
                            //new KeyValuePair<string, string>("grant_type", "password"),
                            new KeyValuePair<string, string>("rolename", "0"),
                            new KeyValuePair<string, string>("username", "admin"),
                            new KeyValuePair<string, string>("password", "123456")
                        });

                    HttpResponseMessage tokenResponse = await client.PostAsync(baseAddress, formContent);

                    if (tokenResponse.IsSuccessStatusCode)
                    {
                        var token = await tokenResponse.Content.ReadAsAsync<Token>(new[] { new JsonMediaTypeFormatter() });
                        if (string.IsNullOrEmpty(token.Error))
                        {
                            tokenResult = token.AccessToken;
                            Plugin.Logger.Information($"{nameof(CreateTokenAsync)} Token issued"); //{token.AccessToken}
                        }
                        else
                        {
                            tokenResult = null;
                            Plugin.Logger.Error($"{nameof(CreateTokenAsync)} Token Error is: {token.Error}");
                        }
                        return tokenResult;
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    Log.Logger.Information(ex.InnerException?.Message);
                    //_logger.Error($"{nameof(CandidatesController)} Candidates action: {ex.InnerException?.Message}");
                    return null; // ex.InnerException?.Message;
                }
            }
        }

        public async Task<string> ReadLocalTokenAsync()
        {
            string token = File.ReadAllText(_licenseFullPath);
            return await Task.FromResult(token);
        }

        public async Task<bool> WriteLocalTokenAsync(string value)
        {
            File.WriteAllText(_licenseFullPath, value);
            return await Task.FromResult(true);
        }

        public async Task<IdentityResult> CreateAsync(Models.Dto.LoginModel userData)
        {
            IdentityResult result = new IdentityResult();

            // validation
            if (string.IsNullOrWhiteSpace(userData.PasswordHash))
                result = new IdentityResult("Password is required");
            //throw new Exception("Password is required");

            IdentityUserDetails user = await UserManager.FindAsync(userData.Username, userData.PasswordHash);
            if (user != null)
                result = new IdentityResult($"Username {userData.Email} is already taken");
            //throw new Exception($"Username {userData.Email} is already taken");

            if (result.Succeeded)
            {
                //string passwordHash, passwordSalt;
                //GeneratePasswordHash(userModel.Password, out passwordHash, out passwordSalt);
                userData.PasswordHash = UserManager.PasswordHasher.HashPassword(userData.PasswordHash);

                result = await UserManager.CreateAsync(user, userData.PasswordHash);
                if (result.Succeeded)
                {
                    AppUser appUser = AppUser.Create(user);
                }
            }

            return result;
        }

        public async Task<IdentityResult> UpdateAsync(string email, string password, object userParams)
        {
            IdentityResult result = new IdentityResult(); ;
            try
            {
                var user = await UserManager.FindByEmailAsync(email);
                if (user == null)
                    return new IdentityResult("User not found");

                if (!UserManager.CheckPassword(user, password))
                    return new IdentityResult("password not aviabled");

                AppUser appUser = AppUser.Create(user);
                // update user properties
                UpdateModelFromParams(appUser, userParams);

                // update password if it was entered
                if (!string.IsNullOrWhiteSpace(password))
                {
                    var passwordHash = Util.EncryptPassword(password, _authOptions.SALT);
                    user.PasswordHash = passwordHash;
                }
                //SaveChanges();
            }
            catch (Exception ex)
            {
                result = new IdentityResult(ex.InnerException?.Message ?? ex.Message);
                Log.Logger.Error($"{nameof(IUserService)}.{nameof(UpdateAsync)}", ex);
            }
            return result;
        }

        public async Task<bool> FindAsync(string userName, string password)
        {
            var user = await UserManager.FindAsync(userName, password);
            return user != null && !await UserManager.CheckPasswordAsync(user, password);
        }

        public void Delete(int id)
        {
            // TODO:
            //var user = _userRepository. _context.Users.Find(id);
            //if (user != null)
            //{
            //    _context.Users.Remove(user);
            //    _context.SaveChanges();
            //}
        }

        public void UpdateModelFromParams(AppUser appUser, object userParams)
        {
            IDictionary<string, object> userData =
                JsonConvert.DeserializeObject<IDictionary<string, object>>(JsonConvert.SerializeObject(userParams));

            if (userData != null)
            {
                foreach (var propertyName in userData)
                {
                    PropertyInfo property = appUser.GetType().GetProperty(propertyName.Key);
                    if (property != null)
                        appUser[property.Name] = userData[property.Name];
                }
            }
        }

        #region private methods
        public bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
            if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
            if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(storedSalt)))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }

        public async Task<IAppUser> GetDefaultUser(string userName, string password)
        {
            string EncryptUserPassword = ""; //TODO:  Util.Encrypt(password, Util.ENCRYPT_DECRYPT_KEY);
            var user = await UserManager.FindAsync(userName, EncryptUserPassword);
            if (user == null && userName == "admin" && password == "123456")
            {
            }
            return user;
        }

        private string ParseToken(HttpContext httpContext, string claimType)
        {
            var handler = new JwtSecurityTokenHandler();
            string authHeader = httpContext.Request.Headers["Authorization"];
            authHeader = authHeader.Replace("Bearer ", "");
            var jsonToken = handler.ReadToken(authHeader);
            var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;

            var value = tokenS.Claims.First(claim => claim.Type == claimType).Value;
            return value;
        }

        private string CreateToken(string userName, string password, int role, string system, object userdata)
        {
            var key = Encoding.ASCII.GetBytes(_authOptions.KEY);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _authOptions.ISSUER,
                Audience = _authOptions.AUDIENCE,
                NotBefore = DateTime.Now,
                Subject = GetIdentity(userName, password, role, system, userdata),
                Expires = DateTime.UtcNow.AddDays(_authOptions.LIFETIME),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        private ClaimsIdentity GetIdentity(string username, string password, int role, string system, object userdata)
        {
            var user = UserManager.FindAsync(username, password).Result;
            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, role.ToString()),
                    new Claim(ClaimTypes.System, system),
                    new Claim(ClaimTypes.UserData, JsonConvert.SerializeObject(userdata))
                };

                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
                return claimsIdentity;
            }

            // user not found
            return null;
        }

        #endregion private
    }
}
