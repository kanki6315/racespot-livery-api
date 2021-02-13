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
using RaceSpotLiveryAPI.Entities;

namespace RaceSpotLiveryAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableCors("CorsPolicy")]
    public class UsersController : ControllerBase
    {
        private readonly RaceSpotDBContext _context;

        public UsersController(RaceSpotDBContext context)
        {
            _context = context;
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
