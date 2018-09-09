﻿using System;

namespace MarginTrading.SettingsService.Core.Domain
{
    public class ScheduleConstraint
    {
        public DateTime? Date { get; set; }
        public DayOfWeek? DayOfWeek { get; set; }
        public TimeSpan Time { get; set; }
    }
}