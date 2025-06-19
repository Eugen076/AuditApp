using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Data;
using OtpNet;

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
        private readonly IConfiguration _configuration;

        public AccountController(
            UserManager<UserAccount> userManager,
            SignInManager<UserAccount> signInManager,
            IAuditService auditService,
            AuditDbContext context,
            IConfiguration configuration) 
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _auditService = auditService;
            _context = context;
            _configuration = configuration;
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

                    // Bypass 2FA temporar
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("SecurePage");
                }
                else
                {
                    await _auditService.LogAsync(user.Id, user.UserName, "Failed Login", "Wrong password.");
                }

                /*                if (await _userManager.CheckPasswordAsync(user, model.Password))
                                {
                                    // Parola este corectă, trimitem către pagina de 2FA
                                    TempData["2FAUserId"] = user.Id; // stocăm temporar userId-ul
                                    await _auditService.LogAsync(user.Id, user.UserName, "Password OK", "Parola validă, urmează 2FA.");
                                    return RedirectToAction("TwoFactor");
                                }
                                else
                                {
                                    await _auditService.LogAsync(user.Id, user.UserName, "Failed Login", "Parolă greșită.");
                                    ModelState.AddModelError("", "Parola este greșită.");
                                    return View(model);
                                }*/

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

        [HttpGet]
        public IActionResult TwoFactor()
        {
            var userId = TempData["2FAUserId"]?.ToString();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            return View(new TwoFactorViewModel { UserId = userId });
        }


        [HttpPost]
        public async Task<IActionResult> TwoFactor(TwoFactorViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                ModelState.AddModelError("", "Utilizatorul nu a fost găsit.");
                return View(model);
            }

            var secret = _configuration["TOTPSecret"];
            var bytes = OtpNet.Base32Encoding.ToBytes(secret);
            var totp = new OtpNet.Totp(bytes);

            if (totp.VerifyTotp(model.Code, out _))
            {
                // Login complet
                await _signInManager.SignInAsync(user, isPersistent: false);
                await _auditService.LogAsync(user.Id, user.UserName, "2FA Success", "2FA code accepted");
                return RedirectToAction("SecurePage");
            }

            await _auditService.LogAsync(user.Id, user.UserName, "2FA Failure", "Cod greșit");
            ModelState.AddModelError("", "Codul este invalid.");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var user = await _userManager.GetUserAsync(User);

            var model = new UserSettingsViewModel
            {
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "-"
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSettings(UserSettingsViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            user.UserName = model.UserName;
            user.NormalizedUserName = model.UserName.ToUpper();

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Username actualizat.";
                return RedirectToAction("Settings");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View("Settings", model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(UserSettingsViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Parolele nu se potrivesc.");
                return View("Settings", model);
            }

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Parola a fost schimbată.";
                return RedirectToAction("Settings");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View("Settings", model);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.UserNameOrEmail)
                ?? await _userManager.FindByNameAsync(model.UserNameOrEmail);

            if (user == null)
            {
                ModelState.AddModelError("", "Contul nu a fost găsit.");
                return View(model);
            }

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError("", "Emailul nu este confirmat.");
                return View(model);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Account",
                new { token = token, email = user.Email }, protocol: HttpContext.Request.Scheme);

            await EmailHelper.SendEmailAsync(_configuration,
                user.Email,
                "Resetare parolă",
                $"Apasă <a href='{callbackUrl}'>aici</a> pentru a reseta parola.");

            return RedirectToAction("ForgotPasswordConfirmation");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
                return RedirectToAction("Login");

            return View(new ResetPasswordViewModel { Token = token, Email = email });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // La fel, nu divulgăm
                return RedirectToAction("ResetPasswordConfirmation");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
            {
                await _auditService.LogAsync(user.Id, user.UserName, "Password Reset", "Parolă resetată cu succes.");
                return RedirectToAction("ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }


    }
}
