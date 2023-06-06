using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WhiteLagoon_DataAccess.Repository.IRepository;
using WhiteLagoon_Models;
using WhiteLagoon_Models.ViewModels;

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
                VillaList = _unitOfWork.Villa.GetAll().ToList(),
                Nights=1
            };
            return View(homeVM);
        }

        [HttpPost]
        public IActionResult Index(HomeVM homeVM)
        {




            homeVM.VillaList = _unitOfWork.Villa.GetAll().ToList();
            foreach(var villa in homeVM.VillaList)
            {
                //based on date get availability
                if (villa.Id % 2 == 0)
                {
                    villa.IsAvailable = true;
                }

            }
            return View(homeVM);
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