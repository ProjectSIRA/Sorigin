﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sorigin.Authorization;
using Sorigin.Models;
using Sorigin.Models.Platforms;
using Sorigin.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sorigin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransferController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IAuthService _authService;
        private readonly SteamService _steamService;
        private readonly DiscordService _discordService;
        private readonly SoriginContext _soriginContext;

        public TransferController(ILogger<TransferController> logger, IAuthService authService, SteamService steamService, DiscordService discordService, SoriginContext soriginContext)
        {
            _logger = logger;
            _authService = authService;
            _steamService = steamService;
            _discordService = discordService;
            _soriginContext = soriginContext;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<User>> Transfer([FromQuery] string grant, [FromQuery] string platform)
        {
            var badGrant = BadRequest(Error.Create("Could not authenticate from the provided grant."));

            User? user = null;
            platform = platform.ToLower();

            if (platform == Platform.Discord.ToStringFast(true))
            {
                string? token = null;
                // If there is no period, it's a code and we need to get an access token from it.
                if (!grant.Contains('.'))
                    token = await _discordService.GetAccessToken(grant);

                if (token is null)
                    return badGrant;

                DiscordUser? discordUserFromAccessToken = await _discordService.GetProfile(token);
                if (discordUserFromAccessToken is null)
                    return badGrant;

                user = await _soriginContext.Users.Include(u => u.Discord).Include(u => u.Steam).FirstOrDefaultAsync(u => u.Discord != null && u.Discord.Id == discordUserFromAccessToken.Id);
                if (user is null)
                    user = new User { ID = default, Discord = discordUserFromAccessToken };
            }
            else if (platform == Platform.Steam.ToStringFast(true))
            {
                // Check in with steam servers so we know who their token belongs to.
                SteamUser? steamUserFromTicket = await _steamService.GetProfile(grant);
                if (steamUserFromTicket is null)
                    return badGrant;

                user = await _soriginContext.Users.Include(u => u.Discord).Include(u => u.Steam).FirstOrDefaultAsync(u => u.Steam != null && u.Steam.Id == steamUserFromTicket.Id);
                if (user is null)
                    user = new User { ID = default, Steam = steamUserFromTicket };
            }

            if (user is null)
                return badGrant;

            var authedUser = (await _authService.GetUser(User.GetID()))!;

            if (authedUser.ID == user.ID)
                return BadRequest(Error.Create("Trying to transfer a user to itself is not allowed!"));

            if (authedUser.Discord == null && user.Discord != null)
            {
                _logger.LogInformation("Migrating a discord account...");
                authedUser.Discord = user.Discord;
                user.Discord = null;
            }
            if (authedUser.Steam == null && user.Steam != null)
            {
                _logger.LogInformation("Migrating a steam account...");
                authedUser.Steam = user.Steam;
                user.Steam = null;
            }
            
            await _soriginContext.SaveChangesAsync();

            if (authedUser.Transfers is null)
                authedUser.Transfers = new List<Guid>();
            authedUser.Transfers.Add(user.ID);

            if (user.Transfers is not null)
                foreach (var id in user.Transfers)
                    authedUser.Transfers.Add(id);

            if (user.ID != default)
                _soriginContext.Users.Remove(user);

            _soriginContext.Transfers.Add(new Transfer
            {
                ID = authedUser.ID,
                TransferID = user.ID
            });

            await _soriginContext.SaveChangesAsync();

            return Ok(authedUser);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<Guid>>> GetOldAccounts([FromQuery] Guid id)
        {
            User? user = await _soriginContext.Users.FirstOrDefaultAsync(u => u.Transfers != null && u.Transfers.Contains(id));
            if (user is null)
                return NotFound();
            return Ok(user.Transfers);
        }
    }
}