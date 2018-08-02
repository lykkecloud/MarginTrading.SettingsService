using System;
using MarginTrading.SettingsService.Contracts.Common;

namespace MarginTrading.SettingsService.Extensions
{
    public static class TraceabilityApiExtensions
    {
        public static void Validate(this TraceableRequestParams traceability)
        {
            if (traceability == null)
            {
                throw new ArgumentNullException(nameof(traceability));
            }   
        }
    }
}