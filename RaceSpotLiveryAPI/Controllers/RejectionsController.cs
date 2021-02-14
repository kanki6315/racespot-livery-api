using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceSpotLiveryAPI.Contexts;
using RaceSpotLiveryAPI.DTOs;
using RaceSpotLiveryAPI.Entities;

namespace RaceSpotLiveryAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableCors("CorsPolicy")]
    public class RejectionsController : ControllerBase
    {
        private readonly RaceSpotDBContext _context;

        public RejectionsController(RaceSpotDBContext context)
        {
            _context = context;
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
        public IActionResult Post([FromBody] RejectionNoticeDTO dto, [FromRoute] Guid liveryId)
        {
            var livery = _context.Liveries.FirstOrDefault(l => l.Id == liveryId);
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
            _context.Rejections.Add(obj);
            livery.IsRejected = true;
            _context.SaveChanges();
            return Ok(new RejectionNoticeDTO(obj));
        }

        [HttpPut]
        [Authorize(Policy = "GlobalAdmin")]
        [Route("~/liveries/{liveryId}/rejections")]
        public IActionResult Put([FromRoute] Guid liveryId, [FromBody] RejectionNoticeDTO dto)
        {
            var livery = _context.Liveries.FirstOrDefault(l => l.Id == liveryId);
            if (livery == null)
            {
                return NotFound($"Could not find livery with id {liveryId}");
            }
            if (livery.IsRejected)
            {
                return BadRequest("Livery is already in the rejected state");
            }
            var rejections = _context.Rejections
                .FirstOrDefault(s => s.LiveryId == liveryId && s.Status != RejectionStatus.Rejected);
            if (rejections == null)
            {
                return BadRequest("Unable to find record of rejection for livery. Contact Support");
            }
            livery.IsRejected = dto.Status != RejectionStatus.Resolved;
            rejections.Status = dto.Status;
            _context.SaveChanges();
            return Ok();
        }
    }
}