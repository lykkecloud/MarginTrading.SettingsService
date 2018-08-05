using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarginTrading.SettingsService.Contracts;
using MarginTrading.SettingsService.Contracts.AssetPair;
using MarginTrading.SettingsService.Contracts.Common;
using MarginTrading.SettingsService.Contracts.Enums;
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
    /// Asset pairs management
    /// </summary>
    [Route("api/assetPairs")]
    [MiddlewareFilter(typeof(RequestLoggingPipeline))]
    public class AssetPairsController : Controller, IAssetPairsApi
    {
        private readonly IAssetsRepository _assetsRepository;
        private readonly IAssetPairsRepository _assetPairsRepository;
        private readonly IMarketRepository _marketRepository;
        private readonly IConvertService _convertService;
        private readonly IEventSender _eventSender;
        private readonly DefaultLegalEntitySettings _defaultLegalEntitySettings;
        
        public AssetPairsController(
            IAssetsRepository assetsRepository,
            IAssetPairsRepository assetPairsRepository,
            IMarketRepository marketRepository,
            IConvertService convertService, 
            IEventSender eventSender,
            DefaultLegalEntitySettings defaultLegalEntitySettings)
        {
            _assetsRepository = assetsRepository;
            _assetPairsRepository = assetPairsRepository;
            _marketRepository = marketRepository;
            _convertService = convertService;
            _eventSender = eventSender;
            _defaultLegalEntitySettings = defaultLegalEntitySettings;
        }
        
        /// <summary>
        /// Get the list of asset pairs based on legal entity and matching engine mode
        /// </summary>
        /// <param name="legalEntity"></param>
        /// <param name="matchingEngineMode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<List<AssetPairContract>> List([FromQuery] string legalEntity = null, 
            [FromQuery] MatchingEngineModeContract? matchingEngineMode = null)
        {
            var data = await _assetPairsRepository.GetByLeAndMeModeAsync(legalEntity, matchingEngineMode?.ToString());
            
            return data.Select(x => _convertService.Convert<IAssetPair, AssetPairContract>(x)).ToList();
        }

        /// <summary>
        /// Get the list of asset pairs based on legal entity and matching engine mode, with optional pagination
        /// </summary>
        [HttpGet]
        [Route("by-pages")]
        public async Task<PaginatedResponseContract<AssetPairContract>> ListByPages([FromQuery] string legalEntity = null, 
            [FromQuery] MatchingEngineModeContract? matchingEngineMode = null, 
            [FromQuery] int? skip = null, [FromQuery] int? take = null)
        {
            ApiValidationHelper.ValidatePagingParams(skip, take);
            
            var data = await _assetPairsRepository.GetByLeAndMeModeByPagesAsync(legalEntity, 
                matchingEngineMode?.ToString(), skip, take);
            
            return new PaginatedResponseContract<AssetPairContract>(
                contents: data.Contents.Select(x => _convertService.Convert<IAssetPair, AssetPairContract>(x)).ToList(),
                start: data.Start,
                size: data.Size,
                totalSize: data.TotalSize
            );
        }

        /// <summary>
        /// Create new asset pair
        /// </summary>
        [HttpPost]
        [Route("")]
        public async Task<AssetPairContract> Insert([FromBody] AssetPairUpsertRequestParams @params)
        {
            @params?.Traceability.Validate();
            await ValidatePair(@params.AssetPair);
            
            _defaultLegalEntitySettings.Set(@params.AssetPair);

            if (!await _assetPairsRepository.TryInsertAsync(
                _convertService.Convert<AssetPairContract, AssetPair>(@params.AssetPair)))
            {
                throw new ArgumentException($"Asset pair with id {@params.AssetPair.Id} already exists", nameof(@params.AssetPair.Id));
            }

            await _eventSender.SendSettingsChangedEvent(
                @params.Traceability.CorrelationId, 
                @params.Traceability.Id,
                $"{Request.Path}", 
                SettingsChangedSourceType.AssetPair);
            
            return @params.AssetPair;
        }

        /// <summary>
        /// Get asset pair by id
        /// </summary>
        [HttpGet]
        [Route("{assetPairId}")]
        public async Task<AssetPairContract> Get(string assetPairId)
        {
            var obj = await _assetPairsRepository.GetAsync(assetPairId);
            return _convertService.Convert<IAssetPair, AssetPairContract>(obj);
        }

        /// <summary>
        /// Update asset pair
        /// </summary>
        [HttpPut]
        [Route("{assetPairId}")]
        public async Task<AssetPairContract> Update(string assetPairId,
            [FromBody] AssetPairUpsertRequestParams @params)
        {
            @params?.Traceability.Validate();
            await ValidatePair(@params.AssetPair);
            ValidateId(assetPairId, @params.AssetPair);

            _defaultLegalEntitySettings.Set(@params.AssetPair);

            await _assetPairsRepository.UpdateAsync(_convertService.Convert<AssetPairContract, AssetPair>(@params.AssetPair));

            await _eventSender.SendSettingsChangedEvent(
                @params.Traceability.CorrelationId, 
                @params.Traceability.Id,
                $"{Request.Path}", 
                SettingsChangedSourceType.AssetPair);
            
            return @params.AssetPair;
        }

        /// <summary>
        /// Delete asset pair
        /// </summary>
        [HttpDelete]
        [Route("{assetPairId}")]
        public async Task Delete(string assetPairId, [FromBody] TraceableRequestParams @params)
        {
            @params.Validate();
            
            await _assetPairsRepository.DeleteAsync(assetPairId);

            await _eventSender.SendSettingsChangedEvent(
                @params.CorrelationId, 
                @params.Id,
                $"{Request.Path}", 
                SettingsChangedSourceType.AssetPair);
        }

        private async Task ValidatePair(AssetPairContract newValue)
        {
            if (newValue == null)
            {
                throw new ArgumentNullException("assetPair", "Model is incorrect");
            }
            
            if (string.IsNullOrWhiteSpace(newValue.Id))
            {
                throw new ArgumentNullException(nameof(newValue.Id), "AssetPair Id must be set");
            }

            if (!Enum.IsDefined(typeof(MatchingEngineModeContract), newValue.MatchingEngineMode))
            {
                throw new ArgumentNullException(nameof(newValue.MatchingEngineMode), "AssetPair MatchingEngineMode must be set");
            }

            if (await _assetsRepository.GetAsync(newValue.BaseAssetId) == null)
            {
                throw new InvalidOperationException($"Base Asset {newValue.BaseAssetId} does not exist");
            }

            if (await _assetsRepository.GetAsync(newValue.QuoteAssetId) == null)
            {
                throw new InvalidOperationException($"Quote Asset {newValue.QuoteAssetId} does not exist");
            }

            if (!string.IsNullOrEmpty(newValue.MarketId)
                && await _marketRepository.GetAsync(newValue.MarketId) == null)
            {
                throw new InvalidOperationException($"Market {newValue.MarketId} does not exist");
            }

            if (newValue.StpMultiplierMarkupAsk <= 0)
            {
                throw new InvalidOperationException($"StpMultiplierMarkupAsk must be greather then zero");
            }
            
            if (newValue.StpMultiplierMarkupBid <= 0)
            {
                throw new InvalidOperationException($"StpMultiplierMarkupBid must be greather then zero");
            }
            
            //base pair check <-- the last one
            if (newValue.BasePairId == null) 
                return;

            if (await _assetPairsRepository.GetAsync(newValue.BasePairId) == null)
            {
                throw new InvalidOperationException($"BasePair with Id {newValue.BasePairId} does not exist");
            }

            if (await _assetPairsRepository.GetByBaseAssetPairAsync(newValue.BasePairId) != null)
            {
                throw new InvalidOperationException($"BasePairId {newValue.BasePairId} does not exist");
            }

            if (await _assetPairsRepository.GetByBaseAssetPairAndNotByIdAsync(newValue.Id, newValue.BasePairId) != null)
            {
                throw new InvalidOperationException($"BasePairId {newValue.BasePairId} cannot be added twice");
            }    
        }

        private void ValidateId(string id, AssetPairContract contract)
        {
            if (contract?.Id != id)
            {
                throw new ArgumentException("Id must match with contract id");
            }
        }
    }
}