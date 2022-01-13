using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.Models.Items
{
    /// <summary>
    /// As per the Rainbow Project Doc:
    /// The Item Variant Table is used for an item that has variants. Each Variant links to the Item Parent via ItemId
    /// 
    /// Field                   Type            Comments
    /// ItemVariantId           GUID            Unique to IT not imported
    /// ItemVariantName         String(100)     Links to WooProductVariants.name
    /// SKU                     String(50)     Links to WooProductVariant.sku
    /// IsEnabled               Bool            Links to WooProductVariant.stock_status = instock
    /// ItemId                  Guid            Points to the Id of the Item that is the Parent
    /// ItemVariantAbbreviation string(10)      Abbreviated name – not imported
    /// BasePrice               decimal (18,4)	Linked to regular price
    /// SortOrder               int             linked to WooProduct Variants.menu_order
    /// ManageStock             Bool            Do we(or Woo) manage stock
    /// QtyInStock              Int             Amount in stock, if we manage stock(order must track if order came from woo or manual, if order from woo then we need not send back to woo, else we do
    /// Image                   String(255)     URL to image
    /// Notes                   String          Is a string for extra Item Variants
    /// </summary>

    public class ItemVariant
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ItemVariantId { get; set; }
        [Required]   // cannot have a variant without a Parent Item this points to the parent item
        public Guid ItemId { get; set; }
        public Guid AssocatedAttributeLookupId { get; set; }
        public Guid AssociatedAttributeVarietyLookupId { get; set; }
        [Required(ErrorMessage = "Item or Product variant is required")]
        [StringLength(100)]
        [DisplayName("Item variant")]
        public String ItemVariantName { get; set; }
        [StringLength(50)]
        public String SKU { get; set; }
        [DefaultValue(true)]
        [DisplayName("Is Enabled?")]
        public bool IsEnabled { get; set; }
        [StringLength(10, ErrorMessage = "Abbreviated name")]
        public string ItemVariantAbbreviation { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal BasePrice { get; set; }
        public int SortOrder { get; set; }
        public bool ManageStock { get; set; }
        public int QtyInStock { get; set; }
        [StringLength(300)]
        public string ImageURL { get; set; }
        public string Notes { get; set; }
        // not working looks like the child cannot refer to the parent.
        //[ForeignKey("ItemId")]   //-> cannot do this here as crashes done in the model config -> ItemVariantModelConfig
        ///public Item Item { get; set; }
    }
}
