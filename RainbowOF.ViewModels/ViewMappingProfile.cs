using AutoMapper;
using RainbowOF.Models.Items;
using RainbowOF.Models.Lookups;
using RainbowOF.ViewModels.Items;
using RainbowOF.ViewModels.Lookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.ViewModels
{
    public class ViewMappingProfile : Profile
    {
        public ViewMappingProfile()
        {
            // Item profile maps
            CreateMap<Item, ItemView>();
            CreateMap<ItemView, Item>();
            // Lookup Profile Maps
            CreateMap<ItemCategoryLookup, ItemCategoryLookupView>();
            CreateMap<ItemCategoryLookupView, ItemCategoryLookup>();
            CreateMap<ItemAttributeLookup, ItemAttributeLookupView>();
            CreateMap<ItemAttributeLookupView, ItemAttributeLookup>();
            CreateMap<ItemAttributeVarietyLookup, ItemAttributeVarietyLookupView>();
            CreateMap<ItemAttributeVarietyLookupView, ItemAttributeVarietyLookup>();
        }
    }
}
