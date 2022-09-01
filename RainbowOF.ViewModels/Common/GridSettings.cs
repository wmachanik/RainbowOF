using RainbowOF.Components.Modals;

namespace RainbowOF.ViewModels.Common
{
    public class GridSettings
    {
        public int PageSize { get; set; } = 15;
        public int TotalItems { get; set; } = 0;
        public int CurrentPage { get; set; } = 1;
        public string CustomFilterValue { get; set; } = string.Empty;
        public bool IsNarrow { get; set; } = true;
        public bool IsFilterable { get; set; } = false;
        public ConfirmModal DeleteConfirmation { get; set; }
        public PopUpAndLogNotification PopUpRef { get; set; }

    }
}
