﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarginTrading.SettingsService.Contracts;
using MarginTrading.SettingsService.Contracts.Common;
using MarginTrading.SettingsService.Contracts.Market;
using MarginTrading.SettingsService.Core.Domain;
using MarginTrading.SettingsService.Core.Interfaces;
using MarginTrading.SettingsService.Core.Services;
using MarginTrading.SettingsService.Extensions;
using MarginTrading.SettingsService.Middleware;
using MarginTrading.SettingsService.StorageInterfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.SettingsService.Controllers
{
    /// <summary>
    /// Markets management
    /// </summary>
    [Route("api/markets")]
    [MiddlewareFilter(typeof(RequestLoggingPipeline))]
    public class MarketsController : Controller, IMarketsApi
    {
        private readonly IMarketRepository _marketRepository;
        private readonly IConvertService _convertService;
        private readonly IEventSender _eventSender;
        
        public MarketsController(
            IMarketRepository marketRepository,
            IConvertService convertService,
            IEventSender eventSender)
        {
            _marketRepository = marketRepository;
            _convertService = convertService;
            _eventSender = eventSender;
        }
        
        /// <summary>
        /// Get the list of Markets
        /// </summary>
        [HttpGet]
        [Route("")]
        public async Task<List<MarketContract>> List()
        {
            var data = await _marketRepository.GetAsync();
            
            return data.Select(x => _convertService.Convert<IMarket, MarketContract>(x)).ToList();
        }
        
        /// <summary>
        /// Create new market
        /// </summary>
        [HttpPost]
        [Route("")]
        public async Task<MarketContract> Insert([FromBody] MarketUpsertRequestParams @params)
        {
            @params?.Traceability.Validate();
            Validate(@params.Market);

            if (!await _marketRepository.TryInsertAsync(_convertService.Convert<MarketContract, Market>(@params.Market)))
            {
                throw new ArgumentException($"Market with id {@params.Market.Id} already exists", nameof(@params.Market.Id));
            }

            await _eventSender.SendSettingsChangedEvent(
                @params.Traceability.CorrelationId, 
                @params.Traceability.Id,
                $"{Request.Path}", 
                SettingsChangedSourceType.Market);

            return @params.Market;
        }

        /// <summary>
        /// Get the market
        /// </summary>
        [HttpGet]
        [Route("{marketId}")]
        public async Task<MarketContract> Get(string marketId)
        {
            var obj = await _marketRepository.GetAsync(marketId);
            
            return _convertService.Convert<IMarket, MarketContract>(obj);
        }

        /// <summary>
        /// Update the market
        /// </summary>
        [HttpPut]
        [Route("{marketId}")]
        public async Task<MarketContract> Update(string marketId, [FromBody] MarketUpsertRequestParams @params)
        {
            @params?.Traceability.Validate();
            Validate(@params.Market);
            ValidateId(marketId, @params.Market);

            await _marketRepository.UpdateAsync(_convertService.Convert<MarketContract, Market>(@params.Market));

            await _eventSender.SendSettingsChangedEvent(
                @params.Traceability.CorrelationId, 
                @params.Traceability.Id,
                $"{Request.Path}", 
                SettingsChangedSourceType.Market);
            
            return @params.Market;
        }

        /// <summary>
        /// Delete the market
        /// </summary>
        [HttpDelete]
        [Route("{marketId}")]
        public async Task Delete(string marketId, [FromBody] TraceableRequestParams @params)
        {
            @params.Validate();
            
            await _marketRepository.DeleteAsync(marketId);

            await _eventSender.SendSettingsChangedEvent(
                @params.CorrelationId, 
                @params.Id,
                $"{Request.Path}", 
                SettingsChangedSourceType.Market);
        }

        private void ValidateId(string id, MarketContract contract)
        {
            if (contract?.Id != id)
            {
                throw new ArgumentException("Id must match with contract id");
            }
        }

        private void Validate(MarketContract newValue)
        {
            if (newValue == null)
            {
                throw new ArgumentNullException("market", "Model is incorrect");
            }
            
            if (string.IsNullOrWhiteSpace(newValue.Id))
            {
                throw new ArgumentNullException(nameof(newValue.Id), "market Id must be set");
            }
        }
    }
}