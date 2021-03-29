using System.Threading.Tasks;
using RaceSpotLiveryAPI.Entities;

namespace RaceSpotLiveryAPI.Services
{
    public interface ISESService
    {
        Task SendRejectionEmail(Livery livery);
        Task SendUpdateEmailSettingsNotification(ApplicationUser user);
    }
}