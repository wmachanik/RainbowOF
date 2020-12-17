using Microsoft.AspNetCore.Components;
using RainbowOF.Models.Logs;
using RanbowOF.Repositories.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RainbowOF.Web.FrontEnd.Pages.Integration
{
    public partial class ViewWooSyncLog : ComponentBase
    {
        // Interface Stuff
        public bool IsSortable = true;
        public bool IsFilterable = true;
        public bool DoShowPager = true;
        public string customFilterValue;

        // variables / Models
        public WooSyncLog WooSyncLogRow;
        public List<WooSyncLog> WooSyncLogRows;

        [Inject]
        IAppUnitOfWork _AppUnitOfWork { get; set; }


        protected override async Task OnInitializedAsync()
        {
           await LoadWooSyncLog();
        }

        private async Task LoadWooSyncLog()
        {
            IAppRepository<WooSyncLog> _WooSyncLog = _AppUnitOfWork.Repository<WooSyncLog>();
            StateHasChanged();
            // need to rather add paging this thing is gonna get arge
            await Task.Run(() => WooSyncLogRows = _WooSyncLog.GetAll().ToList());
            StateHasChanged();
        }

        bool OnCustomFilter(WooSyncLog model)
        {
            if (string.IsNullOrEmpty(customFilterValue))
                return true;

            return
                model.Parameters?.Contains(customFilterValue, StringComparison.OrdinalIgnoreCase) == true
                || model.Notes?.Contains(customFilterValue, StringComparison.OrdinalIgnoreCase) == true;
                // || model.WooSyncDateTime?.Contains(customFilterValue, StringComparison.OrdinalIgnoreCase) == true;
        }


    }
}
