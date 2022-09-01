using RainbowOF.Models.Lookups;
using RainbowOF.Models.Woo;
using RainbowOF.Repositories.Integrations;
using RainbowOF.ViewModels.Lookups;
using System;

namespace RainbowOF.Repositories.Lookups
{
    /// <summary>
    /// Interface that uses the general model for grid CRUD to implement the attribute variety
    /// </summary>
    public interface IAttributeVarietyWooLinkedView : IWooLinkedView<ItemAttributeVarietyLookup, ItemAttributeVarietyLookupView, WooProductAttributeTermMap>
    {
        void SetParentAttributeId(Guid sourceParentAttributeId);  //allows for a rest of the Parent Guid
        //Task<List<ItemUoM>> GetItemUoMsAsync(List<Guid> linkedItemUoMIDs);

    }
}

