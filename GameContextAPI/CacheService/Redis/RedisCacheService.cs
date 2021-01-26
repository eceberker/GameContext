using GameContextAPI.Models;
using GameContextAPI.Repositories;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameContextAPI.CacheService.Redis
{
    public class RedisCacheService : ICacheService
    {
        private const string leaderboardkey = "leaderboard";

        private RedisServer _redisServer;
        private readonly ILeaderBoardService _leaderBoardService;
        private readonly IUserService _userService;
        public RedisCacheService(RedisServer redisServer, ILeaderBoardService leaderBoardService, IUserService userService)
        {
            _redisServer = redisServer;
            _leaderBoardService = leaderBoardService;
            _userService = userService;
        }

        public async Task<string> CreateUser(UserModel user)
        {
            // Creates a user hashset in Redis
            await _redisServer.Database.HashSetAsync(key: user.user_id, new HashEntry[] { new HashEntry("display_name", user.display_name.ToString()), new HashEntry("points", user.points), new HashEntry("country", user.country), new HashEntry("timestamp", user.timestamp) });

            // Adds/updates created user in global leaderboard
            await _redisServer.Database.SortedSetAddAsync("leaderboard", member: user.user_id, score: user.points);

            // Adds/updates created user in country leaderboard
            await _redisServer.Database.SortedSetAddAsync(key: user.country, member: user.user_id, score: user.points);

            // Adds user to Db
            await _userService.CreateUser(user);

            var settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;

            return JsonConvert.SerializeObject(user, settings);
        }

        public async Task<string> GetLeaderBoardAsync()
        {
            // Gets global leaderboard sorted set from Redis
            var list = await _redisServer.Database.SortedSetRangeByRankWithScoresAsync(leaderboardkey, 0, -1, Order.Descending);

            // If Redis is empty, go Db
            if (list.Length == 0)
            {
                //Gets leaderboard from Db
                var dbList = await _leaderBoardService.GetLeaderBoard();

                // If Db is empty, return null
                if (dbList.Count == 0)
                {
                    return null;
                }

                // Adds userlist from Db to Redis
                foreach (var user in dbList)
                {
                    // Adds user global leaderboad in Redis
                    await _redisServer.Database.SortedSetAddAsync(leaderboardkey, member: user.user_id.ToString(), score: user.points);

                    // Adds user country leaderboad in Redis
                    await _redisServer.Database.SortedSetAddAsync(user.country, member: user.user_id.ToString(), score: user.points);

                    // Creates a user hashset in Redis
                    await _redisServer.Database.HashSetAsync(key: user.user_id.ToString(), new HashEntry[] { new HashEntry("display_name", user.display_name.ToString()), new HashEntry("points", user.points), new HashEntry("country", user.country) });
                }

                // Get generated leaderboard from Redis
                list = await _redisServer.Database.SortedSetRangeByRankWithScoresAsync(leaderboardkey, 0, -1, Order.Descending);

            }
            return (await FillReturnList(list));
        }
        public async Task<string> GetLeaderBoardByCountryAsync(string country_iso_code)
        {

            // Gets sorted country leaderboard
            var users_by_country = await _redisServer.Database.SortedSetRangeByRankWithScoresAsync(country_iso_code, 0, -1, Order.Descending);

            // If Redis is empty, go Db
            if (users_by_country.Length == 0)
            {
                //Get country leaderboard from Db
                var dbList = await _leaderBoardService.GetLeaderBoardByISO(country_iso_code);

                // If Db is empty, return null
                if (dbList.Count == 0)
                {
                    return null;
                }

                // Adds userlist from Db to Redis
                foreach (var user in dbList)
                {
                    // Adds user global leaderboad in Redis
                    await _redisServer.Database.SortedSetAddAsync(leaderboardkey, member: user.user_id.ToString(), score: user.points);

                    // Adds user country leaderboad in Redis
                    await _redisServer.Database.SortedSetAddAsync(user.country, member: user.user_id.ToString(), score: user.points);

                    // Creates a user hashset in Redis
                    await _redisServer.Database.HashSetAsync(key: user.user_id.ToString(), new HashEntry[] { new HashEntry("display_name", user.display_name.ToString()), new HashEntry("points", user.points), new HashEntry("country", user.country) });
                }

                // Get generated leaderboard from Redis
                users_by_country = await _redisServer.Database.SortedSetRangeByRankWithScoresAsync(key: country_iso_code, 0, -1, Order.Descending);
            }
            return (await FillReturnList(users_by_country));
        }

        private async Task<string> FillReturnList(SortedSetEntry[] sortedSetEntries)
        {
            var returnList = new List<LeaderBoardModel>();

            for (int i = 0, j = 1; i < sortedSetEntries.Length; i++, j++)
            {
                RedisValue[] userfields = await _redisServer.Database.HashGetAsync(key: sortedSetEntries[i].Element.ToString(), new RedisValue[] { new RedisValue("country").ToString(), new RedisValue("display_name").ToString(), new RedisValue("points").ToString() });

                returnList.Add(new LeaderBoardModel()
                {
                    display_name = userfields[1],
                    rank = j,
                    country = userfields[0],
                    points = Convert.ToDouble(userfields[2]),
                    user_id = null
                });
            }
            var settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            return JsonConvert.SerializeObject(returnList, settings);
        }

        public async Task<string> GetUserById(string user_id)
        {

            // Gets user from Redis
            var user = await _redisServer.Database.HashGetAsync(key: user_id, new RedisValue[] { new RedisValue("country").ToString(), new RedisValue("display_name").ToString(), new RedisValue("points").ToString() });

            //If Redis is empty
            if (!_redisServer.Database.KeyExists(user_id))
            {
                //Gets user from Db
                var userFromDb = await _userService.GetUserById(Guid.Parse(user_id));

                // If user is empty
                if (userFromDb == null)
                {
                    return null;
                }

                // Creates a user hashset in Redis 
                await _redisServer.Database.HashSetAsync(key: userFromDb.user_id, new HashEntry[] { new HashEntry("display_name", userFromDb.display_name.ToString()), new HashEntry("points", userFromDb.points), new HashEntry("country", userFromDb.country), new HashEntry("timestamp", userFromDb.timestamp) });

                // Adds/update Redis global and country leaderboard
                await _redisServer.Database.SortedSetAddAsync(leaderboardkey, member: userFromDb.user_id.ToString(), score: userFromDb.points);

                await _redisServer.Database.SortedSetAddAsync(userFromDb.country, member: userFromDb.user_id.ToString(), score: userFromDb.points);

                // Gets created user from Redis
                user = await _redisServer.Database.HashGetAsync(key: userFromDb.user_id, new RedisValue[] { new RedisValue("country").ToString(), new RedisValue("display_name").ToString(), new RedisValue("points").ToString() });
            }


            string country = user[0].ToString();
            string display_name = user[1].ToString();
            double points = Convert.ToDouble(user[2]);

            var model = new UserModel()
            {
                user_id = user_id.ToString(),
                display_name = display_name,
                points = points,
                country = country

            };
            var settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;

            return JsonConvert.SerializeObject(model, settings);
        }

        public async Task<string> SubmitScoreAsync(ScoreModel scoreModel)
        {
            // Increases user score in Redis
            await _redisServer.Database.HashIncrementAsync(scoreModel.user_id, "points", scoreModel.score_worth);

            // Adds/updates user score timestamp to current time in Redis
            await _redisServer.Database.HashSetAsync(scoreModel.user_id, "timestamp", scoreModel.timestamp.ToString());

            // Gets updated user from Redis
            var user = await _redisServer.Database.HashGetAsync(key: scoreModel.user_id, new RedisValue[] { new RedisValue("country").ToString(), new RedisValue("display_name").ToString(), new RedisValue("points").ToString() });

            // Adds/updates user in global leaderboard 
            await _redisServer.Database.SortedSetAddAsync(leaderboardkey, member: scoreModel.user_id, score: Convert.ToDouble(user[2]));

            // Adds/updates user in country leaderboard
            await _redisServer.Database.SortedSetAddAsync(key: user[0].ToString(), member: scoreModel.user_id, score: Convert.ToDouble(user[2]));

            //Go Db to update user
            await _userService.UpdateUserAsync(scoreModel.user_id, scoreModel.score_worth, scoreModel.timestamp);


            var returnModel = new UserModel()
            {
                display_name = user[1].ToString(),
                points = Convert.ToDouble(user[2]),
                country = user[0].ToString(),
                timestamp = scoreModel.timestamp.ToString()

            };

            var settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;

            return JsonConvert.SerializeObject(returnModel, settings);

        }
    }
}
