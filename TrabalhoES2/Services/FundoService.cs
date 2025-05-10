using Microsoft.EntityFrameworkCore;
using TrabalhoES2.Models;

namespace TrabalhoES2.Services
{
    public class FundoService
    {
        private readonly projetoPraticoDbContext _context;

        public FundoService(projetoPraticoDbContext context)
        {
            _context = context;
        }

        public async Task CriarFundoAsync(Fundoinvestimento fundo, Ativofinanceiro ativo, int utilizadorId)
        {
            var carteira = await _context.Carteiras.FirstOrDefaultAsync(c => c.UtilizadorId == utilizadorId);
            if (carteira == null) throw new Exception("Carteira não encontrada");

            ativo.CarteiraId = carteira.CarteiraId;
            ativo.Datainicio = DateOnly.FromDateTime(DateTime.Now);
            ativo.Percimposto = 28;

            _context.Ativofinanceiros.Add(ativo);
            await _context.SaveChangesAsync();

            fundo.AtivofinanceiroId = ativo.AtivofinanceiroId;

            // ✅ Calcula o valor atual de forma semelhante aos depósitos
            fundo.Valoratual = CalcularValorAtual(fundo.Montanteinvestido, fundo.Taxajuropdefeito, ativo.Duracaomeses ?? 0);

            _context.Fundoinvestimentos.Add(fundo);
            await _context.SaveChangesAsync();
        }

        private decimal CalcularValorAtual(decimal C, decimal taxaAnual, int meses)
        {
            var TANB = taxaAnual / 100m;
            var t = 0.28m;
            return C + (C * TANB * meses / 12m) * (1 - t);
        }
    }
}