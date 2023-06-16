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
using NuGet.Versioning;

namespace WhiteLagoon.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            HomeVM homeVM = new HomeVM()
            {
                VillaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity").ToList(),
                Nights = 1
            };
            return View(homeVM);
        }

        [HttpPost]
        public IActionResult GetVillasByDate(int nights, DateOnly checkInDate)
        {
            var villaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity").ToList();

            var villaNumbersList = _unitOfWork.VillaNumber.GetAll().ToList();
             var bookedVillas = _unitOfWork.Booking.GetAll(u=>u.Status==SD.StatusApproved || 
             u.Status==SD.StatusCheckedIn).ToList();

            foreach (var villa in villaList)
            {
                var roomsInVilla = villaNumbersList.Where(m => m.VillaId == villa.Id);
                //for each villa
                //for each villa number
                List<int> bookingInDate = new List<int>();

                for (int i = 0; i < nights; i++)
                {
                    //we need to ignore checkout date because at the time room will be available
                    var villasBooked = bookedVillas.Where(m => m.CheckInDate <= checkInDate.AddDays(i) && m.CheckOutDate > checkInDate.AddDays(i) &&
                                          m.VillaId == villa.Id).ToList();

                    foreach(var booking in villasBooked)
                    {
                        if (!bookingInDate.Contains(booking.Id))
                        {
                            //we will add booking Id that needs a room in this date range
                            bookingInDate.Add(booking.Id);
                        }
                    }
                   

                    var totalAvailableRooms = roomsInVilla.Count() - bookingInDate.Count();
                    if (totalAvailableRooms == 0 )
                    {
                        villa.IsAvailable = false;
                    }
                    //  CheckInDate	CheckOutDate -- will not work for checkin date of 6/29 and 2 nights
                    //                      2023 - 06 - 29  2023 - 07 - 02
                    //                      2023 - 06 - 30  2023 - 07 - 04
                    //                      2023 - 06 - 29  2023 - 06 - 30
                }
             
            }

            HomeVM homeVM = new()
            {
                CheckInDate = checkInDate,
                VillaList = villaList,
                Nights = nights
            };
            return PartialView("_VillaList", homeVM);
        }

        public IActionResult Details(int villaId)
        {
            DetailsVM detailsVM = new()
            {
                Villa = _unitOfWork.Villa.Get(u => u.Id == villaId),
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