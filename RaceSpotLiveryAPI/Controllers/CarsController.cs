using Microsoft.AspNetCore.Mvc;
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
    [Route("api/[controller]")]
    public class CarsController : ControllerBase
    {
        private readonly RaceSpotDBContext _context;

        public CarsController(RaceSpotDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var cars = _context.Cars.ToList().Select(t => new CarDTO(t)).ToList();
            return Ok(cars);
        }

        [HttpPost]
        public IActionResult Post([FromBody] CarDTO dto)
        {
            Car car = new Car()
            {
                Name = dto.Name,
                Path = dto.Path,
                LogoImgUrl = dto.LogoImgUrl
            };
            _context.Cars.Add(car);
            _context.SaveChanges();
            return Ok(car);
        }

        [HttpPut]
        [Route("{id}")]
        public IActionResult Put([FromBody] CarDTO dto, [FromQuery] Guid id)
        {
            var existing = _context.Cars.FirstOrDefault(s => s.Id == id);
            if (existing == null)
            {
                return NotFound($"Car with id {id} was not found");
            }
            existing.Name = dto.Name;
            existing.Path = dto.Path;
            existing.LogoImgUrl = dto.LogoImgUrl;
            _context.SaveChanges();
            return Ok(existing);
        }
    }
}
