using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Diagnostics;
using System.Linq;
using WhiteLagoon_DataAccess.Repository;
using WhiteLagoon_DataAccess.Repository.IRepository;
using WhiteLagoon_Models;
using WhiteLagoon_Models.ViewModels;
using WhiteLagoon_Utility;
using System.Linq;

namespace WhiteLagoon.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly List<string> _bookedStatus = new List<string> { "Approved", "CheckedIn" };
        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            HomeVM homeVM = new HomeVM()
            {
                VillaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity").ToList(),
                Nights=1
            };
            return View(homeVM);
        }

        [HttpPost]
        public IActionResult GetVillasByDate(int nights, DateOnly checkInDate)
        {
            var villaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity").ToList();

            var VillaNumumbers = _unitOfWork.VillaNumber.GetAll().ToList();

            var bookedVillas = _unitOfWork.Booking.GetAll().Where(m => (m.CheckInDate <= checkInDate && m.CheckOutDate >= checkInDate) &&
                               (_bookedStatus.Any(i => i.ToString() == m.Status))).ToList();


            if (bookedVillas.Count()  > 0)
            {
                foreach (var villa in villaList)
                {
                    foreach (var item in bookedVillas)
                    {
                        var isToBeCheckout = bookedVillas.Where(m => villa.Id == item.VillaId && m.CheckOutDate == checkInDate).ToList();

                        var filteredVillas = bookedVillas.Where(m => m.VillaId == item.VillaId).ToList();

                        var totAvailVillas = (VillaNumumbers.Where(m => m.VillaId == item.VillaId).Count() - filteredVillas.Count()) + isToBeCheckout.Count();

                        if (totAvailVillas == 0 && villa.Id == item.VillaId)
                        {
                            villa.IsAvailable = false;
                        }
                    }
                }
            }

            HomeVM homeVM = new ()
            {
                CheckInDate = checkInDate,
                VillaList = villaList,
                Nights = nights
            };
            return PartialView("_VillaList", homeVM);
        }

        public IActionResult Details(int villaId)
        {
            DetailsVM detailsVM = new ()
            {
                Villa= _unitOfWork.Villa.Get(u=>u.Id==villaId),
                Nights = 1
            };
            return View(detailsVM);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}