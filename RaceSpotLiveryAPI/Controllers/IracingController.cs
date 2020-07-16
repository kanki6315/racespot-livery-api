using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceSpotLiveryAPI.Models;
using RaceSpotLiveryAPI.Services;

namespace RaceSpotLiveryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableCors("CorsPolicy")]
    public class IracingController : ControllerBase
    {
        private readonly IIracingService _iracingService;

        public IracingController(IIracingService iracingService)
        {
            _iracingService = iracingService;
        }

        [HttpPost]
        [Route("pm")]
        [Authorize(Policy = "GlobalAdmin")]
        public async Task<IActionResult> SendPM([FromBody] IracingPm pm)
        {
            var success = await _iracingService.SendPrivateMessage(pm.UserId, pm.Message);
            if(!success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occured sending a PM. Please contact Support for assistance.");
            }
            return Ok();
        }

        [HttpGet]
        [Route("driver", Name = nameof(GetIracingDriverDetailsById))]
        public async Task<IActionResult> GetIracingDriverDetailsById([FromQuery] string iracingId)
        {
            try
            {
                IracingDriverModel driverDetails = await _iracingService.LookupIracingDriverById(iracingId);
                return Ok(driverDetails);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}