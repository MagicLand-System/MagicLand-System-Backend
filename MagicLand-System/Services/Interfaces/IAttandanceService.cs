﻿using MagicLand_System.PayLoad.Response.Schedules;

namespace MagicLand_System.Services.Interfaces
{
    public interface IAttandanceService
    {
        Task<List<StaffAttandaceResponse>> LoadAttandance(string classId, DateTime day);
    }
}
