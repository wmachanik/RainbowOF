using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RainbowOF.Models.Items
{
    public class Item
    {
        public int ItemId { get; set; }
        [Required(ErrorMessage = "Item or Product name is required")]
        [StringLength(100)]
        [DisplayName("Item name")]
        public string ItemName { get; set; }
        [StringLength(50)]
        public string SKU { get; set; }
        [DefaultValue(true)]
        [DisplayName("Enabled?")]
        public bool IsEnabled { get; set; }
        [StringLength(255)]
        public string ItemDetail { get; set; }
        public int? ItemCategoryId { get; set; }
        [DefaultValue(0)]
        public int? ParentItemId { get; set; }
        public int? ReplacementItemId { get; set; }
        [StringLength(10, ErrorMessage = "Abbreviated name")]
        public string ItemAbbreviatedName { get; set; }
        public int SortOrder { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }

        public virtual ItemCategory ItemCategory { get; set; }
        [ForeignKey("ParentItemId")]
        public virtual Item ParentItem { get; set; }
        [ForeignKey("ReplacementItemId")]
        public virtual Item ReplacementItem { get; set; }
        
        // may need these later
        //        public int? MerchantId { get; set; }
        //        [DisplayName("Qty/unit")]
        //        [Column(TypeName = "decimal(12,4)")] 
        //        public decimal QtyPerUnits { get; set; }
        //        public int? ItemUnitId { get; set; }
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
        //        public int? VATTaxTypeId { get; set; }
        ////        [Timestamp]
        //        public DateTime CreatedAt { get; set; }
        //public Merchant Merchant { get; set; }
        //public ItemUnit ItemUnit { get; set; }

        //public VATTaxType VATTaxType { get; set; }
        //[ForeignKey("ItemId")]
        //public IEnumerable<ItemPrice> ItemPrices { get; set; }
    }

}
