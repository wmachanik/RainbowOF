using RainbowOF.FrontEnd.Models;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.System
{
    public interface ISysPrefsRepository
    {
        SysPrefsModel GetSysPrefs();
        bool UpdateSysPreferences(SysPrefsModel updateSysPrefsModel);
        Task<bool> UpdateSysPreferencesAsync(SysPrefsModel updateSysPrefsModel);

    }
}
