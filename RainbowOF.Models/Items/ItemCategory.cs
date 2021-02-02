using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RainbowOF.Models.Items
{
    public class ItemCategory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ItemCategoryId { get; set; }
        public Guid ItemId { get; set; }
        public Guid ItemCategoryLookupId { get; set; }
    }
}