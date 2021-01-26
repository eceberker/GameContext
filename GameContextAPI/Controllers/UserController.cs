using GameContextAPI.CacheService;
using GameContextAPI.Models;
using GameContextAPI.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GameContextAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICacheService _cacheService;
        public UserController(IUserService userService, ICacheService cacheService)
        {
            _userService = userService;
            _cacheService = cacheService;
        }
        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="user"></param>
        /// <remarks>
        /// Sample request:
        /// 
        /// 
        /// 
        ///     POST /user/create
        ///     {
        ///          "points": 20.0,
        ///          "display_name": "dae34",
        ///          "country": "en"
        ///      }
        ///
        /// 
        /// user_id is generated automatically for user
        /// timestamp is generated automatically for user
        /// 
        /// </remarks>
        /// <returns>Returns the newly created user</returns>
        /// <response code="201">Returns the newly created item</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create([FromBody] UserModel user)
        {
            if (user == null)
            {
                return NotFound();
            }
            UserModel userModel = new UserModel()
            {
                display_name = user.display_name,
                points = user.points,
                country = user.country,
                user_id = Guid.NewGuid().ToString(),
                timestamp = DateTime.MinValue.ToString("yyyymmddhhmmss")
            };

            return Ok(await _cacheService.CreateUser(userModel));

        }

        /// <summary>
        /// Gets user profile by id sent from route.
        /// </summary>
        /// <remarks>
        /// e.g. GET /user/profile/7be24bf6-1f9b-4862-9e99-3b016802918d
        /// </remarks>
        /// <param name="user_id"></param>
        /// <response code="200">Returns requested user</response>
        /// <response code="404">Returns when user_id is null</response>   
        /// <response code="204">Returns when user is not found</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("{user_id}")]
        public async Task<IActionResult> Profile([FromRoute] string user_id)
        {
            if (user_id == null)
            {
                return NotFound();
            }
            var user = await _cacheService.GetUserById(user_id);

            if (user == null)
            {
                return NoContent();
            }

            return Ok(user);

        }
    }
}
