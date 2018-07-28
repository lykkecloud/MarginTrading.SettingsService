﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarginTrading.SettingsService.Contracts;
using MarginTrading.SettingsService.Contracts.Common;
using MarginTrading.SettingsService.Contracts.TradingConditions;
using MarginTrading.SettingsService.Contracts.TradingInstruments;
using MarginTrading.SettingsService.Core.Domain;
using MarginTrading.SettingsService.Core.Interfaces;
using MarginTrading.SettingsService.Core.Services;
using MarginTrading.SettingsService.Core.Settings;
using MarginTrading.SettingsService.Extensions;
using MarginTrading.SettingsService.StorageInterfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.SettingsService.Controllers
{
    /// <summary>
    /// Trading instruments management
    /// </summary>
    [Route("api/tradingInstruments")]
    public class TradingInstrumentsController : Controller, ITradingInstrumentsApi
    {
        private readonly IAssetsRepository _assetsRepository;
        private readonly IAssetPairsRepository _assetPairsRepository;
        private readonly ITradingConditionsRepository _tradingConditionsRepository;
        private readonly ITradingInstrumentsRepository _tradingInstrumentsRepository;
        private readonly ITradingService _tradingService;
        private readonly IConvertService _convertService;
        private readonly IEventSender _eventSender;
        private readonly DefaultTradingInstrumentSettings _defaultTradingInstrumentSettings;
        
        public TradingInstrumentsController(
            IAssetsRepository assetsRepository,
            IAssetPairsRepository assetPairsRepository,
            ITradingConditionsRepository tradingConditionsRepository,
            ITradingInstrumentsRepository tradingInstrumentsRepository,
            ITradingService tradingService,
            IConvertService convertService,
            IEventSender eventSender,
            DefaultTradingInstrumentSettings defaultTradingInstrumentSettings)
        {
            _assetsRepository = assetsRepository;
            _assetPairsRepository = assetPairsRepository;
            _tradingConditionsRepository = tradingConditionsRepository;
            _tradingInstrumentsRepository = tradingInstrumentsRepository;
            _tradingService = tradingService;
            _convertService = convertService;
            _eventSender = eventSender;
            _defaultTradingInstrumentSettings = defaultTradingInstrumentSettings;
        }
        
        /// <summary>
        /// Get the list of trading instruments
        /// </summary>
        /// <param name="tradingConditionId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<List<TradingInstrumentContract>> List([FromQuery] string tradingConditionId)
        {
            var data = string.IsNullOrWhiteSpace(tradingConditionId)
                ? await _tradingInstrumentsRepository.GetAsync()
                : await _tradingInstrumentsRepository.GetByTradingConditionAsync(tradingConditionId);
            
            return data.Select(x => _convertService.Convert<ITradingInstrument, TradingInstrumentContract>(x)).ToList();
        }

        /// <summary>
        /// Create new trading instrument
        /// </summary>
        [HttpPost]
        [Route("")]
        public async Task<TradingInstrumentContract> Insert([FromBody] TradingInstrumentUpsertRequestParams @params)
        {
            @params?.Traceability.Validate();
            await ValidateTradingInstrument(@params.TradingInstrument);

            if (!await _tradingInstrumentsRepository.TryInsertAsync(
                _convertService.Convert<TradingInstrumentContract, TradingInstrument>(@params.TradingInstrument)))
            {
                throw new ArgumentException($"Trading instrument with tradingConditionId {@params.TradingInstrument.TradingConditionId}" +
                                            $"and assetPairId {@params.TradingInstrument.Instrument} already exists");
            }

            await _eventSender.SendSettingsChangedEvent(
                @params.Traceability.ExtractCorrelationId(), 
                @params.Traceability.ExtractCausationId(),
                $"{Request.Path}", 
                SettingsChangedSourceType.TradingInstrument);

            return @params.TradingInstrument;
        }

        /// <summary>
        /// Assign trading instrument to a trading condition with default values
        /// </summary>
        [HttpPost]
        [Route("{tradingConditionId}")]
        public async Task<List<TradingInstrumentContract>> AssignCollection(string tradingConditionId, 
            [FromBody] TradingInstrumentAssignCollectionRequestParams @params)
        {
            var currentInstruments =
                await _tradingInstrumentsRepository.GetByTradingConditionAsync(tradingConditionId);

            if (currentInstruments.Any())
            {
                var toRemove = currentInstruments.Where(x => !@params.Instruments.Contains(x.Instrument)).ToArray();
                
                var existingOrderGroups = await _tradingService.CheckActiveByTradingCondition(tradingConditionId);
                
                if (existingOrderGroups.Any())
                {
                    var errorMessage = existingOrderGroups.Aggregate(
                        "Unable to remove following instruments as they have active orders: ",
                        (current, @group) => current + $"{@group} orders) ");

                    throw new InvalidOperationException(errorMessage);
                }
                
                foreach (var pair in toRemove)
                {
                    await _tradingInstrumentsRepository.DeleteAsync(pair.Instrument, pair.TradingConditionId);
                }
            }
            
            var pairsToAdd = @params.Instruments.Where(x => currentInstruments.All(y => y.Instrument != x));

            var addedPairs = await _tradingInstrumentsRepository.CreateDefaultTradingInstruments(tradingConditionId,
                pairsToAdd, _defaultTradingInstrumentSettings);
            
            await _eventSender.SendSettingsChangedEvent(
                @params.Traceability.ExtractCorrelationId(), 
                @params.Traceability.ExtractCausationId(),
                $"{Request.Path}", 
                SettingsChangedSourceType.TradingInstrument);

            return addedPairs.Select(x => _convertService.Convert<ITradingInstrument, TradingInstrumentContract>(x))
                .ToList();
        }

        /// <summary>
        /// Get trading instrument
        /// </summary>
        [HttpGet]
        [Route("{tradingConditionId}/{assetPairId}")]
        public async Task<TradingInstrumentContract> Get(string tradingConditionId, string assetPairId)
        {
            var obj = await _tradingInstrumentsRepository.GetAsync(assetPairId, tradingConditionId);

            return _convertService.Convert<ITradingInstrument, TradingInstrumentContract>(obj);
        }

        /// <summary>
        /// Update the trading instrument
        /// </summary>
        [HttpPut]
        [Route("{tradingConditionId}/{assetPairId}")]
        public async Task<TradingInstrumentContract> Update(string tradingConditionId, string assetPairId, 
            [FromBody] TradingInstrumentUpsertRequestParams @params)
        {
            @params?.Traceability.Validate();
            await ValidateTradingInstrument(@params.TradingInstrument);
            ValidateId(tradingConditionId, assetPairId, @params.TradingInstrument);

            await _tradingInstrumentsRepository.UpdateAsync(
                _convertService.Convert<TradingInstrumentContract, TradingInstrument>(@params.TradingInstrument));

            await _eventSender.SendSettingsChangedEvent(
                @params.Traceability.ExtractCorrelationId(), 
                @params.Traceability.ExtractCausationId(),
                $"{Request.Path}", 
                SettingsChangedSourceType.TradingInstrument);
            
            return @params.TradingInstrument;
        }

        /// <summary>
        /// Delete the trading instrument
        /// </summary>
        [HttpDelete]
        [Route("{tradingConditionId}/{assetPairId}")]
        public async Task Delete(string tradingConditionId, string assetPairId, 
            [FromBody] TraceableRequestParams @params)
        {
            await _tradingInstrumentsRepository.DeleteAsync(assetPairId, tradingConditionId);

            await _eventSender.SendSettingsChangedEvent(
                @params.ExtractCorrelationId(), 
                @params.ExtractCausationId(),
                $"{Request.Path}", 
                SettingsChangedSourceType.TradingInstrument);
        }

        private void ValidateId(string tradingConditionId, string assetPairId, TradingInstrumentContract contract)
        {
            if (contract?.TradingConditionId != tradingConditionId)
            {
                throw new ArgumentException("TradingConditionId must match with contract tradingConditionId");
            }

            if (contract?.Instrument != assetPairId)
            {
                throw new ArgumentException("AssetPairId must match with contract instrument");
            }
        }

        private async Task ValidateTradingInstrument(TradingInstrumentContract instrument)
        {
            if (instrument == null)
            {
                throw new ArgumentNullException("instrument", "Model is incorrect");
            }
            
            if (string.IsNullOrWhiteSpace(instrument?.TradingConditionId))
            {
                throw new ArgumentNullException(nameof(instrument.TradingConditionId), "TradingConditionId must be set");
            }
            
            if (string.IsNullOrWhiteSpace(instrument.Instrument))
            {
                throw new ArgumentNullException(nameof(instrument.Instrument), "Instrument must be set");
            }

            if (await _tradingConditionsRepository.GetAsync(instrument.TradingConditionId) == null)
            {
                throw new InvalidOperationException($"Trading condition {instrument.TradingConditionId} does not exist");
            }

            if (await _assetPairsRepository.GetAsync(instrument.Instrument) == null)
            {
                throw new InvalidOperationException($"Asset pair {instrument.Instrument} does not exist");
            }

            if (instrument.LeverageInit <= 0)
            {
                throw new InvalidOperationException($"LeverageInit must be greather then zero");
            }

            if (instrument.LeverageMaintenance <= 0)
            {
                throw new InvalidOperationException($"LeverageMaintenance must be greather then zero");
            }

            if (await _assetsRepository.GetAsync(instrument.CommissionCurrency) == null)
            {
                throw new InvalidOperationException($"Commission currency {instrument.CommissionCurrency} does not exist");
            }
        }
    }
}