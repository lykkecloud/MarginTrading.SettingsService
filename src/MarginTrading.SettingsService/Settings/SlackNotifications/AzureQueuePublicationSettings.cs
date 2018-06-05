﻿using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace MarginTrading.SettingsService.Settings.SlackNotifications
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AzureQueuePublicationSettings
    {
        public string ConnectionString { get; set; }

        public string QueueName { get; set; }
    }
}
