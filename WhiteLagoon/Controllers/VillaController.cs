using Microsoft.AspNetCore.Mvc;
using WhiteLagoon_DataAccess;
using WhiteLagoon_DataAccess.Repository.IRepository;
using WhiteLagoon_Models;

namespace WhiteLagoon.Controllers
{
    public class VillaController : Controller
    {
        private readonly IVillaRepository _villaRepository;
        public VillaController(IVillaRepository villaRepository)
        {
            _villaRepository = villaRepository;
        }
        public IActionResult Index()
        {
            List<Villa> villaList = _villaRepository.GetAll().ToList();
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
                _villaRepository.Add(obj);
                _villaRepository.Save();
                TempData["success"] = "Villa Created Successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
            
        }

        public IActionResult Update(int villaId)
        {
            Villa? obj = _villaRepository.Get(x => x.Id == villaId);
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
               _villaRepository.Update(obj);
                _villaRepository.Save();
                TempData["success"] = "Villa Updated Successfully";
                return RedirectToAction("Index");
            }
            return View(obj);

        }

        public IActionResult Delete(int villaId)
        {
            Villa? obj = _villaRepository.Get(x => x.Id == villaId);
            if (obj == null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(obj);
            
        }
        [HttpPost]
        public IActionResult Delete(Villa obj)
        {
                Villa? objFromDb = _villaRepository.Get(x => x.Id == obj.Id);
                if (objFromDb != null)
                {
                    _villaRepository.Remove(objFromDb);
                    _villaRepository.Save();    
                TempData["success"] = "Villa Deleted Successfully";
                return RedirectToAction("Index");
                }
            return View(obj);

        }

    }
}
