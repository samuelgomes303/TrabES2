using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using TrabalhoES2.Models;
using TrabalhoES2.Services; // Adicione esta linha
using TrabalhoES2.utils;

var builder = WebApplication.CreateBuilder(args);

// 1) Serviços MVC + Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddTransient<IEmailSender, FakeEmailSender>();

// 2) DbContext
builder.Services.AddDbContext<projetoPraticoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3) Configuração do Identity COM UI integrada
builder.Services.AddIdentity<Utilizador, IdentityRole<int>>(options =>
    {
        // Aqui podes configurar requisitos de password, lockout, etc.
    })
    .AddEntityFrameworkStores<projetoPraticoDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();    // <<< Isto garante que as páginas /Identity/Account/Login, Register, etc. ficam disponíveis

// 4) Claims principal factory (custom)
builder.Services.AddScoped<IUserClaimsPrincipalFactory<Utilizador>, AppUserClaimsPrincipalFactory>();

var app = builder.Build();

// 5) Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
    var userManager = services.GetRequiredService<UserManager<Utilizador>>();
    await SeedRoles.SeedAsync(services, roleManager, userManager);
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 6) Rotas de controllers e Razor Pages
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();  // <<< Isto garante que as páginas em /Areas/Identity/Pages/... são servidas

app.Run();
