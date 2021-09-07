using AutoMapper;
using RainbowOF.Models.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WooCommerceNET.WooCommerce.v3;

namespace RainbowOF.Integration.Repositories.Classes
{
    public class IntegrationMappingProfile : Profile
    {
        public const string CONST_WooItemOutOfStock = "outofstock";
        public string Truncate(string sourceString, int targetLength)
        {
            return (sourceString.Length > targetLength) ?
                sourceString.Substring(0, targetLength) :
                sourceString;
        }
        public string MakeAbbriviation(string originalName)
        {
            string _newName = string.Empty;
            string _vowels = "AEIOUaeiou -+:%$&*@.()!";  // remove vowels and spaces and symbols
            int i = 0;
            while ((i < originalName.Length) && (_newName.Length < 9))  // 9 so links to items mode max 10
            {
                if (!_vowels.Contains(originalName[i]))
                {
                    _newName += originalName[i].ToString(); //.ToUpper();
                }
                i++;
            }
            return _newName;
        }
        public string StripHTML(string originalHTML)
        {
            return System.Text.RegularExpressions.Regex.Replace(originalHTML, @"<(.|\n)*?>", string.Empty);
        }
        /// <summary>
        /// Change string simple, grouped, external and variable. to the ItemType we use
        /// </summary>
        /// <param name="sourceWooProductType">the string to convert from</param>
        /// <returns>ItemType</returns>
        public ItemTypes MapWooTypeToItemType(string sourceWooProductType)
        {
            switch (sourceWooProductType)
            {
                case "grouped":
                    return ItemTypes.Collection;
                case "external":
                    return ItemTypes.URL;
                case "variable":
                    return ItemTypes.Variable;
                case "virtual":
                    return ItemTypes.VirtualItem;
                default:    // all others are simple
                    return ItemTypes.Simple;
            }
        }
        static bool MapObjectToBool(object sourceObject) => 
            (sourceObject.GetType() == typeof(bool)) ?  (bool)sourceObject :  false;

        public IntegrationMappingProfile()
        {
            CreateMap<Product, Item>()
                .ForMember(dest => dest.ItemName, act => act.MapFrom(src => Truncate(src.name, 100)))
                .ForMember(dest => dest.SKU, act => act.MapFrom(src => Truncate(src.sku, 50)))
                .ForMember(dest => dest.IsEnabled, act => act.MapFrom(src => (src.stock_status != CONST_WooItemOutOfStock)))
                .ForMember(dest => dest.ItemDetail, act => act.MapFrom(src => Truncate(StripHTML(src.short_description),500)))
                .ForMember(dest => dest.ItemAbbreviatedName, act => act.MapFrom(src => MakeAbbriviation(src.name)))
                .ForMember(dest => dest.SortOrder, act => act.MapFrom(src => Convert.ToInt32(src.menu_order ?? 0)))
                .ForMember(dest => dest.BasePrice, act => act.MapFrom(src => Convert.ToDecimal(src.price == null ? 0.0 : src.price)))
                .ForMember(dest => dest.ManageStock, act => act.MapFrom(src => src.manage_stock ?? false))
                .ForMember(dest => dest.QtyInStock, act => act.MapFrom(src => Convert.ToInt32(src.stock_quantity ?? 0)))
                .ForMember(dest => dest.ItemType, act => act.MapFrom(src => MapWooTypeToItemType((src._virtual ?? false) ? "virtual" : src.type)));

            CreateMap<Variation, ItemVariant>()
                .ForMember(dest => dest.ItemVariantName, act => act.MapFrom(src => Truncate(StripHTML(src.description), 100)))
                .ForMember(dest => dest.SKU, act => act.MapFrom(src => Truncate(src.sku, 50)))
                .ForMember(dest => dest.ItemVariantAbbreviation, act => act.MapFrom(src => MakeAbbriviation(src.sku)))
                .ForMember(dest => dest.IsEnabled, act => act.MapFrom(src => (src.stock_status != CONST_WooItemOutOfStock)))
                .ForMember(dest => dest.SortOrder, act => act.MapFrom(src => Convert.ToInt32(src.menu_order)))
                .ForMember(dest => dest.BasePrice, act => act.MapFrom(src => Convert.ToDecimal(src.price == null ? 0.0 : src.price)))
                .ForMember(dest => dest.ManageStock, act => act.MapFrom(src => (MapObjectToBool(src.manage_stock))))   //-> can be a string with the value "parent"
                .ForMember(dest => dest.QtyInStock, act => act.MapFrom(src => Convert.ToInt32(src.stock_quantity ?? 0)))
                .ForMember(dest => dest.ImageURL,  act => act.MapFrom(src => Truncate(src.image.src,   300)));   //---> should we make the rather an Item Image?
        }
    }
}
