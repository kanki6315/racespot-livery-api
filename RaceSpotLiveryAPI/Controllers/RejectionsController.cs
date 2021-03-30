using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceSpotLiveryAPI.Contexts;
using RaceSpotLiveryAPI.DTOs;
using RaceSpotLiveryAPI.Entities;
using RaceSpotLiveryAPI.Services;

namespace RaceSpotLiveryAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableCors("CorsPolicy")]
    public class RejectionsController : ControllerBase
    {
        private readonly RaceSpotDBContext _context;
        private readonly ISESService _sesService;

        public RejectionsController(RaceSpotDBContext context, ISESService sesService)
        {
            _context = context;
            _sesService = sesService;
        }
        
        [HttpGet]
        [Route("me")]
        public IActionResult GetMyRejections([FromQuery] int offset=0, [FromQuery] int limit=20)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);

            var rejections = _context.Rejections
                .Include(r => r.Livery)
                .ThenInclude(l => l.Series)
                .Where(s => s.Livery.UserId == user.Id && s.Status != RejectionStatus.Resolved)
                .Skip(offset)
                .Take(limit)
                .Select(s => new RejectionNoticeDTO(s))
                .ToList();
            return Ok(rejections);
        }

        [HttpPost]
        [Authorize(Policy = "GlobalAdmin")]
        [Route("~/liveries/{liveryId}/rejections")]
        public async Task<IActionResult> Post([FromBody] RejectionNoticeDTO dto, [FromRoute] Guid liveryId)
        {
            var livery = await _context.Liveries
                .Include(l => l.User)
                .Include(l => l.Series)
                .FirstOrDefaultAsync(l => l.Id == liveryId);
            if (livery == null)
            {
                return NotFound($"Could not find livery with id {dto.LiveryId}");
            }
            if (livery.IsRejected)
            {
                return BadRequest("Livery is already in the rejected state");
            }
            var rejections = _context.Rejections
                .Count(s => s.LiveryId == dto.LiveryId && s.Status == RejectionStatus.Rejected);
            if (rejections > 0)
            {
                return BadRequest("Livery has existing rejections but is not marked as rejected. Contact Support");
            }
            
            var obj = new RejectionNotice()
            {
                LiveryId = liveryId,
                Livery = livery,
                Message = dto.Message,
                Status = RejectionStatus.Rejected
            };
            await _context.Rejections.AddAsync(obj);
            livery.IsRejected = true;
            await _context.SaveChangesAsync();
            await _sesService.SendRejectionEmail(livery);
            return Ok(new RejectionNoticeDTO(obj));
        }

        [HttpPut]
        [Authorize(Policy = "GlobalAdmin")]
        [Route("~/liveries/{liveryId}/rejections")]
        public async Task<IActionResult> Put([FromRoute] Guid liveryId, [FromBody] RejectionNoticeDTO dto)
        {
            var livery = await _context.Liveries
                .Include(l => l.User)
                .Include(l => l.Series)
                .FirstOrDefaultAsync(l => l.Id == liveryId);
            if (livery == null)
            {
                return NotFound($"Could not find livery with id {liveryId}");
            }
            if (!livery.IsRejected)
            {
                return BadRequest("Livery is not in the rejected state");
            }
            var rejections = await _context.Rejections
                .FirstOrDefaultAsync(s => s.LiveryId == liveryId && s.Status != RejectionStatus.Rejected);
            if (rejections == null)
            {
                return BadRequest("Unable to find record of rejection for livery. Contact Support");
            }
            livery.IsRejected = dto.Status != RejectionStatus.Resolved;
            rejections.Status = dto.Status;
            await _context.SaveChangesAsync();

            if (livery.User.IsAgreedToEmails)
            {
                try
                {
                    if (livery.IsRejected)
                    {
                        await _sesService.SendRejectionEmail(livery);
                    }
                    else
                    {
                        await _sesService.SendApprovalEmail(livery);
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
                }
            }

            return Ok();
        }
    }
}