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

        private ApplicationDbContext _context = null;
        private ILoggerManager _logger { get; set; }
        private IAppUnitOfWork _appUnitOfWork { get; set; }
        public WooSyncLogRepository(ApplicationDbContext dbContext, ILoggerManager logger, IAppUnitOfWork appUnitOfWork) : base (dbContext, logger, appUnitOfWork)
        {
            _context = dbContext;
            _logger = logger;
            _appUnitOfWork = appUnitOfWork;
        }
        public async Task<List<DateTime>> GetDistinctLogDates()
        {
            return await _context.WooSyncLogs.Select(wsl => wsl.WooSyncDateTime).Distinct().OrderByDescending(dt=>dt).ToListAsync();
        }
    }
}
