using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrabalhoES2.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TrabalhoES2.Services.Relatorios;
using TrabalhoES2.Services;
using TrabalhoES2.ViewModels;

namespace TrabalhoES2.Controllers
{
    [Authorize]
    public class CarteiraController : Controller
    {
        private readonly projetoPraticoDbContext _context;
        private readonly DepositoService _depositoService;
        private readonly FundoService _fundoService;
        private readonly RelatorioBancoService _relatorioBancoService;

        public CarteiraController(projetoPraticoDbContext context)
        {
            _context = context;
            _depositoService = new DepositoService(_context); // Inicializa o serviço aqui
            _fundoService = new FundoService(_context);
            _relatorioBancoService = new RelatorioBancoService(_context);
        }

// GET: Carteira
// Antes: public async Task<IActionResult> Index(int? bancoId, string tipo, decimal? montanteAplicado)
public async Task<IActionResult> Index(
    string designacao,
    string tipo,
    decimal? montanteAplicado)
{
    var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

    // Carregar carteira + utilizador + ativos + cada tipo + banco
    var carteira = await _context.Carteiras
        .Include(c => c.Utilizador)
        .Include(c => c.Ativofinanceiros)
            .ThenInclude(a => a.Depositoprazo)
                .ThenInclude(d => d.Banco)
        .Include(c => c.Ativofinanceiros)
            .ThenInclude(a => a.Fundoinvestimento)
                .ThenInclude(f => f.Banco)
        .Include(c => c.Ativofinanceiros)
            .ThenInclude(a => a.Imovelarrendado)
                .ThenInclude(i => i.Banco)
        .FirstOrDefaultAsync(c => c.UtilizadorId == userId);

    if (carteira == null)
    {
        carteira = new Carteira
        {
            Nome = "Carteira Principal",
            UtilizadorId = userId,
            Ativofinanceiros = new List<Ativofinanceiro>()
        };
        _context.Carteiras.Add(carteira);
        await _context.SaveChangesAsync();
        return View(carteira);
    }

    // Garante lista sempre inicializada
    var todosAtivos = carteira.Ativofinanceiros ?? new List<Ativofinanceiro>();

    // 1) Lista de designações para dropdown
    ViewBag.Designacoes = todosAtivos
        .Select(a => a.Depositoprazo?.Titular
                  ?? a.Fundoinvestimento?.Nome
                  ?? a.Imovelarrendado?.Designacao)
        .Where(d => !string.IsNullOrEmpty(d))
        .Distinct()
        .OrderBy(d => d)
        .ToList();

    // 2) Monta query em memória (já está tudo carregado)
    var ativosQuery = todosAtivos.AsQueryable();

    // 3) Filtrar por designação
    if (!string.IsNullOrEmpty(designacao))
    {
        ativosQuery = ativosQuery.Where(a =>
            (a.Depositoprazo     != null && a.Depositoprazo.Titular     == designacao) ||
            (a.Fundoinvestimento != null && a.Fundoinvestimento.Nome    == designacao) ||
            (a.Imovelarrendado   != null && a.Imovelarrendado.Designacao == designacao)
        );
    }

    // 4) Filtrar por tipo
    if (!string.IsNullOrEmpty(tipo))
    {
        ativosQuery = tipo switch
        {
            "DepositoPrazo"     => ativosQuery.Where(a => a.Depositoprazo     != null),
            "FundoInvestimento" => ativosQuery.Where(a => a.Fundoinvestimento != null),
            "ImovelArrendado"   => ativosQuery.Where(a => a.Imovelarrendado   != null),
            _                   => ativosQuery
        };
    }

    // 5) Filtrar por montante mínimo
    if (montanteAplicado.HasValue)
    {
        var min = montanteAplicado.Value;
        ativosQuery = ativosQuery.Where(a =>
            (a.Depositoprazo     != null && a.Depositoprazo.Valorinicial      >= min) ||
            (a.Fundoinvestimento != null && a.Fundoinvestimento.Montanteinvestido>= min) ||
            (a.Imovelarrendado   != null && a.Imovelarrendado.Valorimovel      >= min)
        );
    }

    // 6) Vigência e ordenação
    var hoje = DateOnly.FromDateTime(DateTime.Now);
    ativosQuery = ativosQuery
        .Where(a =>
            a.Datainicio.HasValue &&
            a.Duracaomeses.HasValue &&
            a.Datainicio.Value.AddMonths(a.Duracaomeses.Value) >= hoje)
        .OrderByDescending(a =>
            a.Depositoprazo     != null ? (decimal?)a.Depositoprazo.Valorinicial :
            a.Fundoinvestimento != null ? (decimal?)a.Fundoinvestimento.Montanteinvestido :
            a.Imovelarrendado   != null ? (decimal?)a.Imovelarrendado.Valorimovel :
                                           0m
        );

    var ativosFiltrados = ativosQuery.ToList();
    if (!ativosFiltrados.Any())
    {
        ViewBag.NoResults = "Nenhum ativo corresponde aos filtros informados.";
        // mantém todos se não houver resultados
        carteira.Ativofinanceiros = todosAtivos;
    }
    else
    {
        carteira.Ativofinanceiros = ativosFiltrados;
    }

    // 7) Manter filtros na view
    ViewData["designacao"]       = designacao;
    ViewData["tipo"]             = tipo;
    ViewData["montanteAplicado"] = montanteAplicado?.ToString("F2");

    return View(carteira);
}






