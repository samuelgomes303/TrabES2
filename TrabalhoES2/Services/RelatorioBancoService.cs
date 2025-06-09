using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrabalhoES2.Models;

public class RelatorioBancoService
{
    private readonly projetoPraticoDbContext _context;

    public RelatorioBancoService(projetoPraticoDbContext context)
    {
        _context = context;
    }

    public async Task<List<object>> GerarRelatorioPorBanco(DateTime dataInicio, DateTime dataFim)
    {
        // Converter parâmetros para DateOnly
        DateOnly dataInicioOnly = DateOnly.FromDateTime(dataInicio);
        DateOnly dataFimOnly = DateOnly.FromDateTime(dataFim);

        // Depósitos a prazo
        var depositos = await _context.Depositoprazos
            .Include(d => d.Banco)
            .Include(d => d.Ativofinanceiro)
            .Where(d =>
                d.Ativofinanceiro.Datainicio.HasValue &&
                d.Ativofinanceiro.Duracaomeses.HasValue &&
                d.Ativofinanceiro.Datainicio.Value <= dataFimOnly &&
                d.Ativofinanceiro.Datainicio.Value.AddMonths(d.Ativofinanceiro.Duracaomeses.Value) >= dataInicioOnly
            ).ToListAsync();

        // Fundos de investimento
        var fundos = await _context.Fundoinvestimentos
            .Include(f => f.Banco)
            .Include(f => f.Ativofinanceiro)
            .Where(f =>
                f.Ativofinanceiro.Datainicio.HasValue &&
                f.Ativofinanceiro.Duracaomeses.HasValue &&
                f.Ativofinanceiro.Datainicio.Value <= dataFimOnly &&
                f.Ativofinanceiro.Datainicio.Value.AddMonths(f.Ativofinanceiro.Duracaomeses.Value) >= dataInicioOnly
            ).ToListAsync();

        var resultado = new Dictionary<int, dynamic>();

        // Processar Depósitos a Prazo
        foreach (var dp in depositos)
        {
            if (dp.Banco == null) continue;
            if (!resultado.ContainsKey(dp.BancoId))
            {
                resultado[dp.BancoId] = new
                {
                    BancoNome = dp.Banco.Nome,
                    ValorTotalDepositado = 0m,
                    CustoTotalJuros = 0m
                };
            }

            var valDepos = resultado[dp.BancoId].ValorTotalDepositado + dp.Valorinicial;
            var custoJuros = resultado[dp.BancoId].CustoTotalJuros;

            // Corrigir datas para DateOnly, usando sempre .Value pois já filtrámos por HasValue
            DateOnly dataInicioAtivo = dp.Ativofinanceiro.Datainicio.Value;
            int duracaoMeses = dp.Ativofinanceiro.Duracaomeses.Value;
            DateOnly dataFimAtivo = dataInicioAtivo.AddMonths(duracaoMeses);

            // Determinar intervalo efetivo
            DateOnly dataIni = dataInicioAtivo < dataInicioOnly ? dataInicioOnly : dataInicioAtivo;
            DateOnly dataFimReal = dataFimAtivo > dataFimOnly ? dataFimOnly : dataFimAtivo;

            if (dataFimReal < dataIni) continue;

            var meses = (dataFimReal.Year - dataIni.Year) * 12 + dataFimReal.Month - dataIni.Month + 1;
            decimal montante = dp.Valorinicial;
            decimal jurosTotal = 0;
            for (int i = 0; i < meses; i++)
            {
                var juroMes = (montante * (dp.Taxajuroanual / 100)) / 12m;
                jurosTotal += juroMes;
                montante += juroMes;
            }
            custoJuros += jurosTotal;

            resultado[dp.BancoId] = new
            {
                BancoNome = dp.Banco.Nome,
                ValorTotalDepositado = valDepos,
                CustoTotalJuros = custoJuros
            };
        }

        // Processar Fundos de Investimento
        foreach (var f in fundos)
        {
            if (f.Banco == null) continue;
            if (!resultado.ContainsKey(f.BancoId))
            {
                resultado[f.BancoId] = new
                {
                    BancoNome = f.Banco.Nome,
                    ValorTotalDepositado = 0m,
                    CustoTotalJuros = 0m
                };
            }

            var valDepos = resultado[f.BancoId].ValorTotalDepositado + f.Montanteinvestido;
            var custoJuros = resultado[f.BancoId].CustoTotalJuros;

            DateOnly dataInicioAtivo = f.Ativofinanceiro.Datainicio.Value;
            int duracaoMeses = f.Ativofinanceiro.Duracaomeses.Value;
            DateOnly dataFimAtivo = dataInicioAtivo.AddMonths(duracaoMeses);

            DateOnly dataIni = dataInicioAtivo < dataInicioOnly ? dataInicioOnly : dataInicioAtivo;
            DateOnly dataFimReal = dataFimAtivo > dataFimOnly ? dataFimOnly : dataFimAtivo;

            if (dataFimReal < dataIni) continue;

            var meses = (dataFimReal.Year - dataIni.Year) * 12 + dataFimReal.Month - dataIni.Month + 1;
            decimal montante = f.Montanteinvestido;
            decimal jurosTotal = 0;
            for (int i = 0; i < meses; i++)
            {
                var juroMes = (montante * (f.Taxajuropdefeito / 100)) / 12m;
                jurosTotal += juroMes;
                montante += juroMes;
            }
            custoJuros += jurosTotal;

            resultado[f.BancoId] = new
            {
                BancoNome = f.Banco.Nome,
                ValorTotalDepositado = valDepos,
                CustoTotalJuros = custoJuros
            };
        }

        return resultado.Values.Cast<object>().ToList();
    }
}