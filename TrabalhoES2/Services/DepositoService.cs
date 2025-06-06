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
    
        // ✅ Corrigido: cálculo da expectativa de rendimento (valor atual)
        int meses = ativo.Duracaomeses ?? 0;
        deposito.Valoratual = CalcularValorAtualAoDia(
            deposito.Valorinicial,
            deposito.Taxajuroanual,
            ativo.Datainicio.Value
        );


        _context.Depositoprazos.Add(deposito);
        await _context.SaveChangesAsync();
    }

    // ✅ Fórmula correta de rendimento após imposto
    private decimal CalcularValorAtualAoDia(decimal valorInicial, decimal taxaAnual, DateOnly dataInicio)
    {
        var TANB = taxaAnual / 100m;
        var t = 0.28m;

        var hoje = DateOnly.FromDateTime(DateTime.Today);
        var diasPassados = (hoje.ToDateTime(TimeOnly.MinValue) - dataInicio.ToDateTime(TimeOnly.MinValue)).Days;

        if (diasPassados < 0) diasPassados = 0;

        var jurosProporcionais = valorInicial * TANB * diasPassados / 365m * (1 - t);
        return valorInicial + jurosProporcionais;
    }
    

}