        // GET: Carteira/AtivosCatalogo
        [HttpGet]
        public async Task<IActionResult> AtivosCatalogo()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound();

            ViewBag.TpUtilizador = user.TpUtilizador.ToString();
            ViewBag.UserId = user.Id;

            // Ativos da carteira do próprio utilizador
            var ativosDoUtilizador = await _context.Ativofinanceiros
                .Include(a => a.Carteira)
                .Include(a => a.Depositoprazo).ThenInclude(d => d.Banco)
                .Include(a => a.Fundoinvestimento).ThenInclude(f => f.Banco)
                .Include(a => a.Imovelarrendado).ThenInclude(i => i.Banco)
                .Where(a => a.Carteira.UtilizadorId == userId)
                .ToListAsync();

            ViewBag.AtivosDoUtilizador = ativosDoUtilizador;

            // Fundos de investimento do admin ainda não adicionados
            var fundosAdmin = await _context.Ativofinanceiros
                .Include(a => a.Carteira).ThenInclude(c => c.Utilizador)
                .Include(a => a.Fundoinvestimento).ThenInclude(f => f.Banco)
                .Where(a => a.Fundoinvestimento != null && a.Carteira.Utilizador.TpUtilizador == Utilizador.TipoUtilizador.Admin)
                .ToListAsync();

            // Remove fundos que o utilizador já adicionou (com base no nome do fundo)
            var nomesJaAdicionados = ativosDoUtilizador
                .Where(a => a.Fundoinvestimento != null)
                .Select(a => a.Fundoinvestimento.Nome)
                .ToHashSet();

            var fundosParaAdicionar = fundosAdmin
                .Where(a => !nomesJaAdicionados.Contains(a.Fundoinvestimento.Nome))
                .ToList();

            ViewBag.FundosDoAdmin = fundosParaAdicionar;

            // A view espera um IEnumerable completo
            var todos = ativosDoUtilizador.Concat(fundosParaAdicionar).ToList();

            return View(todos);
        }



        // POST: Carteira/AdicionarAtivo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdicionarAtivo(int ativoId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var carteira = await _context.Carteiras.FirstOrDefaultAsync(c => c.UtilizadorId == userId);
            if (carteira == null) return NotFound();

            //  Corrigido: agora busca o ativo SEM restringir à carteira do cliente
            var ativoOriginal = await _context.Ativofinanceiros
                .Include(a => a.Depositoprazo)
                .Include(a => a.Fundoinvestimento)
                .Include(a => a.Imovelarrendado)
                .FirstOrDefaultAsync(a => a.AtivofinanceiroId == ativoId);

            if (ativoOriginal == null)
                return NotFound("Ativo não encontrado no catálogo.");

            //  Verifica se já existe ativo semelhante na carteira do cliente
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

            //  Cria o novo ativo para a carteira do utilizador
            var novoAtivo = new Ativofinanceiro
            {
                CarteiraId = carteira.CarteiraId,
                Datainicio = DateOnly.FromDateTime(DateTime.Now),
                Duracaomeses = ativoOriginal.Duracaomeses,
                Percimposto = ativoOriginal.Percimposto
            };

