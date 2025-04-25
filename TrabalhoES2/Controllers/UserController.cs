using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrabalhoES2.Models;

namespace TrabalhoES2.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly projetoPraticoDbContext _context;

        public UserController(UserManager<Utilizador> userManager, projetoPraticoDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: User
        public async Task<IActionResult> Index()
        {
            // Listar todos os utilizadores
            var users = await _context.Utilizadors.ToListAsync();
            return View(users);
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Utilizadors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: User/Create
        public IActionResult Create()
        {
            // Usar ViewBag para passar os dados para a view
            ViewBag.TipoUtilizadorOptions = new SelectList(
                Enum.GetValues(typeof(Utilizador.TipoUtilizador))
                .Cast<Utilizador.TipoUtilizador>(), 
                "");
                
            return View();
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string Nome, string Email, string Password, string ConfirmPassword, Utilizador.TipoUtilizador TipoUtilizador)
        {
            if (string.IsNullOrEmpty(Nome) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ModelState.AddModelError("", "Todos os campos são obrigatórios.");
            }
            else if (Password != ConfirmPassword)
            {
                ModelState.AddModelError("", "As passwords não coincidem.");
            }

            if (ModelState.IsValid)
            {
                var user = new Utilizador
                {
                    UserName = Email,
                    Email = Email,
                    Nome = Nome,
                    TpUtilizador = TipoUtilizador
                };

                var result = await _userManager.CreateAsync(user, Password);
                if (result.Succeeded)
                {
                    // Adicionar utilizador ao role correspondente ao seu tipo
                    await _userManager.AddToRoleAsync(user, TipoUtilizador.ToString());
                    TempData["SuccessMessage"] = "Utilizador criado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Se chegamos aqui, algo falhou, recarregar form
            ViewBag.TipoUtilizadorOptions = new SelectList(
                Enum.GetValues(typeof(Utilizador.TipoUtilizador))
                .Cast<Utilizador.TipoUtilizador>(),
                "");
                
            return View();
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Utilizadors.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Usar ViewBag para passar os dados para a view
            ViewBag.TipoUtilizadorOptions = new SelectList(
                Enum.GetValues(typeof(Utilizador.TipoUtilizador))
                .Cast<Utilizador.TipoUtilizador>(), 
                "");
                
            return View(user);
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, string Nome, string Email, string? NewPassword, string? ConfirmNewPassword, Utilizador.TipoUtilizador TipoUtilizador)
        {
            Console.WriteLine($"Edit POST chamado com Id={Id}, Nome={Nome}, TipoUtilizador={TipoUtilizador}");

            // Remover quaisquer erros de validação para os campos de senha
            if (ModelState.ContainsKey("NewPassword"))
                ModelState.Remove("NewPassword");
            if (ModelState.ContainsKey("ConfirmNewPassword"))
                ModelState.Remove("ConfirmNewPassword");

            // Verificar apenas se as senhas coincidem quando ambas foram fornecidas
            if (!string.IsNullOrEmpty(NewPassword) && NewPassword != ConfirmNewPassword)
            {
                ModelState.AddModelError("", "As passwords não coincidem.");
                
                ViewBag.TipoUtilizadorOptions = new SelectList(
                    Enum.GetValues(typeof(Utilizador.TipoUtilizador))
                    .Cast<Utilizador.TipoUtilizador>(), 
                    "");
                    
                return View(await _context.Utilizadors.FindAsync(Id));
            }

            // Verificar se chegamos aqui com dados válidos
            ViewBag.DebugMessage = $"Recebido: Id={Id}, Nome={Nome}, Email={Email}, TipoUtilizador={TipoUtilizador}";

            if (ModelState.IsValid)
            {
                try
                {
                    // Tentar obter o utilizador pela chave primária
                    var user = await _context.Set<Utilizador>().FindAsync(Id);
                    
                    if (user == null)
                    {
                        // Se não encontrou pela chave primária, tentar por Id
                        user = await _context.Utilizadors.FirstOrDefaultAsync(u => u.Id == Id);
                        if (user == null)
                        {
                            ViewBag.DebugMessage += " | ERRO: Utilizador não encontrado!";
                            return View();
                        }
                    }

                    ViewBag.DebugMessage += $" | User encontrado: {user.Nome}, {user.Email}, {user.TpUtilizador}";

                    // Salvar valores originais
                    var nomeOriginal = user.Nome;
                    var emailOriginal = user.Email;
                    var tipoUtilizadorOriginal = user.TpUtilizador;

                    // Atualizar propriedades
                    user.Nome = Nome;
                    user.Email = Email;
                    user.UserName = Email;
                    user.NormalizedEmail = Email.ToUpper();
                    user.NormalizedUserName = Email.ToUpper();
                    user.TpUtilizador = TipoUtilizador;

                    try {
                        _context.Entry(user).State = EntityState.Modified;
                        await _context.SaveChangesAsync();
                        ViewBag.DebugMessage += " | Dados salvos com sucesso!";
                    }
                    catch (Exception ex) {
                        ViewBag.DebugMessage += $" | ERRO ao salvar: {ex.Message}";
                        if (ex.InnerException != null) {
                            ViewBag.DebugMessage += $" | Inner: {ex.InnerException.Message}";
                        }
                        
                        // Restaurar valores originais para o formulário
                        user.Nome = nomeOriginal;
                        user.Email = emailOriginal;
                        user.TpUtilizador = tipoUtilizadorOriginal;
                        
                        ViewBag.TipoUtilizadorOptions = new SelectList(
                            Enum.GetValues(typeof(Utilizador.TipoUtilizador))
                            .Cast<Utilizador.TipoUtilizador>(), 
                            "");
                        
                        return View(user);
                    }

                    // Atualizar senha se uma nova foi fornecida
                    if (!string.IsNullOrEmpty(NewPassword))
                    {
                        var identityUser = await _userManager.FindByIdAsync(Id.ToString());
                        if (identityUser != null)
                        {
                            var token = await _userManager.GeneratePasswordResetTokenAsync(identityUser);
                            var result = await _userManager.ResetPasswordAsync(identityUser, token, NewPassword);
                            
                            if (!result.Succeeded)
                            {
                                foreach (var error in result.Errors)
                                {
                                    ModelState.AddModelError(string.Empty, error.Description);
                                }
                                
                                ViewBag.TipoUtilizadorOptions = new SelectList(
                                    Enum.GetValues(typeof(Utilizador.TipoUtilizador))
                                    .Cast<Utilizador.TipoUtilizador>(), 
                                    "");
                                    
                                return View(user);
                            }
                        }
                    }

                    // Se chegou aqui, a atualização básica foi bem sucedida
                    TempData["SuccessMessage"] = "Utilizador atualizado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ViewBag.DebugMessage += $" | Exceção geral: {ex.Message}";
                    ModelState.AddModelError("", $"Erro ao atualizar o utilizador: {ex.Message}");
                }
            }
            else
            {
                ViewBag.DebugMessage += " | ModelState inválido: ";
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    ViewBag.DebugMessage += error.ErrorMessage + "; ";
                }
            }

            // Se chegamos aqui, algo falhou
            ViewBag.TipoUtilizadorOptions = new SelectList(
                Enum.GetValues(typeof(Utilizador.TipoUtilizador))
                .Cast<Utilizador.TipoUtilizador>(), 
                "");
                
            var userToReturn = await _context.Utilizadors.FindAsync(Id);
            return View(userToReturn ?? new Utilizador { Id = Id });
        }

        // GET: User/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Utilizadors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Utilizadors.FindAsync(id);
            if (user != null)
            {
                _context.Utilizadors.Remove(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Utilizador eliminado com sucesso!";
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Utilizadors.Any(e => e.Id == id);
        }
    }
}