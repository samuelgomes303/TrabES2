using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrabalhoES2.Models;
using TrabalhoES2.utils;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TrabalhoES2.Controllers
{
    [Authorize]
    public class CarteiraController : Controller
    {
        private readonly projetoPraticoDbContext _context;

        public CarteiraController(projetoPraticoDbContext context)
        {
            _context = context;
        }   

        // GET: Carteira
        // Substitua o método Index por este
    public async Task<IActionResult> Index()
    {
        // Obter o ID do utilizador atual
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        
        // Verificar se utilizador existe
        var utilizador = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (utilizador == null)
        {
            return NotFound("Utilizador não encontrado.");
        }

        // Verificar se o utilizador já tem uma carteira, se não, criar uma
        var carteira = await _context.Carteiras
            .Include(c => c.Ativofinanceiros)
            .FirstOrDefaultAsync(c => c.UtilizadorId == userId);

        if (carteira == null)
        {
            // Criar uma nova carteira para o utilizador
            carteira = new Carteira
            {
                Nome = "Carteira Principal",
                UtilizadorId = userId
            };
            _context.Carteiras.Add(carteira);
            await _context.SaveChangesAsync();
            return View(carteira);
        }

        // Carregar explicitamente todos os ativos e seus relacionamentos
        var ativos = await _context.Ativofinanceiros
            .Where(a => a.CarteiraId == carteira.CarteiraId)
            .ToListAsync();

        // Limpar a lista de ativos da carteira para recarregar
        carteira.Ativofinanceiros.Clear();
        
        foreach (var ativo in ativos)
        {
            // Carregar depósito a prazo e seu banco
            var deposito = await _context.Depositoprazos
                .Include(d => d.Banco)
                .FirstOrDefaultAsync(d => d.AtivofinanceiroId == ativo.AtivofinanceiroId);
            
            if (deposito != null)
            {
                ativo.Depositoprazo = deposito;
            }
            
            // Carregar fundo de investimento e seu banco
            var fundo = await _context.Fundoinvestimentos
                .Include(f => f.Banco)
                .FirstOrDefaultAsync(f => f.AtivofinanceiroId == ativo.AtivofinanceiroId);
            
            if (fundo != null)
            {
                ativo.Fundoinvestimento = fundo;
            }
            
            // Carregar imóvel arrendado e seu banco
            var imovel = await _context.Imovelarrendados
                .Include(i => i.Banco)
                .FirstOrDefaultAsync(i => i.AtivofinanceiroId == ativo.AtivofinanceiroId);
            
            if (imovel != null)
            {
                ativo.Imovelarrendado = imovel;
            }
            
            carteira.Ativofinanceiros.Add(ativo);
        }

        return View(carteira);
    }

        // GET: Carteira/AtivosCatalogo
        public async Task<IActionResult> AtivosCatalogo()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var ativosCatalogo = await _context.Ativofinanceiros
                .Include(a => a.Carteira)
                .Where(a => a.Carteira.UtilizadorId == userId)
                .Include(a => a.Depositoprazo).ThenInclude(d => d.Banco)
                .Include(a => a.Fundoinvestimento).ThenInclude(f => f.Banco)
                .Include(a => a.Imovelarrendado).ThenInclude(i => i.Banco)
                .ToListAsync();

            return View(ativosCatalogo);
        }



        // POST: Carteira/AdicionarAtivo
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> AdicionarAtivo(int ativoId)
{
    var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
    var carteira = await _context.Carteiras.FirstOrDefaultAsync(c => c.UtilizadorId == userId);
    if (carteira == null) return NotFound();

    var ativoOriginal = await _context.Ativofinanceiros
        .Include(a => a.Depositoprazo)
        .Include(a => a.Fundoinvestimento)
        .Include(a => a.Imovelarrendado)
        .FirstOrDefaultAsync(a => a.AtivofinanceiroId == ativoId && a.CarteiraId == carteira.CarteiraId);

    if (ativoOriginal == null)
        return NotFound("Ativo não encontrado no catálogo.");

    // Verificar se já existe um ativo igual na carteira do utilizador
    bool jaExiste = await _context.Ativofinanceiros
        .AnyAsync(a => a.CarteiraId == carteira.CarteiraId &&
            a.Depositoprazo != null &&
            ativoOriginal.Depositoprazo != null &&
            a.Depositoprazo.Taxajuroanual == ativoOriginal.Depositoprazo.Taxajuroanual &&
            a.Depositoprazo.Valorinicial == ativoOriginal.Depositoprazo.Valorinicial);

    if (jaExiste)
    {
        TempData["Mensagem"] = "Este ativo já está na sua carteira.";
        return RedirectToAction("Index");
    }

    var novoAtivo = new Ativofinanceiro
    {
        CarteiraId = carteira.CarteiraId,
        Datainicio = DateOnly.FromDateTime(DateTime.Now),
        Duracaomeses = ativoOriginal.Duracaomeses,
        Percimposto = ativoOriginal.Percimposto
    };

    _context.Ativofinanceiros.Add(novoAtivo);
    await _context.SaveChangesAsync();

    if (ativoOriginal.Depositoprazo != null)
    {
        _context.Depositoprazos.Add(new Depositoprazo
        {
            AtivofinanceiroId = novoAtivo.AtivofinanceiroId,
            BancoId = ativoOriginal.Depositoprazo.BancoId,
            Nrconta = ativoOriginal.Depositoprazo.Nrconta,
            Titular = ativoOriginal.Depositoprazo.Titular,
            Taxajuroanual = ativoOriginal.Depositoprazo.Taxajuroanual,
            Valorinicial = ativoOriginal.Depositoprazo.Valorinicial,
            Valoratual = ativoOriginal.Depositoprazo.Valoratual
        });
    }

    await _context.SaveChangesAsync();
    TempData["Mensagem"] = "Ativo adicionado com sucesso!";
    return RedirectToAction("Index");
}

        

        // GET: Carteira/Remover/5
        public async Task<IActionResult> Remover(int id)
        {
            // Obter o ID do utilizador atual
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            // Verificar se o ativo pertence à carteira do utilizador
            var ativo = await _context.Ativofinanceiros
                .Include(a => a.Carteira)
                .Include(a => a.Depositoprazo)
                    .ThenInclude(d => d.Banco)
                .Include(a => a.Fundoinvestimento)
                    .ThenInclude(f => f.Banco)
                .Include(a => a.Imovelarrendado)
                    .ThenInclude(i => i.Banco)
                .FirstOrDefaultAsync(a => a.AtivofinanceiroId == id);

            if (ativo == null)
            {
                return NotFound("Ativo não encontrado.");
            }

            var carteira = await _context.Carteiras
                .FirstOrDefaultAsync(c => c.UtilizadorId == userId);

            if (carteira == null || ativo.CarteiraId != carteira.CarteiraId)
            {
                return Forbid("Não tem permissão para remover este ativo.");
            }

            return View(ativo);
        }

        // POST: Carteira/Remover/5
        [HttpPost, ActionName("Remover")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoverConfirmado(int id)
        {
            var ativo = await _context.Ativofinanceiros
                .Include(a => a.Depositoprazo)
                .Include(a => a.Fundoinvestimento)
                .Include(a => a.Imovelarrendado)
                .FirstOrDefaultAsync(a => a.AtivofinanceiroId == id);

            if (ativo == null)
            {
                return NotFound();
            }

            // Remover registros relacionados
            if (ativo.Depositoprazo != null)
            {
                _context.Depositoprazos.Remove(ativo.Depositoprazo);
            }
            else if (ativo.Fundoinvestimento != null)
            {
                _context.Fundoinvestimentos.Remove(ativo.Fundoinvestimento);
            }
            else if (ativo.Imovelarrendado != null)
            {
                _context.Imovelarrendados.Remove(ativo.Imovelarrendado);
            }

            // Remover o ativo
            _context.Ativofinanceiros.Remove(ativo);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CriarDeposito(Depositoprazo deposito, Ativofinanceiro ativo)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var carteira = await _context.Carteiras.FirstOrDefaultAsync(c => c.UtilizadorId == userId);

            if (carteira == null) return NotFound();

            // Forçar imposto de 28%
            ativo.Percimposto = 28;
            ativo.CarteiraId = carteira.CarteiraId;
            ativo.Datainicio = DateOnly.FromDateTime(DateTime.Now);

            _context.Ativofinanceiros.Add(ativo);
            await _context.SaveChangesAsync();

            // Cálculo do valor atual com imposto de 28%
            var C = deposito.Valorinicial;
            var TANB = deposito.Taxajuroanual / 100m;
            var n = ativo.Duracaomeses ?? 0;
            var t = 0.28m;

            deposito.Valoratual = C + (C * TANB * n / 12m) * (1 - t);

            // Atribuir o ID do ativo criado
            deposito.AtivofinanceiroId = ativo.AtivofinanceiroId;

            // Valores default ou ocultos
            deposito.Nrconta = "auto";
            deposito.Titular = "auto";

            _context.Depositoprazos.Add(deposito);
            await _context.SaveChangesAsync();

            return RedirectToAction("AtivosCatalogo");
        }


        
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Bancos = await _context.Bancos.ToListAsync();
            return View();
        }
        
        // GET: Carteira/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var deposito = await _context.Depositoprazos
                .Include(d => d.Ativofinanceiro)
                .FirstOrDefaultAsync(d => d.DepositoprazoId == id);

            if (deposito == null)
            {
                return NotFound();
            }

            ViewBag.Bancos = await _context.Bancos.ToListAsync();
            ViewBag.Duracao = deposito.Ativofinanceiro.Duracaomeses;

            return View(deposito);
        }