            _context.Ativofinanceiros.Add(novoAtivo);
            await _context.SaveChangesAsync();

            // Cria o tipo de ativo correspondente (Depósito / Fundo / Imóvel)
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
                    Valoratual = ativoOriginal.Depositoprazo.Valorinicial
                });
            }
            else if (ativoOriginal.Fundoinvestimento != null)
            {
                _context.Fundoinvestimentos.Add(new Fundoinvestimento
                {
                    AtivofinanceiroId = novoAtivo.AtivofinanceiroId,
                    BancoId = ativoOriginal.Fundoinvestimento.BancoId,
                    Nome = ativoOriginal.Fundoinvestimento.Nome,
                    Taxajuropdefeito = ativoOriginal.Fundoinvestimento.Taxajuropdefeito,
                    Montanteinvestido = ativoOriginal.Fundoinvestimento.Montanteinvestido
                });
            }
            else if (ativoOriginal.Imovelarrendado != null)
            {
                _context.Imovelarrendados.Add(new Imovelarrendado
                {
                    AtivofinanceiroId = novoAtivo.AtivofinanceiroId,
                    BancoId = ativoOriginal.Imovelarrendado.BancoId,
                    Designacao = ativoOriginal.Imovelarrendado.Designacao,
                    Localizacao = ativoOriginal.Imovelarrendado.Localizacao,
                    Valorimovel = ativoOriginal.Imovelarrendado.Valorimovel,
                    Valorrenda = ativoOriginal.Imovelarrendado.Valorrenda,
                    Valormensalcondo = ativoOriginal.Imovelarrendado.Valormensalcondo,
                    Valoranualdespesas = ativoOriginal.Imovelarrendado.Valoranualdespesas
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
            await _depositoService.CriarDepositoAsync(deposito, ativo, userId);
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

            // Atualizar o valor atual com a fórmula
            var C = deposito.Valorinicial;
            var TANB = deposito.Taxajuroanual / 100m;
            var n = DuracaoMeses;
            var t = 0.28m;
            deposito.Valoratual = C + (C * TANB * n / 12m) * (1 - t);

            _context.Update(deposito);
            _context.Update(ativo);
            await _context.SaveChangesAsync();

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("GestaoFundos");
            }
            return RedirectToAction("AtivosCatalogo");
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
            
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("GestaoFundos");
            }
            return RedirectToAction("AtivosCatalogo");

        }

        //Criar fundo 
        
        [HttpGet]
        public async Task<IActionResult> CreateFundo()
        {
            ViewBag.Bancos = await _context.Bancos.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdicionarQuantidadeFundo(int fundoId, decimal valor)
        {
            var fundo = await _context.Fundoinvestimentos
                .FirstOrDefaultAsync(f => f.FundoinvestimentoId == fundoId);

            if (fundo == null)
            {
                return NotFound("Fundo não encontrado.");
            }

            if (fundo.Quantidade <= 0)
            {
                TempData["Erro"] = "Quantidade inválida para cálculo.";
                return RedirectToAction("AtivosCatalogo");
            }

            // Calcula o valor unitário atual
            decimal valorUnitario = fundo.Valoratual / fundo.Quantidade;

            // Atualiza a quantidade e o valor
            fundo.Quantidade += valor;
            fundo.Valoratual += valorUnitario * valor;

            await _context.SaveChangesAsync();

            TempData["Mensagem"] = "Quantidade adicionada com sucesso.";

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("GestaoFundos");
            }
            return RedirectToAction("AtivosCatalogo");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdicionarFundo(int fundoId, decimal valor)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var carteira = await _context.Carteiras.FirstOrDefaultAsync(c => c.UtilizadorId == userId);
            if (carteira == null) return NotFound();

            var fundoOriginal = await _context.Fundoinvestimentos
                .Include(f => f.Ativofinanceiro)
                .FirstOrDefaultAsync(f => f.FundoinvestimentoId == fundoId);

            if (fundoOriginal == null) return NotFound();

            // Criar novo ativo
            var novoAtivo = new Ativofinanceiro
            {
                CarteiraId = carteira.CarteiraId,
                Datainicio = DateOnly.FromDateTime(DateTime.Now),
                Duracaomeses = fundoOriginal.Ativofinanceiro.Duracaomeses,
                Percimposto = fundoOriginal.Ativofinanceiro.Percimposto
            };

            _context.Ativofinanceiros.Add(novoAtivo);
            await _context.SaveChangesAsync();

            // Criar novo fundo (cópia para este utilizador)
            var novoFundo = new Fundoinvestimento
            {
                AtivofinanceiroId = novoAtivo.AtivofinanceiroId,
                BancoId = fundoOriginal.BancoId,
                Nome = fundoOriginal.Nome,
                Taxajuropdefeito = fundoOriginal.Taxajuropdefeito,
                Montanteinvestido = valor,
                Valoratual = valor,
                Quantidade = 1
            };

            _context.Fundoinvestimentos.Add(novoFundo);
            await _context.SaveChangesAsync();

            return RedirectToAction("AtivosCatalogo");
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFundo(Fundoinvestimento fundo, Ativofinanceiro ativo)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _fundoService.CriarFundoAsync(fundo, ativo, userId);
            return RedirectToAction(nameof(AtivosCatalogo));
        }
        
        

    
        //Editar Fundos
        [HttpGet]
        public async Task<IActionResult> EditFundo(int id)
        {
            var fundo = await _context.Fundoinvestimentos
                .Include(f => f.Ativofinanceiro)
                .FirstOrDefaultAsync(f => f.FundoinvestimentoId == id);

            if (fundo == null) return NotFound();

            ViewBag.Bancos = await _context.Bancos.ToListAsync();
            ViewBag.Duracao = fundo.Ativofinanceiro.Duracaomeses;

            return View(fundo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFundo(Fundoinvestimento fundo, int DuracaoMeses)
        {
            var ativo = await _context.Ativofinanceiros
                .FirstOrDefaultAsync(a => a.AtivofinanceiroId == fundo.AtivofinanceiroId);

            if (ativo == null) return NotFound();

            ativo.Duracaomeses = DuracaoMeses;

            _context.Update(fundo);
            _context.Update(ativo);
            await _context.SaveChangesAsync();

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("GestaoFundos");
            }
            return RedirectToAction("AtivosCatalogo");
        }
        
        //Eliminar Fundos
        
        [HttpGet]
        public async Task<IActionResult> DeleteFundo(int id)
        {
            var fundo = await _context.Fundoinvestimentos
                .Include(f => f.Ativofinanceiro)
                .FirstOrDefaultAsync(f => f.FundoinvestimentoId == id);

            if (fundo == null) return NotFound();

            return View(fundo);
        }

        [HttpPost, ActionName("DeleteFundo")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFundoConfirmed(int id)
        {
            var fundo = await _context.Fundoinvestimentos
                .FirstOrDefaultAsync(f => f.FundoinvestimentoId == id);

            if (fundo == null)
                return NotFound();

            var ativo = await _context.Ativofinanceiros
                .FirstOrDefaultAsync(a => a.AtivofinanceiroId == fundo.AtivofinanceiroId);

            _context.Fundoinvestimentos.Remove(fundo);
            if (ativo != null)
                _context.Ativofinanceiros.Remove(ativo);

            await _context.SaveChangesAsync();
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("GestaoFundos");
            }
            return RedirectToAction("AtivosCatalogo");
        }

        
        //Crud imoveis
        [HttpGet]
        public async Task<IActionResult> CreateImovel()
        {
            ViewBag.Bancos = await _context.Bancos.ToListAsync();
            return View(new ImovelViewModel());
            
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateImovel(ImovelViewModel viewModel)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var carteira = await _context.Carteiras.FirstOrDefaultAsync(c => c.UtilizadorId == userId);

            if (carteira == null) return NotFound();

            var ativo = viewModel.Ativo;
            ativo.CarteiraId = carteira.CarteiraId;
            ativo.Datainicio = DateOnly.FromDateTime(DateTime.Now);

            _context.Ativofinanceiros.Add(ativo);
            await _context.SaveChangesAsync();

            var imovel = viewModel.Imovel;
            imovel.AtivofinanceiroId = ativo.AtivofinanceiroId;

            _context.Imovelarrendados.Add(imovel);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(AtivosCatalogo));
        }


        [HttpGet]
        public async Task<IActionResult> EditImovel(int id)
        {
            var imovel = await _context.Imovelarrendados
                .Include(i => i.Ativofinanceiro)
                .FirstOrDefaultAsync(i => i.ImovelarrendadoId == id);

            if (imovel == null) return NotFound();

            ViewBag.Bancos = await _context.Bancos.ToListAsync();
            ViewBag.Duracao = imovel.Ativofinanceiro.Duracaomeses;

            return View(imovel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditImovel(Imovelarrendado imovel, int DuracaoMeses)
        {
            var ativo = await _context.Ativofinanceiros
                .FirstOrDefaultAsync(a => a.AtivofinanceiroId == imovel.AtivofinanceiroId);

            if (ativo == null) return NotFound();

            ativo.Duracaomeses = DuracaoMeses;

            _context.Update(imovel);
            _context.Update(ativo);
            await _context.SaveChangesAsync();

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("GestaoFundos");
            }
            return RedirectToAction("AtivosCatalogo");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteImovel(int id)
        {
            var imovel = await _context.Imovelarrendados
                .Include(i => i.Ativofinanceiro)
                .FirstOrDefaultAsync(i => i.ImovelarrendadoId == id);

            if (imovel == null) return NotFound();

            return View(imovel);
        }

        [HttpPost, ActionName("DeleteImovel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImovelConfirmed(int id)
        {
            var imovel = await _context.Imovelarrendados
                .FirstOrDefaultAsync(i => i.ImovelarrendadoId == id);

            if (imovel == null) return NotFound();

            var ativo = await _context.Ativofinanceiros
                .FirstOrDefaultAsync(a => a.AtivofinanceiroId == imovel.AtivofinanceiroId);

            _context.Imovelarrendados.Remove(imovel);
            if (ativo != null)
                _context.Ativofinanceiros.Remove(ativo);

            await _context.SaveChangesAsync();
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("GestaoFundos");
            }
            return RedirectToAction("AtivosCatalogo");
        }
        
        
        
       [HttpGet, ActionName("GerarRelatorio")]
public async Task<IActionResult> GerarRelatorio(int id, DateTime dataInicio, DateTime dataFim)
{
    var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

    var carteira = await _context.Carteiras
        .Include(c => c.Utilizador)
        .Include(c => c.Ativofinanceiros)
            .ThenInclude(a => a.Depositoprazo)
                .ThenInclude(d => d.Banco)
        .Include(c => c.Ativofinanceiros)
            .ThenInclude(a => a.Fundoinvestimento)
                .ThenInclude(f => f.Banco)
        .Include(c => c.Ativofinanceiros)
            .ThenInclude(a => a.Imovelarrendado)
        .FirstOrDefaultAsync(c => c.CarteiraId == id && c.UtilizadorId == userId);

    if (carteira == null)
    {
        return NotFound("Carteira não encontrada ou não pertence ao utilizador.");
    }

    if (dataFim <= dataInicio)
    {
        TempData["Erro"] = "A data final deve ser posterior à data inicial";
        return RedirectToAction(nameof(Index));
    }

    // Prepara a lista de meses completos no intervalo pedido
    var meses = new List<DateTime>();
    var dt = new DateTime(dataInicio.Year, dataInicio.Month, 1);
    var ultimoMes = new DateTime(dataFim.Year, dataFim.Month, 1);
    for (; dt <= ultimoMes; dt = dt.AddMonths(1))
        meses.Add(dt);

    // Lista para o gráfico de performance mensal
    var performanceMensal = new List<dynamic>();

    // Relatório geral para o período todo
    var ativosRelatorio = new List<dynamic>();

    decimal lucroTotalBruto = 0;
    decimal impostosTotais = 0;
    decimal lucroTotalLiquido = 0;

    var helper = new TrabalhoES2.Services.Relatorios.RelatorioLucroHelper();

    foreach (var ativo in carteira.Ativofinanceiros)
    {
        var calculadora = AtivoCalculadoraFactory.Criar(ativo);

        // Adiciona ao relatório global (total do intervalo)
        if (calculadora.AtivoRelevante(dataInicio, dataFim))
        {
            dynamic relatorio = calculadora.CalcularLucro(dataInicio, dataFim);
            ativosRelatorio.Add(relatorio);
            lucroTotalBruto += relatorio.LucroBruto;
            impostosTotais += relatorio.Impostos;
            lucroTotalLiquido += relatorio.LucroLiquido;
        }

        // Apenas para Depósito a Prazo: calcula a evolução do saldo mês a mês para o gráfico
        if (ativo.Depositoprazo != null && ativo.Datainicio.HasValue && ativo.Duracaomeses.HasValue)
        {
            var dataInicioAtivo = ativo.Datainicio.Value.ToDateTime(TimeOnly.MinValue);
            var duracao = ativo.Duracaomeses.Value;
            var evolucao = helper.CalcularEvolucaoDepositoMensal(ativo.Depositoprazo, dataInicioAtivo, duracao);

            foreach (var (mes, saldo) in evolucao)
            {
                // Só adicionar se estiver dentro do intervalo do relatório pedido
                if (mes >= dataInicio && mes <= dataFim)
                {
                    performanceMensal.Add(new
                    {
                        Mes = mes.ToString("yyyy-MM"),
                        TipoAtivo = "Depósito a Prazo",
                        Valor = saldo
                    });
                }
            }
        }
        // Imóvel Arrendado: evolução do saldo mês a mês
        if (ativo.Imovelarrendado != null && ativo.Datainicio.HasValue && ativo.Duracaomeses.HasValue)
        {
            var dataInicioAtivo = ativo.Datainicio.Value.ToDateTime(TimeOnly.MinValue);
            var duracao = ativo.Duracaomeses.Value;
            var evolucao = helper.CalcularEvolucaoImovelMensal(ativo.Imovelarrendado, dataInicioAtivo, duracao);

            foreach (var (mes, saldo) in evolucao)
            {
                if (mes >= dataInicio && mes <= dataFim)
                {
                    performanceMensal.Add(new
                    {
                        Mes = mes.ToString("yyyy-MM"),
                        TipoAtivo = "Imóvel Arrendado",
                        Valor = saldo
                    });
                }
            }
        }
        if (ativo.Fundoinvestimento != null && ativo.Datainicio.HasValue && ativo.Duracaomeses.HasValue)
        {
            var dataInicioAtivo = ativo.Datainicio.Value.ToDateTime(TimeOnly.MinValue);
            var duracao = ativo.Duracaomeses.Value;
            var evolucao = helper.CalcularEvolucaoFundoMensal(ativo.Fundoinvestimento, dataInicioAtivo, duracao);

            foreach (var (mes, saldo) in evolucao)
            {
                if (mes >= dataInicio && mes <= dataFim)
                {
                    performanceMensal.Add(new
                    {
                        Mes = mes.ToString("yyyy-MM"),
                        TipoAtivo = "Fundo de Investimento",
                        Valor = saldo
                    });
                }
            }
        }
        
    }

    int totalMeses = meses.Count;

    decimal lucroMensalMedioBruto = totalMeses > 0 ? lucroTotalBruto / totalMeses : 0;
    decimal lucroMensalMedioLiquido = totalMeses > 0 ? lucroTotalLiquido / totalMeses : 0;

    ViewBag.DataInicio = dataInicio;
    ViewBag.DataFim = dataFim;
    ViewBag.LucroTotalBruto = lucroTotalBruto;
    ViewBag.ImpostosTotais = impostosTotais;
    ViewBag.LucroTotalLiquido = lucroTotalLiquido;
    ViewBag.LucroMensalMedioBruto = lucroMensalMedioBruto;
    ViewBag.LucroMensalMedioLiquido = lucroMensalMedioLiquido;
    ViewBag.AtivosRelatorio = ativosRelatorio;
    ViewBag.PerformanceMensal = performanceMensal;

    return View(carteira);
}
       

        
        
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GestaoFundos()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var ativos = await _context.Ativofinanceiros
                .Include(a => a.Depositoprazo).ThenInclude(d => d.Banco)
                .Include(a => a.Fundoinvestimento).ThenInclude(f => f.Banco)
                .Include(a => a.Imovelarrendado).ThenInclude(i => i.Banco)
                .Where(a =>
                    a.Depositoprazo != null ||
                    a.Fundoinvestimento != null ||
                    a.Imovelarrendado != null
                )
                .ToListAsync();


            ViewBag.TpUtilizador = "Admin";
            ViewBag.UserId = userId;

            return View("GestaoFundos", ativos);
        }
        
        
        
        
        [HttpGet, ActionName("GerarRelatorioImpostos")]
        public async Task<IActionResult> GerarRelatorioImpostos(int id, DateTime dataInicio, DateTime dataFim)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var carteira = await _context.Carteiras
                .Include(c => c.Utilizador)
                .Include(c => c.Ativofinanceiros)
                    .ThenInclude(a => a.Depositoprazo)
                        .ThenInclude(d => d.Banco)
                .Include(c => c.Ativofinanceiros)
                    .ThenInclude(a => a.Fundoinvestimento)
                        .ThenInclude(f => f.Banco)
                .Include(c => c.Ativofinanceiros)
                    .ThenInclude(a => a.Imovelarrendado)
                .FirstOrDefaultAsync(c => c.CarteiraId == id && c.UtilizadorId == userId);

            if (carteira == null)
            {
                return NotFound("Carteira não encontrada ou não pertence ao utilizador.");
            }

            if (dataFim <= dataInicio)
            {
                TempData["Erro"] = "A data final deve ser posterior à data inicial.";
                return RedirectToAction(nameof(Index));
            }

            var impostosMensais = new List<dynamic>();

           
            foreach (var ativo in carteira.Ativofinanceiros)
            {
                var calculadora = AtivoCalculadoraFactory.Criar(ativo);

                if (calculadora.AtivoRelevante(dataInicio, dataFim))
                {
                    var impostos = calculadora.CalcularImpostosMensais(dataInicio, dataFim);
                    impostosMensais.AddRange(impostos);
                }
            }

            ViewBag.ImpostosMensais = impostosMensais;
            ViewBag.DataInicio = dataInicio;
            ViewBag.DataFim = dataFim;

            return View("RelatorioImpostos", carteira);
        }
        
        
        
        // relatorio admin com valor por banco
        [Authorize(Roles = "Admin")]
        [HttpGet, ActionName("GerarRelatorioBancos")]
        public async Task<IActionResult> GerarRelatorioBancos(DateTime dataInicio, DateTime dataFim)
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid("Apenas administradores podem aceder a este relatório.");
            }

            if (dataFim <= dataInicio)
            {
                TempData["Erro"] = "A data final deve ser posterior à data inicial.";
                return RedirectToAction(nameof(Index));
            }

            var relatorio = await _relatorioBancoService.GerarRelatorioPorBanco(dataInicio, dataFim);

            ViewBag.TpUtilizador = "Admin";
            ViewBag.RelatorioBanco = relatorio;
            ViewBag.DataInicio = dataInicio;
            ViewBag.DataFim = dataFim;

            return View("RelatorioValPBanco");
        }
        
        
        
        public IActionResult SelecionarRelatorio(int? id)
        {
            TrabalhoES2.Models.Carteira carteira = null;

            if (id.HasValue)
            {
                carteira = _context.Carteiras
                    .Include(c => c.Utilizador)
                    .FirstOrDefault(c => c.CarteiraId == id.Value);

                if (carteira == null)
                    return NotFound();
            }

            return View("SelecionarRelatorio", carteira);
        }
        
        
        public decimal CalcularValorAtualComJuros(Fundoinvestimento fundo, Ativofinanceiro ativo)
        {
            if (ativo.Datainicio == null || ativo.Duracaomeses == null)
                return fundo.Valoratual;

            var mesesDecorridos = (DateTime.Now.Year - ativo.Datainicio.Value.Year) * 12
                + DateTime.Now.Month - ativo.Datainicio.Value.Month;

            if (mesesDecorridos <= 0)
                return fundo.Valoratual;

            decimal taxaMensal = fundo.Taxajuropdefeito / 100 / 12;
            decimal novoValor = fundo.Valoratual * (decimal)Math.Pow((double)(1 + taxaMensal), mesesDecorridos);

            return Math.Round(novoValor, 2);
        }

    }
}


