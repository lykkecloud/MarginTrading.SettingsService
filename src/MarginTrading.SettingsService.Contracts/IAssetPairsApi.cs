// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.SettingsService.Contracts.AssetPair;
using MarginTrading.SettingsService.Contracts.Common;
using MarginTrading.SettingsService.Contracts.Enums;
using Refit;

namespace MarginTrading.SettingsService.Contracts
{
    /// <summary>
    /// Asset pairs management
    /// </summary>
    [PublicAPI]
    public interface IAssetPairsApi
    {
        /// <summary>
        /// Get the list of asset pairs based on legal entity and matching engine mode
        /// </summary>
        [Get("/api/assetPairs")]
        [Obsolete("Use paginated action")]
        Task<List<AssetPairContract>> List(
            [Query] [CanBeNull] string legalEntity = null,
            [Query] [CanBeNull] MatchingEngineModeContract? matchingEngineMode = null, 
            [Query] [CanBeNull] string filter = null);
        
        /// <summary>
        /// Get the list of asset pairs based on legal entity and matching engine mode, with optional pagination
        /// </summary>
        [Get("/api/assetPairs/by-pages")]
        Task<PaginatedResponseContract<AssetPairContract>> ListByPages(
            [Query, CanBeNull] string legalEntity = null,
            [Query, CanBeNull] MatchingEngineModeContract? matchingEngineMode = null,
            [Query] [CanBeNull] string filter = null,
            [Query, CanBeNull] int? skip = null, [Query, CanBeNull] int? take = null);


        [Post("/api/assetPairs")]
        Task<AssetPairContract> Insert([Body] AssetPairContract assetPair);

        
        [Post("/api/assetPairs/batch")]
        Task<List<AssetPairContract>> BatchInsert([Body] AssetPairContract[] assetPairs);


        [ItemCanBeNull]
        [Get("/api/assetPairs/{assetPairId}")]
        Task<AssetPairContract> Get([NotNull] string assetPairId);


        [Put("/api/assetPairs/{assetPairId}")]
        Task<AssetPairContract> Update([NotNull] string assetPairId, 
            [Body] AssetPairUpdateRequest assetPairUpdateRequest);


        [Put("/api/assetPairs/batch")]
        Task<List<AssetPairContract>> BatchUpdate([Body] AssetPairUpdateRequest[] assetPairsUpdateRequest);


        [Delete("/api/assetPairs/{assetPairId}")]
        Task Delete([NotNull] string assetPairId);
    }
}