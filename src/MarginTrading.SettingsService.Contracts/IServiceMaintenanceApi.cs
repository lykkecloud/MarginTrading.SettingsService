using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.SettingsService.Contracts.Common;
using Refit;

namespace MarginTrading.SettingsService.Contracts
{
    /// <summary>
    /// MT Core service maintenance management
    /// </summary>
    [PublicAPI]
    public interface IServiceMaintenanceApi
    {
        /// <summary>
        /// Get current service state
        /// </summary>
        [Get("/api/service/maintenance")]
        Task<bool> Get();

        /// <summary>
        /// Switch maintenance mode
        /// </summary>
        [Post("/api/service/maintenance")]
        Task<bool> Post([Query] bool enabled, [NotNull] [Body] TraceableRequestParams @params);

    }
}
