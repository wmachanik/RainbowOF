using RainbowOF.FrontEnd.Models;
using RainbowOF.Models.System;
using RainbowOF.Repositories.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.System
{
    public interface ISysPrefsRepository
    {
        SysPrefsModel GetSysPrefs();
        bool UpdateSysPreferences(SysPrefsModel updateSysPrefsModel);
        Task <bool> UpdateSysPreferencesAsync(SysPrefsModel updateSysPrefsModel);

    }
}
