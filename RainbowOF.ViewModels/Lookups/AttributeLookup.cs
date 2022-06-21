using System;
using System.Collections.Generic;

namespace RainbowOF.ViewModels.Lookups
{
    /// <summary>
    /// Used to store the collection of Lookups an Item can have in the view
    /// </summary>
    public class AttributeVariableLookup
    {
        public Guid AttributeVariantLookupId { get; set; }
        public string AttributeVariantName { get; set; }
    }

    public class AttributeLookup
    {
        public Guid AttributeLookupId { get; set; }
        public string AttrbiuteName { get; set; }
        public List<AttributeVariableLookup> AttributeVaraibleLookups { get; set; }
    }


}
