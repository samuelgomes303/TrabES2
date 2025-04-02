using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using TrabalhoES2.Models;

public static class SeedRoles
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, RoleManager<IdentityRole<int>> roleManager, UserManager<Utilizador> userManager)
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

        // Criar o utilizador admin se não existir
        var adminUser = await userManager.FindByEmailAsync("admin@example.com");

        if (adminUser == null)
        {
            adminUser = new Utilizador
            {
                UserName = "adminUser",
                Email = "admin@example.com",
                Nome = "Joao Santos", // Certifique-se de que a propriedade Nome está definida na sua classe Utilizador
                EmailConfirmed = true,
                TpUtilizador = Utilizador.TipoUtilizador.Admin // Define a role como Admin
            };

            // Criar o usuário admin com senha forte
            var createAdminResult = await userManager.CreateAsync(adminUser, "SenhaForte123!");

            if (createAdminResult.Succeeded)
            {
                // Atribuir a role 'Admin' ao utilizador admin
                await userManager.AddToRoleAsync(adminUser, Utilizador.TipoUtilizador.Admin.ToString());
            }
        }
    }
}
