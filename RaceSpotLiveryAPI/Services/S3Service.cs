using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.Internal.Transform;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RaceSpotLiveryAPI.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RaceSpotLiveryAPI.Services
{
    public class S3Service : IS3Service
    {
        private readonly string _bucketName;
        private readonly IAmazonS3 _s3Client;

        private ILogger<S3Service> _logger;

        public S3Service(IConfiguration config, ILogger<S3Service> logger)
        {
            _bucketName = config["S3.BucketName"];
            var accessKey = config["S3.AccessKey"];
            var secretKey = config["S3.SecretKey"];
            var bucketRegion = RegionEndpoint.GetBySystemName(config["S3.BucketRegion"]);

            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            _s3Client = new AmazonS3Client(credentials, bucketRegion);
            _logger = logger;
        }

        public string GetPresignedPutUrlForLivery(Livery livery)
        {
            if(livery.LiveryType == LiveryType.SpecMap)
            {
                return GetPresignedPutRequest(GetFileName(livery, "mip"));
            }
            return GetPresignedPutRequest(GetFileName(livery, "tga"));
        }

        public async Task<Stream> GetTgaStreamFromLivery(Livery livery)
        {
            try
            {
                return await GetTgaStream(GetFileName(livery, "tga"));
            } catch(Exception ex)
            {
                _logger.LogError("Issue reading tga from s3");
                return null;
            }
        }

        public async Task DeleteLivery(Livery livery)
        {
            try
            {
                if (livery.LiveryType != LiveryType.SpecMap)
                {
                    var tgaRequest = GetDeleteObjectRequest(GetFileName(livery, "tga"));
                    var tgaResult = await _s3Client.DeleteObjectAsync(tgaRequest);
                    var jpegRequest = GetDeleteObjectRequest(GetFileName(livery, "jpeg"));
                    var jpegResult = await _s3Client.DeleteObjectAsync(jpegRequest);
                } 
                else
                {
                    var mipRequest = GetDeleteObjectRequest(GetFileName(livery, "mip"));
                    var mipResult = await _s3Client.DeleteObjectAsync(mipRequest);
                }
            }
            catch (AmazonS3Exception s3Ex)
            {
                Console.WriteLine("Error encountered ***. Message:'{0}' when writing an object"
                    , s3Ex.Message);
                throw;
            }
        }

        public async Task UploadPreview(Livery livery, Stream jpeg)
        {
            var watch = new Stopwatch();
            _logger.Log(LogLevel.Debug, "Beginning upload process to S3");
            watch.Start();
            var jpgRequest = GetPutRequestForUpload(GetFileName(livery, "jpeg"), jpeg);
            _logger.Log(LogLevel.Debug, $"Elapsed Time after generating put requests to S3: {watch.ElapsedMilliseconds}");
            try
            {
                _logger.Log(LogLevel.Debug, "Beginning jpg upload to S3");
                var result = await _s3Client.PutObjectAsync(jpgRequest);
                _logger.Log(LogLevel.Debug, $"Elapsed Time after jpg upload to S3: {watch.ElapsedMilliseconds}");
            }
            catch (AmazonS3Exception s3Ex)
            {
                Console.WriteLine("Error encountered ***. Message:'{0}' when writing an object"
                    , s3Ex.Message);
                throw;
            }
            watch.Stop();
            _logger.Log(LogLevel.Debug, $"Total Elapsed S3 time: {watch.ElapsedMilliseconds}");
        }

        public string GetPreview(Livery livery)
        {
            if(livery.LiveryType == LiveryType.SpecMap)
            {
                return "";
            }
            return GetPresignedGetRequest(GetFileName(livery, "jpeg"));
        }

        private PutObjectRequest GetPutRequestForUpload(string key, Stream stream)
        {
            var request = new PutObjectRequest()
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = stream,
                AutoCloseStream = true,
            };
            return request;
        }

        private string GetPresignedGetRequest(string key)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = key,
                Expires = DateTime.Now.AddMinutes(60)
            };
            return _s3Client.GetPreSignedURL(request);
        }

        private string GetPresignedPutRequest(string key)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = key,
                Verb = HttpVerb.PUT,
                Expires = DateTime.Now.AddMinutes(60)
            };
            return _s3Client.GetPreSignedURL(request);
        }

        private DeleteObjectRequest GetDeleteObjectRequest(string key)
        {
            var request = new DeleteObjectRequest()
            {
                BucketName = _bucketName,
                Key = key
            };
            return request;
        }
        private string GetFileName(Livery livery, string fileType)
        {
            string id = livery.IsTeam() ? livery.ITeamId : livery.User.IracingId;
            string itemPath;
            string carNumPath = livery.Series.IsLeague ? "_num" : "";
            string teamPath = livery.IsTeam() ? "_team" : "";
            switch (livery.LiveryType)
            {
                case LiveryType.Helmet:
                    itemPath = "helmet";
                    break;
                case LiveryType.Suit:
                    itemPath = "suit";
                    break;
                case LiveryType.SpecMap:
                    itemPath = "car_spec";
                    break;
                case LiveryType.Car:
                default:
                    itemPath = $"car{carNumPath}";
                    break;
            }

            if (livery.LiveryType == LiveryType.Car || livery.LiveryType == LiveryType.SpecMap)
            {
                return $"{livery.SeriesId}/{livery.Car.Path}/{itemPath}{teamPath}_{id}.{fileType}";
            } else
            {
                return $"{livery.SeriesId}/{itemPath}{teamPath}_{id}.{fileType}";
            }
        }

        public async Task<Dictionary<string, string>> GetIracingCredentialsFromS3Async()
        {
            var request = new GetObjectRequest()
            {
                BucketName = _bucketName,
                Key = "iracing.json",
            };

            string responseString = "";
            using (GetObjectResponse response = await _s3Client.GetObjectAsync(request))
            using (Stream responseStream = response.ResponseStream)
            using (StreamReader reader = new StreamReader(responseStream))
            {
                responseString = reader.ReadToEnd();
            }

            var json = JObject.Parse(responseString);
            return json.ToObject<Dictionary<string, string>>();
        }

        public async Task PutIracingCredentialsFromS3Async(Dictionary<string, string> cookies)
        {
            var request = new PutObjectRequest()
            {
                BucketName = _bucketName,
                Key = "iracing.json",
                ContentBody = JsonConvert.SerializeObject(cookies)
            };

            var result = await _s3Client.PutObjectAsync(request);
        }

        private async Task<Stream> GetTgaStream(string key)
        {
            var request = new GetObjectRequest()
            {
                BucketName = _bucketName,
                Key = key
            };

            var returnStream = new MemoryStream();

            using (GetObjectResponse response = await _s3Client.GetObjectAsync(request))
            using (Stream responseStream = response.ResponseStream)
            {
                responseStream.CopyTo(returnStream);
                returnStream.Position = 0;
                return returnStream;
            }
        }
    }
}
