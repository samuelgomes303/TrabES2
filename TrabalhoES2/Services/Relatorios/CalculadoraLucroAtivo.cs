namespace TrabalhoES2.Services.Relatorios;
using TrabalhoES2.Models;

public class CalculadoraLucroAtivo : ICalculadoraLucroAtivo
{
    private readonly Ativofinanceiro _ativo;
    private readonly RelatorioLucroHelper _helper;

    public CalculadoraLucroAtivo(Ativofinanceiro ativo)
    {
        _ativo = ativo;
        _helper = new RelatorioLucroHelper();
    }

    public bool AtivoRelevante(DateTime inicio, DateTime fim)
    {
        var dataInicioAtivo = _ativo.Datainicio?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue;

        if (_ativo.Duracaomeses.HasValue)
        {
            var dataFimAtivo = dataInicioAtivo.AddMonths(_ativo.Duracaomeses.Value);
            return !(dataInicioAtivo > fim || dataFimAtivo < inicio);
        }

        return dataInicioAtivo <= fim;
    }

    public object CalcularLucro(DateTime dataInicio, DateTime dataFim)
    {
        decimal lucroBruto = 0, lucroLiquido = 0, impostos = 0;
        decimal lucroMensalLiquido = 0, lucroMensalBruto = 0;

        var dataInicioAtivo = _ativo.Datainicio ?? DateOnly.FromDateTime(DateTime.MinValue);
        var percImposto = _ativo.Percimposto ?? 0;

        if (_ativo.Depositoprazo != null)
        {
            _helper.CalcularLucroDeposito(_ativo.Depositoprazo, percImposto, dataInicioAtivo,
                _ativo.Duracaomeses ?? 0, dataInicio, dataFim,
                out lucroBruto, out lucroLiquido, out impostos,
                out lucroMensalLiquido, out lucroMensalBruto);

            return new
            {
                TipoAtivo = "Depósito a Prazo",
                LucroBruto = lucroBruto,
                Impostos = impostos,
                LucroLiquido = lucroLiquido
            };
        }

        if (_ativo.Imovelarrendado != null)
        {
            _helper.CalcularLucroImovel(_ativo.Imovelarrendado, percImposto, dataInicioAtivo,
                dataInicio, dataFim,
                out lucroBruto, out lucroLiquido, out impostos,
                out lucroMensalLiquido, out lucroMensalBruto);

            return new
            {
                TipoAtivo = "Imóvel Arrendado",
                LucroBruto = lucroBruto,
                Impostos = impostos,
                LucroLiquido = lucroLiquido
            };
        }

        if (_ativo.Fundoinvestimento != null)
        {
            var resultado = _helper.CalcularLucroFundo(_ativo.Fundoinvestimento, percImposto, dataInicio, dataFim);

            return new
            {
                TipoAtivo = "Fundo de Investimento",
                LucroBruto = resultado.bruto,
                Impostos = resultado.imposto,
                LucroLiquido = resultado.bruto - resultado.imposto
            };
        }

        return new
        {
            TipoAtivo = "Desconhecido",
            LucroBruto = 0m,
            Impostos = 0m,
            LucroLiquido = 0m
        };
    }
}
