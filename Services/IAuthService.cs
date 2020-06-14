using MAdvice.Extensions;
using MAdvice.Models;
using System.Threading.Tasks;

namespace MAdvice.Services
{
    public interface IAuthService
    {
        Task<TaskResult<User>> LoginAsync(string username, string password);
        Task<TaskResult<User>> RegisterAsync(string username, string password,string email);
    }
}
