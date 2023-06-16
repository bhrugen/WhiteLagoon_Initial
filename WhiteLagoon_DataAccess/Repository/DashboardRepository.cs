using Microsoft.EntityFrameworkCore;
using System.Linq;
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
                int totalCount = await _db.BookingDetails.CountAsync();

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

        public async Task<DashboardPieChartVM> GetBookingChartDataAsync()
        {
            DashboardPieChartVM dashboardPieChartVM = new DashboardPieChartVM();
            try
            {
                var newCustomerBookings = _db.BookingDetails.AsEnumerable().GroupBy(b => b.UserId)
                    .Where(g => g.Count() == 1).Select(g => g.Key).Count();

                var returningCustomerBookings = _db.BookingDetails.AsEnumerable().GroupBy(b => b.UserId)
                    .Where(g => g.Count() > 1).Select(g => g.Key).Count();



                dashboardPieChartVM.Labels = new string[] { "New Customers", "Returning Customers" };
                dashboardPieChartVM.Series = new decimal[] { newCustomerBookings, returningCustomerBookings };

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw;
            }

            return dashboardPieChartVM;
        }

        public async Task<DashboardLineChartVM> GetMemberAndBookingChartDataAsync()
        {
            DashboardLineChartVM dashboardLineChartVM = new DashboardLineChartVM();
            try
            {
                DateTime currentDate = DateTime.Now;
                DateTime n_DaysAgo = currentDate.AddDays(-30);

                // Query for new bookings and new customers
                var bookingData = _db.BookingDetails
                    .Where(b => b.BookingDate.Date >= n_DaysAgo && b.BookingDate.Date <= currentDate)
                    .GroupBy(b => b.BookingDate.Date)
                    .Select(g => new
                    {
                        DateTime = g.Key,
                        NewBookingCount = g.Count()
                    })
                    .ToList();

                var customerData = _db.Users
                    .Where(u => u.CreatedAt.Date >= n_DaysAgo && u.CreatedAt.Date <= currentDate)
                    .GroupBy(u => u.CreatedAt.Date)
                    .Select(g => new
                    {
                        DateTime = g.Key,
                        NewCustomerCount = g.Count()
                    })
                    .ToList();

                // Perform a left outer join
                var leftJoin = bookingData.GroupJoin(customerData, booking => booking.DateTime, customer => customer.DateTime,
                    (booking, customers) => new
                    {
                        booking.DateTime,
                        booking.NewBookingCount,
                        Customers = customers.DefaultIfEmpty()
                    })
                    .SelectMany(x => x.Customers.DefaultIfEmpty(),
                        (booking, customer) => new
                        {
                            booking.DateTime,
                            booking.NewBookingCount,
                            NewCustomerCount = customer?.NewCustomerCount ?? 0
                        })
                    .ToList();

                // Perform a right outer join
                var rightJoin = customerData.GroupJoin(bookingData, customer => customer.DateTime, booking => booking.DateTime,
                    (customer, bookings) => new
                    {
                        customer.DateTime,
                        NewBookingCount = bookings.Select(b => b.NewBookingCount).SingleOrDefault(),
                        customer.NewCustomerCount
                    })
                    .Where(x => x.NewBookingCount == 0).ToList();

                // Combine the left and right joins
                var mergedData = leftJoin.Union(rightJoin).OrderBy(data => data.DateTime).ToList();

                // Separate the counts into individual lists
                var newBookingData = mergedData.Select(d => d.NewBookingCount).ToList();
                var newCustomerData = mergedData.Select(d => d.NewCustomerCount).ToList();
                var categories = mergedData.Select(d => d.DateTime.Date.ToString("MM/dd/yyyy")).ToList();


                List<ChartData> chartDataList = new List<ChartData>
                {
                    new ChartData { Name = "New Memebers", Data = newCustomerData.ToArray() },
                    new ChartData { Name = "New Bookings", Data = newBookingData.ToArray() }
                };

                dashboardLineChartVM.ChartData = chartDataList;
                dashboardLineChartVM.Categories = categories.ToArray();

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw;
            }

            return dashboardLineChartVM;
        }
    
    }
}
