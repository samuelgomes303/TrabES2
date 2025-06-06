using System;
using TrabalhoES2.Models;

namespace TrabalhoES2.Services
{
    public static class FundoInvestimentoService
    {
        public static decimal CalcularValorAtualComJuros(Fundoinvestimento fundo, Ativofinanceiro ativo)
        {
            if (ativo == null || fundo == null || ativo.Datainicio == null || fundo.Taxajuropdefeito <= 0 || fundo.Valoratual <= 0)
                return fundo.Valoratual;

            var dataInicio = ativo.Datainicio.Value;
            var agora = DateTime.Now;
            int meses = (agora.Year - dataInicio.Year) * 12 + agora.Month - dataInicio.Month;

            if (meses <= 0)
                return fundo.Valoratual;

            decimal taxaMensal = fundo.Taxajuropdefeito / 100m / 12m;
            decimal valorCalculado = fundo.Valoratual * (decimal)Math.Pow((double)(1 + taxaMensal), meses);

            return Math.Round(valorCalculado, 2);
        }
    }
}