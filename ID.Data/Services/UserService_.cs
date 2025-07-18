using ID.Infrastructure;
using ID.Infrastructure.Interfaces;
using Intellidesk.AcadNet.Data.Repositories.EF6;
using Intellidesk.Data.Repositories;
using Intellidesk.Data.Repositories.EF6.UnitOfWork;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using User = Intellidesk.Data.Models.Cad.User;
using UserSetting = Intellidesk.Data.Models.Cad.UserSetting;

namespace Intellidesk.Data.Services
{

    /// <summary>
    ///     All methods that are exposed from Repository in Service are overridable to add business logic,
    ///     business logic should be in the Service layer and not in repository for separation of concerns.
    /// </summary>
    public class UserService_ : Service<User>, IUserService
    {
        private readonly List<User> _users;
        private readonly IRepository<User> _repository;
        private readonly IPluginSettings _appSettings;

        public UserService_(IUnitOfWorkAsync uow) : base(uow)
        {
            _repository = uow.Repository<User>();
            _appSettings = Plugin.GetService<IPluginSettings>();

            var contextItems = _repository.Queryable()
                .WhenEmpty(this.CreateInstanceByDefault)
                .Select(x => x.Settings_Data).ToList();

            _users = new List<User>();
            foreach (var innerList in contextItems.Select(item => item.XParse<User>()))
            {
                _users.AddRange(innerList);
            }
        }

        public User GetUserByName(string environmentUserName)
        {
            return GetUsers().FirstOrDefault(x => x.Name == environmentUserName);
        }
        public ObservableCollection<UserSetting> GetSettingsByUser(string environmentUserName)
        {
            var user = GetUsers().FirstOrDefault(x => x.Name == environmentUserName);

            return user != null
                ? user.Settings.ToItems()
                : new ObservableCollection<UserSetting>();
        }

        public IEnumerable<User> GetUsers(bool fromCache = true)
        {
            if (!fromCache || _users.Any())
                return _users.ToList();

            return new List<User>() { this.CreateInstanceByDefault() };
        }

        public ObservableCollection<User> GetItems()
        {
            return GetUsers().ToItems();
        }

        public ObservableCollection<UserSetting> GetUserSettingsByName(string environmentUserName)
        {
            return GetUserByName(environmentUserName).Settings.ToItems();
        }

        public override User CreateInstanceByDefault()
        {
            var user = new User()
            {
                Drive = "C:",
                Name = Environment.UserName,
                UserId = 0,
                UserSettingId = 0,
                Email = Environment.UserName + "@gmail.com", //2013-04-28T15:19:36.4384559+03:00,
                Settings = new[]
                    {
                        new UserSetting()
                        {
                            ConfigSetName = "Default",
                            ChainDistance = (float) 2.00,
                            ColorIndex = 191,
                            Drive = "C:",
                            UserSettingId = 0,
                            IsActive = true,
                            IsColorMode = false,
                            ProjectExplorerPGridColumnSplitterPosition = 40,
                            ProjectExplorerRowSplitterPosition = 140,
                            LayoutId = -1,
                            Percent = 0,
                            ProjectStatus = "New",
                            DateStarted = DateTime.Now,
                            ToggleLayoutDataTemplateSelector = 0,
                            UserId = 0,
                            MinWidth = 300
                        }
                    },
                Settings_Data = String.Format("<UserSettings><UserSetting><UserSettingId>0</UserSettingId><ConfigSetName>Default</ConfigSetName>" +
                            "<ChainDistance>2</ChainDistance><DateStarted>{0}</DateStarted><Drive>C:</Drive><Id>42</Id>" +
                            "<IsActive>true</IsActive><IsColorMode>true</IsColorMode>" +
                            "<LayoutId>0</LayoutId>" +
                            "<ProjectExplorerRowSplitterPosition>140</ProjectExplorerRowSplitterPosition>" +
                            "<ProjectExplorerPGridColumnSplitterPosition>40</ProjectExplorerPGridColumnSplitterPosition><Percent>0</Percent>" +
                            "<ProjectStatus>New</ProjectStatus><ToggleLayoutDataTemplateSelector>0</ToggleLayoutDataTemplateSelector>" +
                            "<MinWidth>376</MinWidth></UserSetting></UserSettings>", DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc))
            };
            //    UserId = 0,
            //    UserSettingId = 0
            return user;
        }

        public User Authenticate(string email, string password, int role)
        {
            User applicationUser = null;
            //string password;
            //Encoding.UTF8.TryParseBase64(Encoding.UTF8.GetString(psw), out password);

            try
            {
                if (role == 0)
                {
                    applicationUser = GetUsers().SingleOrDefault(u => u.Email == email && u.Password == password);
                }
                else
                {
                    var user = GetUsers().SingleOrDefault(u => u.Email == email && u.Password == Convert.ToBase64String(Encoding.ASCII.GetBytes(password)));

                    if (user != null)
                    {
                        applicationUser = new User((int)user.UserId, user.Password, role);
                    }
                }

                if (applicationUser == null)
                {
                    //_logger.Error($"{nameof(Authenticate)} of {typeof(UserService<T>).Name} error: applicationUser = null");
                    return null;
                }
                else
                {
                    applicationUser.Token = CreateUserToken(applicationUser.UserId);

                    // remove password before returning
                    applicationUser.Password = null;

                    return applicationUser;
                }
            }
            catch (Exception)
            {
                //_logger.Error($"{nameof(Authenticate)} of {typeof(UserService<T>).Name} error: " + ex?.InnerException.Message);
                return null;
            }

        }

        public IEnumerable<User> GetAll()
        {
            // return users without passwords
            return _users.Select(x =>
            {
                x.Password = null;
                return x;
            });
        }

        private string CreateUserToken(int idNumber, bool isCandidate = false)
        {

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.AppServiceSigningKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, idNumber.ToString()),
                    new Claim(nameof(isCandidate), Convert.ToString(isCandidate))
                }),
                Expires = DateTime.UtcNow.AddDays(_appSettings.AppUserTokenExpires),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public int CodeGenerate()
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

            int.TryParse(PasswordGenerate(options), out int resultCode);

            return resultCode;
        }

        public string PasswordGenerate(PasswordOptions opts = null)
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

        public string PasswordGenerateCrypto(int length = 8)
        {
            RNGCryptoServiceProvider cryptRNG = new RNGCryptoServiceProvider();
            byte[] tokenBuffer = new byte[length];
            cryptRNG.GetBytes(tokenBuffer);
            return Convert.ToBase64String(tokenBuffer);
        }
    }
}