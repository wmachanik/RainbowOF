using Microsoft.AspNetCore.Components;
using RainbowOF.Models.Logs;
using RainbowOF.Repositories.Common;
using RainbowOF.Repositories.Logs;
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
        public int PageSize = 20;
        public string customFilterValue;
        public List<DateTime> DatesInLog;
        public int IndexSelectedDateTimeInLog;

        // variables / Models
        public WooSyncLog WooSyncLogRow;
        public List<WooSyncLog> WooSyncLogRows;

        [Inject]
        IAppUnitOfWork _AppUnitOfWork { get; set; }


        protected override async Task OnInitializedAsync()
        {
            await LoadWooSyncLog();
        }

        private async Task<List<DateTime>> GetAllUniqueLogDates()
        {
            IWooSyncLogRepository _WooSyncLogRepository = _AppUnitOfWork.wooSyncLogRepository();

            return await _WooSyncLogRepository.GetDistinctLogDates();
        }

        private async Task<List<WooSyncLog>> GetSynLogRowsByDate(DateTime selectedDateTimeInLog)
        {
            IWooSyncLogRepository _WooSyncLogRepository = _AppUnitOfWork.wooSyncLogRepository();

            return (List<WooSyncLog>)await _WooSyncLogRepository.GetByAsync(wsl => wsl.WooSyncDateTime == selectedDateTimeInLog);
        }


        private async Task LoadWooSyncLog()
        {
            StateHasChanged();
            ////////
            /// What we want to do here get the dates in the log that are distinct, then display in a list and select the latest one
            /// Then we want to return only those records that are from that date
            /// We need to reselect each time the ddl is changed
            ///
            DatesInLog = await GetAllUniqueLogDates();   /// gets unique dates and sorts decsending
            if (DatesInLog != null)
            {
                IndexSelectedDateTimeInLog = 0;
                WooSyncLogRows = await GetSynLogRowsByDate(DatesInLog[0]);
            }

            // need to rather add paging this thing is gonna get arge
            //await Task.Run(() => WooSyncLogRows = _WooSyncLog.GetAll().OrderByDescending(wslr => wslr.WooSyncDateTime).ToList());
            ////// may need to add error checking into repo
            //DatesInLog = WooSyncLogRows.Select(wsl => wsl.WooSyncDateTime).Distinct().ToList();

            StateHasChanged();
        }

        bool OnCustomFilter(WooSyncLog model)
        {
            if (string.IsNullOrEmpty(customFilterValue))
                return true;

            return
                model.Parameters?.Contains(customFilterValue, StringComparison.OrdinalIgnoreCase) == true
                || model.Section.ToString().Contains(customFilterValue, StringComparison.OrdinalIgnoreCase) == true
                || model.Notes?.Contains(customFilterValue, StringComparison.OrdinalIgnoreCase) == true
                || model.Result.ToString().Contains(customFilterValue, StringComparison.OrdinalIgnoreCase) == true;
            // || model.WooSyncDateTime?.Contains(customFilterValue, StringComparison.OrdinalIgnoreCase) == true;
        }

        public async Task OnLogDateChanged(ChangeEventArgs e)
        {
            string _SelectedIndexStr = (string)e.Value;
            int _SelectedIndex = int.Parse(_SelectedIndexStr);   // positiion in the list
            IndexSelectedDateTimeInLog = _SelectedIndex; //  DatesInLog.FindIndex(0, dt => dt.Date == DatesInLog[_SelectedDate]);
            WooSyncLogRows = await GetSynLogRowsByDate(DatesInLog[_SelectedIndex]);
            StateHasChanged();
        }
    }
}
