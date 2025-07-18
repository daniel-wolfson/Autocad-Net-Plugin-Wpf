using ID.Infrastructure.Helpers;
using ID.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;

namespace AspNet.Identity.Interfaces
{
    public interface IUserService
    {
        string Authenticate(string email, string password);
        string GeneratePasswordHash(PasswordOptions opts = null);
        string GeneratePasswordHash(int length = 8);
        int GenerateCode();
        IEnumerable<IAppUser> GetAll();
        IAppUser GetById(Guid id);
        void Create(IAppUser user, string PasswordHash);
        void Update(IAppUser user, string PasswordHash);
        void Delete(int id);
    }
}
