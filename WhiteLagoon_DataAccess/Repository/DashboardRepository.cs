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

        public async Task<DashboardRadialBarChartVM> GetRevenueChartDataAsync()
        {
            DashboardRadialBarChartVM dashboardRadialBarChartVM = new DashboardRadialBarChartVM();
            try
            {
                decimal totalCost = Convert.ToDecimal(await _db.BookingDetails.SumAsync(x => x.TotalCost));

                DateTime currentDate = DateTime.Now;
                DateTime previousMonthStartDate = new DateTime(currentDate.Year, currentDate.Month - 1, 1);
                DateTime currentMonthStartDate = new DateTime(currentDate.Year, currentDate.Month, 1);

                var sumByCurrentMonth = _db.BookingDetails.Where((r => r.BookingDate >= currentMonthStartDate && r.BookingDate < currentDate)).Sum(x => x.TotalCost);
                var sumByPreviousMonth = _db.BookingDetails.Where(r => r.BookingDate >= previousMonthStartDate && r.BookingDate < currentMonthStartDate).Sum(x => x.TotalCost);

                decimal increaseDecreaseRatio = 100;
                bool isIncrease = true;
                // Considering any non-zero count in current month as 100% increase.

                if (sumByPreviousMonth != 0)
                {
                    increaseDecreaseRatio = Convert.ToDecimal(Math.Round(((double)sumByCurrentMonth - sumByPreviousMonth) / sumByPreviousMonth * 100, 2));
                    isIncrease = sumByCurrentMonth > sumByPreviousMonth;
                }

                dashboardRadialBarChartVM.TotalCount = totalCost;
                dashboardRadialBarChartVM.IncreaseDecreaseRatio = increaseDecreaseRatio;
                dashboardRadialBarChartVM.IsRatioIncrease = isIncrease;
                dashboardRadialBarChartVM.Series = new decimal[] { increaseDecreaseRatio };
            }
            catch (Exception ex)
            {
                throw;
            }
            return dashboardRadialBarChartVM;
        }

        public async Task<DashboardRadialBarChartVM> GetRegisteredUserChartDataAsync()
        {
            DashboardRadialBarChartVM dashboardRadialBarChartVM = new DashboardRadialBarChartVM();
            try
            {
                int totalCount = await _db.Users.CountAsync();

                DateTime currentDate = DateTime.Now;
                DateTime previousMonthStartDate = new DateTime(currentDate.Year, currentDate.Month - 1, 1);
                DateTime currentMonthStartDate = new DateTime(currentDate.Year, currentDate.Month, 1);

                var countByCurrentMonth = _db.Users.Count(r => r.CreatedAt >= currentMonthStartDate && r.CreatedAt < currentDate);
                var countByPreviousMonth = _db.Users.Count(r => r.CreatedAt >= previousMonthStartDate && r.CreatedAt < currentMonthStartDate);

                decimal increaseDecreaseRatio = 100;
                bool isIncrease = true;
                // Considering any non-zero count in current month as 100% increase.

                if (countByPreviousMonth != 0)
                {
                    increaseDecreaseRatio = Math.Round(((decimal)countByCurrentMonth - countByPreviousMonth) / countByPreviousMonth * 100, 2);
                    isIncrease = countByCurrentMonth > countByPreviousMonth;
                }

                dashboardRadialBarChartVM.TotalCount = totalCount;
                dashboardRadialBarChartVM.IncreaseDecreaseRatio = increaseDecreaseRatio;
                dashboardRadialBarChartVM.IsRatioIncrease = isIncrease;
                dashboardRadialBarChartVM.Series = new decimal[] { increaseDecreaseRatio };

            }
            catch (Exception ex)
            {
                throw;
            }
            return dashboardRadialBarChartVM;
        }
    }
}
