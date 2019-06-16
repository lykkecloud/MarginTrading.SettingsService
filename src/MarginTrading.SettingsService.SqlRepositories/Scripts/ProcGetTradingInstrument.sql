CREATE OR ALTER PROCEDURE [dbo].[CompileAndGetTradingInstrument](@TradingConditionId nvarchar(64),
                                                               @TradingInstrumentId nvarchar(64)) AS
BEGIN
  SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
  BEGIN TRANSACTION;

  BEGIN TRY
    DECLARE
      @BaseTradingConditionsId nvarchar(64);

    SELECT @BaseTradingConditionsId = BaseTradingConditionId
    FROM TradingConditions
    WHERE Id = @TradingConditionId;

    DECLARE
      @BaseTradingInstrumentLeverageInit int, @TradingInstrumentLeverageInit int, @TradingPlatformLeverageInit int;
    DECLARE
      @BaseTradingInstrumentLeverageMaintenance int, @TradingInstrumentLeverageMaintenance int, @TradingPlatformLeverageMaintenance int;

    SELECT @TradingPlatformLeverageInit = LeverageInit,
           @TradingPlatformLeverageMaintenance = LeverageMaintenance
    FROM dbo.TradingInstruments
    WHERE TradingConditionId = 'TradingProfile'
      AND Instrument = @TradingInstrumentId;

    SELECT @TradingInstrumentLeverageInit = LeverageInit,
           @TradingInstrumentLeverageMaintenance = LeverageMaintenance
    FROM dbo.TradingInstruments
    WHERE TradingConditionId = @TradingConditionId
      AND Instrument = @TradingInstrumentId;

    SELECT @BaseTradingInstrumentLeverageInit =
           CASE
             WHEN @BaseTradingConditionsId IS NULL
               THEN @TradingInstrumentLeverageInit
             ELSE (SELECT TOP 1 LeverageInit
                   FROM dbo.TradingInstruments
                   WHERE TradingConditionId = @BaseTradingConditionsId
                     AND Instrument = @TradingInstrumentId)
             END,
           @BaseTradingInstrumentLeverageMaintenance =
           CASE
             WHEN @BaseTradingConditionsId IS NULL
               THEN @TradingInstrumentLeverageMaintenance
             ELSE (SELECT TOP 1 LeverageMaintenance
                   FROM dbo.TradingInstruments
                   WHERE TradingConditionId = @BaseTradingConditionsId
                     AND Instrument = @TradingInstrumentId)
             END;

    SELECT @TradingInstrumentLeverageInit =
           CASE
             WHEN @BaseTradingInstrumentLeverageInit < @TradingInstrumentLeverageInit
               THEN @BaseTradingInstrumentLeverageInit
             ELSE @TradingInstrumentLeverageInit
             END,
           @TradingInstrumentLeverageMaintenance =
           CASE
             WHEN @BaseTradingInstrumentLeverageMaintenance < @TradingInstrumentLeverageMaintenance
               THEN @BaseTradingInstrumentLeverageMaintenance
             ELSE @TradingInstrumentLeverageMaintenance
             END;

    SELECT @TradingInstrumentLeverageInit =
           CASE
             WHEN @TradingPlatformLeverageInit < @TradingInstrumentLeverageInit
               THEN @TradingPlatformLeverageInit
             ELSE @TradingInstrumentLeverageInit
             END,
           @TradingInstrumentLeverageMaintenance =
           CASE
             WHEN @TradingPlatformLeverageMaintenance < @TradingInstrumentLeverageMaintenance
               THEN @TradingPlatformLeverageMaintenance
             ELSE @TradingInstrumentLeverageMaintenance
             END;

    SELECT TradingConditionId,
           Instrument,
           @TradingInstrumentLeverageInit,
           @TradingInstrumentLeverageMaintenance,
           SwapLong,
           SwapShort,
           Delta,
           DealMinLimit,
           DealMaxLimit,
           PositionLimit,
           CommissionRate,
           CommissionMin,
           CommissionMax,
           CommissionCurrency,
           LiquidationThreshold,
           ShortPosition,
           OvernightMarginMultiplier
    FROM dbo.TradingInstruments
    WHERE TradingConditionId = @TradingConditionId
      AND Instrument = @TradingInstrumentId
    ORDER BY OID;

    COMMIT TRANSACTION;
  END TRY
  BEGIN CATCH
    IF @@TRANCOUNT > 0
      ROLLBACK TRANSACTION;
  END CATCH
END