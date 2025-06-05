using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Entities;
using WebApplication1.Models;
using WebApplication1.Services;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AuditDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<UserAccount>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = false;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddRoles<IdentityRole>() 
.AddEntityFrameworkStores<AuditDbContext>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<ICreditService, CreditService>();

builder.Services.AddHttpClient();

/*builder.Services.AddHttpClient();
builder.Services.AddScoped<IExchangeRateService, ExchangeRateService>();*/

builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Home/AccessDenied"; 
});



builder.Services.AddControllersWithViews();


builder.Services.AddSingleton<EmailService>();

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserAccount>>();

    var roles = new[] { "Admin", "Employee", "Client" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }


    string adminEmail = "admin@yahoo.com";
    string adminPassword = "Admin123!";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        var user = new UserAccount
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Admin");
        }
    }

    string clientEmail = "client@demo.com";
    string clientPassword = "Client123!";

    var clientUser = await userManager.FindByEmailAsync(clientEmail);
    if (clientUser == null)
    {
        var user = new UserAccount
        {
            UserName = clientEmail,
            Email = clientEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, clientPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Client");
        }
    }
}



app.Run();
