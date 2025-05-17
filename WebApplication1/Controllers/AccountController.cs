using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Data;

//using WebApplication1.Data;
using WebApplication1.Entities;
using WebApplication1.Models;
using WebApplication1.Services;



namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly SignInManager<UserAccount> _signInManager;
        private readonly IAuditService _auditService;
        private readonly AuditDbContext _context;

        public AccountController(
            UserManager<UserAccount> userManager,
            SignInManager<UserAccount> signInManager,
            IAuditService auditService,
            AuditDbContext context) 
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _auditService = auditService;
            _context = context; 
        }

        /* public IActionResult Index()
         {
             return View(_auditDbContext.UserAccounts.ToList());
         }*/

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList(); 
            return View(users);
        }

        private List<SelectListItem> GetRoles()
        {
            return new List<SelectListItem>
    {
        new SelectListItem { Value = "Admin", Text = "Admin" },
        new SelectListItem { Value = "Employee", Text = "Employee" }
    };
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Registration()
        {
            var model = new RegistrationViewModel
            {
                Roles = GetRoles()
            };

            return View(model);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Registration(RegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Roles = GetRoles(); 
                return View(model);
            }

            var user = new UserAccount
            {
                UserName = model.UserName,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                LockoutEnabled = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);

                ViewBag.Message = $"{user.FirstName} {user.LastName} registered successfully with role {model.Role}.";

                // Reîncarcă modelul golit, dar cu dropdown
                return View(new RegistrationViewModel
                {
                    Roles = GetRoles()
                });
            }

            // Erori de creare
            model.Roles = GetRoles();

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
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
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.UserNameorEmail)
                ?? await _userManager.FindByNameAsync(model.UserNameorEmail);

            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    user,
                    model.Password,
                    isPersistent: false,
                    lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    await _auditService.LogAsync(user.Id, user.UserName, "Login", "User successfully logged in.");
                    return RedirectToAction("SecurePage");
                }
                else
                {
                    await _auditService.LogAsync(user.Id, user.UserName, "Failed Login", "Wrong password.");
                }
                if (result.IsLockedOut)
                {
                   
                    var alreadyLogged = await _context.AuditLogs.AnyAsync(l =>
                        l.Action == "Account Locked" &&
                        l.UserName == user.UserName &&
                        l.Timestamp > DateTime.UtcNow.AddMinutes(-2));

                    if (!alreadyLogged)
                    {
                        await _auditService.LogAsync(user.Id, user.UserName, "Account Locked", "Cont blocat după prea multe încercări.");
                    }

                    ModelState.AddModelError("", "Contul a fost blocat temporar.");
                    return View(model);
                }
            }
            else
            {
                await _auditService.LogAsync(null, model.UserNameorEmail, "Failed Login", $"User not found: {model.UserNameorEmail}");
            }

            ModelState.AddModelError("", "Username/Email or Password is incorrect.");
            return View(model);
        }

        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Index", "Home");
        }

        // [Authorize]
        public IActionResult SecurePage()
        {
            ViewBag.Name = HttpContext.User.Identity?.Name;
            return View();
        }

    }
}
