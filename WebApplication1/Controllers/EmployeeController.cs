using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Entities;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EmployeeController : Controller
    {
        private readonly UserManager<UserAccount> _userManager;

        public EmployeeController(UserManager<UserAccount> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.Now)
            {
                user.LockoutEnd = DateTimeOffset.Now;
                TempData["Success"] = "Contul a fost activat.";
            }
            else
            {
                user.LockoutEnd = DateTimeOffset.Now.AddYears(100);
                TempData["Warning"] = "Contul a fost dezactivat.";
            }

            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Index()
        {
            var employees = await _userManager.GetUsersInRoleAsync("Employee"); // sau "Employee"
            return View(employees);
        }
    }
}
