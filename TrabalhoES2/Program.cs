using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TrabalhoES2.Models;
using TrabalhoES2.utils;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();


builder.Services.AddDbContext<projetoPraticoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuração do Identity com chave primária int
builder.Services.AddIdentity<Utilizador, IdentityRole<int>>()
    .AddEntityFrameworkStores<projetoPraticoDbContext>()
    .AddDefaultTokenProviders();
    

builder.Services.AddScoped<IUserClaimsPrincipalFactory<Utilizador>, AppUserClaimsPrincipalFactory>();


var app = builder.Build();

// Inicialização de roles e usuários

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Inicialização de roles e usuários
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