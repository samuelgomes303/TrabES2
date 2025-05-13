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
    
    public IEnumerable<object> CalcularImpostosMensais(DateTime dataInicio, DateTime dataFim)
    {
        var resultados = new List<object>();

        var dataInicioAtivo = _ativo.Datainicio?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue;
        var duracaoMeses = _ativo.Duracaomeses ?? 0;
        var dataFimAtivo = duracaoMeses > 0 ? dataInicioAtivo.AddMonths(duracaoMeses) : DateTime.MaxValue;

        var inicio = dataInicio > dataInicioAtivo ? dataInicio : dataInicioAtivo;
        var fim = dataFim < dataFimAtivo ? dataFim : dataFimAtivo;

        for (var dt = new DateTime(inicio.Year, inicio.Month, 1); dt <= fim; dt = dt.AddMonths(1))
        {
            decimal impostoMensal = 0;
            decimal percImposto = _ativo.Percimposto ?? 0;

            string tipo = "";
            string designacao = "";

            if (_ativo.Depositoprazo != null)
            {
                impostoMensal = _helper.CalcularImpostoMensalDeposito(_ativo.Depositoprazo, percImposto, dt);
                tipo = "Depósito a Prazo";
                designacao = _ativo.Depositoprazo.Titular ?? "Depósito";
            }
            else if (_ativo.Imovelarrendado != null)
            {
                impostoMensal = _helper.CalcularImpostoMensalImovel(_ativo.Imovelarrendado, percImposto, dt);
                tipo = "Imóvel Arrendado";
                designacao = _ativo.Imovelarrendado.Designacao ?? "Imóvel";
            }
            else if (_ativo.Fundoinvestimento != null)
            {
                impostoMensal = _helper.CalcularImpostoMensalFundo(_ativo.Fundoinvestimento, percImposto, dt);
                tipo = "Fundo de Investimento";
                designacao = _ativo.Fundoinvestimento.Nome ?? "Fundo";
            }

            resultados.Add(new
            {
                Designacao = designacao,
                Tipo = tipo,
                MesAno = dt.ToString("MM/yyyy"),
                ValorImposto = impostoMensal
            });
        }

        return resultados;
    }
    
}
