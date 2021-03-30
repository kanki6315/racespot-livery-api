using System.Threading.Tasks;
using RaceSpotLiveryAPI.Entities;

namespace RaceSpotLiveryAPI.Services
{
    public interface ISESService
    {
        Task SendApprovalEmail(Livery livery);
        Task SendRejectionEmail(Livery livery);
        Task SendUpdateEmailSettingsNotification(ApplicationUser user);
    }
}