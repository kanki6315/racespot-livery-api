﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RaceSpotLiveryAPI.Contexts;
using RaceSpotLiveryAPI.DTOs;
using RaceSpotLiveryAPI.Entities;
using RaceSpotLiveryAPI.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RaceSpotLiveryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("LocalDev")]
    public class LiveriesController : ControllerBase
    {
        private readonly RaceSpotDBContext _context;
        private readonly IS3Service _s3Service;
        private readonly IIracingService _iracingService;
        private ILogger<LiveriesController> _logger;

        public LiveriesController(
            RaceSpotDBContext context,
            IS3Service s3Service,
            IIracingService iracingService,
            ILogger<LiveriesController> logger)
        {
            _context = context;
            _s3Service = s3Service;
            _iracingService = iracingService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var liveries = _context.Liveries.ToList().Select(t => new LiveryDTO(t)).ToList();
            return Ok(liveries);
        }

        [HttpGet]
        [Route("~/api/series/{seriesId}/liveries")]
        public IActionResult GetAllForSeries([FromRoute] Guid seriesId)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (user == null)
            {
                return Unauthorized();
            }

            List<LiveryDTO> liveries;
            if (user.IsAdmin)
            {
                liveries = _context.Liveries.Where(l => l.SeriesId == seriesId).Include(l => l.Car)
                .ToList().Select(t => new LiveryDTO(t, _s3Service.GetPreview(t))).ToList();
            }
            else
            {
                liveries = _context.Liveries.Where(l => l.SeriesId == seriesId && l.UserId == user.Id).Include(l => l.Car)
                    .ToList().Select(t => new LiveryDTO(t, _s3Service.GetPreview(t))).ToList();
            }
            return Ok(liveries);
        }

        [HttpPost]
        [Route("~/api/series/{seriesId}/liveries")]
        public async Task<IActionResult> Post([FromRoute] Guid seriesId, [FromBody] LiveryDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user == null)
            {
                return Unauthorized();
            }
            else if (String.IsNullOrEmpty(user.IracingId))
            {
                return Forbid("User has not verified iRacing account yet");
            }
            var series = await _context.Series.FirstOrDefaultAsync(s => s.Id == seriesId);
            if (series == null)
            {
                return NotFound($"Could not find series with id {seriesId}");
            }
            if (series.IsTeam && String.IsNullOrEmpty(dto.ITeamId))
            {
                return BadRequest("Series is a team series but no Team ID was provided");
            }
            if (!series.IsTeam && !String.IsNullOrEmpty(dto.ITeamId))
            {
                return BadRequest("Series is not a team series but Team ID was provided");
            }

            Livery livery;
            if (String.IsNullOrEmpty(dto.ITeamId))
            {
                livery = _context.Liveries.Include(l => l.Car).FirstOrDefault(l => l.SeriesId == seriesId
                    && l.CarId == dto.CarId && l.UserId == user.Id && l.LiveryType == dto.LiveryType);
            }
            else
            {
                livery = _context.Liveries.Include(l => l.Car).FirstOrDefault(l => l.SeriesId == seriesId
                    && l.CarId == dto.CarId && l.ITeamId == dto.ITeamId && l.LiveryType == dto.LiveryType);
            }
            if (livery == null)
            {
                string teamName = "";
                if (!String.IsNullOrEmpty(dto.ITeamId))
                {
                    try
                    {
                        var iTeam = await _iracingService.LookupIracingTeamById(dto.ITeamId, false);
                        if (iTeam.TeamOwnerId != user.IracingId)
                        {
                            return BadRequest("User is not owner of iRacing Team. Please ask Team Owner to upload livery");
                        }
                        teamName = iTeam.TeamName;
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError,
                            new { message = "Unable to verify user owns team. Please contact Support for assistance." });
                    }
                }
                var car = await _context.Cars.Include(c => c.SeriesCars).FirstOrDefaultAsync(c => c.Id == dto.CarId);
                if (car == null)
                {
                    return NotFound($"Could not find car with id {dto.CarId}");
                }
                if (car.SeriesCars.Where(s => s.SeriesId == seriesId).Count() == 0)
                {
                    return BadRequest("Car is not in series");
                }

                livery = new Livery()
                {
                    SeriesId = seriesId,
                    CarId = dto.CarId,
                    ITeamId = dto.ITeamId,
                    ITeamName = teamName,
                    LiveryType = dto.LiveryType,
                    User = user,
                    UserId = user.Id,
                    Status = UploadStatus.WAITING
                };
                _context.Liveries.Add(livery);
                _context.SaveChanges();
            }
            else
            {
                if (!String.IsNullOrEmpty(dto.ITeamId) && livery.UserId != user.Id)
                {
                    return BadRequest("You are not the original uploader of the livery");
                }
                livery.Status = UploadStatus.WAITING;
                _context.SaveChanges();
            }

            LiveryDTO returnDto = new LiveryDTO(livery);
            returnDto.UploadUrl = _s3Service.GetPresignedPutUrlForLivery(livery);
            return Ok(returnDto);
        }

        [HttpPost]
        [Route("{id}/finalize")]
        public async Task<IActionResult> FinalizeLivery([FromRoute] Guid id)
        {
            var livery = _context.Liveries.Include(l => l.Car)
                .Include(l => l.User).FirstOrDefault(l => l.Id == id);
            if (livery == null)
            {
                return NotFound($"Unable to find livery with id {id}");
            }
            if (livery.Status != UploadStatus.WAITING)
            {
                return BadRequest("Livery is not in Waiting status");
            }

            var watch = new Stopwatch();
            _logger.Log(LogLevel.Debug, "Beginning tga to img conversion");
            watch.Start();

            var tgaStream = await _s3Service.GetTgaStreamFromLivery(livery);
            var outputStream = new MemoryStream();
            tgaStream.Position = 0;
            using (var image = Image.Load(tgaStream))
            {
                image.SaveAsJpeg(outputStream, new JpegEncoder() { Quality = 80 });
            }
            _logger.Log(LogLevel.Debug, $"Elapsed Time after converting to jpeg: {watch.ElapsedMilliseconds}");
            outputStream.Position = 0;
            _logger.Log(LogLevel.Debug, $"Elapsed Time after resetting stream: {watch.ElapsedMilliseconds}");
            try
            {
                await _s3Service.UploadPreview(livery, outputStream);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occured uploading the file. Please contact Support for assistance." });
            }
            livery.Status = UploadStatus.UPLOADED;
            await _context.SaveChangesAsync();

            return Ok(new LiveryDTO(livery, _s3Service.GetPreview(livery)));
        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult GetById([FromRoute] Guid id)
        {
            var livery = _context.Liveries.Include(l => l.Car)
                .Include(l => l.User).FirstOrDefault(l => l.Id == id);
            if(livery == null)
            {
                return NotFound($"Unable to find livery with id {id}");
            }

            return Ok(new LiveryDTO(livery, _s3Service.GetPreview(livery)));
        }

        /*[HttpGet]
        [Route("~/api/series/{seriesId}/liveries")]
        public IActionResult GetByITeamId([FromRoute] Guid seriesId, [FromQuery] LiveryType type, [FromQuery] string iTeamId)
        {
            var livery = _context.Liveries.FirstOrDefault(l => l.SeriesId == seriesId
                && l.ITeamId == iTeamId && l.LiveryType == type);
            if(livery == null)
            {
                return NotFound("Did not find livery matching query");
            }

            string previewUrl = _s3Service.GetPreview(seriesId, iTeamId, type);
            return Ok(new LiveryDTO(livery, previewUrl));
        }*/
    }
}
