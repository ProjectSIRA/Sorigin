using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sorigin.Authorization;
using Sorigin.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sorigin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IAuthService _authService;
        private readonly SoriginContext _soriginContext;

        public UserController(ILogger<UserController> logger, IAuthService authService, SoriginContext soriginContext)
        {
            _logger = logger;
            _authService = authService;
            _soriginContext = soriginContext;
        }

        [HttpGet]
        public async Task<IEnumerable<string>> AllUsernames()
        {
            _logger.LogInformation("Fetching all usernames.");
            return (await _soriginContext.Users.ToArrayAsync()).Select(s => s.Username);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<User>> EditDescription([FromBody] EditDescriptionBody body)
        {
            User user = (await _authService.GetUser(User.GetID()))!;
            if (string.IsNullOrEmpty(body.NewDescription))
            {
                user.Bio = null;
                await _soriginContext.SaveChangesAsync();
                return Ok(user);
            }
            if (body.NewDescription.Length > 2000)
            {
                return BadRequest(Error.Create("Description is too long!"));
            }
            user.Bio = body.NewDescription;
            await _soriginContext.SaveChangesAsync();
            return Ok(user);
        }

        public class EditDescriptionBody
        {
            public string? NewDescription { get; set; } = null!;
        }
    }
}