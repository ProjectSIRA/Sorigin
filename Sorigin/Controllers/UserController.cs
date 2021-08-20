using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sorigin.Authorization;
using Sorigin.Models;
using System;
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

        [HttpGet("by-username/{name}")]
        public async Task<ActionResult<User>> GetUserByUsername(string name)
        {
            name = name.ToLower();
            User? user = await _soriginContext.Users.Include(u => u.Discord).Include(u => u.Steam).FirstOrDefaultAsync(u => u.Username.ToLower() == name);
            if (user is null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpGet("by-id/{id}")]
        public async Task<ActionResult<User>> GetUserByUsername(Guid id)
        {
            User? user = await _soriginContext.Users.Include(u => u.Discord).Include(u => u.Steam).FirstOrDefaultAsync(u => u.ID == id);
            if (user is null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpGet("by-steam/{id}")]
        public async Task<ActionResult<User>> GetSteamUser(string id)
        {
            User? user = await _soriginContext.Users.Include(u => u.Discord).Include(u => u.Steam).FirstOrDefaultAsync(u => u.Steam != null && u.Steam.Id == id);
            if (user is null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [Authorize]
        [HttpPost("edit/description")]
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

        [Authorize]
        [HttpPost("edit/username")]
        public async Task<ActionResult<User>> ChangeUsername([FromBody] ChangeUsernameBody body)
        {
            if (string.IsNullOrWhiteSpace(body.Username))
            {
                return BadRequest(Error.Create("Username cannot be empty."));
            }
            string lUsername = body.Username.ToLower();
            User user = (await _authService.GetUser(User.GetID()))!;
            User? existing = await _soriginContext.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == lUsername);
            if (existing is not null)
            {
                return BadRequest(Error.Create("The username is already in use."));
            }
            user.Username = body.Username;
            await _soriginContext.SaveChangesAsync();
            return Ok(user);
        }

        public class EditDescriptionBody
        {
            public string? NewDescription { get; set; } = null!;
        }

        public class ChangeUsernameBody
        {
            public string Username { get; set; } = null!;
        }
    }
}