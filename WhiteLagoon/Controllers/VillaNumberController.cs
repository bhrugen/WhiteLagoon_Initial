using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WhiteLagoon_DataAccess;
using WhiteLagoon_DataAccess.Repository.IRepository;
using WhiteLagoon_Models;
using WhiteLagoon_Models.ViewModels;

namespace WhiteLagoon.Controllers
{
    public class VillaNumberController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public VillaNumberController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index(int villaId)
        {
            List<VillaNumber> villaNumberList = _unitOfWork.VillaNumber.GetAll(includeProperties:"Villa").OrderBy(u=>u.Villa.Name).ToList();
            return View(villaNumberList);
        }
        public IActionResult Create()
        {
            VillaNumberVM villaNumberVM = new()
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                })
            };
            return View(villaNumberVM);
        }

        [HttpPost]
        public IActionResult Create(VillaNumberVM villaNumberVM)
        {
            bool isNumberUnique = _unitOfWork.VillaNumber.GetAll(u => u.Villa_Number == villaNumberVM.VillaNumber.Villa_Number).Count()==0;
            if (ModelState.IsValid && isNumberUnique)
            {
                _unitOfWork.VillaNumber.Add(villaNumberVM.VillaNumber);
                _unitOfWork.Save();
                TempData["success"] = "Villa Number Successfully";
                return RedirectToAction(nameof(Index));
            }
            if (!isNumberUnique)
            {
                TempData["error"] = "Villa number already exists. Please use a different villa number.";
            }
            return View(villaNumberVM);
        }

        public IActionResult Update(int villaId)
        {
            VillaNumberVM villaNumberVM = new()
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                VillaNumber = _unitOfWork.VillaNumber.Get(u => u.Villa_Number == villaId)
            };
            if (villaNumberVM.VillaNumber==null)
            {
                return RedirectToAction("error", "home");
            }
            return View(villaNumberVM);
        }

        [HttpPost]
        public IActionResult Update(VillaNumberVM villaNumberVM)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.VillaNumber.Update(villaNumberVM.VillaNumber);
                _unitOfWork.Save();
                TempData["success"] = "Villa Number Successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(villaNumberVM);
        }

        public IActionResult Delete(int villaId)
        {
            VillaNumberVM villaNumberVM = new()
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                VillaNumber = _unitOfWork.VillaNumber.Get(u => u.Villa_Number == villaId)
            };
            if (villaNumberVM.VillaNumber == null)
            {
                return RedirectToAction("error", "home");
            }
            return View(villaNumberVM);
        }

        [HttpPost]
        public IActionResult Delete(VillaNumberVM villaNumberVM)
        {
            if (ModelState.IsValid)
            {
                VillaNumber? objFromDb = _unitOfWork.VillaNumber.Get(x => x.Villa_Number == villaNumberVM.VillaNumber.Villa_Number);
                if (objFromDb != null)
                {
                    _unitOfWork.VillaNumber.Remove(objFromDb);
                    _unitOfWork.Save();
                    TempData["success"] = "Villa Number Deleted Successfully";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(villaNumberVM);
        }

    }
}
