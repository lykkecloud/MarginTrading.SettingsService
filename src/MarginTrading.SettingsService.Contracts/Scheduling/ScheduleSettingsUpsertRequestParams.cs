using MarginTrading.SettingsService.Contracts.Common;

namespace MarginTrading.SettingsService.Contracts.Scheduling
{
    public class ScheduleSettingsUpsertRequestParams
    {
        public ScheduleSettingsContract ScheduleSettings { get; set; }
        
        public TraceableRequestParams Traceability { get; set; }
    }
}