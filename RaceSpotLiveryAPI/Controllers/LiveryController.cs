using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RaceSpotLiveryAPI.Contexts;
using RaceSpotLiveryAPI.DTOs;
using RaceSpotLiveryAPI.Entities;
using RaceSpotLiveryAPI.Services;
using SixLabors.ImageSharp;
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
    public class LiveryController : ControllerBase
    {
        private readonly RaceSpotDBContext _context;
        private readonly IS3Service _s3Service;

        private ILogger<LiveryController> _logger;

        public LiveryController(RaceSpotDBContext context, IS3Service s3Service, ILogger<LiveryController> logger)
        {
            _context = context;
            _s3Service = s3Service;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var liveries = _context.Liveries.ToList().Select(t => new LiveryDTO(t)).ToList();
            return Ok(liveries);
        }

        [HttpPost]
        [Route("~/api/series/{seriesId}/liveries")]
        public async Task<IActionResult> Post([FromRoute] Guid seriesId, IFormFile file, [FromForm] LiveryType type, [FromForm] string iTeamId)
        {
            var series = _context.Series.FirstOrDefault(s => s.Id == seriesId);
            if (series == null)
            {
                return BadRequest($"Unable to find Series with id {seriesId}");
            }

            var livery = _context.Liveries.FirstOrDefault(l => l.SeriesId == seriesId
                    && l.ITeamId == iTeamId && l.LiveryType == type);
            if (livery == null)
            {
                livery = new Livery()
                {
                    SeriesId = seriesId,
                    ITeamId = iTeamId,
                    LiveryType = type
                };
                _context.Liveries.Add(livery);
                _context.SaveChanges();
            }

            var watch = new Stopwatch();
            _logger.Log(LogLevel.Debug, "Beginning tga to img conversion");
            watch.Start();

            var tgaStream = file.OpenReadStream();
            var outputStream = new MemoryStream();
            using (var image = Image.Load(tgaStream))
            {
                image.SaveAsJpeg(outputStream, new JpegEncoder() { Quality = 80 });
            }
            _logger.Log(LogLevel.Debug, $"Elapsed Time after converting to jpeg: {watch.ElapsedMilliseconds}");
            tgaStream.Position = 0;
            outputStream.Position = 0;
            _logger.Log(LogLevel.Debug, $"Elapsed Time after resetting streams: {watch.ElapsedMilliseconds}");
            try
            {
                await _s3Service.UploadLivery(seriesId, iTeamId, type, tgaStream, outputStream);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occured uploading the file. Please contact Support for assistance." });
            }

            return Ok(new LiveryDTO(livery));
        }

        [HttpGet]
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
        }
    }
}
