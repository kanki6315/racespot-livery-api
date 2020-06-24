using RaceSpotLiveryAPI.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RaceSpotLiveryAPI.Services
{
    public interface IS3Service
    {
        Task UploadLivery(Guid seriesId, string iTeamId, LiveryType type, Stream tga, Stream thumbnail);
        string GetPreview(Guid seriesId, string iTeamId, LiveryType type);
    }
}
