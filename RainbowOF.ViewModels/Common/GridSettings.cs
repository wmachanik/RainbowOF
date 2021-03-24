using RainbowOF.Components.Modals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.ViewModels.Common
{
    public class GridSettings
    {
        public int PageSize  {get; set; }= 15;
        public string customFilterValue { get; set; } = string.Empty;
        public bool IsNarrow { get; set; } = true;
        public bool IsFilterable { get; set; } = false;
        public ConfirmModal AddToWooConfirmation { get; set; }
        public ConfirmModal DeleteWooItemConfirmation { get; set; }
        public ConfirmModalWithOption DeleteConfirmation { get; set; }
        public PopUpAndLogNotification PopUpRef { get; set; }

    }
}
