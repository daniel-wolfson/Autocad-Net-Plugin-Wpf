using ID.Infrastructure.Interfaces;
using Intellidesk.Data.Models.Dto;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;

namespace Intellidesk.Data.Services
{
    public interface IAdminService<TUser> where TUser : IAppUser
    {
        Task<string> AuthenticateAsync(string username, string password);
        Task<TUser> AuthenticateWithUserManager(string username, string password);
        Task<IdentityResult> CreateAsync(LoginModel userModel);
        Task<IdentityResult> UpdateAsync(string email, string password, object userParams);
        Task<bool> FindAsync(string email, string password);
        Task<string> ReadLocalTokenAsync();
        Task<bool> WriteLocalTokenAsync(string value);
        Task<string> CreateTokenAsync();
    }
}
