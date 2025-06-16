using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using TrabalhoES2.Models;
using TrabalhoES2.Services;
using TrabalhoES2.utils; // para TestAuthHandler

var builder = WebApplication.CreateBuilder(args);

// 1) Serviços MVC + Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddTransient<IEmailSender, FakeEmailSender>();

// 2) DbContext
builder.Services.AddDbContext<projetoPraticoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3) Identity COM UI integrada
builder.Services.AddIdentity<Utilizador, IdentityRole<int>>(options =>
    {
        // configure requisitos de password, lockout, etc.
    })
    .AddEntityFrameworkStores<projetoPraticoDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

// 4) Claims principal factory (custom)
builder.Services.AddScoped<IUserClaimsPrincipalFactory<Utilizador>, AppUserClaimsPrincipalFactory>();

// 5) Autenticação de teste em ambiente “Test”
if (builder.Environment.IsEnvironment("Test"))
{
    // substitui o esquema default para “Test” que sempre autentica um utilizador fictício
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "Test";
        options.DefaultChallengeScheme = "Test";
    })
    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
    // Se tiver configurações de Authorization policies, elas serão avaliadas com este principal.
}
// Caso contrário, em ambiente normal, UseAuthentication/UseAuthorization mantém o Identity normal.

var app = builder.Build();

// 6) Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Seed roles/usuário
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

public partial class Program { }
