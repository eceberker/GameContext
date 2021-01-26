using GameContextAPI.CacheService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GameContextAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LeaderBoardController : ControllerBase
    {
        private ICacheService _cacheService;

        public LeaderBoardController(ICacheService cacheService)
        {

            _cacheService = cacheService;
        }

        /// <summary>
        /// Gets top 20 user in global leaderboard.
        /// </summary>
        /// <remarks>
        /// 
        /// GET /leaderboard
        /// </remarks>
        /// <returns></returns>
        /// <response code="200">Returns Top 20 user in Leaderboard</response>
        /// <response code="204">Returns when leaderboard is null</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Get()
        {
            var items = await _cacheService.GetLeaderBoardAsync();

            if (items != null)
            {
                return Ok(items);
            }
            return NoContent();
        }


        /// <summary>
        /// Gets top 20 user in country leaderboard for given country ISO Code.
        /// </summary>
        /// <remarks>
        /// e.g. GET leaderboard/tr
        /// </remarks>
        /// <param name="country_iso_code"></param>
        /// <returns></returns>
        /// <response code="200">Returns Top 20 user in country leaderboard</response>
        /// <response code="204">Returns when leaderboard is null</response>
        /// <response code="404">Returns when country ISO is wrong</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("{country_iso_code}")]
        public async Task<IActionResult> Get([FromRoute] string country_iso_code)

        {

            if (!string.IsNullOrEmpty(country_iso_code))
            {
                var board = await _cacheService.GetLeaderBoardByCountryAsync(country_iso_code);
                if (board == null)
                {
                    return NoContent();
                }
                return Ok(board);
            }
            return NotFound();
        }

    }
}
