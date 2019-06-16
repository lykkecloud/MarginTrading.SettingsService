IF NOT EXISTS(SELECT 'X'
              FROM INFORMATION_SCHEMA.TABLES
              WHERE TABLE_NAME = 'TradingInstruments'
                AND TABLE_SCHEMA = 'dbo')
  BEGIN
    CREATE TABLE [dbo].[TradingInstruments]
    (
      [Oid]                       [bigint]       NOT NULL IDENTITY (1,1) PRIMARY KEY,
      [TradingConditionId]        [nvarchar](64) NOT NULL,
      [Instrument]                [nvarchar](64) NOT NULL,
      [LeverageInit]              [int]          NULL,
      [LeverageMaintenance]       [int]          NULL,
      [SwapLong]                  float          NULL,
      [SwapShort]                 float          NULL,
      [Delta]                     float          NULL,
      [DealMinLimit]              float          NULL,
      [DealMaxLimit]              float          NULL,
      [PositionLimit]             float          NULL,
      [ShortPosition]             bit            NULL,
      [LiquidationThreshold]      float          NULL,
      [OvernightMarginMultiplier] float          NULL,
      [CommissionRate]            float          NULL,
      [CommissionMin]             float          NULL,
      [CommissionMax]             float          NULL,
      [CommissionCurrency]        [nvarchar](64) NULL,
      INDEX IX_TradingInstruments_Base (TradingConditionId, Instrument)
    );
  END