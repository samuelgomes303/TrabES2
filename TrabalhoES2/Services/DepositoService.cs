using Microsoft.EntityFrameworkCore;
using TrabalhoES2.Models;

namespace TrabalhoES2.Services;



public class DepositoService
{
    private readonly projetoPraticoDbContext _context;

    public DepositoService(projetoPraticoDbContext context)
    {
        _context = context;
    }
    public async Task CriarDepositoAsync(Depositoprazo deposito, Ativofinanceiro ativo, int utilizadorId)
    {
        var carteira = await _context.Carteiras.FirstOrDefaultAsync(c => c.UtilizadorId == utilizadorId);
        if (carteira == null) throw new Exception("Carteira não encontrada");

        ativo.CarteiraId = carteira.CarteiraId;
        ativo.Percimposto = 28;
        ativo.Datainicio = DateOnly.FromDateTime(DateTime.Now);

        _context.Ativofinanceiros.Add(ativo);
        await _context.SaveChangesAsync();
        
        deposito.Valoratual = CalcularValorAtual(deposito.Valorinicial, deposito.Taxajuroanual, ativo.Duracaomeses ?? 0);

        deposito.AtivofinanceiroId = ativo.AtivofinanceiroId;
        deposito.Nrconta = "auto";

        _context.Depositoprazos.Add(deposito);
        await _context.SaveChangesAsync();
    }
    private decimal CalcularValorAtual(decimal C, decimal taxaAnual, int meses)
    {
        var TANB = taxaAnual / 100m;
        var t = 0.28m;
        return C + (C * TANB * meses / 12m) * (1 - t);
    }
}
