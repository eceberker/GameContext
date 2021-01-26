using GameContextAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameContextAPI.Repositories
{
    public interface ILeaderBoardService
    {
        Task<List<LeaderBoardModel>> GetLeaderBoardByISO(string country_iso_code);
        Task<List<LeaderBoardModel>> GetLeaderBoard();
    }
}
