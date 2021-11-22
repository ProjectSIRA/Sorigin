using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sorigin.Models;
using Sorigin.Services;

namespace Sorigin.Controllers;
[Route("api/users")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [Authorize]
    [HttpGet("@me")]
    public async Task<ActionResult<User>> GetSelf()
    {
        User user = (await _userService.GetUser(User.GetID().GetValueOrDefault()))!;
        return Ok(user);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetUser(ulong id)
    {
        User? user = await _userService.GetUser(id);
        if (user is null)
        {
            return NotFound(new Error("no-user", $"Could not find a user with the ID {id}."));
        }
        return Ok(user);
    }
}