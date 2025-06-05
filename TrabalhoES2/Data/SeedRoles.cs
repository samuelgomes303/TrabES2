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

    }
}

