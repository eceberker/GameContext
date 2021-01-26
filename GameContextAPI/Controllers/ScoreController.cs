using GameContextAPI.CacheService;
using GameContextAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GameContextAPI.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class ScoreController : ControllerBase
    {
        private ICacheService _cacheService;

        public ScoreController( ICacheService cacheService)
        {

            _cacheService = cacheService;
        }

        /// <summary>
        /// Updates score of the given user 
        /// </summary>
        /// <param name="model"></param>
        /// <remarks>
        /// Sample Request
        /// 
        ///     POST /score/submit
        ///     {
        ///          "score_worth": 20,
        ///          "user_id": "7be24bf6-1f9b-4862-9e99-3b016802918d"
        ///      }
        ///
        /// 
        /// 
        /// timestamp is updated automatically when a score submitted
        /// </remarks>
        /// <response code="200">Returns updated user</response>
        /// <response code="404">Returns when user is not found</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("[action]")]
        public async Task<IActionResult> Submit([FromBody] ScoreModel model)
        {
            var scoreModel = new ScoreModel()
            {
                score_worth = model.score_worth,
                user_id = model.user_id,
                timestamp = (DateTime.Now).ToString("yyyymmddhhmmss")
            };


            var user = await _cacheService.SubmitScoreAsync(scoreModel);
            if (user != null)
            {
                return Ok(user);
            }
            return NotFound();
        }
    }
}
