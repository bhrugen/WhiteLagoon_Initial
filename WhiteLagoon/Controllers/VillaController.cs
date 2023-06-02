using Microsoft.AspNetCore.Mvc;
using WhiteLagoon_DataAccess;
using WhiteLagoon_DataAccess.Repository.IRepository;
using WhiteLagoon_Models;

namespace WhiteLagoon.Controllers
{
    public class VillaController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public VillaController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Villa> villaList = _unitOfWork.Villa.GetAll().ToList();
            return View(villaList);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Villa obj)
        {
            if (obj.Name == obj.Description?.ToString())
            {
                ModelState.AddModelError("name","The DisplayOrder cannot exactly match the Name.");
                TempData["error"] = "Error encountered";
             }
            if (ModelState.IsValid)
            {
                _unitOfWork.Villa.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Villa Created Successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
            
        }

        public IActionResult Update(int villaId)
        {
            Villa? obj = _unitOfWork.Villa.Get(x => x.Id == villaId);
            if (obj == null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(obj);
        }
        [HttpPost]
        public IActionResult Update(Villa obj)
        {
            if (ModelState.IsValid && obj.Id>0)
            {
                _unitOfWork.Villa.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Villa Updated Successfully";
                return RedirectToAction("Index");
            }
            return View(obj);

        }

        public IActionResult Delete(int villaId)
        {
            Villa? obj = _unitOfWork.Villa.Get(x => x.Id == villaId);
            if (obj == null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(obj);
            
        }
        [HttpPost]
        public IActionResult Delete(Villa obj)
        {
                Villa? objFromDb = _unitOfWork.Villa.Get(x => x.Id == obj.Id);
                if (objFromDb != null)
                {
                _unitOfWork.Villa.Remove(objFromDb);
                _unitOfWork.Save();    
                TempData["success"] = "Villa Deleted Successfully";
                return RedirectToAction("Index");
                }
            return View(obj);

        }

    }
}
