using MarginTrading.SettingsService.Core.Interfaces;

namespace MarginTrading.SettingsService.Core.Domain
{
    public class TradingInstrument : ITradingInstrument
    {
        public TradingInstrument(string tradingConditionId, string instrument, int leverageInit,
            int leverageMaintenance, decimal swapLong, decimal swapShort, decimal delta, decimal dealMinLimit,
            decimal dealMaxLimit, decimal positionLimit, bool shortPosition, decimal liquidationThreshold,
            decimal overnightMarginMultiplier,
            decimal commissionRate, decimal commissionMin, decimal commissionMax, string commissionCurrency)
        {
            TradingConditionId = tradingConditionId;
            Instrument = instrument;
            LeverageInit = leverageInit;
            LeverageMaintenance = leverageMaintenance;
            SwapLong = swapLong;
            SwapShort = swapShort;
            Delta = delta;
            DealMinLimit = dealMinLimit;
            DealMaxLimit = dealMaxLimit;
            PositionLimit = positionLimit;
            ShortPosition = shortPosition;
            LiquidationThreshold = liquidationThreshold;
            OvernightMarginMultiplier = overnightMarginMultiplier;

            CommissionRate = commissionRate;
            CommissionMin = commissionMin;
            CommissionMax = commissionMax;
            CommissionCurrency = commissionCurrency;
        }

        private TradingInstrument()
        {
        }

        public string TradingConditionId { get; private set; }
        public string Instrument { get; private set; }
        public int LeverageInit { get; private set; }
        public int LeverageMaintenance { get; private set; }
        public decimal SwapLong { get; private set; }
        public decimal SwapShort { get; private set; }
        public decimal Delta { get; private set; }
        public decimal DealMinLimit { get; private set; }
        public decimal DealMaxLimit { get; private set; }
        public decimal PositionLimit { get; private set; }
        public bool ShortPosition { get; private set; }
        public decimal LiquidationThreshold { get; private set; }
        public decimal OvernightMarginMultiplier { get; private set; }

        public decimal CommissionRate { get; private set; }
        public decimal CommissionMin { get; private set; }
        public decimal CommissionMax { get; private set; }
        public string CommissionCurrency { get; private set; }

        public static TradingInstrument Create(ITradingInstrument tradingInstrument)
            => new TradingInstrument
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