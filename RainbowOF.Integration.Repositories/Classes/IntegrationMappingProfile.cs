using AutoMapper;
using RainbowOF.Models.Items;
using System;
using WooCommerceNET.WooCommerce.v3;

namespace RainbowOF.Integration.Repositories.Classes
{
    public class IntegrationMappingProfile : Profile
    {
        public const string CONST_WOOITEMINSTOCK = "instock";
        public const string CONST_WOOITEMOUTOFSTOCK = "outofstock";
        public static string Truncate(string sourceString, int targetLength)
            => (sourceString.Length > targetLength) ?
                sourceString[..targetLength] :
                sourceString;

        public static string MakeAbbriviation(string originalName)
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
        public static string StripHTML(string originalHTML)
            => System.Text.RegularExpressions.Regex.Replace(originalHTML, @"<(.|\n)*?>", string.Empty);
        /// <summary>
        /// Change string simple, grouped, external and variable. to the ItemType we use
        /// </summary>
        /// <param name="sourceWooProductType">the string to convert from</param>
        /// <returns>ItemType</returns>
        public static ItemTypes MapWooTypeToItemType(string sourceWooProductType)
        {
            return (sourceWooProductType) switch
            {
                "grouped" => ItemTypes.Collection,
                "external" => ItemTypes.URL,
                "variable" => ItemTypes.Variable,
                "virtual" => ItemTypes.VirtualItem,
                "simple" => ItemTypes.Simple,
                _ => ItemTypes.Other, // there are lots of others depending on the plugins
            };
        }
        public static string MapItemTypeToWooType(ItemTypes sourceItemType)
        {
            return (sourceItemType) switch
            {
                ItemTypes.Collection => "grouped",
                ItemTypes.URL => "external",
                ItemTypes.Variable => "variable",
                ItemTypes.Simple => "simple",
                _ => "other", // all others are simple
            };
        }
        static bool MapObjectToBool(object sourceObject)
            => (sourceObject.GetType() == typeof(bool)) && (bool)sourceObject;

        public IntegrationMappingProfile()
        {
            CreateMap<Product, Item>()
                .ForMember(dest => dest.ItemName, act => act.MapFrom(src => Truncate(src.name, 100)))
                .ForMember(dest => dest.SKU, act => act.MapFrom(src => Truncate(src.sku, 50)))
                .ForMember(dest => dest.IsEnabled, act => act.MapFrom(src => (src.stock_status != CONST_WOOITEMOUTOFSTOCK)))
                .ForMember(dest => dest.ItemDetail, act => act.MapFrom(src => Truncate(src.short_description, 500)))
                .ForMember(dest => dest.ItemAbbreviatedName, act => act.MapFrom(src => MakeAbbriviation(src.name)))
                .ForMember(dest => dest.SortOrder, act => act.MapFrom(src => Convert.ToInt32(src.menu_order ?? 0)))
                .ForMember(dest => dest.BasePrice, act => act.MapFrom(src => Convert.ToDecimal(src.price == null ? 0.0 : src.price)))
                .ForMember(dest => dest.ManageStock, act => act.MapFrom(src => src.manage_stock ?? false))
                .ForMember(dest => dest.QtyInStock, act => act.MapFrom(src => Convert.ToInt32(src.stock_quantity ?? 0)))
                .ForMember(dest => dest.ItemType, act => act.MapFrom(src => MapWooTypeToItemType((src._virtual ?? false) ? "virtual" : src.type)));

            CreateMap<Item, Product>()
                .ForMember(dest => dest.name, act => act.MapFrom(src => String.IsNullOrEmpty(src.ItemName) ? "N/A" : src.ItemName))
                .ForMember(dest => dest.sku, act => act.MapFrom(src => src.SKU))
                .ForMember(dest => dest.stock_status, act => act.MapFrom(src => src.IsEnabled ? CONST_WOOITEMINSTOCK : CONST_WOOITEMOUTOFSTOCK))
                .ForMember(dest => dest.short_description, act => act.MapFrom(src => src.ItemDetail))
                .ForMember(dest => dest.menu_order, act => act.MapFrom(src => src.SortOrder))
                .ForMember(dest => dest.price, act => act.MapFrom(src => src.BasePrice))
                .ForMember(dest => dest.manage_stock, act => act.MapFrom(src => src.ManageStock))
                .ForMember(dest => dest.stock_quantity, act => act.MapFrom(src => src.QtyInStock))
                .ForMember(dest => dest.type, act => act.MapFrom(src => MapItemTypeToWooType(src.ItemType)));
            //mapping of the lists of categories and product category
            // cannot do this since we need to lookup the woo item.

            CreateMap<Variation, ItemVariant>()
                .ForMember(dest => dest.ItemVariantName, act =>
                                   act.MapFrom(src => String.IsNullOrEmpty(src.description) ? "N/A" : Truncate(StripHTML(src.description), 100)))
                .ForMember(dest => dest.SKU, act =>
                                   act.MapFrom(src => String.IsNullOrEmpty(src.sku) ? "NoSKU" : Truncate(src.sku, 50)))
                .ForMember(dest => dest.ItemVariantAbbreviation, act =>
                                   act.MapFrom(src => String.IsNullOrEmpty(src.sku) ? "ChangeMe" : MakeAbbriviation(src.sku)))
                .ForMember(dest => dest.IsEnabled, act => act.MapFrom(src => (src.stock_status != CONST_WOOITEMOUTOFSTOCK)))
                .ForMember(dest => dest.SortOrder, act => act.MapFrom(src => Convert.ToInt32(src.menu_order)))
                .ForMember(dest => dest.BasePrice, act => act.MapFrom(src => Convert.ToDecimal(src.price == null ? 0.0 : src.price)))
                .ForMember(dest => dest.ManageStock, act => act.MapFrom(src => (MapObjectToBool(src.manage_stock))))   //-> can be a string with the value "parent"
                .ForMember(dest => dest.QtyInStock, act => act.MapFrom(src => Convert.ToInt32(src.stock_quantity ?? 0)))
                .ForMember(dest => dest.ImageURL, act => act.MapFrom(src => Truncate(src.image.src, 300)));   //---> should we make the rather an Item Image?

            CreateMap<ItemVariant, Variation>()
                .ForMember(dest => dest.description, act => act.MapFrom(src => src.ItemVariantName))
                .ForMember(dest => dest.sku, act => act.MapFrom(src => src.SKU))
                .ForMember(dest => dest.stock_status, act => act.MapFrom(src => src.IsEnabled ? CONST_WOOITEMINSTOCK : CONST_WOOITEMOUTOFSTOCK))
                .ForMember(dest => dest.menu_order, act => act.MapFrom(src => src.SortOrder))
                .ForMember(dest => dest.price, act => act.MapFrom(src => src.BasePrice))
                .ForMember(dest => dest.manage_stock, act => act.MapFrom(src => src.ManageStock))
                .ForMember(dest => dest.stock_quantity, act => act.MapFrom(src => src.QtyInStock));

            //CreateMap<Order,RainbowOF.Order>
            //CreateMap<Customer, Contact>
        }
    }
}
