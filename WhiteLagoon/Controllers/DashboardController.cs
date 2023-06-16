using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhiteLagoon_DataAccess.Repository.IRepository;
using WhiteLagoon_Models.ViewModels;
using WhiteLagoon_Utility;

namespace WhiteLagoon.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class DashboardController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public DashboardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetTotalBookingsChartData()
        {
            DashboardRadialBarChartVM dashboardRadialBarChartVM = await _unitOfWork.AdminDashboard.GetBookingsChartDataAsync();

            // Retrieve your data and format it as needed
            var data = new
            {
                series = dashboardRadialBarChartVM.Series, //new int[] { 30 },
                totalCount = dashboardRadialBarChartVM.TotalCount,
                increaseDecreaseRatio = dashboardRadialBarChartVM.IncreaseDecreaseRatio,
                isRatioIncrease = dashboardRadialBarChartVM.IsRatioIncrease
            };

            // Manually serialize the data to JSON
            return Json(data);
        }

        public async Task<IActionResult> GetTotalRevenueChartData()
        {
            DashboardRadialBarChartVM dashboardRadialBarChartVM = await _unitOfWork.AdminDashboard.GetRevenueChartDataAsync();

            // Retrieve your data and format it as needed
            var data = new
            {
                series = dashboardRadialBarChartVM.Series, //new int[] { 30 },
                totalCount = dashboardRadialBarChartVM.TotalCount,
                increaseDecreaseRatio = dashboardRadialBarChartVM.IncreaseDecreaseRatio,
                isRatioIncrease = dashboardRadialBarChartVM.IsRatioIncrease
            };

            // Manually serialize the data to JSON
            return Json(data);
        }

        public async Task<IActionResult> GetRegisteredUserChartData()
        {
            DashboardRadialBarChartVM dashboardRadialBarChartVM = await _unitOfWork.AdminDashboard.GetRegisteredUserChartDataAsync();

            // Retrieve your data and format it as needed
            var data = new
            {
                series = dashboardRadialBarChartVM.Series, //new int[] { 30 },
                totalCount = dashboardRadialBarChartVM.TotalCount,
                increaseDecreaseRatio = dashboardRadialBarChartVM.IncreaseDecreaseRatio,
                isRatioIncrease = dashboardRadialBarChartVM.IsRatioIncrease
            };

            // Manually serialize the data to JSON
            return Json(data);
        }

        public async Task<IActionResult> GetCustomerBookingsChartData()
        {
            DashboardPieChartVM dashboardPieChartVM = await _unitOfWork.AdminDashboard.GetBookingChartDataAsync();

            // Retrieve your data and format it as needed
            var data = new
            {
                series = dashboardPieChartVM.Series,
                labels = dashboardPieChartVM.Labels
            };

            // Manually serialize the data to JSON
            return Json(data);
        }

        public async Task<IActionResult> GetMemberAndBookingChartData()
        {
            DashboardLineChartVM dashboardLineChartVM = await _unitOfWork.AdminDashboard.GetMemberAndBookingChartDataAsync();

            // Retrieve your data and format it as needed
            var data = new
            {
                series = dashboardLineChartVM.ChartData,
                categories = dashboardLineChartVM.Categories
            };

            // Manually serialize the data to JSON
            return Json(data);
        }
        
    }
}
