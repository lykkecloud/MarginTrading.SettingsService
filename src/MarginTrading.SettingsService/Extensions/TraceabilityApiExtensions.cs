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
        
        public static string ExtractCorrelationId(this TraceableRequestParams @params)
        {
            return string.IsNullOrWhiteSpace(@params.CorrelationId)
                ? @params.Id
                : @params.CorrelationId;
        }
         
        public static string ExtractCausationId(this TraceableRequestParams @params)
        {
            return string.IsNullOrWhiteSpace(@params.CausationId)
                ? @params.Id
                : @params.CausationId;
        }   
    }
}