using ID.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ID.Api.Interfaces
{
    public interface IUserService
    {
        Task<string> Authenticate(string email, string password);
        IEnumerable<IUserDetails> GetAll();
        IUserDetails GetById(Guid id);
        void Create(IUserDetails user, string PasswordHash);
        void Update(IUserDetails user, string PasswordHash);
        void Delete(int id);
        Task<bool> WriteLocalTokenAsync(string value);
    }
}
