using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Entities;
using WebApplication1.Models;

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

       
        public async Task<IActionResult> Index()
        {
            var employees = await _userManager.GetUsersInRoleAsync("Employee");
            return View(employees);
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


        [HttpGet]
        public async Task<IActionResult> EditPassword(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Id-ul utilizatorului este gol.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Utilizatorul nu a fost găsit.";
                return RedirectToAction("Index");
            }

            var model = new ResetPasswordViewModel
            {
                UserId = user.Id
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPassword([FromForm] ResetPasswordViewModel model)

        {
            Console.WriteLine($"DEBUG: UserId primit: {model.UserId}");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Model invalid: UserId=" + model.UserId;
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Parola a fost resetată cu succes.";
                return RedirectToAction("Index");
            }

            
            TempData["ErrorMessage"] = "Resetarea parolei a eșuat: " + string.Join(", ", result.Errors.Select(e => e.Description));
            return View(model);
        }

    }
}
