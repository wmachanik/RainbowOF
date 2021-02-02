using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RainbowOF.Models.Lookups
{
    public class ItemCategoryLookup
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ItemCategoryLookupId { get; set; }
        [Required]
        [StringLength(255)]
        [DisplayName("Item Category Name")]
        public string CategoryName { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public string Notes { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey("ParentCategoryId")]
        public ItemCategoryLookup ParentCategory { get; set; }
    }
}
