using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon_DataAccess.Repository.IRepository;
using WhiteLagoon_Models.ViewModels;

namespace WhiteLagoon_DataAccess.Repository
{
    public class DashboardRepository : IDashboardRepository
    {
        private ApplicationDbContext _db;
        public DashboardRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<DashboardRadialBarChartVM> GetBookingsChartDataAsync()
        {
            DashboardRadialBarChartVM dashboardRadialBarChartVM = new DashboardRadialBarChartVM();
            try
            {
                int totalBooking = await _db.BookingDetails.CountAsync();

                DateTime currentDate = DateTime.Now;
                DateTime previousMonthStartDate = new DateTime(currentDate.Year, currentDate.Month - 1, 1);
                DateTime currentMonthStartDate = new DateTime(currentDate.Year, currentDate.Month, 1);

                var countByCurrentMonth = _db.BookingDetails.Count(r => r.BookingDate >= currentMonthStartDate && r.BookingDate < currentDate);
                var countByPreviousMonth = _db.BookingDetails.Count(r => r.BookingDate >= previousMonthStartDate && r.BookingDate < currentMonthStartDate);

                decimal increaseDecreaseRatio = 100;
                bool isIncrease = true;
                // Considering any non-zero count in current month as 100% increase.

                if (countByPreviousMonth != 0)
                {
                    increaseDecreaseRatio = Math.Round(((decimal)countByCurrentMonth - countByPreviousMonth) / countByPreviousMonth * 100, 2);
                    isIncrease = countByCurrentMonth > countByPreviousMonth;
                }

                dashboardRadialBarChartVM.TotalCount = totalBooking;
                dashboardRadialBarChartVM.IncreaseDecreaseRatio = increaseDecreaseRatio;
                dashboardRadialBarChartVM.IsRatioIncrease = isIncrease;
                dashboardRadialBarChartVM.Series = new decimal[] { increaseDecreaseRatio };
            }
            catch (Exception)
            {
                throw;
            }
            return dashboardRadialBarChartVM;
        }

        
    }
}
