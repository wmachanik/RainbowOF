using Microsoft.EntityFrameworkCore;
using RainbowOF.Data.SQL;
using RainbowOF.Models.Logs;
using RainbowOF.Tools;
using RainbowOF.Repositories.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Logs
{
    public class WooSyncLogRepository : Repository<WooSyncLog>, IWooSyncLogRepository
    {

        //private ApplicationDbContext appContext = null;
        //private ILoggerManager appLoggerManager { get; set; }
        //private IUnitOfWork appUnitOfWork { get; set; }
        public WooSyncLogRepository(ApplicationDbContext sourceContext, ILoggerManager sourceLogger, IUnitOfWork sourceAppUnitOfWork) : base (sourceContext, sourceLogger, sourceAppUnitOfWork)
        {
            //appContext = sourceContext;
            //appLoggerManager = sourceLogger;
            //appUnitOfWork = sourceAppUnitOfWork;
        }
        public async Task<List<DateTime>> GetDistinctLogDatesAsync()
        {
            return await appContext.WooSyncLogs.Select(wsl => wsl.WooSyncDateTime).Distinct().OrderByDescending(dt=>dt).ToListAsync();
        }
    }
}
