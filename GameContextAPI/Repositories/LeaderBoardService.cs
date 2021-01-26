using GameContextAPI.DataContext;
using GameContextAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameContextAPI.Repositories
{
    public class LeaderBoardService : ILeaderBoardService
    {
        private readonly GameContext _context;

        public LeaderBoardService(GameContext context)
        {
            _context = context;
        }

        public async Task<List<LeaderBoardModel>> GetLeaderBoard()
        {
            var LeaderBoardList = new List<LeaderBoardModel>();

            //Gets clustered user list from Db
            var rank = await _context.Users.Take(20).ToListAsync();

            foreach (var item in rank)
            {
                LeaderBoardList.Add(new LeaderBoardModel()
                {
                    user_id = item.user_id,
                    points = item.points,
                    display_name = item.display_name,
                    country = item.country,

                });
            }
            return LeaderBoardList;
        }

        public async Task<List<LeaderBoardModel>> GetLeaderBoardByISO(string country_iso_code)
        {
            var LeaderBoardList = new List<LeaderBoardModel>();

            var rank = await _context.Users.Where(x => x.country == country_iso_code).Take(20).ToListAsync();

            foreach (var item in rank)
            {
                LeaderBoardList.Add(new LeaderBoardModel()
                {
                    user_id = item.user_id,
                    points = item.points,
                    display_name = item.display_name,
                    country = item.country,

                });
            }

            return LeaderBoardList;
        }
    }
}
