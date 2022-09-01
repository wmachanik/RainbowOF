using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [DefaultValue("true")]
        public bool UsedForPrediction { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public Guid? UoMBaseId { get; set; }
        public string Notes { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey("ParentCategoryId")]
        public ItemCategoryLookup ParentCategory { get; set; }

        [ForeignKey("UoMBaseId")]
        public ItemUoMLookup CategoryUoMBase { get; set; }

        // excluding this because cannot figure out how to get it to work should be LEFT JOIN [ItemCategoriesLookup] AS [i0] ON [i].[ItemCategoryLookupId] = [i0].[ParentCategoryId]
        //[ForeignKey("ItemCategoryLookupId")]
        //public List<ItemCategoryLookup> ChildItemCategories { get; set; }

        //-> if we wanted the data base to do the work[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        // -> if we implement this then the case of HasGeneratedSql is  CASE WHEN ParentCategoryId IS NULL THEN (CategoryName) ELSE (SELET FullCategoryName + '->' + CategoryName from ItemCategoriesLookup where ParentCategoryId=ItemCategoryLookupId) END AS fullName
        [NotMapped]    ///-> we want to do the work.
        public string FullCategoryName => (ParentCategory == null) ? CategoryName : ParentCategory.FullCategoryName + "—>" + CategoryName;
        [NotMapped]    ///-> we want to do the work.
        public string CategoryIndent => (ParentCategory == null) ? string.Empty : ParentCategory.CategoryIndent + " —";

    }
}
