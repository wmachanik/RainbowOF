using RainbowOF.Components.Modals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.ViewModels.Common
{
    public class WooLinkedGridSettings : GridSettings
    {
        //-> moved to GridSettings class
        //public int PageSize  {get; set; }= 15;
        //public int TotalItems { get; set; } = 0;
        //public int CurrentPage { get; set; } = 1;
        //public string CustomFilterValue { get; set; } = string.Empty;
        //public bool IsNarrow { get; set; } = true;
        //public bool IsFilterable { get; set; } = false;
        public bool WooIsActive { get; set; } = false;
        public ConfirmModal AddToWooConfirmation { get; set; }
        public ConfirmModal DeleteWooItemConfirmation { get; set; }
        public ConfirmModalWithOption DeleteConfirmationWithOption { get; set; }
        //public PopUpAndLogNotification PopUpRef { get; set; }

    }
}
