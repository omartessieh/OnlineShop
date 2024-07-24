using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;

namespace OnlineShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private ApplicationDbContext _db;
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment _he;

        public ProductController(ApplicationDbContext db, Microsoft.AspNetCore.Hosting.IHostingEnvironment he)
        {
            _db = db;
            _he = he;
        }

        public IActionResult Index()
        {
            return View(_db.Products.Include(c => c.ProductTypes).Include(f => f.SpecialTag).ToList());
        }

        //POST Index action method
        [HttpPost]
        public IActionResult Index(decimal? lowAmount, decimal? largeAmount)
        {
            var products = _db.Products.Include(c => c.ProductTypes).Include(c => c.SpecialTag).Where(c => c.Price >= lowAmount && c.Price <= largeAmount).ToList();

            if (lowAmount == null || largeAmount == null)
            {
                products = _db.Products.Include(c => c.ProductTypes).Include(c => c.SpecialTag).ToList();
            }

            return View(products);
        }

        //Get Create/Edit method
        public IActionResult CreateOrEdit(int? id)
        {
            ViewData["productTypeId"] = new SelectList(_db.ProductTypes.ToList(), "Id", "ProductType");
            ViewData["TagId"] = new SelectList(_db.SpecialTags.ToList(), "Id", "Name");

            if (id == null)
            {
                return View(new Products());
            }
            else
            {
                var product = _db.Products.FirstOrDefault(c => c.Id == id);
                if (product == null)
                {
                    return NotFound();
                }
                return View(product);
            }
        }

        //Post Create/Edit method
        [HttpPost]
        public async Task<IActionResult> CreateOrEdit(Products product, IFormFile? image)
        {
            ViewData["productTypeId"] = new SelectList(_db.ProductTypes.ToList(), "Id", "ProductType");
            ViewData["TagId"] = new SelectList(_db.SpecialTags.ToList(), "Id", "Name");

            if (image != null)
            {
                var name = Path.Combine(_he.WebRootPath + "/Images", Path.GetFileName(image.FileName));
                await image.CopyToAsync(new FileStream(name, FileMode.Create));
                product.Image = "Images/" + image.FileName;
            }
            else if (product.Id == 0)
            {
                product.Image = "Images/noimage.PNG";
            }

            if (ModelState.IsValid)
            {
                if (product.Id == 0)
                {
                    var searchProduct = _db.Products.FirstOrDefault(c => c.Name == product.Name);
                    if (searchProduct != null)
                    {
                        TempData["err"] = "This product already exists";
                        return View(product);
                    }

                    _db.Products.Add(product);
                    TempData["save"] = "Product has been saved";
                }
                else
                {
                    if (image == null)
                    {
                        var existingProduct = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == product.Id);
                        product.Image = existingProduct.Image;
                    }

                    var searchProduct = _db.Products.FirstOrDefault(c => c.Name == product.Name && c.Id != product.Id);
                    if (searchProduct != null)
                    {
                        TempData["err"] = "This product already exists";
                        return View(product);
                    }

                    _db.Products.Update(product);
                    TempData["edit"] = "Product has been updated";
                }

                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        //Product Exists Method
        private bool ProductExists(int id)
        {
            return _db.Products.Any(e => e.Id == id);
        }

        //GET Details Action Method
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = _db.Products.Include(c => c.ProductTypes).Include(c => c.SpecialTag).FirstOrDefault(c => c.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        //GET Delete Action Method
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = _db.Products.Include(c => c.SpecialTag).Include(c => c.ProductTypes).Where(c => c.Id == id).FirstOrDefault();

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        //POST Delete Action Method
        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirm(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = _db.Products.FirstOrDefault(c => c.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            TempData["delete"] = "Product has been deleted";
            return RedirectToAction(nameof(Index));
        }
    }
}