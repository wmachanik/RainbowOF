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
            CreateMap<ItemAttributeVarietyLookupView, ItemAttributeVarietyLookup>()
                .ForMember(dest => dest.UoMId, act => act.MapFrom(src => ((src.UoMId ?? Guid.Empty) == Guid.Empty) ? null : src.UoMId));
            //CreateMap<ItemAttributeVarietyLookupView, ItemAttributeVarietyLookup>();
            //CreateMap<ItemAttributeVarietyLookup, ItemAttributeVarietyLookupView>();
            // for update mapping - this is recommended for any EF class with nullable property.
            CreateMap<ItemCategory, ItemCategory>()
                .ForMember(dest => dest.UoMBaseId, act => act.MapFrom(src => ((src.UoMBaseId ?? Guid.Empty) == Guid.Empty) ? null : src.UoMBaseId));
            CreateMap<ItemAttribute, ItemAttribute>();
            CreateMap<ItemAttributeVarietyLookup, ItemAttributeVariety>();
            CreateMap<ItemAttributeVariety, ItemAttributeLookup>();
            CreateMap<ItemAttributeVariety, ItemAttributeVariety>();
                //.ForMember(dest => dest.UoMId, act => act.MapFrom(src => ((src.UoMId ?? Guid.Empty) == Guid.Empty) ? null : src.UoMId));

        }
    }
}
