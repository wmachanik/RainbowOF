using Microsoft.EntityFrameworkCore;
using RainbowOF.Data.SQL;
using RainbowOF.FrontEnd.Models;
using RainbowOF.Models.System;
using RainbowOF.Repositories.Common;
using RainbowOF.Tools;
using System.Linq;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.System
{
    public class SysPrefsRepository : ISysPrefsRepository
    {
        private IRepository<SysPrefs> currSysPrefsRepo { get; }
        private IRepository<WooSettings> currWooSettingsRepo { get; }

        private IDbContextFactory<ApplicationDbContext> appDbContext { get; }
        //private DbSet<SysPrefs> currSysPrefsTable { get; } = null;
        //private DbSet<WooSettings> currWooSettingsTable { get; } = null;
        private ILoggerManager appLoggerManager { get; }
        //private IUnitOfWork appUnitOfWork { get; set; }
        public SysPrefsRepository(IDbContextFactory<ApplicationDbContext> sourceDbContext, ILoggerManager sourceLogger, IUnitOfWork sourceAppUnitOfWork) : base() //(dbContext, logger, unitOfWork)
        {
            //appUnitOfWork = sourceAppUnitOfWork;
            //appDbContext = sourceAppUnitOfWork.AppDbContext;

            ////////-> need to rewrite code to not us a private, and do a using var when called.
            appLoggerManager = sourceLogger;
            //AppUnitOfWork = sourceAppUnitOfWork;
            currSysPrefsRepo = sourceAppUnitOfWork.Repository<SysPrefs>();
            currWooSettingsRepo = sourceAppUnitOfWork.Repository<WooSettings>();
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug("SysPrefgsRepository initialised.");
        }

        public SysPrefsModel GetSysPrefs()
        {
            using var context = appDbContext.CreateDbContext(); 
            DbSet<SysPrefs> currSysPrefsTable = context.Set<SysPrefs>();
            SysPrefs sysPrefs = currSysPrefsTable.FirstOrDefault();
            sysPrefs ??= new SysPrefs();   // if nothing send back a empty record
            DbSet<WooSettings> currWooSettingsTable = context.Set<WooSettings>();
            WooSettings wooSettings = currWooSettingsTable.FirstOrDefault();
            wooSettings ??= new WooSettings();   // if null set to an empty record
            return new SysPrefsModel
            {
                SysPrefs = sysPrefs,
                WooSettings = wooSettings
            };
        }

        public bool UpdateSysPreferences(SysPrefsModel updateSysPrefsModel)
        {
            bool updated; // = false;
            if (updateSysPrefsModel.SysPrefs.SysPrefsId > 0)
            {
                // it means that there was a record in the database.
                int recsUpdated = currSysPrefsRepo.Update(updateSysPrefsModel.SysPrefs);
                updated = (recsUpdated == 1);
            }
            else
            {
                // it means that there was a record in the database.
                int recIsUpdated = currSysPrefsRepo.Add(updateSysPrefsModel.SysPrefs);
                updated = recIsUpdated > 0;
            }
            // run this update regardless 
            if (updateSysPrefsModel.WooSettings.WooSettingsId > 0)
            {
                // it means that there was a record in the database.
                int recsUpdated = currWooSettingsRepo.Update(updateSysPrefsModel.WooSettings);
                updated = updated && (recsUpdated == 1);
            }
            else
            {
                // it means that there was a record in the database.
                var recIsUpdated = currWooSettingsRepo.Add(updateSysPrefsModel.WooSettings) > 0;
                updated = updated && recIsUpdated;
            }
            return updated;
        }
        public async Task<bool> UpdateSysPreferencesAsync(SysPrefsModel updateSysPrefsModel)
        {
            bool updated; // = false;
            int recsUpdated; // = 0;
            if (updateSysPrefsModel.SysPrefs.SysPrefsId > 0)
            {
                // it means that there was a record in the database.
                recsUpdated = await currSysPrefsRepo.UpdateAsync(updateSysPrefsModel.SysPrefs);
                updated = (recsUpdated > 0);
            }
            else
            {
                // it means that there was a record in the database.
                updated = (await currSysPrefsRepo.AddAsync(updateSysPrefsModel.SysPrefs)) != null;
            }
            // run this update regardless 
            if (updateSysPrefsModel.WooSettings.WooSettingsId > 0)
            {
                // it means that there was a record in the database.
                recsUpdated = await currWooSettingsRepo.UpdateAsync(updateSysPrefsModel.WooSettings);
                updated = updated && (recsUpdated > 0);
            }
            else
            {
                // it means that there was a record in the database.
                updated = (await currWooSettingsRepo.AddAsync(updateSysPrefsModel.WooSettings)) != null;
            }
            return updated;
        }
    }
}
