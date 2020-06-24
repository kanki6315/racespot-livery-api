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
    }
}
