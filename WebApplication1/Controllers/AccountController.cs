using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Entities;
using WebApplication1.Models;
using WebApplication1.Services;



namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuditDbContext _auditDbContext;
        private readonly IAuditService _auditService;

        public AccountController(AuditDbContext auditDbContext, IAuditService auditService)
        {
            _auditDbContext = auditDbContext;
            _auditService = auditService;
        }

        public IActionResult Index()
        {
            return View(_auditDbContext.UserAccounts.ToList());
        }

        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registration(RegistrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                UserAccount account = new UserAccount
                {
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Password = model.Password,
                    UserName = model.UserName
                };

                try
                {
                    _auditDbContext.UserAccounts.Add(account);
                    _auditDbContext.SaveChanges();

                    ModelState.Clear();
                    ViewBag.Message = $"{account.FirstName} {account.LastName} registered successfully. Please login.";
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "Please introduce a unique username or password.");
                    return View(model);
                }
                return View();
            }
            return View(model);
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _auditDbContext.UserAccounts
                    .Where(x => (x.UserName == model.UserNameorEmail || x.Email == model.UserNameorEmail) && x.Password == model.Password)
                    .FirstOrDefault();

                if (user != null)
                {

                    var claims = new List<Claim>
                    {
                      new Claim(ClaimTypes.Name, user.Email),
                      new Claim("Name", user.FirstName),
                      new Claim(ClaimTypes.Role, "User"),
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    // LOGARE ÎN AUDIT
                    await _auditService.LogAsync(user.Id,user.UserName, "Login", "User successfully logged in.");

                    return RedirectToAction("SecurePage");
                }
                else
                {
                    await _auditService.LogAsync(null, model.UserNameorEmail, "Failed Login", $"Failed login attempt for {model.UserNameorEmail}");

                    ModelState.AddModelError("", "Username/Email or Password is incorrect.");
                }
            }
            return View(model);
        }


        public IActionResult LogOut()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }

        [Authorize]
        public IActionResult SecurePage()
        {
            ViewBag.Name = HttpContext.User.Identity.Name;
            return View();
        }
    }
}
