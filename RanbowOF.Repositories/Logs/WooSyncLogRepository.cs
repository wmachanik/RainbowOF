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
    public class WooSyncLogRepository : AppRepository<WooSyncLog>, IWooSyncLogRepository
    {

        private ApplicationDbContext _Context = null;
        private ILoggerManager _Logger { get; set; }
        private IAppUnitOfWork _AppUnitOfWork { get; set; }
        public WooSyncLogRepository(ApplicationDbContext sourceContext, ILoggerManager sourceLogger, IAppUnitOfWork sourceAppUnitOfWork) : base (sourceContext, sourceLogger, sourceAppUnitOfWork)
        {
            _Context = sourceContext;
            _Logger = sourceLogger;
            _AppUnitOfWork = sourceAppUnitOfWork;
        }
        public async Task<List<DateTime>> GetDistinctLogDates()
        {
            return await _Context.WooSyncLogs.Select(wsl => wsl.WooSyncDateTime).Distinct().OrderByDescending(dt=>dt).ToListAsync();
        }
    }
}
