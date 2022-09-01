using RainbowOF.FrontEnd.Models.Classes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RainbowOF.Integration.Repositories.Woo
{
    public interface IWooImportWithAParent<TEntity, TWooEntity, TWooMapEntity> : IWooImportBase<TWooEntity> where TEntity : class
                                                                                                            where TWooEntity : class
                                                                                                            where TWooMapEntity : class
    {
        Task<List<TWooEntity>> GetWooEntityDataAsync(uint parentAttributeId);
        Task<Guid> AddEntityAsync(TWooEntity newWooEntity, TWooMapEntity sourceWooMap, Guid sourceParentId);
        //-> Should have and AddWooMappingEntity(TWooEntity sourceWooEntity, TWooMapEntity targetWooMap, Guid sourceParentId); 
        Task<Guid> AddOrGetEntityIDAsync(TWooEntity sourceEntity, Guid sourceParentId);
        Task<Guid> AddOrUpdateEntityAsync(TWooEntity sourceEntity, TWooMapEntity sourceWooMap, Guid sourceParentId);   // sourceWooMapped was Guid sourceWooMappedEntityId, 
        Task<bool> FindAndSetParentEntityAsync(WooItemWithParent sourceWooEntityWithParent);
        Task<Guid> ImportAndMapWooEntityDataAsync(TWooEntity sourceEntity, Guid sourceParentId);   // ImportAndMapCategoryData (ProductData sourcePC, List<WooItemWithParenmt> sourceCategoriesWithParents)
        //Task<bool> SetWooEntityParent(Guid sourceChildWooEntityId, Guid sourceParentWooEntityId);   //SetParentCategory
        Task<Guid> UpdateEntityAsync(TWooEntity updatedWooEntity, Guid sourceParentId, TEntity updatedEntity); //, List<WooItemWithParent> sourceWooEntityWithParents);    //UpdateItemCategoryLookup(TWooEntity updatedPC, ItemCategoryLookup updatedItemCategoryLookup, List<WooItemWithParent> sourceAttribsWithParents);
        Task<Guid> UpdateWooMappingEntityAsync(TWooEntity sourceWooEntity, TWooMapEntity targetWooMap, Guid sourceParentId);     //WooCategoryMap targetWooCategoryMap, List<WooItemWithParent> sourceCategoriesWithParents)
    }
}
