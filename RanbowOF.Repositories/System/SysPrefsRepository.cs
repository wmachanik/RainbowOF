using Microsoft.EntityFrameworkCore;
using RainbowOF.Data.SQL;
using RainbowOF.FrontEnd.Models;
using RainbowOF.Models.System;
using RainbowOF.Tools;
using RainbowOF.Repositories.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.System
{
    public class SysPrefsRepository : ISysPrefsRepository
    {
        private IRepository<SysPrefs> _SysPrefsRepo;
        private IRepository<WooSettings> _WooSettingsRepo;

        private ApplicationDbContext appContext = null;
        private DbSet<SysPrefs> _SysPrefsTable = null;
        private DbSet<WooSettings> _WooSettingsTable = null;
        private ILoggerManager appLoggerManager { get; }
        private IUnitOfWork appUnitOfWork { get; set; }
        public SysPrefsRepository(ApplicationDbContext sourceDbContext, ILoggerManager sourceLogger, IUnitOfWork sourceAppUnitOfWork) :
            base() //(dbContext, logger, unitOfWork)
        {
            appContext = sourceDbContext;
            _SysPrefsTable = appContext.Set<SysPrefs>();
            _WooSettingsTable = appContext.Set<WooSettings>();
            appLoggerManager = sourceLogger;
            appUnitOfWork = sourceAppUnitOfWork;
            _SysPrefsRepo = sourceAppUnitOfWork.Repository<SysPrefs>();
            _WooSettingsRepo = sourceAppUnitOfWork.Repository<WooSettings>();
            if (appLoggerManager.IsDebugEnabled()) appLoggerManager.LogDebug("SysPrefgsRepository initialised.");
        }

        public SysPrefsModel GetSysPrefs()
        {
            SysPrefs sysPrefs = _SysPrefsTable.FirstOrDefault();
            if (sysPrefs == null)
                sysPrefs = new SysPrefs();   // if nothing send back a empty record

            WooSettings wooSettings = _WooSettingsTable.FirstOrDefault();
            if (wooSettings == null)
                wooSettings = new WooSettings();   // if nothing send back a empty record

            return new SysPrefsModel
            {
                SysPrefs = sysPrefs,
                WooSettings = wooSettings
            };
        }

        public bool UpdateSysPreferences(SysPrefsModel updateSysPrefsModel)
        {
            bool updated = false;
            if (updateSysPrefsModel.SysPrefs.SysPrefsId > 0)
            {
                // it means that there was a record in the database.
                var recsUpdated = _SysPrefsRepo.Update(updateSysPrefsModel.SysPrefs);
                updated = (recsUpdated == 1);
            }
            else
            {
                // it means that there was a record in the database.
                var recIsUpdated = _SysPrefsRepo.Add(updateSysPrefsModel.SysPrefs) > 0;
                updated = recIsUpdated;
            }
            // run this update regardless 
            if (updateSysPrefsModel.WooSettings.WooSettingsId > 0)
            {
                // it means that there was a record in the database.
                var recsUpdated = _WooSettingsRepo.Update(updateSysPrefsModel.WooSettings);
                updated = updated && (recsUpdated == 1);
            }
            else
            {
                // it means that there was a record in the database.
                var recIsUpdated = _WooSettingsRepo.Add(updateSysPrefsModel.WooSettings) > 0;
                updated = updated && recIsUpdated;
            }
            return updated;
        }
        public async Task<bool> UpdateSysPreferencesAsync(SysPrefsModel updateSysPrefsModel)
        {
            bool updated = false;
            int recsUpdated = 0;

            if (updateSysPrefsModel.SysPrefs.SysPrefsId > 0)
            {
                // it means that there was a record in the database.
                recsUpdated = await _SysPrefsRepo.UpdateAsync(updateSysPrefsModel.SysPrefs);
                updated = (recsUpdated > 0);
            }
            else
            {
                // it means that there was a record in the database.
                updated = (await _SysPrefsRepo.AddAsync(updateSysPrefsModel.SysPrefs)) != null;
            }
            // run this update regardless 
            if (updateSysPrefsModel.WooSettings.WooSettingsId > 0)
            {
                // it means that there was a record in the database.
                recsUpdated = await _WooSettingsRepo.UpdateAsync(updateSysPrefsModel.WooSettings);
                updated = updated && (recsUpdated > 0);
            }
            else
            {
                // it means that there was a record in the database.
                updated = (await _WooSettingsRepo.AddAsync(updateSysPrefsModel.WooSettings)) != null;
            }
            return updated;
        }
    }
}
