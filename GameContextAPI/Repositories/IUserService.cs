using GameContextAPI.Models;
using System;
using System.Threading.Tasks;

namespace GameContextAPI.Repositories
{
    public interface IUserService
    {
        Task<UserModel> GetUserById(Guid user_id);
        Task<UserModel> CreateUser(UserModel user);

        Task UpdateUserAsync(string user_id, double points, string timestamp);
        
    }
}
