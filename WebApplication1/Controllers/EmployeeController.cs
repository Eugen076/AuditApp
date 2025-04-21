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

        public async Task<IActionResult> Index()
        {
            var employees = await _userManager.GetUsersInRoleAsync("Employee"); // sau "Employee"
            return View(employees);
        }
    }
}
