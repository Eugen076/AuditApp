using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text.RegularExpressions;
using WebApplication1.Data;
using WebApplication1.Entities;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly AuditDbContext _context;
        private readonly UserManager<UserAccount> _userManager;
        private readonly IAuditService _auditService;
        private readonly ICryptoService _crypto;

        public CustomersController(AuditDbContext context, UserManager<UserAccount> userManager, IAuditService auditService, ICryptoService crypto)
        {
            _context = context;
            _userManager = userManager;
            _auditService = auditService;
            _crypto = crypto;
        }

   
        public async Task<IActionResult> Index(string search)
        {
            var customers = _context.Customers
                .Include(c => c.CreatedBy)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                customers = customers.Where(c =>
                    c.FullName.Contains(search) ||
                    c.Email.Contains(search) ||
                    c.Phone.Contains(search));
            }

            ViewBag.SearchQuery = search; 

            return View(await customers.ToListAsync());

        }

     
        [HttpGet]
        public IActionResult Reauth(int id)
        {
            ViewBag.CustomerId = id;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Reauth([FromBody] ReauthModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return BadRequest(new { error = "Utilizator neautentificat." });
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!passwordValid)
            {
                return BadRequest(new { error = "Parola este incorectă." });
            }

            var customer = await _context.Customers
                .Include(c => c.CreatedBy)
                .FirstOrDefaultAsync(c => c.Id == model.Id);

            if (customer == null)
            {
                return NotFound();
            }

            var userId = user.Id;
            var userName = user.UserName;
            var details = $"A vizualizat detaliile lui'{customer.FullName}' (ID: {customer.Id})";
            await _auditService.LogAsync(userId, userName, "CustomerSensitiveViewed", details);
            
            HttpContext.Session.SetInt32("SensitiveCustomerId", customer.Id);
            HttpContext.Session.SetString("SensitiveDataAccessTime", DateTime.UtcNow.ToString("o"));

            var phone = _crypto.Decrypt(customer.Phone);
            var address = _crypto.Decrypt(customer.Address);
            var cnp = _crypto.Decrypt(customer.CNP);

            return Json(new
            {
                fullname = customer.FullName,
                phone = phone,
                address = address,
                cnp = cnp,
                dob = customer.DateOfBirth.ToString("dd.MM.yyyy"),
                created = customer.CreatedAt.ToString("dd.MM.yyyy HH:mm")
            });
        }

/*        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var customer = await _context.Customers
                .Include(c => c.CreatedBy)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (customer == null)
                return NotFound();

            customer.Phone = _crypto.Decrypt(customer.Phone);
            customer.Address = _crypto.Decrypt(customer.Address);
            customer.CNP = _crypto.Decrypt(customer.CNP);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.Identity?.Name;
            var details = $"Viewed basic details of customer '{customer.FullName}' (ID: {customer.Id})";
            await _auditService.LogAsync(userId, userName, "CustomerViewedBasic", details);
            Console.WriteLine(">>> AUDIT pentru vizualizare a fost apelat.");
            return View(customer);
        }*/

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FullName,CNP,Email,Phone,Address,IsActive,DateOfBirth")] Customer customer)
        {
            if (string.IsNullOrWhiteSpace(customer.CNP) || !Regex.IsMatch(customer.CNP, @"^\d{13}$"))
            {
                ModelState.AddModelError("CNP", "CNP-ul trebuie să conțină exact 13 cifre.");
            }

            if (string.IsNullOrWhiteSpace(customer.Phone) || !Regex.IsMatch(customer.Phone, @"^07\d{8}$"))
            {
                ModelState.AddModelError("Phone", "Numărul de telefon trebuie să conțină 10 cifre și să înceapă cu '07'.");
            }

            var age = DateTime.Today.Year - customer.DateOfBirth.Year;
            if (customer.DateOfBirth > DateTime.Today.AddYears(-age)) age--;

            if (age < 18)
            {
                ModelState.AddModelError("DateOfBirth", "Clientul trebuie să aibă cel puțin 18 ani.");
            }

            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            try
            {
               
                var encryptedCNP = _crypto.Encrypt(customer.CNP);
                var encryptedPhone = _crypto.Encrypt(customer.Phone);

                bool cnpExists = await _context.Customers.AnyAsync(c => c.CNP == encryptedCNP);
                bool phoneExists = await _context.Customers.AnyAsync(c => c.Phone == encryptedPhone);

                if (cnpExists || phoneExists)
                {
                    
                    ModelState.AddModelError(string.Empty, "Există deja un client cu aceste date personale.");
                    return View(customer);
                }

                if (!ModelState.IsValid)
                {
                    return View(customer);
                }

                customer.CNP = encryptedCNP;
                customer.Phone = encryptedPhone;
                customer.Address = _crypto.Encrypt(customer.Address);

                customer.CreatedAt = DateTime.Now;
                customer.CreatedById = _userManager.GetUserId(User);

                _context.Add(customer);
                await _context.SaveChangesAsync();

                var userName = User.Identity?.Name;
                var details = $"Created customer '{customer.FullName}'";
                await _auditService.LogAsync(customer.CreatedById, userName, "CustomerCreated", details);

                TempData["Success"] = "Clientul a fost adăugat cu succes.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine("EROARE: " + ex.ToString());
                TempData["Error"] = "A apărut o eroare la salvarea clientului.";
                return View(customer);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return NotFound();

            var model = new CustomerEditViewModel
            {
                Id = customer.Id,
                FullName = customer.FullName,
                Email = customer.Email,
                Phone = _crypto.Decrypt(customer.Phone),
                Address = _crypto.Decrypt(customer.Address),
                IsActive = customer.IsActive,
               // DateOfBirth = customer.DateOfBirth
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CustomerEditViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var customer = await _context.Customers.FindAsync(id);
                    if (customer == null)
                        return NotFound();

                    customer.FullName = model.FullName;
                    customer.Email = model.Email;
                    customer.Phone = _crypto.Encrypt(model.Phone);
                    customer.Address = _crypto.Encrypt(model.Address);
                    customer.IsActive = model.IsActive;
                    

                    await _context.SaveChangesAsync();

                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var userName = User.Identity?.Name;
                    var details = $"Edited customer '{customer.FullName}' (ID: {customer.Id})";
                    await _auditService.LogAsync(userId, userName, "CustomerEdited", details);

                    TempData["Success"] = "Clientul a fost modificat cu succes.";

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(model.Id))
                        return NotFound();
                    else
                        throw;
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var customer = await _context.Customers
                .Include(c => c.CreatedBy)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (customer == null)
                return NotFound();

            return View(customer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
                return NotFound();

            bool hasAccounts = await _context.BankAccounts.AnyAsync(a => a.CustomerId == id);
            bool hasLoans = await _context.Loans.AnyAsync(l => l.CustomerId == id);

            if (hasAccounts || hasLoans)
            {
                TempData["Error"] = "Clientul nu poate fi șters deoarece are conturi sau credite active.";
                return RedirectToAction(nameof(Index));
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.Identity?.Name;

            var details = $"Deleted customer '{customer.FullName}'";
            await _auditService.LogAsync(userId, userName, "CustomerDeleted", details);

            TempData["Success"] = "Clientul a fost șters.";
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.Id == id);
        }

    }
}
