using Microsoft.AspNetCore.Mvc;
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
    }
}
