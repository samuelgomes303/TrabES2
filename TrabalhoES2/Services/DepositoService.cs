using Microsoft.EntityFrameworkCore;
using TrabalhoES2.Models;
using TrabalhoES2.utils;

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
        ativo.Datainicio = DateOnly.FromDateTime(DateTime.Now); // assegura que temos uma data válida

        _context.Ativofinanceiros.Add(ativo);
        await _context.SaveChangesAsync();

        deposito.AtivofinanceiroId = ativo.AtivofinanceiroId;
    
        // Aqui vamos calcular o valor atual corretamente
        deposito.Valoratual = InvestimentoUtils.CalcularValorAtualComJuros(
            deposito.Valorinicial,
            deposito.Taxajuroanual,
            ativo.Datainicio.Value,
            ativo.Percimposto.Value
        );

        _context.Depositoprazos.Add(deposito);
        await _context.SaveChangesAsync();
    }

}
