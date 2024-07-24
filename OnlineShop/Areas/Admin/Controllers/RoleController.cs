using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineShop.Areas.Admin.Models;
using OnlineShop.Data;
using System.Security.Claims;

namespace OnlineShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RoleController : Controller
    {
        RoleManager<IdentityRole> _roleManager;
        UserManager<IdentityUser> _userManager;
        ApplicationDbContext _db;

        public RoleController(RoleManager<IdentityRole> roleManager, ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _roleManager = roleManager;
            _db = db;

            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var roles = _roleManager.Roles.ToList();
            ViewBag.Roles = roles;
            return View();
        }

        public async Task<IActionResult> CreateOrEdit(string id = null)
        {
            if (id == null) // Create
            {
                ViewBag.Id = null;
                ViewBag.Name = string.Empty;
                ViewBag.Title = "Create Role";
                return View();
            }
            else // Edit
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return NotFound();
                }

                ViewBag.Id = role.Id;
                ViewBag.Name = role.Name;
                ViewBag.Title = "Edit Role";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrEdit(string id, string name)
        {
            if (id == null) // Create
            {
                var role = new IdentityRole { Name = name };
                var isExist = await _roleManager.RoleExistsAsync(role.Name);

                if (isExist)
                {
                    ModelState.AddModelError(string.Empty, "This role already exists");
                    ViewBag.Title = "Create Role";
                    return View();
                }

                var result = await _roleManager.CreateAsync(role);

                if (result.Succeeded)
                {
                    TempData["save"] = "Role has been saved";
                    return RedirectToAction(nameof(Index));
                }
            }
            else // Edit
            {
                var role = await _roleManager.FindByIdAsync(id);

                if (role == null)
                {
                    return NotFound();
                }

                role.Name = name;
                var isExist = await _roleManager.RoleExistsAsync(role.Name);

                if (isExist)
                {
                    ModelState.AddModelError(string.Empty, "This role already exists");
                    ViewBag.Title = "Edit Role";
                    return View();
                }

                var result = await _roleManager.UpdateAsync(role);

                if (result.Succeeded)
                {
                    TempData["save"] = "Role has been updated";
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewBag.Id = id;
            ViewBag.Name = name;
            ViewBag.Title = id == null ? "Create Role" : "Edit Role";
            return View();
        }

        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            ViewBag.id = role.Id;
            ViewBag.name = role.Name;
            return View();
        }

        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirm(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
            {
                TempData["delete"] = "Role has been deleted";
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        public async Task<IActionResult> Assign()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get the list of users excluding the current user
            var users = _db.ApplicationUsers.Where(f => (f.LockoutEnd < DateTime.Now || f.LockoutEnd == null) && f.Id != currentUserId).ToList();
            ViewData["UserId"] = new SelectList(users, "Id", "UserName");
            ViewData["RoleId"] = new SelectList(_roleManager.Roles.ToList(), "Name", "Name");

            // Get the user-role mappings
            var result = from ur in _db.UserRoles
                         join r in _db.Roles on ur.RoleId equals r.Id
                         join a in _db.ApplicationUsers on ur.UserId equals a.Id
                         select new UserRoleMaping
                         {
                             UserId = ur.UserId,
                             RoleId = ur.RoleId,
                             UserName = a.UserName,
                             RoleName = r.Name
                         };
            ViewBag.UserRoles = result;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Assign(RoleUserVm roleUser)
        {
            var user = _db.ApplicationUsers.FirstOrDefault(c => c.Id == roleUser.UserId);
            if (user == null)
            {
                TempData["err"] = "User not found.";
                return RedirectToAction(nameof(Assign));
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            if (userRoles.Contains(roleUser.RoleId))
            {
                TempData["err"] = "This user is already assigned to this role.";
                return RedirectToAction(nameof(Assign));
            }

            // Remove all roles from the user
            var removeResult = await _userManager.RemoveFromRolesAsync(user, userRoles);
            if (!removeResult.Succeeded)
            {
                TempData["err"] = "Error removing user roles.";
                return RedirectToAction(nameof(Assign));
            }

            // Add the new role
            var addResult = await _userManager.AddToRoleAsync(user, roleUser.RoleId);
            if (addResult.Succeeded)
            {
                TempData["save"] = "User role updated.";
                return RedirectToAction(nameof(Assign));
            }

            TempData["err"] = "Error assigning new role.";
            return RedirectToAction(nameof(Assign));
        }
    }
}