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
        Task UploadLivery(Livery livery, Stream tga, Stream thumbnail);
        string GetPreview(Livery livery);

        Task PutIracingCredentialsFromS3Async(Dictionary<string, string> cookies);
        Task<Dictionary<string, string>> GetIracingCredentialsFromS3Async();
    }
}
