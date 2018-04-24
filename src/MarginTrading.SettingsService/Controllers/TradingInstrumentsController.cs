﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarginTrading.SettingsService.AzureRepositories.Entities;
using MarginTrading.SettingsService.Client;
using MarginTrading.SettingsService.Client.TradingConditions;
using MarginTrading.SettingsService.Core.Domain;
using MarginTrading.SettingsService.Core.Services;
using MarginTrading.SettingsService.Core.Settings;
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
        private readonly ITradingInstrumentsRepository _tradingInstrumentsRepository;
        private readonly ITradingService _tradingService;
        private readonly IConvertService _convertService;
        private readonly IEventSender _eventSender;
        private readonly DefaultTradingInstrumentSettings _defaultTradingInstrumentSettings;
        
        public TradingInstrumentsController(
            ITradingInstrumentsRepository tradingInstrumentsRepository,
            IConvertService convertService,
            IEventSender eventSender,
            DefaultTradingInstrumentSettings defaultTradingInstrumentSettings)
        {
            _tradingInstrumentsRepository = tradingInstrumentsRepository;
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
            var data = await _tradingInstrumentsRepository.GetAsync(x => x.TradingConditionId == tradingConditionId);
            
            return data.Select(x => _convertService.Convert<TradingInstrument, TradingInstrumentContract>(x)).ToList();
        }

        /// <summary>
        /// Create new trading instrument
        /// </summary>
        /// <param name="instrument"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<TradingInstrumentContract> Insert([FromBody] TradingInstrumentContract instrument)
        {
            if (string.IsNullOrWhiteSpace(instrument?.TradingConditionId))
            {
                throw new ArgumentNullException(nameof(instrument.TradingConditionId),
                    "TradingConditionId must be set");
            }
            if (string.IsNullOrWhiteSpace(instrument.Instrument))
            {
                throw new ArgumentNullException(nameof(instrument.Instrument),
                    "Instrument must be set");
            }

            await _tradingInstrumentsRepository.InsertAsync(
                _convertService.Convert<TradingInstrumentContract, TradingInstrument>(instrument));

            await _eventSender.SendSettingsChangedEvent($"{Request.Path}", SettingsChangedSourceType.TradingInstrument);

            return instrument;
        }

        /// <summary>
        /// Assing trading instrument to a trading condition
        /// </summary>
        /// <param name="tradingConditionId"></param>
        /// <param name="instruments"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{tradingConditionId}")]
        public async Task<List<TradingInstrumentContract>> AssignCollection(string tradingConditionId, 
            [FromBody] string[] instruments)
        {
            var currentInstruments =
                await _tradingInstrumentsRepository.GetAsync(x => x.TradingConditionId == tradingConditionId);

            if (currentInstruments.Any())
            {
                var toRemove = currentInstruments.Where(x => !instruments.Contains(x.Instrument)).ToArray();
                
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
                    await _tradingInstrumentsRepository.DeleteAsync(
                        TradingInstrumentEntity.GetId(pair.TradingConditionId, pair.Instrument));
                }
            }
            
            var pairsToAdd = instruments.Where(x => currentInstruments.All(y => y.Instrument != x));

            var addedPairs = await _tradingInstrumentsRepository.CreateDefaultTradingInstruments(tradingConditionId,
                pairsToAdd, _defaultTradingInstrumentSettings);
            
            await _eventSender.SendSettingsChangedEvent($"{Request.Path}", SettingsChangedSourceType.TradingInstrument);

            return addedPairs.Select(x => _convertService.Convert<TradingInstrument, TradingInstrumentContract>(x))
                .ToList();
        }

        /// <summary>
        /// Get trading instrument
        /// </summary>
        /// <param name="tradingConditionId"></param>
        /// <param name="assetPairId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{tradingConditionId}/{assetPairId}")]
        public async Task<TradingInstrumentContract> Get(string tradingConditionId, string assetPairId)
        {
            var obj = await _tradingInstrumentsRepository.GetAsync(
                TradingInstrumentEntity.GetId(tradingConditionId, assetPairId));

            return _convertService.Convert<TradingInstrument, TradingInstrumentContract>(obj);
        }

        /// <summary>
        /// Update the trading instrument
        /// </summary>
        /// <param name="tradingConditionId"></param>
        /// <param name="assetPairId"></param>
        /// <param name="instrument"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{tradingConditionId}/{assetPairId}")]
        public async Task<TradingInstrumentContract> Update(string tradingConditionId, string assetPairId, 
            [FromBody] TradingInstrumentContract instrument)
        {
            if (string.IsNullOrWhiteSpace(instrument?.TradingConditionId))
            {
                throw new ArgumentNullException(nameof(instrument.TradingConditionId),
                    "TradingConditionId must be set");
            }
            if (string.IsNullOrWhiteSpace(instrument.Instrument))
            {
                throw new ArgumentNullException(nameof(instrument.Instrument),
                    "Instrument must be set");
            }

            await _tradingInstrumentsRepository.ReplaceAsync(
                _convertService.Convert<TradingInstrumentContract, TradingInstrument>(instrument));

            await _eventSender.SendSettingsChangedEvent($"{Request.Path}", SettingsChangedSourceType.TradingInstrument);
            
            return instrument;
        }

        /// <summary>
        /// Delete the trading instrument
        /// </summary>
        /// <param name="tradingConditionId"></param>
        /// <param name="assetPairId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{tradingConditionId}/{assetPairId}")]
        public async Task Delete(string tradingConditionId, string assetPairId)
        {
            await _tradingInstrumentsRepository.DeleteAsync(assetPairId);

            await _eventSender.SendSettingsChangedEvent($"{Request.Path}", SettingsChangedSourceType.TradingInstrument);
        }
    }
}