using AutoMapper;
using ID.Api.Interfaces;
using ID.Infrastructure.Helpers;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using ID.Infrastructure.Services;
using ID.WebApi.Models.Identity;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ID.Api.Services
{
    public class UserService : BaseService, IUserService // BaseService<IdentityContext>
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IAuthOptions _authOptions;
        private string _licenseFullPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Intellidesk", "esnecil.txt");

        public new IdentityContext DbContext { get; set; }

        public UserService(ILogger logger, IMapper mapper, IdentityContext dbContext, IAuthOptions authOptions)
        {
            _logger = logger;
            _mapper = mapper;
            _authOptions = authOptions;
            DbContext = dbContext;
        }

        public async Task<string> Authenticate(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return null;

            var user = DbContext.AspNetUsers.SingleOrDefault(u => u.Email == email && u.PasswordHash == password);

            IUserDetails userDetails = user != null ? _mapper.Map<IUserDetails>(user) : null;
            if (userDetails != null)
            {
                IAppUser appUser = AppUser.Create(userDetails);
                var token = Util.CreateToken(JsonConvert.SerializeObject(appUser), _authOptions.KEY);
                await this.WriteLocalTokenAsync(token);
                return token;
            }
            else
            {
                _logger.Error($"{nameof(Authenticate)} of {typeof(UserService).Name} error: applicationUser = null");
                return null;
            }
        }

        public IEnumerable<IUserDetails> GetAll()
        {
            List<IUserDetails> items = null; //DbContext.AspNetUsers.Include(blog => blog.AspNetUserRoles).ToList();
            var userDetails = _mapper.Map<List<IUserDetails>>(items);

            // return users without passwords
            return userDetails.Select(x => { x.PasswordHash = null; return x; });
        }

        public IUserDetails GetById(Guid id)
        {
            IUserDetails _userDetails = null; // DbContext.AspNetUsers.Find(id.ToString());
            IUserDetails userDetails = _mapper.Map<IUserDetails>(_userDetails);
            return userDetails;
        }

        public void Create(IUserDetails user, string password)
        {
            // validation
            if (string.IsNullOrWhiteSpace(password))
                throw new ApiException("Password is required");

            if (DbContext.AspNetUsers.Any(x => x.UserName == user.UserName))
                throw new ApiException("Username \"" + user.UserName + "\" is already taken");

            var passwordHash = Util.EncryptPassword(password, _authOptions.SALT);

            user.Id = Guid.NewGuid();
            user.EmailConfirmed = false;
            user.PhoneNumberConfirmed = false;
            user.TwoFactorEnabled = false;
            user.LockoutEnabled = false;
            user.PasswordHash = passwordHash;
            //user.PasswordSalt = passwordSalt;
            user.UserName = user.Email;
            user.SecurityStamp = Guid.NewGuid().ToString();

            var _user = _mapper.Map<AspNetUsers>(user);

            DbContext.AspNetUsers.Add(_user);
            DbContext.SaveChanges();
        }

        public void Update(IUserDetails userDetails, string password = null)
        {
            var user = DbContext.AspNetUsers.Find(userDetails.Id);

            if (user == null)
                throw new ApiException("User not found");

            if (userDetails.UserName != user.UserName)
            {
                // username has changed so check if the new username is already taken
                if (DbContext.AspNetUsers.Any(x => x.UserName == userDetails.UserName))
                    throw new ApiException("Username " + userDetails.UserName + " is already taken");
            }

            // update user properties
            user.UserName = userDetails.UserName;

            // update password if it was entered
            if (!string.IsNullOrWhiteSpace(password))
            {
                var passwordHash = Util.EncryptPassword(password, _authOptions.SALT);
                user.PasswordHash = passwordHash;
                //user.PasswordSalt = Encoding.Unicode.GetString(passwordSalt, 0, passwordSalt.Length);
            }

            DbContext.AspNetUsers.Update(user);
            DbContext.SaveChanges();
        }

        public void Delete(int id)
        {
            var user = DbContext.AspNetUsers.Find(id);
            if (user != null)
            {
                DbContext.AspNetUsers.Remove(user);
                DbContext.SaveChanges();
            }
        }

        public async Task<bool> WriteLocalTokenAsync(string value)
        {
            File.WriteAllText(_licenseFullPath, value);
            return await Task.FromResult(true);
        }
    }
}
