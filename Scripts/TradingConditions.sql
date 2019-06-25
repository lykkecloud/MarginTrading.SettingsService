IF NOT EXISTS(SELECT 'X'
              FROM INFORMATION_SCHEMA.TABLES
              WHERE TABLE_NAME = 'TradingConditions'
                AND TABLE_SCHEMA = 'dbo')
  BEGIN
    CREATE TABLE [dbo].[TradingConditions]
    (
      [Oid]                    [bigint]        NOT NULL IDENTITY (1,1) PRIMARY KEY,
      [Id]                     [nvarchar](64)  NOT NULL,
      [Name]                   [nvarchar](64)  NOT NULL,
      [LegalEntity]            [nvarchar](64)  NULL,
      [BaseTradingConditionId] [nvarchar](64)  NULL,
      [MarginCall1]            float           NULL,
      [MarginCall2]            float           NULL,
      [StopOut]                float           NULL,
      [DepositLimit]           float           NULL,
      [WithdrawalLimit]        float           NULL,
      [LimitCurrency]          [nvarchar](64)  NULL,
      [BaseAssets]             [nvarchar](MAX) NULL,
      [IsDefault]              [bit]           NOT NULL,
      [IsBase]                 [bit]           NOT NULL,
      INDEX IX_TradingConditions_Default (BaseTradingConditionId, IsDefault, IsBase)
    );
  END