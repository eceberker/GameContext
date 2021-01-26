using GameContextAPI.DataContext;
using GameContextAPI.Entities;
using GameContextAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GameContextAPI.Repositories
{
    public class UserService : IUserService
    {
        private readonly GameContext _context;

        public UserService(GameContext context)
        {
            _context = context;
        }
        
        public async Task<UserModel> CreateUser(UserModel user)
        {
            User newUser = new User()
            {
                user_id = Guid.Parse(user.user_id),
                display_name = user.display_name,
                points = user.points,
                country = user.country,
                timestamp = user.timestamp
            };

            await _context.Users.AddAsync(newUser);

            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<UserModel> GetUserById(Guid user_id)
        {
            var user = await _context.Users.FindAsync(user_id);

            if (user != null)
            {
                UserModel model = new UserModel()
                {

                    display_name = user.display_name,
                    points = user.points,
                    country = user.country,
                    timestamp = user.timestamp,
                    user_id = user.user_id.ToString()

                };
                return model;
            }
            return null;

        }

        public async Task UpdateUserAsync(string user_id, double points, string timestamp)
        {
            //Attach the instance
            var user = await _context.Users.Where(x => x.user_id == Guid.Parse(user_id)).FirstOrDefaultAsync();
            if (user != null)
            {
                user.timestamp = timestamp;
                user.points += points;
                await _context.SaveChangesAsync();
            }
        }

    }
}
