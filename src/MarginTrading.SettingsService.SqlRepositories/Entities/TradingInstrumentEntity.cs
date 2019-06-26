using JetBrains.Annotations;
using MarginTrading.SettingsService.Core.Interfaces;

namespace MarginTrading.SettingsService.SqlRepositories.Entities
{
    [UsedImplicitly]
    public class TradingInstrumentEntity : ITradingInstrument
    {   
        public string TradingConditionId { get; set; }
        public string Instrument { get; set; }
        public int LeverageInit { get; set; }
        public int LeverageMaintenance { get; set; }
        public decimal SwapLong { get; set; }
        public decimal SwapShort { get; set; }
        public decimal Delta { get; set; }
        public decimal DealMinLimit { get; set; }
        public decimal DealMaxLimit { get; set; }
        public decimal PositionLimit { get; set; }
        public bool ShortPosition { get; set; }
        public decimal LiquidationThreshold { get; set; }
        public decimal OvernightMarginMultiplier { get; set; }

        public decimal CommissionRate { get; set; }
        public decimal CommissionMin { get; set; }
        public decimal CommissionMax { get; set; }
        public string CommissionCurrency { get; set; }
        
        public int TotalRows { get; set; }

        public static TradingInstrumentEntity Create(ITradingInstrument tradingInstrument)
            => new TradingInstrumentEntity
            {
                TradingConditionId = tradingInstrument.TradingConditionId,
                Instrument = tradingInstrument.Instrument,
                LeverageInit = tradingInstrument.LeverageInit,
                LeverageMaintenance = tradingInstrument.LeverageMaintenance,
                SwapLong = tradingInstrument.SwapLong,
                SwapShort = tradingInstrument.SwapShort,
                Delta = tradingInstrument.Delta,
                DealMinLimit = tradingInstrument.DealMinLimit,
                DealMaxLimit = tradingInstrument.DealMaxLimit,
                PositionLimit = tradingInstrument.PositionLimit,
                ShortPosition = tradingInstrument.ShortPosition,
                LiquidationThreshold = tradingInstrument.LiquidationThreshold,
                OvernightMarginMultiplier = tradingInstrument.OvernightMarginMultiplier,
                CommissionRate = tradingInstrument.CommissionRate,
                CommissionMin = tradingInstrument.CommissionMin,
                CommissionMax = tradingInstrument.CommissionMax,
                CommissionCurrency = tradingInstrument.CommissionCurrency,
            };
    }
}