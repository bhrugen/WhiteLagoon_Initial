using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Data;
using WhiteLagoon.Models;

namespace WhiteLagoon.Controllers
{
    public class VillaController : Controller
    {
        private readonly ApplicationDbContext _context;
        public VillaController(ApplicationDbContext context)
        {
                _context = context;
        }
        public IActionResult Index()
        {
            List<Villa> villaList = _context.Villas.ToList();
            return View(villaList);
        }
        public IActionResult Create()
        {
            TempData["success"] = "Creating Villa";
            return View();
        }
        [HttpPost]
        public IActionResult Create(Villa obj)
        {
            if (obj.Name == obj.Description?.ToString())
            {
                ModelState.AddModelError("name","The DisplayOrder cannot exactly match the Name.");
                TempData["error"] = "Error encountered";
                //for eaxmple only
                return RedirectToAction("Index");
            }
            if (ModelState.IsValid)
            {
                _context.Villas.Add(obj);
                _context.SaveChanges();
                TempData["success"] = "Villa Created Successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
            
        }

        public IActionResult Update(int villaId)
        {
            Villa? obj = _context.Villas.FirstOrDefault(x => x.Id == villaId);
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
                _context.Villas.Update(obj);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(obj);

        }

        public IActionResult Delete(int villaId)
        {
            Villa? obj = _context.Villas.FirstOrDefault(x => x.Id == villaId);
            if (obj == null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(obj);
            
        }
        [HttpPost]
        public IActionResult Delete(Villa obj)
        {
                Villa? objFromDb = _context.Villas.FirstOrDefault(x => x.Id == obj.Id);
                if (objFromDb != null)
                {
                    _context.Villas.Remove(objFromDb);
                    _context.SaveChanges();

                    return RedirectToAction("Index");
                }
            return View(obj);

        }

    }
}
