using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RainbowOF.Models.Items
{
    public class ItemCategory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ItemCategoryId { get; set; }
        [Required]
        [StringLength(255)]
        [DisplayName("Item Category")]
        public string ItemCategoryName { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public string Notes { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey("ParentCategoryId")]
        public ItemCategory ParentCategory { get; set; }
    }
}
