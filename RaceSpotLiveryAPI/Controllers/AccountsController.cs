using Amazon.Runtime.Internal.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using RaceSpotLiveryAPI.Contexts;
using RaceSpotLiveryAPI.DTOs;
using RaceSpotLiveryAPI.Entities;
using RaceSpotLiveryAPI.Models;
using RaceSpotLiveryAPI.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace RaceSpotLiveryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("CorsPolicy")]
    public class AccountsController : ControllerBase
    {
        private SymmetricSecurityKey _secretKey;

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IIracingService _iracingService;

        private readonly RaceSpotDBContext _context;

        private ILogger<AccountsController> _logger;


        public AccountsController(
            IConfiguration configuration,
            RaceSpotDBContext context,
            SignInManager<ApplicationUser> signInManager, 
            UserManager<ApplicationUser> userManager,
            IIracingService iracingService,
            ILogger<AccountsController> logger)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
            _secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Authentication.SigningKey"]));
            _iracingService = iracingService;
            _logger = logger;
        }

        [Route("facebook")]
        public IActionResult LoginFacebook()
        {
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Facebook", Url.Action(nameof(HandleExternalLogin)));
            return Challenge(properties, "Facebook");
        }

        [Route("google")]
        public IActionResult LoginGoogle()
        {
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", Url.Action(nameof(HandleExternalLogin)));
            return Challenge(properties, "Google");
        }

        public async Task<IActionResult> HandleExternalLogin()
        {
            _logger.LogDebug("Starting external Login");
            var info = await _signInManager.GetExternalLoginInfoAsync();
            _logger.LogDebug("got external Login info");
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            _logger.LogDebug("attempted to signin to signin manager");

            if (!result.Succeeded) //user does not exist yet
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                if(String.IsNullOrEmpty(email))
                {
                    return BadRequest("No email address was found for user");
                }
                var user = await _userManager.FindByEmailAsync(email); // verify user doesn't exist on another provider
                if (user == null)
                {
                    var newUser = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                    };
                    var createResult = await _userManager.CreateAsync(newUser);
                    if (!createResult.Succeeded)
                        throw new Exception(createResult.Errors.Select(e => e.Description).Aggregate((errors, error) => $"{errors}, {error}"));

                    await _userManager.AddLoginAsync(newUser, info);
                    var newUserClaims = info.Principal.Claims.Append(new Claim("userId", newUser.Id));
                    await _userManager.AddClaimsAsync(newUser, newUserClaims);
                }
                else
                {
                    await _userManager.AddLoginAsync(user, info);
                }
                String jwtToken = getTokenFromEmail(email);
                return Redirect($"https://racespot.media/#token={jwtToken}");
            }
            else
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                ApplicationUser user = await _userManager.FindByEmailAsync(email);
                String jwtToken = getTokenFromEmail(email);
                _logger.LogDebug("Redirecting");
                Console.WriteLine("Redirecting");
                return Redirect($"https://racespot.media/#token={jwtToken}");
            }
        }
        [HttpPost]
        [Route("logout", Name = "Logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }

        [HttpPost]
        [Route("send-iracing-verification", Name = "SendIracingVerification")]
        [Authorize]
        public async Task<IActionResult> SendIracingVerification([FromBody] IracingIdWrapper wrapper)
        {
            if(String.IsNullOrEmpty(wrapper.IracingId))
            {
                return BadRequest("Iracing Id must be populated");
            }
            var user = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (user == null)
            {
                return Unauthorized();
            }
            if(!String.IsNullOrEmpty(user.IracingId))
            {
                return BadRequest("User has already been verified on iRacing");
            }
            var existingUser = _context.Users.FirstOrDefault(u => u.IracingId == wrapper.IracingId);
            if(existingUser != null)
            {
                return BadRequest($"User with iracing {wrapper.IracingId} has already been created");
            }


            var invite = _context.UserInvites.FirstOrDefault(u => u.UserId == user.Id && u.Status == InviteStatus.SENT);
            if(invite == null)
            {
                var duplicateInvite = 
                    _context.UserInvites.FirstOrDefault(u => u.IracingId == wrapper.IracingId && u.Status == InviteStatus.SENT);

                if (duplicateInvite != null)
                {
                    return BadRequest("Someone else has claimed this iRacing account already. Please contact Support to resolve this");
                }

                invite = new UserInvite
                {
                    UserId = user.Id,
                    Status = InviteStatus.SENT,
                    LastUpdated = DateTime.UtcNow,
                    IracingId = wrapper.IracingId
                };
                _context.UserInvites.Add(invite);
            } 
            else
            {
                DateTime now = DateTime.UtcNow;
                if(invite.LastUpdated > now.AddHours(-24) && invite.LastUpdated <= now)
                {
                    return BadRequest("You must wait 24 hours after a verification message to request an additional one");
                }
                if(invite.IracingId != wrapper.IracingId)
                {
                    return BadRequest("You have an existing invite with a different iRacing Id. Please contact Support to resolve this");
                }
                invite.LastUpdated = DateTime.UtcNow;
            }

            var success = await _iracingService.SendPrivateMessage(wrapper.IracingId,
                $"Click on the link to validate your RaceSpot Liveries Account! https://racespot.media/#key={invite.Id}");
            if(!success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occured sending a PM. Please contact Support for assistance." });
            }
            _context.SaveChanges();
            return Ok();
        }

        [HttpPost]
        [Route("iracing-verification", Name = "IracingVerification")]
        [Authorize]
        public async Task<IActionResult> IracingVerification([FromBody] IracingVerificationWrapper wrapper)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (user == null)
            {
                return Unauthorized();
            }
            if (!String.IsNullOrEmpty(user.IracingId))
            {
                return BadRequest("User has already been verified on iRacing");
            }
            var invite = _context.UserInvites.FirstOrDefault(u => u.Id == wrapper.Key);
            if(invite == null)
            {
                return BadRequest("Could not find verification key associated with invite");
            }
            if(invite.Status == InviteStatus.DONE)
            {
                return BadRequest("Invite has already been used");
            }
            if(invite.UserId != user.Id)
            {
                return Forbid("Invite is for a different user than the one logged in");
            }
            IracingDriverModel driverDetails;
            try
            {
                driverDetails = await  _iracingService.LookupIracingDriverById(invite.IracingId);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occured pulling driver details from iRacing. Please contact Support for assistance." });
            }

            invite.Status = InviteStatus.DONE;
            user.IracingId = invite.IracingId;

            string[] nameSplit = driverDetails.DriverName.Split(" ");
            user.FirstName = nameSplit[0];
            user.LastName = nameSplit[nameSplit.Length - 1];
            _context.SaveChanges();

            return Ok(new UserDTO(user));
        }

        private string getTokenFromEmail(string emailAddress)
        {
            var claims = new Claim[] {
          new Claim(ClaimTypes.Name, emailAddress),
          new Claim(JwtRegisteredClaimNames.Email, emailAddress),
          new Claim(JwtRegisteredClaimNames.Exp, $"{new DateTimeOffset(DateTime.Now.AddDays(1)).ToUnixTimeSeconds()}"),
          new Claim(JwtRegisteredClaimNames.Nbf, $"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}")
        };
            var token = new JwtSecurityToken(
                    new JwtHeader(new SigningCredentials(_secretKey, SecurityAlgorithms.HmacSha256)),
                    new JwtPayload(claims));
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
