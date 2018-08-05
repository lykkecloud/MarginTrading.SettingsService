using System;
using JetBrains.Annotations;
using MarginTrading.SettingsService.Contracts.Enums;

namespace MarginTrading.SettingsService.Contracts.Common
{
    public class SettingsChangedEvent : TraceableMessageBase
    {
        public SettingsTypeContract SettingsType { get; set; }
        public string Route { get; set; }

        public SettingsChangedEvent([NotNull] string correlationId, [CanBeNull] string causationId,
                DateTime eventTimestamp, SettingsTypeContract settingsType, string route)
            :base(correlationId, causationId, eventTimestamp)
        {
            SettingsType = settingsType;
            Route = route;
        }
    }
}