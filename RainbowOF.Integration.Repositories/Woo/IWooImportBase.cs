using RainbowOF.FrontEnd.Models.Classes;
using RainbowOF.Models.System;
using RainbowOF.Repositories.Common;
using RainbowOF.Tools;
using System;
using System.Threading.Tasks;

namespace RainbowOF.Integration.Repositories.Woo
{
    public interface IWooImportBase<TWooEntity> where TWooEntity : class
    {
        IUnitOfWork AppUnitOfWork { get; set; }
        ILoggerManager AppLoggerManager { get; set; }
        WooSettings AppWooSettings { get; set; }
        ImportCounters CurrImportCounters { get; set; }
        Task<Guid> GetWooMappedEntityIdByIdAsync(uint sourceWooEntityId);
        //---> no sure if this should exists - as it interfaces with the UI
        ////Task<int> ImportWooEntityData(List<TWooEntity> sourceWooEntities);

    }
}