// POST: Carteira/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Depositoprazo deposito, int DuracaoMeses)
        {
            var ativo = await _context.Ativofinanceiros
                .FirstOrDefaultAsync(a => a.AtivofinanceiroId == deposito.AtivofinanceiroId);

            if (ativo == null)
                return NotFound();

            ativo.Duracaomeses = DuracaoMeses;

            // Garantir valores obrigatórios para os campos que não estão no formulário
            deposito.Nrconta ??= "auto";
            deposito.Titular ??= "auto";

            // Atualizar o valor atual com a fórmula
            var C = deposito.Valorinicial;
            var TANB = deposito.Taxajuroanual / 100m;
            var n = DuracaoMeses;
            var t = 0.28m;
            deposito.Valoratual = C + (C * TANB * n / 12m) * (1 - t);

            _context.Update(deposito);
            _context.Update(ativo);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(AtivosCatalogo));
        }



        // GET: Carteira/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var deposito = await _context.Depositoprazos
                .Include(d => d.Ativofinanceiro)
                .FirstOrDefaultAsync(d => d.DepositoprazoId == id);
        
            if (deposito == null)
            {
                return NotFound();
            }
        
            return View(deposito);
        }
        
        // POST: Carteira/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deposito = await _context.Depositoprazos
                .FirstOrDefaultAsync(d => d.DepositoprazoId == id);
        
            if (deposito == null)
                return NotFound();
        
            var ativo = await _context.Ativofinanceiros
                .FirstOrDefaultAsync(a => a.AtivofinanceiroId == deposito.AtivofinanceiroId);
        
            _context.Depositoprazos.Remove(deposito);
            if (ativo != null)
                _context.Ativofinanceiros.Remove(ativo);
        
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(AtivosCatalogo));
        }

        

 







    }
}