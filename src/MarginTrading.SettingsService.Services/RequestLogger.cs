using Common.Log;

namespace MarginTrading.SettingsService.Services
{
    public class RequestLogger : AggregateLogger, ILog
    {
        public RequestLogger(params ILog[] logs)
            : base(logs)
        {
            
        }
    }
}