using System.Threading.Tasks;
using apirest.Services;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v{version:apiVersion}/user")]
    public class UserController : ControllerBase
    {
        private IUserService userService;

        public UserController(IUserService userService)
        {
            this.userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticationModel model)
        {
            var user = await userService.Authenticate(model.Username, model.Password);

            if (user == null)
            {
                return BadRequest("Username or password is incorrect.");
            }

            return Ok(user);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await userService.GetAll();
            return Ok(users);
        }
    }
}