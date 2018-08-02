using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarginTrading.SettingsService.Contracts;
using MarginTrading.SettingsService.Contracts.Asset;
using MarginTrading.SettingsService.Contracts.AssetPair;
using MarginTrading.SettingsService.Contracts.Common;
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
    /// Assets management
    /// </summary>
    [Route("api/assets")]
    [MiddlewareFilter(typeof(RequestLoggingPipeline))]
    public class AssetsController : Controller, IAssetsApi
    {
        private readonly IAssetsRepository _assetsRepository;
        private readonly IConvertService _convertService;
        private readonly IEventSender _eventSender;
        
        public AssetsController(
            IAssetsRepository assetsRepository,
            IConvertService convertService,
            IEventSender eventSender)
        {
            _assetsRepository = assetsRepository;
            _convertService = convertService;
            _eventSender = eventSender;
        }
        
        /// <summary>
        /// Get the list of assets
        /// </summary>
        [HttpGet]
        [Route("")]
        public async Task<List<AssetContract>> List()
        {
            var data = await _assetsRepository.GetAsync();
            
            return data.Select(x => _convertService.Convert<IAsset, AssetContract>(x)).ToList();
        }

        /// <summary>
        /// Create new asset
        /// </summary>
        [HttpPost]
        [Route("")]
        public async Task<AssetContract> Insert([FromBody] AssetUpsertRequestParams @params)
        {
            @params?.Traceability.Validate();
            Validate(@params.Asset);

            if (!await _assetsRepository.TryInsertAsync(_convertService.Convert<AssetContract, Asset>(@params.Asset)))
            {
                throw new ArgumentException($"Asset with id {@params.Asset.Id} already exists", nameof(@params.Asset.Id));
            }

            await _eventSender.SendSettingsChangedEvent(
                @params.Traceability.CorrelationId, 
                @params.Traceability.Id,
                $"{Request.Path}", 
                SettingsChangedSourceType.Asset);

            return @params.Asset;
        }

        /// <summary>
        /// Get the asset
        /// </summary>
        [HttpGet]
        [Route("{assetId}")]
        public async Task<AssetContract> Get(string assetId)
        {
            var obj = await _assetsRepository.GetAsync(assetId);
            
            return _convertService.Convert<IAsset, AssetContract>(obj);
        }

        /// <summary>
        /// Update the asset
        /// </summary>
        [HttpPut]
        [Route("{assetId}")]
        public async Task<AssetContract> Update(string assetId, [FromBody] AssetUpsertRequestParams @params)
        {
            @params?.Traceability.Validate();
            Validate(@params.Asset);
            ValidateId(assetId, @params.Asset);

            await _assetsRepository.UpdateAsync(_convertService.Convert<AssetContract, Asset>(@params.Asset));

            await _eventSender.SendSettingsChangedEvent(
                @params.Traceability.CorrelationId, 
                @params.Traceability.Id,
                $"{Request.Path}", 
                SettingsChangedSourceType.Asset);
            
            return @params.Asset;
        }

        /// <summary>
        /// Delete the asset
        /// </summary>
        [HttpDelete]
        [Route("{assetId}")]
        public async Task Delete(string assetId, [FromBody] TraceableRequestParams @params)
        {
            @params.Validate();
            
            await _assetsRepository.DeleteAsync(assetId);

            await _eventSender.SendSettingsChangedEvent(
                @params.CorrelationId, 
                @params.Id,
                $"{Request.Path}", 
                SettingsChangedSourceType.Asset);
        }

        private void ValidateId(string id, AssetContract contract)
        {
            if (contract?.Id != id)
            {
                throw new ArgumentException("Id must match with contract id");
            }
        }

        private void Validate(AssetContract newValue)
        {
            if (newValue == null)
            {
                throw new ArgumentNullException("asset", "Model is incorrect");
            }
            
            if (string.IsNullOrWhiteSpace(newValue.Id))
            {
                throw new ArgumentNullException(nameof(newValue.Id), "asset Id must be set");
            }
        }
    }
}