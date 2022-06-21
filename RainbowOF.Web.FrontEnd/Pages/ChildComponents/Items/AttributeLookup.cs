using System;
using System.Collections.Generic;

namespace RainbowOF.Web.FrontEnd.Pages.ChildComponents.Items
{
    public partial class ItemVariantsComponent
    {
        class AttributeLookup
        {
            public Guid AttributeLookupId { get; set; }
            public string AttrbiuteName { get; set; }
            public List<AttributeVariableLookup> AttributeVaraibleLookups { get; set; }
        }

    }
}
