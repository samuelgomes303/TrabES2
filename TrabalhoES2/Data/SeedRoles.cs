using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using TrabalhoES2.Models;

public class SeedRoles
{
    public static async Task InitializeRoles(IServiceProvider serviceProvider, RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, projetoPraticoDbContext context)
    {
        //valores do enum TipoUtilizador para string
        var roles = Enum.GetNames(typeof(Utilizador.TipoUtilizador)).ToList();

        //cria os roles mediante o enum de TipoUtilizador
        foreach (var roleName in roles)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
        
        //criar admin caso nao exista
        string adminNome = "admin";
        string adminEmail = "admin@ipvc.pt";
        string adminPassword = "adm$N1234";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new IdentityUser()
            {
                UserName = adminNome,
                Email = adminEmail,
                EmailConfirmed = true,
            }; 
            
            var userResult = await userManager.CreateAsync(adminUser, adminPassword);
            
            if (userResult.Succeeded)
            {
                // 3️⃣ Adicionar o usuário ao role "Admin"
                await userManager.AddToRoleAsync(adminUser, Utilizador.TipoUtilizador.Admin.ToString());

                // 4️⃣ Criar um registro correspondente na tabela "Utilizador"
                var adminUtilizador = new Utilizador
                {
                    
                    Nome = adminNome, 
                    Email = adminEmail,
                    TpUtilizador = Utilizador.TipoUtilizador.Admin,
                    Password = adminPassword,
                    IdentityUserId = adminUser.Id
                };

                context.Utilizadors.Add(adminUtilizador);
                await context.SaveChangesAsync();
            } else {
                var logger = serviceProvider.GetRequiredService<ILogger<SeedRoles>>();
                
                foreach (var error in userResult.Errors)
                { 
                    logger.LogError($"Erro ao criar o usuário: {error.Description}");
                }
                throw new InvalidOperationException("Falha ao criar o usuário administrador.");
            }
        }

    }
}