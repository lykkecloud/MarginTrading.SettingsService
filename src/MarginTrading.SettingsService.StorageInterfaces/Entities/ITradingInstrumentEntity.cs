﻿namespace MarginTrading.SettingsService.StorageInterfaces.Entities
{
    public interface ITradingInstrumentEntity
    {
        string TradingConditionId { get; }
        string Instrument { get; }
        int LeverageInit { get; }
        int LeverageMaintenance { get; }
        decimal SwapLong { get; }
        decimal SwapShort { get; }
        decimal Delta { get; }
        decimal DealMinLimit { get; }
        decimal DealMaxLimit { get; }
        decimal PositionLimit { get; }
        
        decimal CommissionRate { get; }
        decimal CommissionMin { get; }
        decimal CommissionMax { get; }
        decimal CommissionCurrency { get; }
    }
}
