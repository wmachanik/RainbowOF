﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RainbowOF.Models.Items
{
    public class ItemCategory
    {
        public int ItemCategoryID { get; set; }
        [Required]
        [StringLength(255)]
        [DisplayName("Item Category")]
        public string ItemTypeName { get; set; }
        public string Notes { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }

    }
}
