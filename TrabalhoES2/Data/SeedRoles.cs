using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using TrabalhoES2.Models;

public static class SeedRoles
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, RoleManager<IdentityRole<int>> roleManager,
        UserManager<Utilizador> userManager)
    {
        // Criar roles a partir do enum TipoUtilizador
        var enumValues = Enum.GetValues(typeof(Utilizador.TipoUtilizador)).Cast<Utilizador.TipoUtilizador>();

        foreach (var roleName in enumValues)
        {
            // Verifica se a role já existe, caso contrário, cria a role
            var roleExist = await roleManager.RoleExistsAsync(roleName.ToString());
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole<int>(roleName.ToString()));
            }
        }


        var adminUser = new Utilizador
        {
            UserName = "admin@example.com",
            Email = "admin@example.com",
            Nome = "Joao Santos", // Certifique-se de que a propriedade Nome está definida na sua classe Utilizador
            EmailConfirmed = true,
            TpUtilizador = Utilizador.TipoUtilizador.Admin // Define a role como Admin
        };
        
        var createAdminResul = await userManager.FindByEmailAsync(adminUser.Email);
        if (createAdminResul == null)
        {
            await userManager.CreateAsync(adminUser, "Admin@123");
            await userManager.AddToRoleAsync(adminUser, Utilizador.TipoUtilizador.Admin.ToString());
        }
        var existingUsers = userManager.Users.ToList();
        foreach (var u in existingUsers)
        {
            if (!await userManager.IsInRoleAsync(u, "Cliente") && u.TpUtilizador == Utilizador.TipoUtilizador.Cliente)
            {
                await userManager.AddToRoleAsync(u, "Cliente");
            }
        }

        // Seed Selenium test user (tf@ipvc.pt) with password Piruças123# and a default asset
        var seleniumUser = await userManager.FindByEmailAsync("tf@ipvc.pt");
        if (seleniumUser == null)
        {
            seleniumUser = new Utilizador
            {
                UserName = "tf@ipvc.pt",
                Email = "tf@ipvc.pt",
                Nome = "Test User",
                EmailConfirmed = true,
                TpUtilizador = Utilizador.TipoUtilizador.Cliente,
                SecurityStamp = Guid.NewGuid().ToString(),
                IsBlocked = false,
                IsDeleted = false
            };
            var createResult = await userManager.CreateAsync(seleniumUser, "Piruças123#");
            Console.WriteLine($"[SEED] Created Selenium user: {createResult.Succeeded}");
            var addRoleResult = await userManager.AddToRoleAsync(seleniumUser, "Cliente");
            Console.WriteLine($"[SEED] Added Selenium user to Cliente role: {addRoleResult.Succeeded}");
        }
        else
        {
            // Always reset password and confirm email for Selenium
            // Remove and re-add password to guarantee hash is up to date
            var removeResult = await userManager.RemovePasswordAsync(seleniumUser);
            if (removeResult.Succeeded)
            {
                var addResult = await userManager.AddPasswordAsync(seleniumUser, "Piruças123#");
                Console.WriteLine($"[SEED] Re-added Selenium user password: {addResult.Succeeded}");
            }
            else
            {
                // fallback to reset token if remove fails (e.g. no password set)
                var token = await userManager.GeneratePasswordResetTokenAsync(seleniumUser);
                var resetResult = await userManager.ResetPasswordAsync(seleniumUser, token, "Piruças123#");
                Console.WriteLine($"[SEED] Reset Selenium user password: {resetResult.Succeeded}");
            }
            seleniumUser.EmailConfirmed = true;
            seleniumUser.IsBlocked = false;
            seleniumUser.IsDeleted = false;
            seleniumUser.UnblockedAt = DateTime.UtcNow;
            seleniumUser.BlockedAt = null;
            seleniumUser.DeletedAt = null;
            var updateResult = await userManager.UpdateAsync(seleniumUser);
            Console.WriteLine($"[SEED] Updated Selenium user: {updateResult.Succeeded}");
            // Ensure user is in Cliente role
            if (!await userManager.IsInRoleAsync(seleniumUser, "Cliente"))
            {
                var addRoleResult = await userManager.AddToRoleAsync(seleniumUser, "Cliente");
                Console.WriteLine($"[SEED] Added Selenium user to Cliente role: {addRoleResult.Succeeded}");
            }
        }
        // Ensure user has a carteira and at least one asset
        var db = serviceProvider.GetRequiredService<TrabalhoES2.Models.projetoPraticoDbContext>();
        var carteira = db.Carteiras.FirstOrDefault(c => c.UtilizadorId == seleniumUser.Id);
        if (carteira == null)
        {
            carteira = new TrabalhoES2.Models.Carteira
            {
                Nome = "Carteira Selenium",
                UtilizadorId = seleniumUser.Id
            };
            db.Carteiras.Add(carteira);
            db.SaveChanges();
            Console.WriteLine("[SEED] Created Selenium carteira");
        }
        if (!db.Ativofinanceiros.Any(a => a.CarteiraId == carteira.CarteiraId))
        {
            var ativo = new TrabalhoES2.Models.Ativofinanceiro
            {
                CarteiraId = carteira.CarteiraId,
                Datainicio = DateOnly.FromDateTime(DateTime.Now.AddMonths(-2)),
                Duracaomeses = 12,
                Percimposto = 0.28m
            };
            db.Ativofinanceiros.Add(ativo);
            db.SaveChanges();
            db.Depositoprazos.Add(new TrabalhoES2.Models.Depositoprazo
            {
                AtivofinanceiroId = ativo.AtivofinanceiroId,
                BancoId = 1,
                Nrconta = "123456",
                Titular = "Test User",
                Taxajuroanual = 1.5m,
                Valorinicial = 1000m,
                Valoratual = 1000m
            });
            db.SaveChanges();
            Console.WriteLine("[SEED] Created Selenium asset and depositoprazo");
        }
        // --- Ensure Selenium test user is always unblocked and undeleted ---
        seleniumUser = await userManager.FindByEmailAsync("tf@ipvc.pt");
        if (seleniumUser != null)
        {
            bool changed = false;
            if (seleniumUser.IsBlocked)
            {
                seleniumUser.IsBlocked = false;
                seleniumUser.UnblockedAt = DateTime.UtcNow;
                seleniumUser.BlockedAt = null;
                changed = true;
            }
            if (seleniumUser.IsDeleted)
            {
                seleniumUser.IsDeleted = false;
                seleniumUser.DeletedAt = null;
                changed = true;
            }
            if (changed)
            {
                var updateResult = await userManager.UpdateAsync(seleniumUser);
                Console.WriteLine($"[SEED] Unblocked/undeleted Selenium user: {updateResult.Succeeded}");
            }
            // Log final status
            Console.WriteLine($"[SEED] Selenium user status: Blocked={seleniumUser.IsBlocked}, Deleted={seleniumUser.IsDeleted}, EmailConfirmed={seleniumUser.EmailConfirmed}");
        }
    }
}

