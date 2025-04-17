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
    
    [HttpGet]
    public IActionResult CriarDeposito()
    {
        return View();
    }

    [HttpGet]
    public IActionResult CriarFundo()
    {
        return View();
    }

    [HttpGet]
    public IActionResult CriarImovel()
    {
        return View();
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
        
        // Delete AtivoFinanceiro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            // Busca o ativo apenas se estiver no catálogo (carteira sistema)
            var ativo = await _context.Ativofinanceiros
                .Include(a => a.Depositoprazo)
                .Include(a => a.Fundoinvestimento)
                .Include(a => a.Imovelarrendado)
                .FirstOrDefaultAsync(a => a.AtivofinanceiroId == id && a.CarteiraId == Constantes.CarteiraSistemaId);

            if (ativo == null)
            {
                return NotFound("Ativo não encontrado no catálogo.");
            }

            // Remove dependências específicas
            if (ativo.Depositoprazo != null)
                _context.Depositoprazos.Remove(ativo.Depositoprazo);

            if (ativo.Fundoinvestimento != null)
                _context.Fundoinvestimentos.Remove(ativo.Fundoinvestimento);

            if (ativo.Imovelarrendado != null)
                _context.Imovelarrendados.Remove(ativo.Imovelarrendado);

            _context.Ativofinanceiros.Remove(ativo);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(AtivosCatalogo));
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
        
       

        [HttpGet, ActionName("GerarRelatorio")]
        public async Task<IActionResult> GerarRelatorio(int id, DateTime dataInicio, DateTime dataFim)
        {
            // Obter ID do utilizador autenticado
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            // Carregar a carteira com os ativos e dados relacionados
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
            
            // 1. Validações básicas
            if (dataFim <= dataInicio)
            {
                TempData["Erro"] = "A data final deve ser posterior à data inicial";
                return RedirectToAction(nameof(Index));
            }
            
            // 3. Filtrar ativos pelo período e calcular valores
            decimal lucroTotalBruto = 0;
            decimal impostosTotais = 0;
            int totalMeses = (dataFim.Year - dataInicio.Year) * 12 + dataFim.Month - dataInicio.Month;

            var ativosRelatorio = new List<dynamic>();
            foreach (var ativo in carteira.Ativofinanceiros)
            {
                var dataInicioAtivo = ativo.Datainicio ?? DateOnly.FromDateTime(DateTime.MinValue);
                if (dataInicioAtivo.ToDateTime(TimeOnly.MinValue) > dataFim || 
                    (ativo.Duracaomeses.HasValue && dataInicioAtivo.ToDateTime(TimeOnly.MinValue).AddMonths(ativo.Duracaomeses.Value) < dataInicio))
                {
                    continue;
                }

                decimal lucroBruto = 0, lucroLiquido=0, impostos=0, lucroMensalLiquido=0, lucroMensalBruto = 0;
                
                // Calcular para cada tipo de ativo
                if (ativo.Depositoprazo != null)
                {
                    CalcularLucroDeposito(
                        ativo.Depositoprazo,
                        ativo.Percimposto ?? 0,
                        dataInicioAtivo,
                        ativo.Duracaomeses ?? 0,
                        dataInicio,
                        dataFim,
                        out lucroBruto,
                        out lucroLiquido,
                        out impostos,
                        out lucroMensalLiquido,
                        out lucroMensalBruto
                    );
                }
                else if (ativo.Imovelarrendado != null)
                {
                    CalcularLucroImovel(
                        ativo.Imovelarrendado,
                        ativo.Percimposto ?? 0,
                        dataInicioAtivo,
                        dataInicio,
                        dataFim,
                        out lucroBruto,
                        out lucroLiquido,
                        out impostos,
                        out lucroMensalLiquido,
                        out lucroMensalBruto
                    );
                }
                // Armazenando os cálculos para cada ativo na lista
                ativosRelatorio.Add(new
                {
                    TipoAtivo = ativo.Depositoprazo != null ? "Depósito a Prazo" :
                        ativo.Imovelarrendado != null ? "Imóvel Arrendado" : "Outro",
                    LucroBruto = lucroBruto,
                    Impostos = impostos,
                    LucroLiquido = lucroLiquido
                });

                lucroTotalBruto += lucroBruto;
                impostosTotais += impostos;
            }

            decimal lucroTotalLiquido = lucroTotalBruto - impostosTotais;
            decimal lucroMensalMedioBruto = totalMeses > 0 ? lucroTotalBruto / totalMeses : 0;
            decimal lucroMensalMedioLiquido = totalMeses > 0 ? lucroTotalLiquido / totalMeses : 0;

            // Passando os dados para a View
            ViewBag.DataInicio = dataInicio;
            ViewBag.DataFim = dataFim;
            ViewBag.LucroTotalBruto = lucroTotalBruto;
            ViewBag.ImpostosTotais = impostosTotais;
            ViewBag.LucroTotalLiquido = lucroTotalLiquido;
            ViewBag.LucroMensalMedioBruto = lucroMensalMedioBruto;
            ViewBag.LucroMensalMedioLiquido = lucroMensalMedioLiquido;
            ViewBag.AtivosRelatorio = ativosRelatorio;

            return View(carteira);
        }
        
        
        // Métodos auxiliares para cálculos (implementar na mesma classe do controlador)
        private void CalcularLucroDeposito(
            Depositoprazo deposito,
            decimal percImposto,
            DateOnly dataInicioAtivo,
            int duracaoMeses,
            DateTime inicioPeriodo,
            DateTime fimPeriodo,
            out decimal lucroBruto,
            out decimal lucroDepoisImpostos,
            out decimal impostos,
            out decimal lucroMensalMedioDepoisImpostos, out decimal lucroMedioMensalBruto)
        {
            // Inicializar valores de retorno
            lucroBruto = 0;
            impostos = 0;
            lucroDepoisImpostos = 0;
            lucroMedioMensalBruto = 0;
            lucroMensalMedioDepoisImpostos = 0;

            // 1. Converter datas e verificar período válido
            DateTime dtInicioAtivo = dataInicioAtivo.ToDateTime(TimeOnly.MinValue);
            DateTime dtFimAtivo = dtInicioAtivo.AddMonths(duracaoMeses);

            // 2. Calcular interseção de períodos
            DateTime inicioCalculo = dtInicioAtivo < inicioPeriodo ? inicioPeriodo : dtInicioAtivo;
            DateTime fimCalculo = dtFimAtivo > fimPeriodo ? fimPeriodo : dtFimAtivo;

            if (fimCalculo <= inicioCalculo) return;

            // 3. Calcular meses no período
            int mesesNoPeriodo = ((fimCalculo.Year - inicioCalculo.Year) * 12) + fimCalculo.Month - inicioCalculo.Month;
            if (mesesNoPeriodo <= 0) return;
            
            // 4. calculo do lucro de deposito a prazo
            decimal taxaMensal = deposito.Taxajuroanual / 12 / 100;
            decimal valorFinal = deposito.Valorinicial * (decimal)Math.Pow(1 + (double)taxaMensal, mesesNoPeriodo);
            
            lucroBruto = valorFinal - deposito.Valorinicial;
            impostos = lucroBruto * ( percImposto / 100);
            lucroDepoisImpostos = lucroBruto - impostos; // depois de impostos
                // periodo mensal médio
            lucroMedioMensalBruto = lucroBruto / mesesNoPeriodo;
            lucroMensalMedioDepoisImpostos = lucroDepoisImpostos / mesesNoPeriodo; // depois de impostos
        }
        

        private void CalcularLucroImovel(Imovelarrendado imovel, decimal percImposto, DateOnly dataInicioAtivo, DateTime inicioPeriodo, DateTime fimPeriodo,
            out decimal lucroBruto, 
            out decimal lucroDepoisImpostos, 
            out decimal impostos, 
            out decimal lucroMensalMedioDepoisImpostos, out decimal lucroMedioMensalBruto)
        {
            lucroBruto = 0;
            lucroDepoisImpostos = 0;
            impostos = 0;
            lucroMensalMedioDepoisImpostos = 0;
            lucroMedioMensalBruto = 0;

            // 1. Converter datas e verificar período válido
            DateTime dtInicioAtivo = dataInicioAtivo.ToDateTime(TimeOnly.MinValue);
            DateTime dtFimAtivo = fimPeriodo; // Imóveis geralmente não têm data fim fixa

            // 2. Calcular interseção de períodos
            DateTime inicioCalculo = dtInicioAtivo < inicioPeriodo ? inicioPeriodo : dtInicioAtivo;
            DateTime fimCalculo = dtFimAtivo > fimPeriodo ? fimPeriodo : dtFimAtivo;

            if (fimCalculo <= inicioCalculo) return;

            // 3. Calcular meses no período
            int mesesNoPeriodo = ((fimCalculo.Year - inicioCalculo.Year) * 12) + 
                fimCalculo.Month - inicioCalculo.Month;
            if (mesesNoPeriodo <= 0) return;

            // 4. Cálculo do lucro bruto (rendas - despesas)
            decimal rendaBruta = imovel.Valorrenda * mesesNoPeriodo;
            decimal despesaAnualRespetiva = (imovel.Valoranualdespesas / 12) * mesesNoPeriodo;
            decimal despesaTotal = (imovel.Valormensalcondo * mesesNoPeriodo) + despesaAnualRespetiva;
    
            lucroBruto = rendaBruta - despesaTotal;
            lucroMedioMensalBruto = lucroBruto / mesesNoPeriodo;
            impostos = lucroBruto * (percImposto / 100);
            
            lucroDepoisImpostos = lucroBruto - impostos;
            lucroMensalMedioDepoisImpostos = lucroDepoisImpostos / mesesNoPeriodo;
        }
        
        private (decimal bruto, decimal imposto) CalcularLucroFundo(Fundoinvestimento fundo, decimal percImposto, DateTime inicio, DateTime fim)
        {
            // Implementar cálculo real baseado nas datas
            decimal rendimento = fundo.Montanteinvestido * (fundo.Taxajuropdefeito / 100);
            decimal imposto = rendimento * (percImposto / 100);
            return (rendimento, imposto);
        }
    }
}