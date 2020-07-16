using RaceSpotLiveryAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaceSpotLiveryAPI.Services
{
    public interface IIracingService
    {
        Task<IracingDriverModel> LookupIracingDriverById(string id);
        Task<bool> SendPrivateMessage(string userId, string message);
        Task<bool> DeletePrivateMessages();
        Task<IracingTeamModel> LookupIracingTeamById(String id, bool findDrivers);
    }
}
