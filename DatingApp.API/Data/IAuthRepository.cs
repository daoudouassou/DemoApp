using System.Threading.Tasks;
namespace DatingApp.API.Models;

namespace DatingApp.API.Data
{
    public interface IAuthRepository
    {
         Task<User> Register (User user, string password);
        
        Task<User> Login (string login, string password);
        Task<bool> UserExists(string username);
    }
}