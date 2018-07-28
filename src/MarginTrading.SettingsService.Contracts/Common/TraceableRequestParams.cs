using System;

namespace MarginTrading.SettingsService.Contracts.Common
{
    public class TraceableRequestParams : TraceableMessageBase
    {
        public TraceableRequestParams() : base()
        {
            Id = Id ?? Guid.NewGuid().ToString("N"); //TODO check
        }
    }
}