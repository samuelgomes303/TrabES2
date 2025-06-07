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
    [Authorize(Roles = "Admin,UserManager")]
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
        public async Task<IActionResult> Index(bool includeDeleted = false)
        {
            // Listar utilizadores, filtrando os deletados se necessário
            IQueryable<Utilizador> query = _context.Utilizadors;
            
            if (!includeDeleted)
            {
                query = query.Where(u => !u.IsDeleted);
            }

            // UserManager só vê Clientes, Admin vê todos
            if (User.IsInRole("UserManager") && !User.IsInRole("Admin"))
            {
                query = query.Where(u => u.TpUtilizador == Utilizador.TipoUtilizador.Cliente);
            }
            
            var users = await query.ToListAsync();
            
            // Passar a flag para a view
            ViewBag.IncludeDeleted = includeDeleted;
            
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

            // UserManager só pode ver detalhes de Clientes
            if (User.IsInRole("UserManager") && !User.IsInRole("Admin"))
            {
                if (user.TpUtilizador != Utilizador.TipoUtilizador.Cliente)
                {
                    return Forbid("Não tem permissão para ver este utilizador.");
                }
            }

            ViewBag.IsUserManager = User.IsInRole("UserManager") && !User.IsInRole("Admin");
            ViewBag.IsAdmin = User.IsInRole("Admin");

            return View(user);
        }

        // GET: User/Create
        public IActionResult Create()
        {
            // UserManager só pode criar Clientes, Admin pode criar qualquer tipo
            if (User.IsInRole("UserManager") && !User.IsInRole("Admin"))
            {
                ViewBag.TipoUtilizadorOptions = new SelectList(
                    new[] { Utilizador.TipoUtilizador.Cliente }, 
                    "");
            }
            else
            {
                ViewBag.TipoUtilizadorOptions = new SelectList(
                    Enum.GetValues(typeof(Utilizador.TipoUtilizador))
                    .Cast<Utilizador.TipoUtilizador>(), 
                    "");
            }
                
            return View();
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string Nome, string Email, string Password, string ConfirmPassword, Utilizador.TipoUtilizador TipoUtilizador)
        {
            // UserManager só pode criar Clientes
            if (User.IsInRole("UserManager") && !User.IsInRole("Admin") && TipoUtilizador != Utilizador.TipoUtilizador.Cliente)
            {
                ModelState.AddModelError("", "UserManager só pode criar utilizadores do tipo Cliente.");
            }

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
                    TpUtilizador = TipoUtilizador,
                    IsDeleted = false,
                    IsBlocked = false
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
            if (User.IsInRole("UserManager") && !User.IsInRole("Admin"))
            {
                ViewBag.TipoUtilizadorOptions = new SelectList(
                    new[] { Utilizador.TipoUtilizador.Cliente }, 
                    "");
            }
            else
            {
                ViewBag.TipoUtilizadorOptions = new SelectList(
                    Enum.GetValues(typeof(Utilizador.TipoUtilizador))
                    .Cast<Utilizador.TipoUtilizador>(),
                    "");
            }
                
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

            // UserManager só pode editar Clientes
            if (User.IsInRole("UserManager") && !User.IsInRole("Admin") && user.TpUtilizador != Utilizador.TipoUtilizador.Cliente)
            {
                return Forbid("Não tem permissão para editar este utilizador.");
            }

            // UserManager só vê dropdown com Cliente, Admin vê todos
            if (User.IsInRole("UserManager") && !User.IsInRole("Admin"))
            {
                ViewBag.TipoUtilizadorOptions = new SelectList(
                    new[] { Utilizador.TipoUtilizador.Cliente }, 
                    "");
            }
            else
            {
                ViewBag.TipoUtilizadorOptions = new SelectList(
                    Enum.GetValues(typeof(Utilizador.TipoUtilizador))
                    .Cast<Utilizador.TipoUtilizador>(), 
                    "");
            }
                
            return View(user);
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, string Nome, string Email, string? NewPassword, string? ConfirmNewPassword, Utilizador.TipoUtilizador TipoUtilizador)
        {
            var isUserManager = User.IsInRole("UserManager") && !User.IsInRole("Admin");

            // Verificar se o utilizador pode ser editado
            var existingUser = await _context.Utilizadors.FindAsync(Id);
            if (existingUser == null)
            {
                return NotFound();
            }

            // UserManager só pode editar Clientes
            if (isUserManager && existingUser.TpUtilizador != Utilizador.TipoUtilizador.Cliente)
            {
                return Forbid("Não tem permissão para editar este utilizador.");
            }

            // UserManager não pode alterar o tipo de utilizador
            if (isUserManager && TipoUtilizador != Utilizador.TipoUtilizador.Cliente)
            {
                ModelState.AddModelError("", "UserManager não pode alterar o tipo de utilizador.");
                ViewBag.TipoUtilizadorOptions = new SelectList(
                    new[] { Utilizador.TipoUtilizador.Cliente }, 
                    "");
                ViewBag.IsUserManager = isUserManager;
                return View(existingUser);
            }

            // Remover quaisquer erros de validação para os campos de senha
            if (ModelState.ContainsKey("NewPassword"))
                ModelState.Remove("NewPassword");
            if (ModelState.ContainsKey("ConfirmNewPassword"))
                ModelState.Remove("ConfirmNewPassword");

            // Verificar apenas se as senhas coincidem quando ambas foram fornecidas
            if (!string.IsNullOrEmpty(NewPassword) && NewPassword != ConfirmNewPassword)
            {
                ModelState.AddModelError("", "As passwords não coincidem.");
                
                if (isUserManager)
                {
                    ViewBag.TipoUtilizadorOptions = new SelectList(
                        new[] { Utilizador.TipoUtilizador.Cliente }, 
                        "");
                }
                else
                {
                    ViewBag.TipoUtilizadorOptions = new SelectList(
                        Enum.GetValues(typeof(Utilizador.TipoUtilizador))
                        .Cast<Utilizador.TipoUtilizador>(), 
                        "");
                }

                ViewBag.IsUserManager = isUserManager;
                return View(await _context.Utilizadors.FindAsync(Id));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _context.Set<Utilizador>().FindAsync(Id);
                    
                    if (user == null)
                    {
                        user = await _context.Utilizadors.FirstOrDefaultAsync(u => u.Id == Id);
                        if (user == null)
                        {
                            return NotFound("Utilizador não encontrado!");
                        }
                    }

                    // Atualizar propriedades
                    user.Nome = Nome;
                    user.Email = Email;
                    user.UserName = Email;
                    user.NormalizedEmail = Email.ToUpper();
                    user.NormalizedUserName = Email.ToUpper();
                    user.TpUtilizador = TipoUtilizador;

                    _context.Entry(user).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

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
                                
                                if (isUserManager)
                                {
                                    ViewBag.TipoUtilizadorOptions = new SelectList(
                                        new[] { Utilizador.TipoUtilizador.Cliente }, 
                                        "");
                                }
                                else
                                {
                                    ViewBag.TipoUtilizadorOptions = new SelectList(
                                        Enum.GetValues(typeof(Utilizador.TipoUtilizador))
                                        .Cast<Utilizador.TipoUtilizador>(), 
                                        "");
                                }

                                ViewBag.IsUserManager = isUserManager;
                                return View(user);
                            }
                        }
                    }

                    TempData["SuccessMessage"] = "Utilizador atualizado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Erro ao atualizar o utilizador: {ex.Message}");
                }
            }

            // Se chegamos aqui, algo falhou
            if (isUserManager)
            {
                ViewBag.TipoUtilizadorOptions = new SelectList(
                    new[] { Utilizador.TipoUtilizador.Cliente }, 
                    "");
            }
            else
            {
                ViewBag.TipoUtilizadorOptions = new SelectList(
                    Enum.GetValues(typeof(Utilizador.TipoUtilizador))
                    .Cast<Utilizador.TipoUtilizador>(), 
                    "");
            }

            ViewBag.IsUserManager = isUserManager;
            ViewBag.IsAdmin = User.IsInRole("Admin");
                
            var userToReturn = await _context.Utilizadors.FindAsync(Id);
            return View(userToReturn ?? new Utilizador { Id = Id });
        }

        // GET: User/Delete/5 - UserManager NÃO TEM ACESSO
        [Authorize(Roles = "Admin")]
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

        // POST: User/Delete/5 - UserManager NÃO TEM ACESSO
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Utilizadors.FindAsync(id);
            if (user != null)
            {
                // Soft delete em vez de remover definitivamente
                user.IsDeleted = true;
                user.DeletedAt = DateTime.UtcNow;
                
                _context.Update(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Utilizador removido com sucesso!";
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: User/Block/5 - UserManager TEM ACESSO para Clientes
        public async Task<IActionResult> Block(int? id)
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

            var isUserManager = User.IsInRole("UserManager") && !User.IsInRole("Admin");

            // UserManager só pode bloquear Clientes
            if (isUserManager && user.TpUtilizador != Utilizador.TipoUtilizador.Cliente)
            {
                return Forbid("Não tem permissão para bloquear este utilizador.");
            }

            ViewBag.IsUserManager = isUserManager;
            ViewBag.IsAdmin = User.IsInRole("Admin");

            return View(user);
        }

        // POST: User/Block/5 - UserManager TEM ACESSO para Clientes
        [HttpPost, ActionName("Block")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockConfirmed(int id)
        {
            var user = await _context.Utilizadors.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var isUserManager = User.IsInRole("UserManager") && !User.IsInRole("Admin");

            // UserManager só pode bloquear Clientes
            if (isUserManager && user.TpUtilizador != Utilizador.TipoUtilizador.Cliente)
            {
                return Forbid("Não tem permissão para bloquear este utilizador.");
            }

            user.IsBlocked = true;
            user.BlockedAt = DateTime.UtcNow;
            
            _context.Update(user);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Utilizador bloqueado com sucesso!";
            
            return RedirectToAction(nameof(Index));
        }

        // GET: User/Unblock/5 - UserManager TEM ACESSO para Clientes
        public async Task<IActionResult> Unblock(int? id)
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

            var isUserManager = User.IsInRole("UserManager") && !User.IsInRole("Admin");

            // UserManager só pode desbloquear Clientes
            if (isUserManager && user.TpUtilizador != Utilizador.TipoUtilizador.Cliente)
            {
                return Forbid("Não tem permissão para desbloquear este utilizador.");
            }

            ViewBag.IsUserManager = isUserManager;
            ViewBag.IsAdmin = User.IsInRole("Admin");

            return View(user);
        }

        // POST: User/Unblock/5 - UserManager TEM ACESSO para Clientes
        [HttpPost, ActionName("Unblock")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnblockConfirmed(int id)
        {
            var user = await _context.Utilizadors.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var isUserManager = User.IsInRole("UserManager") && !User.IsInRole("Admin");

            // UserManager só pode desbloquear Clientes
            if (isUserManager && user.TpUtilizador != Utilizador.TipoUtilizador.Cliente)
            {
                return Forbid("Não tem permissão para desbloquear este utilizador.");
            }

            user.IsBlocked = false;
            user.UnblockedAt = DateTime.UtcNow;
            
            _context.Update(user);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Utilizador desbloqueado com sucesso!";
            
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Utilizadors.Any(e => e.Id == id);
        }
    }
}