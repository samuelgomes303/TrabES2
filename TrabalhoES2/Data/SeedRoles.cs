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
        try
        {
            // Criação dos Roles se não existirem
            Console.WriteLine("Iniciando a criação de roles...");
            string[] roleNames = new string[] { "Admin", "User", "Cliente" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    var role = new IdentityRole(roleName);
                    await roleManager.CreateAsync(role);
                    Console.WriteLine($"Role {roleName} criada.");
                }
                else
                {
                    Console.WriteLine($"Role {roleName} já existe.");
                }
            }

            // Criando o usuário Admin se ele não existir
            string adminEmail = "admin@ipvc.pt";
            string adminPassword = "Admin@123";
            string adminNome = "Administrador";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                Console.WriteLine("Criando o usuário admin...");

                // Criar o usuário admin
                adminUser = new IdentityUser()
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                };

                var userResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (userResult.Succeeded)
                {
                    // Adicionar o usuário ao Role Admin
                    await userManager.AddToRoleAsync(adminUser, "Admin");

                    // Criar o registro correspondente na tabela Utilizador
                    var adminUtilizador = new Utilizador
                    {
                        Nome = adminNome,
                        Email = adminEmail,
                        TpUtilizador = Utilizador.TipoUtilizador.Admin,
                        Password = adminPassword,
                        IdentityUserId = adminUser.Id
                    };

                    // Verificar se o Utilizador já existe antes de criar
                    var existingUtilizador = await context.Utilizadors
                        .FirstOrDefaultAsync(u => u.IdentityUserId == adminUser.Id);
                    if (existingUtilizador == null)
                    {
                        context.Utilizadors.Add(adminUtilizador);
                        await context.SaveChangesAsync();
                        Console.WriteLine($"Admin {adminNome} adicionado na tabela 'Utilizador'.");
                    }
                    else
                    {
                        Console.WriteLine("O registro de 'Utilizador' já existe para o usuário admin.");
                    }
                }
                else
                {
                    foreach (var error in userResult.Errors)
                    {
                        Console.WriteLine($"Erro ao criar o usuário: {error.Description}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Usuário admin com o email {adminEmail} já existe.");
                // Caso o usuário já exista, verificamos a tabela Utilizador
                var existingUtilizador = await context.Utilizadors
                    .FirstOrDefaultAsync(u => u.IdentityUserId == adminUser.Id);
                if (existingUtilizador == null)
                {
                    // Se o registro não existir, criamos o registro na tabela Utilizador
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
                    Console.WriteLine($"Admin {adminNome} adicionado na tabela 'Utilizador'.");
                }
                else
                {
                    Console.WriteLine("O registro de 'Utilizador' já existe para o usuário admin.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao inicializar roles e usuários: {ex.Message}");
        }
    }
}