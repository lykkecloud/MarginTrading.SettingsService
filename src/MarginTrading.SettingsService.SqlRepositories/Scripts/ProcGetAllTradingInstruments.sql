CREATE OR ALTER PROCEDURE [dbo].[CompileAndGetAllTradingInstruments](
  @TradingConditionId nvarchar(64)
) AS
BEGIN
  SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
  BEGIN TRANSACTION;

  BEGIN TRY
    DECLARE
      @TradingInstrumentsParams TABLE
                                (
                                  Instrument                         nvarchar(64),
                                  BaseLeverageInit                   int,
                                  LeverageInit                       int,
                                  TradingPlatformLeverageInit        int,
                                  BaseLeverageMaintenance            int,
                                  LeverageMaintenance                int,
                                  TradingPlatformLeverageMaintenance int
                                );

    INSERT INTO @TradingInstrumentsParams
    SELECT ti.Instrument,
           (CASE
              WHEN tc.BaseTradingConditionId IS NULL
                THEN ti.LeverageInit
              ELSE ISNULL((SELECT TOP 1 LeverageInit
                           FROM TradingInstruments
                           WHERE TradingInstruments.TradingConditionId = tc.BaseTradingConditionId), ti.LeverageInit)
             END)                                                               AS BaseLeverageInit,
           ti.LeverageInit                                                      AS LeverageInit,
           (ISNULL((SELECT TOP 1 LeverageInit
                    FROM TradingInstruments
                    WHERE TradingConditionId = 'TradingProfile'
                      AND Instrument = ti.Instrument), ti.LeverageInit))        AS TradingPlatformLeverageInit,
           (CASE
              WHEN tc.BaseTradingConditionId IS NULL
                THEN ti.LeverageMaintenance
              ELSE ISNULL((SELECT TOP 1 LeverageMaintenance
                           FROM TradingInstruments
                           WHERE TradingInstruments.TradingConditionId = tc.BaseTradingConditionId),
                          ti.LeverageMaintenance)
             END)                                                               AS BaseLeverageMaintenance,
           ti.LeverageMaintenance                                               AS LeverageMaintenance,
           (ISNULL((SELECT TOP 1 LeverageMaintenance
                    FROM TradingInstruments
                    WHERE TradingConditionId = 'TradingProfile'
                      AND Instrument = ti.Instrument), ti.LeverageMaintenance)) AS TradingPlatformLeverageMaintenance
    FROM dbo.TradingInstruments ti,
         dbo.TradingConditions tc
    WHERE ti.TradingConditionId = tc.Id
      AND tc.Id = @TradingConditionId

    SELECT TradingConditionId,
           ti.Instrument,
           (SELECT MIN(Col)
            FROM (VALUES (p.LeverageInit),
                         (p.BaseLeverageInit),
                         (p.TradingPlatformLeverageInit)) AS X(Col))        AS LeverageInit,
           (SELECT MIN(Col)
            FROM (VALUES (p.LeverageMaintenance),
                         (p.BaseLeverageMaintenance),
                         (p.TradingPlatformLeverageMaintenance)) AS X(Col)) AS LeverageMaintenance,
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
    FROM dbo.TradingInstruments ti,
         @TradingInstrumentsParams p
    WHERE p.Instrument = ti.Instrument
      AND TradingConditionId = @TradingConditionId
    ORDER BY ti.OID;

    COMMIT TRANSACTION;
  END TRY
  BEGIN CATCH
    IF @@TRANCOUNT > 0
      ROLLBACK TRANSACTION;
  END CATCH
END