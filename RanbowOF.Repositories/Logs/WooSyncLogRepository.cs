using Microsoft.EntityFrameworkCore;
using RainbowOF.Data.SQL;
using RainbowOF.Models.Logs;
using RainbowOF.Repositories.Common;
using RainbowOF.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Logs
{
    public class WooSyncLogRepository : Repository<WooSyncLog>, IWooSyncLogRepository
    {

        private IDbContextFactory<ApplicationDbContext> appDbContext = null;
        private ILoggerManager appLoggerManager { get; set; }
        private IUnitOfWork appUnitOfWork { get; set; }
        public WooSyncLogRepository(IDbContextFactory<ApplicationDbContext> sourceContext, ILoggerManager sourceLogger, IUnitOfWork sourceAppUnitOfWork) : base(sourceContext, sourceLogger, sourceAppUnitOfWork)
        {
            appDbContext = sourceContext;
            appLoggerManager = sourceLogger;
            appUnitOfWork = sourceAppUnitOfWork;
        }
        public async Task<List<DateTime>> GetDistinctLogDatesAsync()
        {
            string statusString = "Getting distinct log dates async ";
            if (!appUnitOfWork.WooSyncLogRepository.CanDoDbAsyncCall(statusString))
                return null;
            try
            {
                using var context = await appDbContext.CreateDbContextAsync();
                var _table = context.Set<WooSyncLog>();
                return await _table.Select(wsl => wsl.WooSyncDateTime).Distinct().OrderByDescending(dt => dt).ToListAsync();
            }
            catch (Exception ex)
            {
                appLoggerManager.LogError($"Error getting distinct log dates async {ex.Message} - Inner Exception: {ex.InnerException}");
                return null;
            }
            finally
            {
                appUnitOfWork.WooSyncLogRepository.DbCallDone(statusString);
            }
        }
    }
}
