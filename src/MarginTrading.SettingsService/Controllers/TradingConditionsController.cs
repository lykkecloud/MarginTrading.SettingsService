using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarginTrading.SettingsService.Contracts;
using MarginTrading.SettingsService.Contracts.TradingConditions;
using MarginTrading.SettingsService.Core.Domain;
using MarginTrading.SettingsService.Core.Interfaces;
using MarginTrading.SettingsService.Core.Services;
using MarginTrading.SettingsService.Core.Settings;
using MarginTrading.SettingsService.Extensions;
using MarginTrading.SettingsService.Middleware;
using MarginTrading.SettingsService.StorageInterfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.SettingsService.Controllers
{
    /// <summary>
    /// Trading conditions management
    /// </summary>
    [Route("api/tradingConditions")]
    [MiddlewareFilter(typeof(RequestLoggingPipeline))]
    public class TradingConditionsController : Controller, ITradingConditionsApi
    {
        private readonly IAssetsRepository _assetsRepository;
        private readonly ITradingConditionsRepository _tradingConditionsRepository;
        private readonly IConvertService _convertService;
        private readonly IEventSender _eventSender;
        private readonly DefaultLegalEntitySettings _defaultLegalEntitySettings;
        
        public TradingConditionsController(
            IAssetsRepository assetsRepository,
            ITradingConditionsRepository tradingConditionsRepository,
            IConvertService convertService,
            IEventSender eventSender,
            DefaultLegalEntitySettings defaultLegalEntitySettings)
        {
            _assetsRepository = assetsRepository;
            _tradingConditionsRepository = tradingConditionsRepository;
            _convertService = convertService;
            _eventSender = eventSender;
            _defaultLegalEntitySettings = defaultLegalEntitySettings;
        }
        
        /// <summary>
        /// Get the list of trading conditions
        /// </summary>
        [HttpGet]
        [Route("")]
        public async Task<List<TradingConditionContract>> List([FromQuery] bool? isDefault = null)
        {
            var data = await _tradingConditionsRepository.GetAsync();
            
            return data
                .Where(x => isDefault == null || x.IsDefault == isDefault)
                .Select(x => _convertService.Convert<ITradingCondition, TradingConditionContract>(x)).ToList();
        }

        /// <summary>
        /// Create new trading condition
        /// </summary>
        [HttpPost]
        [Route("")]
        public async Task<TradingConditionContract> Insert([FromBody] TradingConditionUpsertRequestParams @params)
        {
            @params?.Traceability.Validate();
            await ValidateTradingCondition(@params.TradingCondition);
            
            var defaultTradingCondition =
                (await _tradingConditionsRepository.GetDefaultAsync()).FirstOrDefault();

            if (@params.TradingCondition.IsDefault 
                && defaultTradingCondition != null && defaultTradingCondition.Id != @params.TradingCondition.Id)
            {
                await SetDefault(defaultTradingCondition, false);
            }

            if (defaultTradingCondition == null)
            {
                @params.TradingCondition.IsDefault = true;
            }
            
            _defaultLegalEntitySettings.Set(@params.TradingCondition);
                
            if (!await _tradingConditionsRepository.TryInsertAsync(
                _convertService.Convert<TradingConditionContract, TradingCondition>(@params.TradingCondition)))
            {
                throw new ArgumentException($"Trading condition with id {@params.TradingCondition.Id} already exists",
                    nameof(@params.TradingCondition.Id));
            }

            await _eventSender.SendSettingsChangedEvent(
                @params.Traceability.CorrelationId, 
                @params.Traceability.Id,
                $"{Request.Path}", 
                SettingsChangedSourceType.TradingCondition);

            return @params.TradingCondition;
        }

        /// <summary>
        /// Get the trading condition
        /// </summary>
        [HttpGet]
        [Route("{tradingConditionId}")]
        public async Task<TradingConditionContract> Get(string tradingConditionId)
        {
            var obj = await _tradingConditionsRepository.GetAsync(tradingConditionId);
            
            return _convertService.Convert<ITradingCondition, TradingConditionContract>(obj);
        }

        /// <summary>
        /// Update the trading condition
        /// </summary>
        [HttpPut]
        [Route("{tradingConditionId}")]
        public async Task<TradingConditionContract> Update(string tradingConditionId, 
            [FromBody] TradingConditionUpsertRequestParams @params)
        {
            @params?.Traceability.Validate();
            await ValidateTradingCondition(@params.TradingCondition);
            
            ValidateId(tradingConditionId, @params.TradingCondition);
            
            var defaultTradingCondition =
                (await _tradingConditionsRepository.GetDefaultAsync()).FirstOrDefault();
            if (defaultTradingCondition == null && !@params.TradingCondition.IsDefault)
            {
                @params.TradingCondition.IsDefault = true;
            }

            if (defaultTradingCondition != null 
                && @params.TradingCondition.IsDefault && defaultTradingCondition.Id != @params.TradingCondition.Id)
            {
                await SetDefault(defaultTradingCondition, false);
            }
            
            _defaultLegalEntitySettings.Set(@params.TradingCondition);

            var existingCondition = await _tradingConditionsRepository.GetAsync(@params.TradingCondition.Id);
            if (existingCondition == null)
            {
                throw new Exception($"Trading condition with Id = {@params.TradingCondition.Id} not found");
            }

            if (existingCondition.LegalEntity != @params.TradingCondition.LegalEntity)
            {
                throw new Exception("LegalEntity cannot be changed");
            }

            await _tradingConditionsRepository.UpdateAsync(
                _convertService.Convert<TradingConditionContract, TradingCondition>(@params.TradingCondition));

            await _eventSender.SendSettingsChangedEvent(
                @params.Traceability.CorrelationId, 
                @params.Traceability.Id,
                $"{Request.Path}", 
                SettingsChangedSourceType.TradingCondition);
            
            return @params.TradingCondition;
        }

        private async Task SetDefault(ITradingCondition obj, bool state)
        {
            var defaultTrConDomain =
                _convertService.Convert<ITradingCondition, TradingCondition>(obj);
            defaultTrConDomain.IsDefault = state;
            await _tradingConditionsRepository.UpdateAsync(defaultTrConDomain);
        }

        private void ValidateId(string id, TradingConditionContract contract)
        {
            if (contract?.Id != id)
            {
                throw new ArgumentException("Id must match with contract id");
            }
        }

        private async Task ValidateTradingCondition(TradingConditionContract tradingCondition)
        {
            if (tradingCondition == null)
            {
                throw new ArgumentNullException("tradingCondition", "Model is incorrect");
            }
            
            if (string.IsNullOrWhiteSpace(tradingCondition?.Id))
            {
                throw new ArgumentNullException(nameof(tradingCondition.Id), "TradingCondition Id must be set");
            }

            if (!string.IsNullOrEmpty(tradingCondition.LimitCurrency)
                && await _assetsRepository.GetAsync(tradingCondition.LimitCurrency) == null)
            {
                throw new InvalidOperationException($"LimitCurrency asset {tradingCondition.LimitCurrency} does not exist");
            }

            foreach (var baseAsset in tradingCondition.BaseAssets)
            {//TODO optimization may be applied here
                if (await _assetsRepository.GetAsync(baseAsset) == null)
                {
                    throw new InvalidOperationException($"Base asset {baseAsset} does not exist");
                }
            }
        }
    }
}