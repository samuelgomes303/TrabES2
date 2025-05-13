using TrabalhoES2.Models;

namespace TrabalhoES2.utils;

public static class InvestimentoUtils
{
    public static decimal CalcularValorAtualComJuros(
        decimal valorInicial,
        decimal taxaAnualPercentual,
        DateOnly dataInicio,
        decimal impostoPercentual)
    {
        var meses = ((DateTime.Now.Year - dataInicio.Year) * 12) + DateTime.Now.Month - dataInicio.Month;
        if (meses < 0) meses = 0;

        var taxa = taxaAnualPercentual / 100m;
        var imposto = impostoPercentual / 100m;

        var juros = valorInicial * taxa * (meses / 12m);
        var valorLiquido = valorInicial + (juros * (1 - imposto));
        return decimal.Round(valorLiquido, 2);
    }
    
    public static decimal CalcularValorAtualFundo(Fundoinvestimento fundo, decimal taxaAnualPercentual, decimal impostoPercentual)
    {
        decimal valorTotal = 0;

        foreach (var compra in fundo.FundoCompras)
        {
            var meses = ((DateTime.Now.Year - compra.DataCompra.Year) * 12) + DateTime.Now.Month - compra.DataCompra.Month;
            if (meses < 0) meses = 0;

            var taxa = taxaAnualPercentual / 100m;
            var imposto = impostoPercentual / 100m;

            var juros = compra.ValorPorUnidade * taxa * (meses / 12m);
            var valorLiquido = compra.ValorPorUnidade + (juros * (1 - imposto));

            valorTotal += decimal.Round(valorLiquido, 2);
        }

        return decimal.Round(valorTotal, 2);
    }

    
}
