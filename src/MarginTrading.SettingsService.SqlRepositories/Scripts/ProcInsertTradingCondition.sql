CREATE OR ALTER PROCEDURE [dbo].[InsertTradingCondition](@Id [nvarchar](64),
                                                         @Name [nvarchar](64),
                                                         @LegalEntity [nvarchar](64),
                                                         @BaseTradingConditionId [nvarchar](64),
                                                         @MarginCall1 [float],
                                                         @MarginCall2 [float],
                                                         @StopOut [float],
                                                         @DepositLimit [float],
                                                         @WithdrawalLimit [float],
                                                         @LimitCurrency [nvarchar](64),
                                                         @BaseAssets [nvarchar](MAX),
                                                         @IsDefault [bit],
                                                         @IsBase [bit]) AS
BEGIN
  SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
  BEGIN TRANSACTION;

  BEGIN TRY
    INSERT INTO [dbo].[TradingConditions]
    VALUES (@Id, @Name, @LegalEntity, @BaseTradingConditionId, @MarginCall1, @MarginCall2, @StopOut, @DepositLimit,
            @WithdrawalLimit, @LimitCurrency, @BaseAssets, @IsDefault, @IsBase)

    IF DATALENGTH(@BaseTradingConditionId) > 0
      BEGIN
        INSERT INTO [dbo].[TradingInstruments]
        SELECT @Id AS 'TradingConditionId',
               [Instrument],
               [LeverageInit],
               [LeverageMaintenance],
               [SwapLong],
               [SwapShort],
               [Delta],
               [DealMinLimit],
               [DealMaxLimit],
               [PositionLimit],
               [ShortPosition],
               [LiquidationThreshold],
               [OvernightMarginMultiplier],
               [CommissionRate],
               [CommissionMin],
               [CommissionMax],
               [CommissionCurrency]
        FROM [dbo].[TradingInstruments]
        WHERE TradingConditionId = @BaseTradingConditionId
      END;

    COMMIT TRANSACTION;
  END TRY
  BEGIN CATCH
    IF @@TRANCOUNT > 0
      ROLLBACK TRANSACTION;
  END CATCH
END