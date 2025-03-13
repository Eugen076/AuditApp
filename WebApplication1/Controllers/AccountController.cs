using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Entities;
using WebApplication1.Models;


namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {   
        private readonly AuditDbContext _auditDbContext;

        public AccountController(AuditDbContext auditDbContext)
        {
            _auditDbContext = auditDbContext;
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

            if(ModelState.IsValid)
            {   
                UserAccount account = new UserAccount();
                account.Email = model.Email;
                account.FirstName = model.FirstName;
                account.LastName = model.LastName;
                account.Password = model.Password;
                account.UserName = model.UserName;

                try
                {
                    _auditDbContext.UserAccounts.Add(account);
                    _auditDbContext.SaveChanges();

                    ModelState.Clear();
                    ViewBag.Message = $"{account.FirstName} {account.LastName} registered succesfully. Please login.";
                    
                }
                catch (DbUpdateException ex)
                {

                    ModelState.AddModelError("", "Please introduce an unique unsername or password.");
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
        public IActionResult Login(LoginViewModel model)
        {
            if(ModelState.IsValid)
            {
                var user = _auditDbContext.UserAccounts.Where(x => (x.UserName == model.UserNameorEmail || x.Email == model.UserNameorEmail) && x.Password == model.Password).FirstOrDefault();
                if(user != null)
                {
                    //succes, create cookie
                    var claims = new List<Claim>
                   {
                       new Claim(ClaimTypes.Name, user.Email),
                       new Claim("Name", user.FirstName),
                       new Claim(ClaimTypes.Role, "User"),
                   };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                   
                    return RedirectToAction("SecurePage");
                }
                else
                {
                    ModelState.AddModelError("", "Username/Email or Password is not correct");
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
