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
            // Obter todos os ativos do catálogo (carteira sistema)
            var ativosCatalogo = await _context.Ativofinanceiros
                .Where(a => a.CarteiraId == Constantes.CarteiraSistemaId)
                .Include(a => a.Depositoprazo)
                    .ThenInclude(d => d.Banco)
                .Include(a => a.Fundoinvestimento)
                    .ThenInclude(f => f.Banco)
                .Include(a => a.Imovelarrendado)
                    .ThenInclude(i => i.Banco)
                .ToListAsync();

            return View(ativosCatalogo);
        }

        // POST: Carteira/AdicionarAtivo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdicionarAtivo(int ativoId)
        {
            // Obter o ativo do catálogo
            var ativoOriginal = await _context.Ativofinanceiros
                .Include(a => a.Depositoprazo)
                    .ThenInclude(d => d.Banco)
                .Include(a => a.Fundoinvestimento)
                    .ThenInclude(f => f.Banco)
                .Include(a => a.Imovelarrendado)
                    .ThenInclude(i => i.Banco)
                .FirstOrDefaultAsync(a => a.AtivofinanceiroId == ativoId && a.CarteiraId == Constantes.CarteiraSistemaId);

            if (ativoOriginal == null)
            {
                return NotFound("Ativo não encontrado no catálogo.");
            }

            // Obter o ID do utilizador atual
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var utilizador = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (utilizador == null)
            {
                return NotFound("Utilizador não encontrado.");
            }

            // Obter a carteira do utilizador
            var carteira = await _context.Carteiras
                .FirstOrDefaultAsync(c => c.UtilizadorId == userId);

            if (carteira == null)
            {
                // Criar uma nova carteira se não existir
                carteira = new Carteira
                {
                    Nome = "Carteira Principal",
                    UtilizadorId = userId
                };
                _context.Carteiras.Add(carteira);
                await _context.SaveChangesAsync();
            }

            // Criar cópia do ativo para a carteira do utilizador
            var novoAtivo = new Ativofinanceiro
            {
                Percimposto = ativoOriginal.Percimposto,
                Duracaomeses = ativoOriginal.Duracaomeses,
                Datainicio = DateOnly.FromDateTime(DateTime.Now),  // Data atual como data de início
                CarteiraId = carteira.CarteiraId
            };

            _context.Ativofinanceiros.Add(novoAtivo);
            await _context.SaveChangesAsync();

            // Copiar informações específicas baseadas no tipo de ativo
            if (ativoOriginal.Depositoprazo != null)
            {
                var depositoOriginal = ativoOriginal.Depositoprazo;
                var novoDeposito = new Depositoprazo
                {
                    AtivofinanceiroId = novoAtivo.AtivofinanceiroId,
                    BancoId = depositoOriginal.BancoId,
                    Nrconta = depositoOriginal.Nrconta,
                    Titular = depositoOriginal.Titular,
                    Taxajuroanual = depositoOriginal.Taxajuroanual,
                    Valorinicial = depositoOriginal.Valorinicial,
                    Valoratual = depositoOriginal.Valoratual
                };
                _context.Depositoprazos.Add(novoDeposito);
            }
            else if (ativoOriginal.Fundoinvestimento != null)
            {
                var fundoOriginal = ativoOriginal.Fundoinvestimento;
                var novoFundo = new Fundoinvestimento
                {
                    AtivofinanceiroId = novoAtivo.AtivofinanceiroId,
                    BancoId = fundoOriginal.BancoId,
                    Nome = fundoOriginal.Nome,
                    Montanteinvestido = fundoOriginal.Montanteinvestido,
                    Taxajuropdefeito = fundoOriginal.Taxajuropdefeito
                };
                _context.Fundoinvestimentos.Add(novoFundo);
            }
            else if (ativoOriginal.Imovelarrendado != null)
            {
                var imovelOriginal = ativoOriginal.Imovelarrendado;
                var novoImovel = new Imovelarrendado
                {
                    AtivofinanceiroId = novoAtivo.AtivofinanceiroId,
                    BancoId = imovelOriginal.BancoId,
                    Designacao = imovelOriginal.Designacao,
                    Localizacao = imovelOriginal.Localizacao,
                    Valorimovel = imovelOriginal.Valorimovel,
                    Valorrenda = imovelOriginal.Valorrenda,
                    Valormensalcondo = imovelOriginal.Valormensalcondo,
                    Valoranualdespesas = imovelOriginal.Valoranualdespesas
                };
                _context.Imovelarrendados.Add(novoImovel);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
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
    }
}