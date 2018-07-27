using System.Threading.Tasks;
using MarginTrading.SettingsService.Core.Domain;

namespace MarginTrading.SettingsService.Core.Services
{
    public interface IEventSender
    {
        Task SendSettingsChangedEvent(string correlationId, string causationId, 
            string route, SettingsChangedSourceType sourceType);
    }
}