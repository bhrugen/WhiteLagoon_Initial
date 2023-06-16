using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon_Models;
using WhiteLagoon_Models.ViewModels;

namespace WhiteLagoon_DataAccess.Repository.IRepository
{
    public interface IDashboardRepository
    {
        Task<DashboardRadialBarChartVM> GetBookingsChartDataAsync();
        Task<DashboardRadialBarChartVM> GetRevenueChartDataAsync();
        Task<DashboardRadialBarChartVM> GetRegisteredUserChartDataAsync();
        Task<DashboardPieChartVM> GetBookingChartDataAsync();
        Task<DashboardLineChartVM> GetMemberAndBookingChartDataAsync();
    }
}
