using Microsoft.AspNetCore.Mvc;
using Stripe;
using WhiteLagoon_DataAccess;
using WhiteLagoon_DataAccess.Repository.IRepository;
using WhiteLagoon_Models;

namespace WhiteLagoon.Controllers
{
    public class VillaController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public VillaController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
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
                if (obj.Image != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(obj.Image.FileName);
                    string productPath = Path.Combine(_webHostEnvironment.WebRootPath, @"images\products");

                    if (!string.IsNullOrEmpty(obj.ImageUrl))
                    {
                        //delete the old image
                        var oldImagePath =
                            Path.Combine(_webHostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        obj.Image.CopyTo(fileStream);
                    }

                    obj.ImageUrl= @"\images\products\" + fileName;
                }
                else
                {
                    obj.ImageUrl = "https://placehold.co/600x400";
                }


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
                if (obj.Image != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(obj.Image.FileName);
                    string productPath = Path.Combine(_webHostEnvironment.WebRootPath, @"images\products");

                    if (!string.IsNullOrEmpty(obj.ImageUrl))
                    {
                        //delete the old image
                        var oldImagePath =
                            Path.Combine(_webHostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        obj.Image.CopyTo(fileStream);
                    }

                    obj.ImageUrl = @"\images\products\" + fileName;
                }

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
                if (!string.IsNullOrEmpty(objFromDb.ImageUrl))
                {
                     var oldImagePath =
                            Path.Combine(_webHostEnvironment.WebRootPath, objFromDb.ImageUrl.TrimStart('\\'));
                    FileInfo file = new(oldImagePath);

                    if (file.Exists)
                    {
                        file.Delete();
                    }
                }
                _unitOfWork.Villa.Remove(objFromDb);
                _unitOfWork.Save();    
                TempData["success"] = "Villa Deleted Successfully";
                return RedirectToAction("Index");
                }
            return View(obj);

        }
        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Villa> objVillaList = _unitOfWork.Villa.GetAll().ToList();
            return Json(new { data = objVillaList });
        }


        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var villaToBeDeleted = _unitOfWork.Villa.Get(u => u.Id == id);
            if (villaToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            if (!string.IsNullOrEmpty(villaToBeDeleted.ImageUrl))
            {
                var oldImagePath =
                       Path.Combine(_webHostEnvironment.WebRootPath, villaToBeDeleted.ImageUrl.TrimStart('\\'));
                FileInfo file = new(oldImagePath);

                if (file.Exists)
                {
                    file.Delete();
                }
            }

            _unitOfWork.Villa.Remove(villaToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion
    }
}
