using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WhiteLagoon_DataAccess.Repository.IRepository;
using WhiteLagoon_Models;

namespace WhiteLagoon.Controllers
{
    public class BookingController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public BookingController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [Authorize]
        public IActionResult FinalizeBooking(int villaId, DateOnly checkInDate, int nights)
        {
            BookingDetail booking = new()
            {
                Villa=_unitOfWork.Villa.Get(u=>u.Id==villaId, includeProperties: "VillaAmenity"),
                CheckInDate=checkInDate,
                Nights=nights,
                CheckOutDate=checkInDate.AddDays(nights),
            };
            return View(booking);
        }
        [Authorize]
        [HttpPost]
        public IActionResult FinalizeBooking(BookingDetail bookingDetail)
        {
            bookingDetail.BookingDate = DateTime.Now;


            _unitOfWork.Booking.Add(bookingDetail);
            _unitOfWork.Save();

            return View();
        }
        [Authorize]
        public IActionResult BookingConfirmation(int bookingId)
        {
            
            return View(bookingId);
        }
    }
}
