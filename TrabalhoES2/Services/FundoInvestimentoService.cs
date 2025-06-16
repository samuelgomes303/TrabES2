using System;
using TrabalhoES2.Models;

namespace TrabalhoES2.Services
{
    public static class FundoInvestimentoService
    {
        public static decimal CalcularValorAtualComJuros(Fundoinvestimento fundo, Ativofinanceiro ativo)
        {
            if (fundo == null) throw new ArgumentNullException(nameof(fundo));
            if (ativo == null) throw new ArgumentNullException(nameof(ativo));
            if (ativo.Duracaomeses == null || ativo.Duracaomeses == 0)
                return fundo.Valoratual;
            var anos = ativo.Duracaomeses.Value / 12.0m;
            var taxa = fundo.Taxajuropdefeito / 100m;
            var principal = fundo.Montanteinvestido;
            // Juros compostos: M = P * (1 + r)^n
            var montante = principal * (decimal)Math.Pow((double)(1 + taxa), (double)anos);
            return Math.Round(montante, 2);
        }
    }
}