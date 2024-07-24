using Microsoft.AspNetCore.Mvc;
using OnlineShop.Data;
using OnlineShop.Models;

namespace OnlineShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SpecialTagController : Controller
    {
        private ApplicationDbContext _db;

        public SpecialTagController(ApplicationDbContext db)
        {
            _db = db;
        }

        //GET Index Action Method
        public IActionResult Index()
        {
            return View(_db.SpecialTags.ToList());
        }

        //GET: CreateOrEdit
        public ActionResult CreateOrEdit(int? id)
        {
            if (id == null)
            {
                return View(new SpecialTag()); // Create new
            }

            var specialTag = _db.SpecialTags.Find(id);
            if (specialTag == null)
            {
                return NotFound();
            }

            return View(specialTag); // Edit existing
        }

        //POST: CreateOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(SpecialTag specialTag)
        {
            if (ModelState.IsValid)
            {
                if (specialTag.Id == 0)
                {
                    var searchspecialTag = _db.SpecialTags.FirstOrDefault(c => c.Name == specialTag.Name);
                    if (searchspecialTag != null)
                    {
                        TempData["err"] = "Special Tag already exists";
                        return View(specialTag);
                    }

                    _db.SpecialTags.Add(specialTag); // Create
                    TempData["save"] = "Special Tag has been saved";
                }
                else
                {
                    var searchspecialTag = _db.SpecialTags.FirstOrDefault(c => c.Name == specialTag.Name && c.Id != specialTag.Id);
                    if (searchspecialTag != null)
                    {
                        TempData["err"] = "Special Tag already exists";
                        return View(specialTag);
                    }

                    _db.Update(specialTag); // Edit
                    TempData["edit"] = "Special Tag has been updated";
                }
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(specialTag);
        }

        //GET Details Action Method
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var specialTag = _db.SpecialTags.Find(id);

            if (specialTag == null)
            {
                return NotFound();
            }

            return View(specialTag);
        }

        //POST Edit Action Method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details(SpecialTag specialTag)
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

            var specialTag = _db.SpecialTags.Find(id);

            if (specialTag == null)
            {
                return NotFound();
            }

            return View(specialTag);
        }

        //POST Delete Action Method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id, SpecialTag specialTag)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (id != specialTag.Id)
            {
                return NotFound();
            }

            var specialTags = _db.SpecialTags.Find(id);

            if (specialTags == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _db.Remove(specialTags);
                await _db.SaveChangesAsync();
                TempData["delete"] = "Special Tag has been deleted";
                return RedirectToAction(nameof(Index));
            }

            return View(specialTag);
        }
    }
}