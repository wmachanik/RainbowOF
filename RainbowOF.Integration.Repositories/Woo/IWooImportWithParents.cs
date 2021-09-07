using RainbowOF.FrontEnd.Models.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.Integration.Repositories.Woo
{
    public interface IWooImportWithParents<TEntity, TWooEntity, TWooMapEntity> : IWooImportBase<TWooEntity> where TEntity : class
                                                                                                            where TWooEntity : class
                                                                                                            where TWooMapEntity : class
    {
        List<WooItemWithParent> EntityWithParents { get; set; }
        Task<List<TWooEntity>> GetWooEntityDataAsync(bool OnlyItemsInStock = true);
        Task<Guid> AddEntityAsync(TWooEntity newWooEntity); //---> not sure if we need this here, TWooMapEntity sourceWooMap);
        Task<Guid> AddOrGetEntityIDAsync(TWooEntity sourceEntity); // using the class level var instead --, List<WooItemWithParent> sourceWooEntityWithParents);
        Task<Guid> AddOrUpdateEntityAsync(TWooEntity sourceEntity, Guid sourceWooMappedEntityId); // using the class level var instead --,, List<WooItemWithParent> sourceWooEntityWithParents);
        Task<bool> FindAndSetParentEntityAsync(WooItemWithParent sourceWooEntityWithParent);
        Task<Guid> ImportAndMapWooEntityDataAsync(TWooEntity sourceEntity); // using the class level var instead --,, List<WooItemWithParent> sourceWooEntityWithParents);   // ImportAndMapCategoryData (ProductData sourcePC, List<WooItemWithParenmt> sourceCategoriesWithParents)
        Task<bool> SetWooEntityParentAsync(Guid sourceChildWooEntityId, Guid sourceParentWooEntityId);   //SetParentCategory
        Task<Guid> UpdateEntityAsync(TWooEntity updatedWooEntity, TEntity updateEntity); //, List<WooItemWithParent> sourceWooEntityWithParents);    //UpdateItemCategoryLookup(TWooEntity updatedPC, ItemCategoryLookup updatedItemCategoryLookup, List<WooItemWithParent> sourceAttribsWithParents);
        Task<Guid> UpdateWooMappingEntityAsync(TWooEntity updatedWooEntity, TWooMapEntity targetWooMap); // using the class level var instead --,, List<WooItemWithParent> sourceWooEntityWithParents);     //WooCategoryMap targetWooCategoryMap, List<WooItemWithParent> sourceCategoriesWithParents)
    }
}
