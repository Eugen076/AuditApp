using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Data;
using WebApplication1.Entities;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly AuditDbContext _context;
        private readonly UserManager<UserAccount> _userManager;
        private readonly IAuditService _auditService;

        public CustomersController(AuditDbContext context, UserManager<UserAccount> userManager, IAuditService auditService)
        {
            _context = context;
            _userManager = userManager;
            _auditService = auditService;
        }

        // GET: Customers
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

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(int? id)
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

        // GET: Customers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FullName,CNP,Email,Phone,Address,IsActive,DateOfBirth")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                customer.CreatedAt = DateTime.Now;

                var userId = _userManager.GetUserId(User);
                customer.CreatedById = userId;

                _context.Add(customer);
                await _context.SaveChangesAsync();

                var userName = User.Identity?.Name;

                var details = $"Created customer '{customer.FullName}'";

                await _auditService.LogAsync(userId, userName, "CustomerCreated", details);

                TempData["Success"] = "Clientul a fost adăugat cu succes.";
                return RedirectToAction(nameof(Index));
            }
            // Aici adaugi blocul pentru diagnosticare:
            foreach (var entry in ModelState)
            {
                foreach (var error in entry.Value.Errors)
                {
                    Console.WriteLine($"[{entry.Key}] Error: {error.ErrorMessage}");
                }
            }

            return View(customer);
        }

        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return NotFound();

            return View(customer);
        }

        // POST: Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,CNP,Email,Phone,Address,IsActive,DateOfBirth")] Customer customer)
        {
            if (id != customer.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCustomer = await _context.Customers.FindAsync(id);
                    if (existingCustomer == null)
                        return NotFound();

                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var userName = User.Identity?.Name;

                    var details = $"Edited customer '{customer.FullName}'";

                    await _auditService.LogAsync(userId, userName, "CustomerEdited", details);


                    // Actualizează doar câmpurile editabile
                    existingCustomer.FullName = customer.FullName;
                    existingCustomer.CNP = customer.CNP;
                    existingCustomer.Email = customer.Email;
                    existingCustomer.Phone = customer.Phone;
                    existingCustomer.Address = customer.Address;
                    existingCustomer.IsActive = customer.IsActive;
                    existingCustomer.DateOfBirth = customer.DateOfBirth;

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Clientul a fost modificat cu succes.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(customer);
        }

        // GET: Customers/Delete/5
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

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userName = User.Identity?.Name;

                var details = $"Deleted customer '{customer.FullName}'";

                await _auditService.LogAsync(userId, userName, "CustomerDeleted", details);


                TempData["Success"] = "Clientul a fost șters.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.Id == id);
        }
    }
}
