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
    [Route("api/[controller]")]
    [EnableCors("CorsPolicy")]
    public class SeriesController : ControllerBase
    {
        private readonly RaceSpotDBContext _context;

        public SeriesController(RaceSpotDBContext context)
        {
            _context = context;
        }


        [HttpGet]
        public IActionResult GetAllActiveSeries()
        {
            var series = _context.Series.Where(s => !s.IsArchived)
                    .ToList().Select(s => new SeriesDTO(s)).ToList();
            return Ok(series);
        }

        [HttpPost]
        [Authorize(Policy = "GlobalAdmin")]
        public IActionResult Post([FromBody] SeriesDTO dto)
        {
            var cars = dto.CarIds.Count == 0 ? new List<Car>() :_context.Cars.Where(c => dto.CarIds.Contains(c.Id)).ToList();
            if(cars.Count != dto.CarIds.Count)
            {
                return BadRequest("Unable to find all the car ids for the series");
            }
            Series series = new Series()
            {
                Name = dto.Name,
                IsTeam = dto.IsTeam,
                IsArchived = dto.IsArchived,
                LastUpdated = DateTime.UtcNow,
                LogoImgUrl = dto.LogoImgUrl,
                Description = dto.Description
            };
            
            _context.Series.Add(series);
            _context.SaveChanges();

            if (cars.Count > 0)
            {
                var list = new List<SeriesCar>();
                foreach(var car in cars)
                {
                    SeriesCar seriesCar = new SeriesCar
                    {
                        Series = series,
                        SeriesId = series.Id,
                        Car = car,
                        CarId = car.Id
                    };
                    list.Add(seriesCar);
                }
                _context.AddRangeAsync(list);
                _context.SaveChangesAsync();
                series.SeriesCars = list;
            }

            return Ok(new SeriesDTO(series));
        }

        [HttpPut]
        [Route("{id}")]
        [Authorize(Policy = "GlobalAdmin")]
        public IActionResult Put([FromBody] SeriesDTO dto, [FromRoute] Guid id)
        {
            var existing = _context.Series
                .Include(s => s.SeriesCars).ThenInclude(s => s.Car).FirstOrDefault(s => s.Id == id);
            if(existing == null)
            {
                return NotFound($"Series with id {id} was not found");
            }
            existing.Name = dto.Name;
            existing.IsArchived = dto.IsArchived;
            existing.LastUpdated = DateTime.UtcNow;
            existing.LogoImgUrl = dto.LogoImgUrl;
            existing.Description = dto.Description;
            _context.SaveChanges();
            return Ok(new SeriesDTO(existing));
        }

        [HttpPut]
        [Route("{id}/cars")]
        [Authorize(Policy = "GlobalAdmin")]
        public IActionResult PutCars([FromRoute] Guid id, [FromBody] List<Guid> carIds)
        {
            var existing = _context.Series.FirstOrDefault(s => s.Id == id);
            if (existing == null)
            {
                return NotFound($"Series with id {id} was not found");
            }
            var existingSeriesCars = _context.SeriesCars.Where(s => s.SeriesId == id).Include(c => c.Car);
            var seriesCars = new List<SeriesCar>();
            foreach (var seriesCar in existingSeriesCars)
            {
                if (!carIds.Contains(seriesCar.CarId))
                {
                    _context.SeriesCars.Remove(seriesCar);
                } 
                else
                {
                    seriesCars.Add(seriesCar);
                }
            }

            var newCarIds = carIds.Where(c => existingSeriesCars.Where(s => s.CarId == c).Count() == 0).ToList();
            var cars = _context.Cars.Where(c => newCarIds.Contains(c.Id)).ToList();
            foreach (var car in cars)
            {
                SeriesCar seriesCar = new SeriesCar
                {
                    Series = existing,
                    SeriesId = existing.Id,
                    Car = car,
                    CarId = car.Id
                };
                _context.SeriesCars.Add(seriesCar);
                seriesCars.Add(seriesCar);
            }
            _context.SaveChanges();
            existing.SeriesCars = seriesCars;
            return Ok(new SeriesDTO(existing));
        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult GetById([FromRoute] Guid id)
        {
            var existing = _context.Series
                .Include(s => s.SeriesCars)
                .ThenInclude(s => s.Car)
                .Include(s => s.Events)
                .FirstOrDefault(s => s.Id == id);
            if (existing == null)
            {
                return NotFound($"Series with id {id} was not found");
            }
            return Ok(new SeriesDTO(existing));
        }
    }
}

