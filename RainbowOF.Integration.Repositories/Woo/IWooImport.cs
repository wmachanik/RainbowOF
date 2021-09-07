using Microsoft.AspNetCore.Components;
using RainbowOF.FrontEnd.Models.Classes;
using RainbowOF.Models.System;
using RainbowOF.Repositories.Common;
using RainbowOF.Tools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RainbowOF.Integration.Repositories.Woo
{
    /// <summary>
    /// Generic Interface for import of WooEntities with or without parents
    /// </summary>
    /// <typeparam name="TEntity">The systems data we are importing to</typeparam>
    /// <typeparam name="TWooEntity">The Woo Entity we are importing</typeparam>
    /// <typeparam name="TWooMapEntity">The map</typeparam>
    public interface IWooImport<TEntity, TWooEntity, TWooMapEntity> : IWooImportBase<TWooEntity> where TEntity : class
                                                                                                 where TWooEntity : class
                                                                                                 where TWooMapEntity : class
    {
        Task<List<TWooEntity>> GetWooEntityDataAsync();
        Task<Guid> AddWooEntityToMappingAsync(TWooEntity newEntity, TWooMapEntity sourceWooMap);
        Task<Guid> AddOrGetEntityIDAsync(TWooEntity sourceEntity);
        Task<Guid> AddOrUpdateEntityAsync(TWooEntity sourceEntity, Guid sourceWooMappedEntityId);
        Task<Guid> ImportAndMapWooEntityDataAsync(TWooEntity sourceEntity);   // ImportAndMapCategoryData (ProductData sourcePC, List<WooItemWithParenmt> sourceCategoriesWithParents)
        Task<Guid> UpdateEntityAsync(TWooEntity updatedWooEntity, TEntity updatedEntity); //, List<WooItemWithParent> sourceWooEntityWithParents);    //UpdateItemCategoryLookup(TWooEntity updatedPC, ItemCategoryLookup updatedItemCategoryLookup, List<WooItemWithParent> sourceAttribsWithParents);
        Task<Guid> UpdatedWooEntityAsync(TWooEntity updatedWooEntity, TWooMapEntity targetWooMap);     //WooCategoryMap targetWooCategoryMap, List<WooItemWithParent> sourceCategoriesWithParents)
    }
}