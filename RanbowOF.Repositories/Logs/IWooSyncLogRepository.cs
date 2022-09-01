﻿using RainbowOF.Models.Logs;
using RainbowOF.Repositories.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RainbowOF.Repositories.Logs
{
    public interface IWooSyncLogRepository : IRepository<WooSyncLog>
    {
        Task<List<DateTime>> GetDistinctLogDatesAsync();
    }
}
