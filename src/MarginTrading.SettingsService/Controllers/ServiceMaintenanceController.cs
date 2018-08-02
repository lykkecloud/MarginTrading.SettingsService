using System.Threading.Tasks;
using MarginTrading.SettingsService.Contracts;
using MarginTrading.SettingsService.Contracts.Common;
using MarginTrading.SettingsService.Core.Domain;
using MarginTrading.SettingsService.Core.Services;
using MarginTrading.SettingsService.Extensions;
using MarginTrading.SettingsService.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.SettingsService.Controllers
{
    /// <summary>
    /// MT Core service maintenance management
    /// </summary>
    [Route("api/service/maintenance")]
    [MiddlewareFilter(typeof(RequestLoggingPipeline))]
    public class ServiceMaintenanceController : Controller, IServiceMaintenanceApi
    {
        private readonly IMaintenanceModeService _maintenanceModeService;
        private readonly IEventSender _eventSender;

        public ServiceMaintenanceController(
            IMaintenanceModeService maintenanceModeService,
            IEventSender eventSender)
        {
            _maintenanceModeService = maintenanceModeService;
            _eventSender = eventSender;
        }
        
        /// <summary>
        /// Get current service state
        /// </summary>
        [HttpGet]
        [Route("")]
        public Task<bool> Get()
        {
            return Task.FromResult(_maintenanceModeService.CheckIsEnabled());
        }

        /// <summary>
        /// Switch maintenance mode
        /// </summary>
        [HttpPost]
        [Route("")]
        public Task<bool> Post([FromQuery] bool enabled, [FromBody] TraceableRequestParams @params)
        {
            _maintenanceModeService.SetMode(enabled);
            
            _eventSender.SendSettingsChangedEvent(
                @params.CorrelationId, 
                @params.Id,
                $"{Request.Path}", 
                SettingsChangedSourceType.ServiceMaintenance);
            
            return Task.FromResult(true);
        }
    }
}