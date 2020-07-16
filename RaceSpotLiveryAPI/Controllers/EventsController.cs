using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceSpotLiveryAPI.Contexts;
using RaceSpotLiveryAPI.DTOs;
using RaceSpotLiveryAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaceSpotLiveryAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableCors("CorsPolicy")]
    public class EventsController : ControllerBase
    {
        private readonly RaceSpotDBContext _context;

        public EventsController(RaceSpotDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var events = _context.Events.ToList().Select(t => new EventDTO(t)).ToList();
            return Ok(events);
        }

        [HttpGet]
        [Route("~/series/{seriesId}/events")]

        public IActionResult GetAllBySeriesId([FromRoute] Guid seriesId)
        {
            var events = _context.Events.Where(e => e.SeriesId == seriesId)
                .ToList().Select(t => new EventDTO(t)).OrderBy(e => e.Order).ToList();
            return Ok(events);
        }

        [HttpPost]
        [Route("~/series/{seriesId}/events")]
        [Authorize(Policy = "GlobalAdmin")]
        public IActionResult Post([FromBody] EventDTO dto)
        {
            Event eventObj = new Event()
            {
                SeriesId = dto.SeriesId,
                RaceTime = dto.RaceTime,
                BroadcastLink = dto.BroadcastLink,
                EventState = dto.EventState,
                Order = dto.Order
            };
            _context.Events.Add(eventObj);
            _context.SaveChanges();
            return Ok(new EventDTO(eventObj));
        }

        [HttpPut]
        [Route("{id}")]
        [Authorize(Policy = "GlobalAdmin")]
        public IActionResult Put([FromBody] EventDTO dto, [FromRoute] Guid id)
        {
            var existing = _context.Events.FirstOrDefault(s => s.Id == id);
            if (existing == null)
            {
                return NotFound($"Event with id {id} was not found");
            }
            existing.RaceTime = dto.RaceTime;
            existing.BroadcastLink = dto.BroadcastLink;
            existing.EventState = dto.EventState;
            existing.Order = dto.Order;
            _context.SaveChanges();
            return Ok(new EventDTO(existing));
        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult GetById([FromRoute] Guid id)
        {
            var existing = _context.Events
                .FirstOrDefault(s => s.Id == id);
            if (existing == null)
            {
                return NotFound($"Event with id {id} was not found");
            }
            return Ok(new EventDTO(existing));
        }
    }
}
