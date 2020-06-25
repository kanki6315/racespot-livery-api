using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using RaceSpotLiveryAPI.Entities;
using RaceSpotLiveryAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RaceSpotLiveryAPI.Services
{
    public class IracingService : IIracingService
    {
        private readonly IS3Service _s3Service;
        private readonly HttpClient _httpClient;
        private readonly string _iracingPassword;
        private readonly string _iracingUsername;

        private const string IracingBaseUrl = "members.iracing.com";

        private bool _isLoggedIn;

        private HttpClientHandler _handler;

        public IracingService(IConfiguration configuration, IS3Service s3Service)
        {
            this._s3Service = s3Service;
            _handler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer()
            };
            _httpClient = new HttpClient(_handler);

            _iracingUsername = configuration["Iracing.Username"];
            _iracingPassword = configuration["Iracing.Password"];

            _isLoggedIn = false;
        }

        public async Task<bool> SendPrivateMessage(string userId, string message)
        {
            if (!_isLoggedIn)
            {
                try
                {
                    await Login();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("action", "sendSave"),
                new KeyValuePair<string, string>("module", "pm"),
                new KeyValuePair<string, string>("preview", "0"),
                new KeyValuePair<string, string>("start", ""),
                new KeyValuePair<string, string>("toUsername", ""),
                new KeyValuePair<string, string>("toUserId", userId),
                new KeyValuePair<string, string>("disa1ble_html", "on"),
                new KeyValuePair<string, string>("attach_sig", "on"),
                new KeyValuePair<string, string>("addbbcode24", "pm"),
                new KeyValuePair<string, string>("addbbcode26", "pm"),
                new KeyValuePair<string, string>("subject", "Livery API Test"),
                new KeyValuePair<string, string>("message", message)
            });

            var response = await _httpClient.PostAsync(ConstructRequestUrl("jforum/jforum.page"), formContent);
            return response.IsSuccessStatusCode;
        }

        public async Task<IracingDriverModel> LookupIracingDriverById(string id)
        {
            if(!_isLoggedIn)
            {
                try
                {
                    await Login();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            var getNameUrl = ConstructRequestUrl(
                "membersite/member/GetDriverStatus",
                new Dictionary<string, string> { { "searchTerms", id } });

            var nameResponse = await _httpClient.GetStringAsync(getNameUrl);
            var nameJson = JObject.Parse(nameResponse);

            if (nameJson["searchRacers"][0]["name"].ToString() == "")
            {
                throw new Exception($"Could not find driver with id {id}");
            }
            var getDetailsUrl = ConstructRequestUrl(
                "memberstats/member/GetDriverStats",
                new Dictionary<string, string>
                {
                    {"search", nameJson["searchRacers"][0]["name"].ToString()},
                    {"sort", "irating"},
                    {"order", "desc"},
                    {"category", "2"},
                    {"upperbound", "1" }
                });

            var detailResponse = await _httpClient.GetStringAsync(getDetailsUrl);
            var nfo = MapToNFOs(detailResponse).ToList();
            return new IracingDriverModel
            {
                DriverName = HttpUtility.UrlDecode(nfo[0].GetValueOrDefault("displayname")).Replace("+", " "),
                DriverId = nfo[0].GetValueOrDefault("custid"),
                DriverIrating = nfo[0].GetValueOrDefault("irating"),
                LicenseLevel = nfo[0].GetValueOrDefault("licenseclass").Split("+")[0],
                SafetyRating = nfo[0].GetValueOrDefault("licenseclass").Split("+")[1]
            };
        }

        public async Task<IracingTeamModel> LookupIracingTeamById(string id, bool findDrivers)
        {
            if (!_isLoggedIn)
            {
                try
                {
                    await Login();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            var getTeamUrl = ConstructRequestUrl(
                "membersite/member/GetTeamDirectory",
                new Dictionary<string, string> { { "search", id } });
            var teamResponse = await _httpClient.GetStringAsync(getTeamUrl);
            var teamNfo = MapToNFOs(teamResponse).ToList();

            var team = new IracingTeamModel
            {
                TeamName = teamNfo[0].GetValueOrDefault("teamname").Replace("+", " "),
                TeamId = teamNfo[0].GetValueOrDefault("teamid").Replace("-", ""),
                NumDrivers = teamNfo[0].GetValueOrDefault("rostercount"),
                TeamOwner = teamNfo[0].GetValueOrDefault("displayname"),
                TeamOwnerId = teamNfo[0].GetValueOrDefault("custid"),
                Drivers = new List<IracingDriverModel>()
            };

            if (findDrivers)
            {
                var getDriversUrl = ConstructRequestUrl(
                    "membersite/member/GetTeamMembers",
                    new Dictionary<string, string> { { "teamid", $"-{team.TeamId}" } });
                var driversResponse = await _httpClient.GetStringAsync(getDriversUrl);
                var driversJson = JArray.Parse(driversResponse);
                foreach (JObject driver in driversJson)
                {
                    try
                    {
                        IracingDriverModel driverModel = await LookupIracingDriverById(driver["custID"].ToString());
                        team.Drivers.Add(driverModel);
                    }
                    catch (Exception)
                    {
                        team.NumDrivers = (Int32.Parse(team.NumDrivers) - 1).ToString();
                    }
                }
            }

            return team;
        }



        private async Task Login()
        {
            if (await CheckLoginStatus())
            {
                _isLoggedIn = true;
                return;
            }

            var cookies = await _s3Service.GetIracingCredentialsFromS3Async();
            Uri uri = new Uri("https://members.iracing.com");
            foreach (var cookie in cookies)
            {
                _handler.CookieContainer.Add(uri, 
                    new Cookie(cookie.Key, cookie.Value));
            }

            var cookiesAccepted = await CheckLoginStatus();
            if(cookiesAccepted)
            {
                _isLoggedIn = true;
                return;
            }

            await SubmitLoginAsync();
        }

        private async Task SubmitLoginAsync()
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", _iracingUsername),
                new KeyValuePair<string, string>("password", _iracingPassword),
                new KeyValuePair<string, string>("utcoffset", "420"),
                new KeyValuePair<string, string>("todaysdate", "")
            });
            var url = ConstructRequestUrl(
                "download/Login");

            var response = await _httpClient.PostAsync(url, formContent);

            _isLoggedIn = await CheckLoginStatus();
            if (!_isLoggedIn)
            {
                throw new Exception("Unable to connect to iRacing service at this time. Please try again later.");
            }

            var newCookies = new Dictionary<string, string>();
            foreach (var cookie in _handler.CookieContainer.GetCookies(new Uri("https://members.iracing.com")))
            {
                string[] cookieSplit = cookie.ToString().Split("=");
                newCookies.Add(cookieSplit[0], cookieSplit[1]);
            }
            await _s3Service.PutIracingCredentialsFromS3Async(newCookies);
        }

        private async Task<bool> CheckLoginStatus()
        {
            var url = ConstructRequestUrl(
                "membersite/member/GetDriverCounts",
                new Dictionary<string, string>
                {
                    {"invokedby", "racepanel"}
                });

            try
            {
                var response = await _httpClient.GetStringAsync(url);
                var json = JObject.Parse(response);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static IEnumerable<Dictionary<string, string>> MapToNFOs(string json)
        {
            var root = JObject.Parse(json);

            var metaFieldMap = root["m"].Values<JProperty>()
                .ToDictionary(field => field.Name, field => field.Value.ToString());

            var dataNodes = root["d"]["r"].Values<JObject>();

            return dataNodes.Select(obj =>
                obj.Values<JProperty>().Select(p => new { key = metaFieldMap[p.Name], value = p.Value.ToString() })
                    .ToDictionary(t => t.key, t => t.value));
        }

        private static string ConstructRequestUrl(string path, Dictionary<string, string> queryParams)
        {
            var builder = new UriBuilder("https", IracingBaseUrl) { Path = path };

            var query = HttpUtility.ParseQueryString(builder.Query);
            foreach (var (key, value) in queryParams)
            {
                query[key] = value;
            }

            builder.Query = HttpUtility.UrlDecode(query.ToString());
            return builder.ToString();
        }

        private static string ConstructRequestUrl(string path)
        {
            var builder = new UriBuilder("https", IracingBaseUrl) { Path = path };
            return builder.ToString();
        }
    }
}
