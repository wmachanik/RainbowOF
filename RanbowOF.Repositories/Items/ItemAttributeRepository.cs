using Microsoft.EntityFrameworkCore;
using RainbowOF.Data.SQL;
using RainbowOF.Models.Items;
using RainbowOF.Repositories.Common;
using RainbowOF.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Items
{
    internal class ItemAttributeRepository : Repository<ItemAttribute>, IItemAttributeRepository
    {
        #region Initialisation
        public ItemAttributeRepository(ApplicationDbContext dbContext, ILoggerManager logger, IUnitOfWork appUnitOfWork) : base(dbContext, logger, appUnitOfWork)
        {
            if (logger.IsDebugEnabled()) logger.LogDebug("ItemAttributeRepository initialised...");
        }
        #endregion
    }
}
