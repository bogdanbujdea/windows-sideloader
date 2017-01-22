using System.Threading.Tasks;

namespace Sideloader.Services
{
    public interface IAuthenticationService
    {
        Task<Report> LoginAsync(string username, string password);
    }
}