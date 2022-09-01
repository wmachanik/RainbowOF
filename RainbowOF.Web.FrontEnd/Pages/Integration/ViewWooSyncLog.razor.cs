using Microsoft.AspNetCore.Components;
using RainbowOF.Models.Logs;
using RainbowOF.Repositories.Common;
using System;
using System.Collections.Generic;
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
        public IUnitOfWork AppUnitOfWork { get; set; }
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await LoadWooSyncLogAsync();
        }

        private async Task<List<DateTime>> GetAllUniqueLogDatesAsync()
        {
            return await AppUnitOfWork.WooSyncLogRepository.GetDistinctLogDatesAsync();
        }

        private async Task<List<WooSyncLog>> GetSyncLogRowsByDateAsync(DateTime selectedDateTimeInLog)
        {
            return (List<WooSyncLog>)await AppUnitOfWork.WooSyncLogRepository.GetByAsync(wsl => wsl.WooSyncDateTime == selectedDateTimeInLog);
        }


        private async Task LoadWooSyncLogAsync()
        {
            StateHasChanged();
            ////////
            /// What we want to do here get the dates in the log that are distinct, then display in a list and select the latest one
            /// Then we want to return only those records that are from that date
            /// We need to reselect each time the DDL is changed
            ///
            DatesInLog = await GetAllUniqueLogDatesAsync();   /// gets unique dates and sorts descending
            if (DatesInLog != null)
            {
                IndexSelectedDateTimeInLog = 0;
                WooSyncLogRows = await GetSyncLogRowsByDateAsync(DatesInLog[0]);
            }

            // need to rather add paging this thing is gonna get large
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

        public async Task OnLogDateChangedAsync(ChangeEventArgs e)
        {
            string _SelectedIndexStr = (string)e.Value;
            int _SelectedIndex = int.Parse(_SelectedIndexStr);   // position in the list
            IndexSelectedDateTimeInLog = _SelectedIndex; //  DatesInLog.FindIndex(0, dt => dt.Date == DatesInLog[_SelectedDate]);
            WooSyncLogRows = await GetSyncLogRowsByDateAsync(DatesInLog[_SelectedIndex]);
            StateHasChanged();
        }
    }
}
