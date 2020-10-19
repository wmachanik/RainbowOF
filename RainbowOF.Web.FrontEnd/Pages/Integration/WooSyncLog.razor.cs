using Microsoft.AspNetCore.Components;
using RanbowOF.Repositories.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RainbowOF.Web.FrontEnd.Pages.Integration
{
    public partial class WooSyncLog
    {
        List<WooSyncLog> WooSyncLogModel;

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
            await Task.Run(() => WooSyncLogModel = _WooSyncLog.GetAll().ToList());
            StateHasChanged();
        }


    }
}
