using System.Threading.Tasks;
using Unity;

namespace ID.Infrastructure.Interfaces
{
    public interface IRegisterModule
    {
        Task<bool> Register(IUnityContainer container);
        Task<bool> Initialize(IUnityContainer container);
    }
}
