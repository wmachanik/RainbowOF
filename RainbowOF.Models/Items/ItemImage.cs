using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RainbowOF.Models.Items
{
    public class ItemImage
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ItemImageId { get; set; }
        [Required]
        public Guid ItemId { get; set; }
        [StringLength(100)]
        [DisplayName("Item Image Name")]
        public string Name { get; set; }
        [StringLength(100)]
        [DisplayName("Item Image Alternate Description used in HTML src tag")]
        public string Alt { get; set; }
        [StringLength(500)]
        [DisplayName("Item Image URL")]
        public string ImageURL { get; set; }
        public bool IsPrimary { get; set; }

    }
}
