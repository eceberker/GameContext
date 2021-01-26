using GameContextAPI.Models;
using System.Threading.Tasks;

namespace GameContextAPI.CacheService
{
    public interface ICacheService
    {
        Task<string> GetLeaderBoardAsync();
        Task<string> CreateUser(UserModel user);
        Task<string> GetUserById(string user_id);
        Task<string> GetLeaderBoardByCountryAsync(string country_iso_code);
        Task<string> SubmitScoreAsync(ScoreModel scoreModel);
    }
}
