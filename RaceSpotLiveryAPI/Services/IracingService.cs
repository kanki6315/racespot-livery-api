using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using RaceSpotLiveryAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace RaceSpotLiveryAPI.Services
{
    public class IracingService : IIracingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _iracingPassword;
        private readonly string _iracingUsername;

        private const string IracingBaseUrl = "members.iracing.com";

        private bool _isLoggedIn;

        public IracingService(IConfiguration configuration)
        {
            var handler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer()
            };
            _httpClient = new HttpClient(handler);

            _iracingUsername = configuration["Iracing.Username"];
            _iracingPassword = configuration["Iracing.Password"];

            _isLoggedIn = false;
        }

        public async Task<bool> SendPrivateMessage(string username, string message)
        {
            try
            {
                await Login();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("action", "sendSave"),
                new KeyValuePair<string, string>("module", "pm"),
                new KeyValuePair<string, string>("preview", "0"),
                new KeyValuePair<string, string>("start", ""),
                new KeyValuePair<string, string>("toUsername", username),
                new KeyValuePair<string, string>("toUserId", ""),
                new KeyValuePair<string, string>("disa1ble_html", "on"),
                new KeyValuePair<string, string>("attach_sig", "on"),
                new KeyValuePair<string, string>("addbbcode24", "pm"),
                new KeyValuePair<string, string>("addbbcode26", "pm"),
                new KeyValuePair<string, string>("subject", "Livery API Test"),
                new KeyValuePair<string, string>("message", message)
            });

            var response = await _httpClient.PostAsync(ConstructForumPost(), formContent);
            return response.IsSuccessStatusCode;
        }

        public async Task<IracingDriverModel> LookupIracingDriverById(string id)
        {
            try
            {
                await Login();
            }
            catch (Exception ex)
            {
                throw ex;
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

        private async Task Login()
        {
            if (_isLoggedIn)
            {
                return;
            }

            var url = ConstructRequestUrl(
                "download/Login",
                new Dictionary<string, string>
                {
                    {"username", _iracingUsername},
                    {"password", _iracingPassword}
                });

            var response = await _httpClient.PostAsync(url, null);

            _isLoggedIn = response.StatusCode == HttpStatusCode.Found;
            if (!_isLoggedIn)
            {
                throw new Exception("Unable to login");
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

        private static string ConstructForumPost()
        {
            var builder = new UriBuilder("https", IracingBaseUrl) { Path = "jforum/jforum.page" };
            return builder.ToString();
        }
    }
}
