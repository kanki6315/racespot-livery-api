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
        string GetPresignedPutUrlForLivery(Livery livery);
        Task<Stream> GetTgaStreamFromLivery(Livery livery);
        Task UploadPreview(Livery livery, Stream thumbnail);
        string GetPreview(Livery livery);

        Task PutIracingCredentialsFromS3Async(Dictionary<string, string> cookies);
        Task<Dictionary<string, string>> GetIracingCredentialsFromS3Async();
    }
}
