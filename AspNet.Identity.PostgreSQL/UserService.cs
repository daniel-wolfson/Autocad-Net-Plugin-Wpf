using AspNet.Identity.Interfaces;
using ID.Infrastructure.Core;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace ID.Api.Services
{
    public class UserService<TSettings> : IUserService where TSettings : IAppConfig
    {
        // users hardcoded for simplicity, store in a db with hashed passwords in production applications
        //private List<User> _users = new List<User>
        //{
        //    new User { IdNumber = -1, Username = "Test", Password = "test" }
        //};

        private readonly IAppConfig _appSettings;
        private readonly MapitContext _dbContext;
        //private readonly List<IUserDetails> _userDetails;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public UserService(IOptions<GeneralSettings> appSettings, MapitContext context,
            ILogger logger, IMapper mapper)
        {
            _appSettings = appSettings.Value;
            _dbContext = context;
            _logger = logger;
            _mapper = mapper;
            //_userDetails = mapper.Map<List<IUserDetails>>(_dbContext.Users.ToList());
        }

        public string Authenticate(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return null;

            var user = _dbContext.Users.SingleOrDefault(u => u.Email == email);

            IUserDetails userDetails = _mapper.Map<IUserDetails>(user);

            // check if username exists
            if (userDetails == null)
            {
                _logger.Error($"{nameof(Authenticate)} of {typeof(UserService<TSettings>).Name} error: applicationUser = null");
                return null;
            }
            else
            {
                if (!VerifyPasswordHash(password, Convert.FromBase64String(userDetails.PasswordHash), Convert.FromBase64String(userDetails.PasswordSalt)))
                    return null;

                IAppUser appUser = AppUser.Create(userDetails);
                IAuthOptions authOptions = GeneralContext.GetService<IAuthOptions>();
                var token = Util.CreateToken(JsonConvert.SerializeObject(appUser), authOptions.KEY);
                return token;
            }
        }

        public IEnumerable<IUserDetails> GetAll()
        {
            var _userDetails = _mapper.Map<List<IUserDetails>>(_dbContext.Users.ToList());

            // return users without passwords
            return _userDetails.Select(x => { x.PasswordHash = null; return x; });
        }

        public IUserDetails GetById(Guid id)
        {
            var _userDetails = _dbContext.Users.Find(id);
            IUserDetails userDetails = _mapper.Map<IUserDetails>(_userDetails);
            return userDetails;
        }

        public void Create(IUserDetails user, string password)
        {
            // validation
            if (string.IsNullOrWhiteSpace(password))
                throw new AppException("Password is required");

            if (_dbContext.Users.Any(x => x.UserName == user.UserName))
                throw new AppException("Username \"" + user.UserName + "\" is already taken");

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            //user.PasswordHash = Encoding.Unicode.GetString(passwordHash, 0, passwordHash.Length);
            //user.PasswordSalt = Encoding.Unicode.GetString(passwordSalt, 0, passwordSalt.Length); 
            user.Id = Guid.NewGuid();
            user.EmailConfirmed = false;
            user.PhoneNumberConfirmed = false;
            user.TwoFactorEnabled = false;
            user.LockoutEnabled = false;
            user.PasswordHash = Convert.ToBase64String(passwordHash);
            user.PasswordSalt = Convert.ToBase64String(passwordSalt);
            user.UserName = user.Email;
            user.SecurityStamp = Guid.NewGuid().ToString();

            var _user = _mapper.Map<Users>(user);

            _dbContext.Users.Add(_user);
            _dbContext.SaveChanges();
        }

        public void Update(IUserDetails userDetails, string password = null)
        {
            var user = _dbContext.Users.Find(userDetails.Id);

            if (user == null)
                throw new AppException("User not found");

            if (userDetails.UserName != user.UserName)
            {
                // username has changed so check if the new username is already taken
                if (_dbContext.Users.Any(x => x.UserName == userDetails.UserName))
                    throw new AppException("Username " + userDetails.UserName + " is already taken");
            }

            // update user properties
            user.FirstName = userDetails.FirstName;
            user.LastName = userDetails.LastName;
            user.UserName = userDetails.UserName;

            // update password if it was entered
            if (!string.IsNullOrWhiteSpace(password))
            {
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash(password, out passwordHash, out passwordSalt);

                user.PasswordHash = Encoding.Unicode.GetString(passwordHash, 0, passwordHash.Length);
                user.PasswordSalt = Encoding.Unicode.GetString(passwordSalt, 0, passwordSalt.Length);
            }

            _dbContext.Users.Update(user);
            _dbContext.SaveChanges();
        }

        public void Delete(int id)
        {
            var user = _dbContext.Users.Find(id);
            if (user != null)
            {
                _dbContext.Users.Remove(user);
                _dbContext.SaveChanges();
            }
        }

        // private helper methods

        private string CreateUserToken(IUserDetails userDetails)
        {
            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.AppServiceSigningKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, userDetails.UserName),
                    new Claim(ClaimTypes.NameIdentifier, userDetails.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(_appSettings.AppUserTokenExpires),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };

            List<string> roles = userDetails.UserRoles.Select(x => x.Role.Name).ToList();
            if (roles.Any())
            {
                tokenDescriptor.Subject.AddClaims(roles.Select(x => new Claim(ClaimTypes.Role, x)));
            }

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public int GenerateCode()
        {
            PasswordOptions options = new PasswordOptions()
            {
                RequiredLength = 4,
                RequiredUniqueChars = 3,
                RequireDigit = true,
                RequireLowercase = false,
                RequireNonAlphanumeric = false,
                RequireUppercase = false
            };

            int.TryParse(GeneratePasswordHash(options), out int resultCode);

            return resultCode;
        }

        public string GeneratePasswordHash(PasswordOptions opts = null)
        {
            if (opts == null) opts = new PasswordOptions()
            {
                RequiredLength = 8,
                RequiredUniqueChars = 4,
                RequireDigit = true,
                RequireLowercase = true,
                RequireNonAlphanumeric = true,
                RequireUppercase = true
            };

            string[] randomChars = new[] {
            "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
            "abcdefghijkmnopqrstuvwxyz",    // lowercase
            "0123456789",                   // digits
            "!@$?_-"                        // non-alphanumeric
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
                {
                    randomCharsIndex = rand.Next(0, randomChars.Length);
                }

                string rcs = randomChars[randomCharsIndex];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }

        public string CreatePasswordHashCrypto(int length = 8)
        {
            RNGCryptoServiceProvider cryptRNG = new RNGCryptoServiceProvider();
            byte[] tokenBuffer = new byte[length];
            cryptRNG.GetBytes(tokenBuffer);
            return Convert.ToBase64String(tokenBuffer);
        }

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
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

        public string GeneratePasswordHash(int length = 8)
        {
            throw new NotImplementedException();
        }

        private static bool ValidateToken(string token)
        {
            try
            {
                //TokenValidationParameters validationParameters = new TokenValidationParameters
                //{
                //    IssuerSigningKey = new SymmetricSecurityKey(token_salt),
                //    ValidAudience = token_audience,
                //    ValidIssuer = token_issuer,
                //    RequireExpirationTime = true
                //};

                //var lifeTime = new JwtSecurityTokenHandler().ReadToken(token).ValidTo;

                //ClaimsPrincipal principal = new JwtSecurityTokenHandler().ValidateToken(token_last, validationParameters, out SecurityToken validatedToken);

                return true;
            }
            catch (Exception ex)
            {

            }
            return false;
        }

    }
}
