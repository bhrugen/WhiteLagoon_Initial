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
        public IActionResult Index(int villaId)
        {
            BookingDetail booking = new();
            return View(booking);
        }
    }
}
