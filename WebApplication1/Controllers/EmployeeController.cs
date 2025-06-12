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


        public async Task<IActionResult> Index(string sortOrder, string searchString, int pageNumber = 1, int pageSize = 10)
        {
            var employees = await _userManager.GetUsersInRoleAsync("Employee");

            // Filtrare
            if (!string.IsNullOrEmpty(searchString))
            {
                employees = employees
                    .Where(u => u.UserName.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                             || u.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Sortare
            employees = sortOrder switch
            {
                "name_desc" => employees.OrderByDescending(e => e.FirstName).ThenByDescending(e => e.LastName).ToList(),
                _ => employees.OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToList()
            };

            // Paginare
            int totalCount = employees.Count;
            var pagedEmployees = employees
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalCount = totalCount;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;

            return View(pagedEmployees);
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
      
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("ID invalid");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            return Json(new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.UserName,
                Status = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.Now ? "Inactiv" : "Activ"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string firstName, string lastName, string email,string userName, string status)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            user.FirstName = firstName;
            user.LastName = lastName;
            user.Email = email;
            user.UserName = userName;
            user.NormalizedEmail = email.ToUpper();
            user.NormalizedUserName = userName.ToUpper();

            user.LockoutEnd = status == "Inactiv"
                ? DateTimeOffset.Now.AddYears(100)
                : DateTimeOffset.Now;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                TempData["Warning"] = "Eroare la actualizare: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }
            else
            {
                TempData["Success"] = "Datele au fost actualizate cu succes.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Angajatul a fost șters.";
            }
            else
            {
                TempData["Warning"] = "Eroare la ștergerea angajatului: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
