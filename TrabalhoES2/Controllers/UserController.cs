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
    // Este controller só pode ser acedido por Admin ou UserManager
    [Authorize(Roles = "Admin,UserManager")]
    public class UserController : Controller
    {
        // Variáveis para aceder ao gestor de utilizadores e à base de dados
        private readonly UserManager<Utilizador> _userManager;
        private readonly projetoPraticoDbContext _context;

        // Construtor - recebe as dependências necessárias
        public UserController(UserManager<Utilizador> userManager, projetoPraticoDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: User - Página principal que lista os utilizadores
        public async Task<IActionResult> Index(bool includeDeleted = false)
        {
            // Começamos por pesquisar todos os utilizadores da base de dados
            IQueryable<Utilizador> query = _context.Utilizadors;
            
            // Se não quisermos mostrar os utilizadores apagados, filtramos
            if (!includeDeleted)
            {
                query = query.Where(u => !u.IsDeleted);
            }

            // AQUI É A PARTE IMPORTANTE: UserManager só vê Clientes, Admin vê todos
            // Verificamos se o utilizador logado é UserManager (mas não Admin) (como só pode estar aqui ou um ou outro)
            if (User.IsInRole("UserManager") && !User.IsInRole("Admin"))
            {
                // Se for UserManager, só mostra utilizadores do tipo Cliente
                query = query.Where(u => u.TpUtilizador == Utilizador.TipoUtilizador.Cliente);
            }
            // Executamos a query e obtemos a lista final
            var users = await query.ToListAsync();
            
            // Passamos informação para a view saber se deve mostrar apagados
            ViewBag.IncludeDeleted = includeDeleted;
            
            return View(users);
        }

        // GET: User/Details/5 - Mostra detalhes de um utilizador específico
        public async Task<IActionResult> Details(int? id)
        {
            // Verificação básica - se não há ID, não encontramos nada obviamente....
            if (id == null)
            {
                return NotFound();
            }
            // Procuramos o utilizador na base de dados
            var user = await _context.Utilizadors
                .FirstOrDefaultAsync(m => m.Id == id);
            
            // Se não encontramos, devolvemos erro 404 (not found)
            if (user == null)
            {
                return NotFound();
            }

            // UserManager só pode ver detalhes de Clientes
            if (User.IsInRole("UserManager") && !User.IsInRole("Admin"))
            {
                // Se por alguma razao conseguir chegar ao ponto de tentar ver um utilizador que não é Cliente, bloqueamos
                if (user.TpUtilizador != Utilizador.TipoUtilizador.Cliente)
                {
                    return Forbid("Não tem permissão para ver este utilizador.");
                }
            }
            // Passamos informação do tipo de utilizador logado para a view
            ViewBag.IsUserManager = User.IsInRole("UserManager") && !User.IsInRole("Admin");
            ViewBag.IsAdmin = User.IsInRole("Admin");

            return View(user);
        }

        // GET: User/Create - Formulário para criar novo utilizador
        public IActionResult Create()
        {
            // Aqui decidimos que tipos de utilizador podem ser criados
            // UserManager só pode criar Clientes, Admin pode criar qualquer tipo
            if (User.IsInRole("UserManager") && !User.IsInRole("Admin"))
            {
                // Para UserManager, só mostramos a opção "Cliente"
                ViewBag.TipoUtilizadorOptions = new SelectList(
                    new[] { Utilizador.TipoUtilizador.Cliente }, 
                    "");
            }
            else
            {
                // Para Admin, mostramos todos os tipos possíveis (enum completo)
                ViewBag.TipoUtilizadorOptions = new SelectList(
                    Enum.GetValues(typeof(Utilizador.TipoUtilizador))
                    .Cast<Utilizador.TipoUtilizador>(), 
                    "");
            }
                
            return View();
        }

        // POST: User/Create - Processa criação do utilizador
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string Nome, string Email, string Password, string ConfirmPassword, Utilizador.TipoUtilizador TipoUtilizador)
        {
            // Validação de segurança: UserManager só pode criar Clientes
            if (User.IsInRole("UserManager") && !User.IsInRole("Admin") && TipoUtilizador != Utilizador.TipoUtilizador.Cliente)
            {
                ModelState.AddModelError("", "UserManager só pode criar utilizadores do tipo Cliente.");
            }
            // Validações básicas dos campos obrigatórios
            if (string.IsNullOrEmpty(Nome) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ModelState.AddModelError("", "Todos os campos são obrigatórios.");
            }
            // Verificamos se as passwords coincidem
            else if (Password != ConfirmPassword)
            {
                ModelState.AddModelError("", "As passwords não coincidem.");
            }
            // Se tudo estiver válido, criamos o utilizador
            if (ModelState.IsValid)
            {
                // Criamos um novo objeto Utilizador com os dados fornecidos
                var user = new Utilizador
                {
                    UserName = Email,  // No .NET Identity, o username é geralmente o email
                    Email = Email,
                    Nome = Nome,
                    TpUtilizador = TipoUtilizador,
                    IsDeleted = false, // Utilizador novo não está apagado
                    IsBlocked = false  // Utilizador novo não está bloqueado
                };
                // Tentamos criar o utilizador no sistema Identity
                var result = await _userManager.CreateAsync(user, Password);
                if (result.Succeeded)
                {
                    // Adicionar utilizador ao role correspondente ao seu tipo
                    await _userManager.AddToRoleAsync(user, TipoUtilizador.ToString());
                    TempData["SuccessMessage"] = "Utilizador criado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                // Se houve erros, adicionamos à lista de erros do modelo
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Se chegamos aqui, algo falhou, então recarregamos o formulário
            // Temos de repor as opções do dropdown conforme o tipo de utilizador
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

        // GET: User/Edit/5 - Formulário para editar utilizador existente
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Procuramos o utilizador na base de dados
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

            // Preparamos as opções do dropdown conforme as permissões
            // UserManager só vê dropdown com Cliente, Admin vê todos os tipos
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

        // POST: User/Edit/5 - Processar edição do utilizador
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, string Nome, string Email, string? NewPassword, string? ConfirmNewPassword, Utilizador.TipoUtilizador TipoUtilizador)
        {
            // Determinamos se o utilizador é UserManager
            var isUserManager = User.IsInRole("UserManager") && !User.IsInRole("Admin");

            // Verificar se o utilizador pode ser editado 
            var existingUser = await _context.Utilizadors.FindAsync(Id);
            if (existingUser == null)
            {
                return NotFound();
            }

            // UserManager só pode editar Clientes - validação de segurança
            if (isUserManager && existingUser.TpUtilizador != Utilizador.TipoUtilizador.Cliente)
            {
                return Forbid("Não tem permissão para editar este utilizador.");
            }

            // UserManager não pode alterar o tipo de utilizador de Cliente para outra coisa
            if (isUserManager && TipoUtilizador != Utilizador.TipoUtilizador.Cliente)
            {
                ModelState.AddModelError("", "UserManager não pode alterar o tipo de utilizador.");
                ViewBag.TipoUtilizadorOptions = new SelectList(
                    new[] { Utilizador.TipoUtilizador.Cliente }, 
                    "");
                ViewBag.IsUserManager = isUserManager;
                return View(existingUser);
            }

            // Limpamos erros de validação para campos de password (são opcionais na edição)
            if (ModelState.ContainsKey("NewPassword"))
                ModelState.Remove("NewPassword");
            if (ModelState.ContainsKey("ConfirmNewPassword"))
                ModelState.Remove("ConfirmNewPassword");

            // Verificamos se as passwords coincidem quando ambas foram fornecidas
            if (!string.IsNullOrEmpty(NewPassword) && NewPassword != ConfirmNewPassword)
            {
                ModelState.AddModelError("", "As passwords não coincidem.");
                
                // Recarregamos as opções do dropdown
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

            // Se tudo está válido, procedemos com a atualização
            if (ModelState.IsValid)
            {
                try
                {
                    // Procuramos o utilizador novamente (por precaução)
                    var user = await _context.Set<Utilizador>().FindAsync(Id);
                    
                    if (user == null)
                    {
                        // Tentativa alternativa de procurar
                        user = await _context.Utilizadors.FirstOrDefaultAsync(u => u.Id == Id);
                        if (user == null)
                        {
                            return NotFound("Utilizador não encontrado!");
                        }
                    }


                    // Atualizamos as propriedades básicas do utilizador
                    user.Nome = Nome;
                    user.Email = Email;
                    user.UserName = Email;
                    user.NormalizedEmail = Email.ToUpper();
                    user.NormalizedUserName = Email.ToUpper();
                    user.TpUtilizador = TipoUtilizador;

                    // Marcamos o entity como modificado e guardamos
                    _context.Entry(user).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                    // Se uma nova password foi fornecida, atualizamos também
                    if (!string.IsNullOrEmpty(NewPassword))
                    {
                        // Procuramos o utilizador no sistema Identity
                        var identityUser = await _userManager.FindByIdAsync(Id.ToString());
                        if (identityUser != null)
                        {
                            // Geramos um token para reset de password e aplicamos a nova
                            var token = await _userManager.GeneratePasswordResetTokenAsync(identityUser);
                            var result = await _userManager.ResetPasswordAsync(identityUser, token, NewPassword);
                            
                            // Se houve erro na alteração da password
                            if (!result.Succeeded)
                            {
                                foreach (var error in result.Errors)
                                {
                                    ModelState.AddModelError(string.Empty, error.Description);
                                }
                                
                                // Recarregamos o formulário com erros
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
                    // Se chegamos aqui, tudo correu bem!
                    TempData["SuccessMessage"] = "Utilizador atualizado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Se houve algum erro inesperado, mostramos a mensagem
                    ModelState.AddModelError("", $"Erro ao atualizar o utilizador: {ex.Message}");
                }
            }

            // Se houve algum erro inesperado, mostramos a mensagem
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

        
        // GET: User/Delete/5 - IMPORTANTE: UserManager NÃO TEM ACESSO A ESTA FUNÇÃO o butão esta escondido e temos o authorize para evitar que o user manager mexa no que não deve 
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            // Procuramos o utilizador que queremos apagar
            var user = await _context.Utilizadors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: User/Delete/5 - IMPORTANTE: UserManager NÃO TEM ACESSO A ESTA FUNÇÃO
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Utilizadors.FindAsync(id);
            if (user != null)
            {
                // Fazemos "soft delete" - marcamos como apagado em vez de remover da BD
                user.IsDeleted = true;
                user.DeletedAt = DateTime.UtcNow;
                
                _context.Update(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Utilizador removido com sucesso!";
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: User/Block/5 - UserManager TEM ACESSO para bloquear Clientes
        public async Task<IActionResult> Block(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            // Procuramos o utilizador que queremos bloquear
            var user = await _context.Utilizadors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var isUserManager = User.IsInRole("UserManager") && !User.IsInRole("Admin");

            // UserManager só pode bloquear Clientes - validação de segurança
            if (isUserManager && user.TpUtilizador != Utilizador.TipoUtilizador.Cliente)
            {
                return Forbid("Não tem permissão para bloquear este utilizador.");
            }
            // Passamos informação para a view
            ViewBag.IsUserManager = isUserManager;
            ViewBag.IsAdmin = User.IsInRole("Admin");

            return View(user);
        }

        // POST: User/Block/5 - UserManager TEM ACESSO para bloquear Clientes
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
            
            // UserManager só pode bloquear Clientes - validação de segurança
            if (isUserManager && user.TpUtilizador != Utilizador.TipoUtilizador.Cliente)
            {
                return Forbid("Não tem permissão para bloquear este utilizador.");
            }

            // Marcamos o utilizador como bloqueado
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

            // Procuramos o utilizador que queremos desbloquear
            var user = await _context.Utilizadors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var isUserManager = User.IsInRole("UserManager") && !User.IsInRole("Admin");

            // UserManager só pode desbloquear Clientes - validação de segurança
            if (isUserManager && user.TpUtilizador != Utilizador.TipoUtilizador.Cliente)
            {
                return Forbid("Não tem permissão para desbloquear este utilizador.");
            }

            ViewBag.IsUserManager = isUserManager;
            ViewBag.IsAdmin = User.IsInRole("Admin");

            return View(user);
        }

        // POST: User/Unblock/5 - UserManager TEM ACESSO para desbloquear Clientes
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

            // UserManager só pode desbloquear Clientes - validação de segurança
            if (isUserManager && user.TpUtilizador != Utilizador.TipoUtilizador.Cliente)
            {
                return Forbid("Não tem permissão para desbloquear este utilizador.");
            }
            // Removemos o bloqueio do utilizador
            user.IsBlocked = false;
            user.UnblockedAt = DateTime.UtcNow;
            
            _context.Update(user);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Utilizador desbloqueado com sucesso!";
            
            return RedirectToAction(nameof(Index));
        }
        
        // Método auxiliar para verificar se um utilizador existe
        private bool UserExists(int id)
        {
            return _context.Utilizadors.Any(e => e.Id == id);
        }
    }
}