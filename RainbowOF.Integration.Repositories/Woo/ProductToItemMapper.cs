using AutoMapper;
using Microsoft.AspNetCore.Components;
using RainbowOF.FrontEnd.Models.Classes;
using RainbowOF.Models.Items;
using RainbowOF.Tools;
using System;
using System.Collections.Generic;
using WooCommerceNET.WooCommerce.v3;

namespace RainbowOF.Integration.Repositories.Woo
{
    public class ProductToItemMapper
    {
        #region Local Variables
        private IMapper _Mapper { get; set; }
        #endregion
        public const string CONST_WooItemOutOfStock = "outofstock";
        #region Variables
        private StringTools _StringTools = new StringTools();

        public ProductToItemMapper(IMapper mapper)
        {
            _Mapper = mapper;
        }
        #endregion
        #region Methods
        /// <summary>
        /// Change string simple, grouped, external and variable. to the ItemType we use
        /// </summary>
        /// <param name="sourceWooProductType">the string to convert from</param>
        /// <returns>ItemType</returns>
        //public ItemTypes MapWooTypeToItemType(string sourceWooProductType)
        //{
        //    switch (sourceWooProductType)
        //    {
        //        case "grouped":
        //            return ItemTypes.Collection;
        //        case "external":
        //            return ItemTypes.URL;
        //        case "variable":
        //            return ItemTypes.Variable;
        //        case "virtual":
        //            return ItemTypes.VirtualItem;
        //        default:    // all others are simple
        //            return ItemTypes.Simple;
        //    }
        //}
        /// <summary>
        /// Take the product date and map it to the Item data.
        /// The copies the info across:
        /// •	Set ItemName, SKU, IsEnabled, ItemDetail, SourtOrder as per above mapping in table
        /// •	For ItemCategotyId – selected the first Category in the array find the CategoryId of it using the WooCategoryMapping and set.We assume the first is primary.Perhaps we should look for one those without parents?
        /// •	For ParentItemId Add to a List ItemParents the ParentId and the ItemId</summary>
        /// <param name="sourceWooProd"></param>
        /// <param name="currItem"></param>
        /// <param name="currWooProductsWithParents"></param>
        /// <returns></returns>
        public Item MapWooProductInfo(Product sourceWooProd, Item currItem) //, ref List<WooItemWithParent> currWooProductsWithParents)
        {
            if (currItem == null)
                currItem = new Item();
            _Mapper.Map(sourceWooProd, currItem);

            //currItem.ItemName = _StringTools.Truncate(sourceWooProd.name, 100);  // max length is 100
            //currItem.SKU = _StringTools.Truncate(sourceWooProd.sku, 50);  // max length is 50
            //currItem.IsEnabled = !sourceWooProd.stock_status.Equals(CONST_WooItemOutOfStock);  //--  true if in stock or on backorder;
            //currItem.ItemDetail = _StringTools.Truncate(_StringTools.StripHTML(sourceWooProd.short_description), 250);   // max length is 255
            //currItem.ItemAbbreviatedName = _StringTools.MakeAbbriviation(sourceWooProd.name);
            //currItem.SortOrder = Convert.ToInt32(sourceWooProd.menu_order ?? 0);  //  (currWooProd.menu_order == null) ? 50 : (int)currWooProd.menu_order;
            //currItem.BasePrice = Convert.ToDecimal(sourceWooProd.price == null ? 0.0 : sourceWooProd.price); // (currWooProd.price == null) ? 0 :(decimal)currWooProd.price;
            //currItem.ManageStock = Convert.ToBoolean(sourceWooProd.manage_stock ?? false);
            //currItem.QtyInStock = Convert.ToInt32(sourceWooProd.stock_quantity ?? 0);
            //currItem.ItemType = MapWooTypeToItemType((sourceWooProd._virtual ?? false) ? "virtual" : sourceWooProd.type);
            // copy the image data across
            if ((sourceWooProd.images != null) && (sourceWooProd.images.Count > 0))
            {
                if (currItem.ItemImages == null)
                    currItem.ItemImages = new();
                // only add if it does not exist
                bool isPrimaryImage = true;
                foreach (var srcImg in sourceWooProd.images)
                {
                    if (!currItem.ItemImages.Exists(tgtImg => tgtImg.ImageURL == srcImg.src))
                    {
                        // add this URL and associated item as it does not exist
                        currItem.ItemImages.Add(new ItemImage
                        {
                            IsPrimary = isPrimaryImage,
                            Name = srcImg.name,
                            Alt = srcImg.alt,
                            ImageURL = srcImg.src,
                            ItemId = currItem.ItemId,     ///?? is this correct?
                        }
                        );
                    }
                    isPrimaryImage = false;  // the first one is assumed to be the primary image
                }
            }
            //// This may not be needed, as we add child item either via the UI or by the import
            //if ((entityWithParents != null) && (sourceWooProd.parent_id != null) && (sourceWooProd.parent_id > 0))     ////// not sure this is ever executed and if it is not sure how the data is getting back, since it is all async
            //    entityWithParents.Add(new WooItemWithParent
            //    {
            //        ChildId = (uint)sourceWooProd.id,
            //        ParentId = (uint)sourceWooProd.parent_id
            //    });
            return currItem;
        }
    }
    #endregion
}
