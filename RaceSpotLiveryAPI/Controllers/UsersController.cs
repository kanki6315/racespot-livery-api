using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceSpotLiveryAPI.Contexts;
using RaceSpotLiveryAPI.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RaceSpotLiveryAPI.Entities;
using RaceSpotLiveryAPI.Services;

namespace RaceSpotLiveryAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableCors("CorsPolicy")]
    public class UsersController : ControllerBase
    {
        private readonly RaceSpotDBContext _context;
        private readonly ISESService _sesService;

        public UsersController(RaceSpotDBContext context, ISESService sesService)
        {
            _context = context;
            _sesService = sesService;
        }

        [HttpGet]
        [Route("me")]
        [Authorize]
        public async Task<IActionResult> GetUser()
        {
            var user = await _context.Users
                .Include(u => u.Invite).FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user == null)
            {
                return Unauthorized();
            }

            return Ok(new UserDTO(user));
        }
        
        [HttpPut]
        [Route("me")]
        [Authorize]
        public async Task<IActionResult> PutUser(ApplicationUser userBody)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Invite).FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user == null)
                {
                    return Unauthorized();
                }

                user.IsAgreedToEmails = userBody.IsAgreedToEmails;
                user.LastUpdated = DateTime.UtcNow;
            
                await _context.SaveChangesAsync();
                await _sesService.SendUpdateEmailSettingsNotification(user);
                return Ok(new UserDTO(user));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        
        [HttpGet]
        [Route("unverifiedUsers")]
        [Authorize(Policy = "GlobalAdmin")]
        public IActionResult GetUsers()
        {
            var users = _context.Users
                .Include(u => u.Invite)
                .Where(u => u.Invite != null && u.Invite.Status == InviteStatus.SENT)
                .Select(u => new UserDTO(u))
                .ToList();

            return Ok(users);
        }
        
        [HttpPost]
        [Route("{id}/reset")]
        [Authorize(Policy = "GlobalAdmin")]
        public IActionResult ResetUser([FromRoute] string id)
        {
            var user = _context.Users
                .Include(u => u.Invite)
                .FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return NotFound("Unable to find user to reset invitation");
            }

            _context.UserInvites.Remove(user.Invite);
            _context.SaveChanges();
            return Ok();
        }
    }
}
