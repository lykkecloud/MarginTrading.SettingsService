﻿using System.Collections.Generic;

namespace MarginTrading.SettingsService.Contracts.TradingConditions
{
    public class TradingConditionContract
    {
        public const string TradingProfileId = "";
        
        public string Id { get; set; }
        public string Name { get; set; }
        public string LegalEntity { get; set; }
        public string BaseTradingConditionId { get; set; }
        public decimal MarginCall1 { get; set; }
        public decimal MarginCall2 { get; set; }
        public decimal StopOut { get; set; }
        public decimal DepositLimit { get; set; }
        public decimal WithdrawalLimit { get; set; }
        public string LimitCurrency { get; set; }
        public List<string> BaseAssets { get; set; }
        public bool IsDefault { get; set; }
        public bool IsBase { get; set; }
        
        public bool IsTradingProfile => Id == TradingProfileId;
    }
}
