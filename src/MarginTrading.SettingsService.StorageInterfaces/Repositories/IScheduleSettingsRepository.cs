﻿using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.SettingsService.Core.Interfaces;

namespace MarginTrading.SettingsService.StorageInterfaces.Repositories
{
    public interface IScheduleSettingsRepository
    {
        Task<IReadOnlyList<IScheduleSettings>> GetAsync();
        Task<IScheduleSettings> GetAsync(string scheduleSettingsId);
        Task<bool> TryInsertAsync(IScheduleSettings scheduleSettings);
        Task UpdateAsync(IScheduleSettings scheduleSettings);
        Task DeleteAsync(string scheduleSettingsId);
    }
}
