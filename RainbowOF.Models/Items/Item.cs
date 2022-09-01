using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RainbowOF.Models.Items
{
    public class Item
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ItemId { get; set; }
        [Required(ErrorMessage = "Item or Product name is required")]
        [StringLength(100)]
        [DisplayName("Item name")]
        public string ItemName { get; set; }
        [StringLength(50)]
        public string SKU { get; set; }
        [DefaultValue(true)]
        [DisplayName("Is Enabled?")]
        public bool IsEnabled { get; set; }
        [StringLength(500)]    /// need to increase to allow more from Woo
        public string ItemDetail { get; set; }
        public Guid? PrimaryItemCategoryLookupId { get; set; }
        //[DefaultValue(0)]
        //public Guid? ParentItemId { get; set; } --> removed added as variant
        public Guid? ReplacementItemId { get; set; }
        [StringLength(10, ErrorMessage = "Abbreviated name")]
        public string ItemAbbreviatedName { get; set; }
        public int SortOrder { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal BasePrice { get; set; }
        // stock stuff
        public bool ManageStock { get; set; }
        public int QtyInStock { get; set; }
        // control stuff
        public ItemTypes ItemType { get; set; }
        // MISC stuff
        public string Notes { get; set; }
        // management stuff
        //[Timestamp]
        //public byte[] RowVersion { get; set; }
        // Related tables
        [ForeignKey("ReplacementItemId")]
        public virtual Item ReplacementItem { get; set; }
        [ForeignKey("ItemId")]
        public virtual List<ItemCategory> ItemCategories { get; set; }
        [ForeignKey("ItemId")]
        public virtual List<ItemAttribute> ItemAttributes { get; set; }
        [ForeignKey("ItemId")]
        public virtual List<ItemImage> ItemImages { get; set; }
        [ForeignKey("ItemId")]
        public virtual List<ItemVariant> ItemVariants { get; set; }

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
        // old design
        //[ForeignKey("ItemId")]
        //public virtual List<ItemAttributeVariety> ItemAttributeVarieties { get; set; }

    }

}
