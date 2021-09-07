using Microsoft.AspNetCore.Components;
using RainbowOF.FrontEnd.Models.Classes;
using RainbowOF.Models.Lookups;
using RainbowOF.Models.System;
using RainbowOF.Models.Woo;
using RainbowOF.Repositories.Common;
using RainbowOF.Tools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WooCommerceNET.WooCommerce.v3;

namespace RainbowOF.Integration.Repositories.Woo
{
    /// <summary>
    /// Generic Interface for import of WooEntities with or without parents
    /// </summary>
    /// <typeparam name="TEntity">The systems data we are importing to</typeparam>
    /// <typeparam name="TWooEntity">The Woo Entity we are importing</typeparam>
    /// <typeparam name="TWooMapEntity">The map</typeparam>
    public interface IWooImport<TEntity, TWooEntity, TWooMapEntity> where TEntity : class
                                                                    where TWooEntity : class
                                                                    where TWooMapEntity : class
    {
        IAppUnitOfWork _AppUnitOfWork { get; set; }
        ILoggerManager _Logger { get; set; }

        Task<Guid> AddOrGetEntityID(TWooEntity sourceEntity, List<WooItemWithParent> sourceWooEntityWithParents);
        Task<Guid> AddOrUpdateEntity(TWooEntity sourcePC, Guid sourceWooMappedEntityId, List<WooItemWithParent> sourceWooEntityWithParents);
        Task<Guid> AddWooEntity(TWooEntity newPC, List<WooItemWithParent> newWooEntityWithParents);
        Task<bool> FindAndSetParentEntity(WooItemWithParent sourceWooEntityWithParent);
        Task<Guid> GetWooMappedEntityById(int sourceWooEntityId);
        Task<List<TWooEntity>> GetWooEntityData(bool onlyInStock = true);    //?GetWooCategoryData
        Task<Guid> ImportAndMapWooEntityData(TWooEntity sourcePC, List<WooItemWithParent> sourceWooEntityWithParents);   // ImportAndMapCategoryData (ProductData sourcePC, List<WooItemWithParenmt> sourceCategoriesWithParents)
        Task<int> ImportWooEntityData(List<TWooEntity> sourceWooEntities);  // ImportCategoryData
        bool IsImporting();
        Task<bool> SetWooEntityParent(Guid sourceChildWooEntityId, Guid sourceParentWooEntityId);   //SetParentCategory
        Task<Guid> UpdateEntity(TWooEntity updatedPC, TEntity updatedEntity, List<WooItemWithParent> sourceWooEntityWithParents);    //UpdateItemCategoryLookup(TWooEntity updatedPC, ItemCategoryLookup updatedItemCategoryLookup, List<WooItemWithParent> sourceAttribsWithParents);
        Task<Guid> UpdateWooEntity(TWooEntity updatedPC, TWooMapEntity targetWooMap, List<WooItemWithParent> sourceWooEntityWithParents);     //WooCategoryMap targetWooCategoryMap, List<WooItemWithParent> sourceCategoriesWithParents)
    }
}