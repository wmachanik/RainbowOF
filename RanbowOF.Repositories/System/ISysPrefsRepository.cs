using RainbowOF.FrontEnd.Models;
using RainbowOF.Models.System;
using RanbowOF.Repositories.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RanbowOF.Repositories.System
{
    public interface ISysPrefsRepository
    {
        SysPrefsModel GetSysPrefs();
        bool UpdateSysPreferences(SysPrefsModel updateSysPrefsModel);
        Task <bool> UpdateSysPreferencesAsync(SysPrefsModel updateSysPrefsModel);

    }
}
