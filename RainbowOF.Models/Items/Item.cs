using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RainbowOF.Models.Items
{
    public class Item
    {
        public int ItemID { get; set; }
        [Required(ErrorMessage = "Item or Product name is required")]
        [StringLength(100)]
        [DisplayName("Item name")]
        public string ItemName { get; set; }
        [StringLength(50)]
        public string SKU { get; set; }
        [DisplayName("Enabled?")]
        public bool? IsEnabled { get; set; }
        [StringLength(255)]
        public string ItemDetail { get; set; }
        public int? ItemCategoryID { get; set; }
        [DefaultValue(0)]
        public int? ParentItemID { get; set; }
        public int? ReplacementItemID { get; set; }
        [StringLength(10, ErrorMessage = "Abbreviated name")]
        public string ItemAbbreviatedName { get; set; }
        //public int? MerchantID { get; set; }
        public short SortOrder { get; set; }
//        [DisplayName("Qty/unit")]
//        [Column(TypeName = "decimal(12,4)")] 
//        public decimal QtyPerUnits { get; set; }
//        public int? ItemUnitID { get; set; }
//        [DisplayName("Cost Price X VAT")]
//        [Column(TypeName = "decimal(18,4)")]
//        public decimal CostPriceEXVAT { get; set; }
//        [DisplayName("Cost Price inc VAT")]
//        [Column(TypeName = "decimal(18,4)")]
//        public decimal CostPriceIncVAT { get; set; }
//        [DisplayName("Base Price X VAT")]
//        [Column(TypeName = "decimal(18,4)")]
//        public decimal BasePriceEXVAT { get; set; }
//        [Column(TypeName = "decimal(18,4)")]
//        [DisplayName("Base Price inc VAT")]
//        public decimal BasePriceIncVAT { get; set; }
//        public int? VATTaxTypeID { get; set; }
////        [Timestamp]
//        public DateTime CreatedAt { get; set; }
        [Timestamp] 
        public byte[] RowVersion { get; set; }

        public virtual ItemCategory ItemCategory { get; set; }
        [ForeignKey("ParentItemID")]
        public virtual Item ParentItem { get; set; }
        [ForeignKey("ReplacementItemID")]
        public virtual Item ReplacementItem { get; set; }
        //public Merchant Merchant { get; set; }
        //public ItemUnit ItemUnit { get; set; }

        //public VATTaxType VATTaxType { get; set; }
        //[ForeignKey("ItemID")]
        //public IEnumerable<ItemPrice> ItemPrices { get; set; }
    }

}
