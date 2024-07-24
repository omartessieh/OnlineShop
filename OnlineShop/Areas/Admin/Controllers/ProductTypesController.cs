using Microsoft.AspNetCore.Mvc;
using OnlineShop.Data;
using OnlineShop.Models;

namespace OnlineShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductTypesController : Controller
    {
        private ApplicationDbContext _db;

        public ProductTypesController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View(_db.ProductTypes.ToList());
        }


        //GET Create/Edit Action Method
        public IActionResult CreateOrEdit(int? id)
        {
            if (id == null)
            {
                return View(new ProductTypes());
            }

            var productType = _db.ProductTypes.Find(id);
            if (productType == null)
            {
                return NotFound();
            }

            return View(productType);
        }

        //POST Save Action Method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(ProductTypes productTypes)
        {
            if (ModelState.IsValid)
            {
                if (productTypes.Id == 0)
                {
                    var searchProductType = _db.ProductTypes.FirstOrDefault(c => c.ProductType == productTypes.ProductType);
                    if (searchProductType != null)
                    {
                        TempData["err"] = "This product Type already exists";
                        return View(productTypes);
                    }

                    _db.ProductTypes.Add(productTypes);
                    TempData["save"] = "Product type has been saved";
                }
                else
                {
                    var searchProductType = _db.ProductTypes.FirstOrDefault(c => c.ProductType == productTypes.ProductType && c.Id != productTypes.Id);
                    if (searchProductType != null)
                    {
                        TempData["err"] = "This product Type already exists";
                        return View(productTypes);
                    }

                    _db.Update(productTypes);
                    TempData["edit"] = "Product type has been updated";
                }

                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(productTypes);
        }

        //GET Details Action Method
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productType = _db.ProductTypes.Find(id);

            if (productType == null)
            {
                return NotFound();
            }

            return View(productType);
        }

        //POST Edit Action Method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details(ProductTypes productTypes)
        {
            return RedirectToAction(nameof(Index));
        }

        //GET Delete Action Method
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productType = _db.ProductTypes.Find(id);

            if (productType == null)
            {
                return NotFound();
            }

            return View(productType);
        }

        //POST Delete Action Method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id, ProductTypes productTypes)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (id != productTypes.Id)
            {
                return NotFound();
            }

            var productType = _db.ProductTypes.Find(id);

            if (productType == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _db.Remove(productType);
                await _db.SaveChangesAsync();
                TempData["delete"] = "Product type has been deleted";
                return RedirectToAction(nameof(Index));
            }

            return View(productTypes);
        }
    }
}