CREATE OR ALTER PROCEDURE [dbo].[CompileAndGetAllTradingInstruments](@TradingConditionId nvarchar(64) = NULL,
                                                                     @Skip INT = 0,
                                                                     @Take INT = 20,
                                                                     @SortAscending BIT = 1) AS
BEGIN
    SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
    BEGIN TRANSACTION;

    BEGIN TRY
        WITH parameters AS (
            SELECT tc.Id                                                       AS TradingConditionId,
                   ti.Instrument,
                   ISNULL((SELECT TOP 1 LeverageInit
                           FROM TradingInstruments
                           WHERE TradingInstruments.TradingConditionId = tc.BaseTradingConditionId),
                          ti.LeverageInit)                                     AS BaseLeverageInit,
                   ti.LeverageInit                                             AS LeverageInit,
                   ISNULL((SELECT TOP 1 LeverageInit
                           FROM TradingInstruments
                           WHERE LEN(TradingConditionId) = 0 --'TradingProfile'
                             AND Instrument = ti.Instrument), ti.LeverageInit) AS TradingPlatformLeverageInit,
                   ISNULL((SELECT TOP 1 LeverageMaintenance
                           FROM TradingInstruments
                           WHERE TradingInstruments.TradingConditionId = tc.BaseTradingConditionId),
                          ti.LeverageMaintenance)
                                                                               AS BaseLeverageMaintenance,
                   ti.LeverageMaintenance                                      AS LeverageMaintenance,
                   ISNULL((SELECT TOP 1 LeverageMaintenance
                           FROM TradingInstruments
                           WHERE LEN(TradingConditionId) = 0 --'TradingProfile'

                             AND Instrument = ti.Instrument),
                          ti.LeverageMaintenance)                              AS TradingPlatformLeverageMaintenance
            FROM dbo.TradingInstruments ti,
                 dbo.TradingConditions tc
            WHERE ti.TradingConditionId = tc.Id
              AND (@TradingConditionId IS NULL OR tc.Id = @TradingConditionId)
        ),
             data AS (
                 SELECT ti.TradingConditionId,
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
                      parameters p
                 WHERE p.Instrument = ti.Instrument
                   AND p.TradingConditionId = ti.TradingConditionId
             ),
             rowsCount AS (
                 SELECT COUNT(*) AS Total
                 FROM data
             )
        SELECT TradingConditionId,
               Instrument,
               LeverageInit,
               LeverageMaintenance,
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
               OvernightMarginMultiplier,
               CONVERT(INT, rowsCount.Total) AS TotalRows
        FROM data,
             rowsCount
        ORDER BY CASE WHEN @SortAscending = 0 THEN TradingConditionId END DESC,
                 CASE WHEN @SortAscending = 1 THEN TradingConditionId END ASC,
                 CASE WHEN @SortAscending = 0 THEN Instrument END DESC,
                 CASE WHEN @SortAscending = 1 THEN Instrument END ASC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
    END CATCH
END