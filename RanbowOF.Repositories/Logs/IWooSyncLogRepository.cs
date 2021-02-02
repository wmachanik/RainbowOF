using RainbowOF.Models.Logs;
using RainbowOF.Repositories.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Logs
{
    public interface IWooSyncLogRepository : IAppRepository<WooSyncLog>
    {
        Task<List<DateTime>> GetDistinctLogDates();
    }
}
