﻿using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.SettingsService.Contracts.Common;
using MarginTrading.SettingsService.Contracts.Scheduling;
using Refit;

namespace MarginTrading.SettingsService.Contracts
{
    [PublicAPI]
    public interface IScheduleSettingsApi
    {
        [Get("/api/scheduleSettings")]
        Task<List<ScheduleSettingsContract>> List();


        [Post("/api/scheduleSettings")]
        Task<ScheduleSettingsContract> Insert([NotNull] [Body] ScheduleSettingsUpsertRequestParams @params);


        [ItemCanBeNull]
        [Get("/api/scheduleSettings/{settingId}")]
        Task<ScheduleSettingsContract> Get([NotNull] string settingId);


        [Put("/api/scheduleSettings/{settingId}")]
        Task<ScheduleSettingsContract> Update(
            [NotNull] string settingId,
            [NotNull] [Body] ScheduleSettingsUpsertRequestParams @params);


        [Delete("/api/scheduleSettings/{settingId}")]
        Task Delete([NotNull] string settingId, [NotNull] [Body] TraceableRequestParams @params);


        /// <summary>
        /// Get the list of compiled schedule settings based on array of asset pairs
        /// </summary>
        /// <param name="assetPairIds"></param>
        /// <returns></returns>
        [Post("/api/scheduleSettings/compiled")]
        Task<List<CompiledScheduleContract>> StateList([Body][CanBeNull] string[] assetPairIds);

    }
}
