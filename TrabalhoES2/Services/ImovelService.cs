using TrabalhoES2.Models;

namespace TrabalhoES2.Services;

public class ImovelService
{
    public static decimal CalcularValorAtualComRendimentos(Imovelarrendado imovel, Ativofinanceiro ativo)
    {
        if (ativo.Datainicio == null) return imovel.Valorimovel;

        var dataInicio = ativo.Datainicio.Value.ToDateTime(TimeOnly.MinValue);
        var hoje = DateTime.Today;
        var dias = (hoje - dataInicio).Days;
        if (dias < 0) dias = 0;

        var imposto = (ativo.Percimposto ?? 0m) / 100m; // imposto inserido pelo utilizador

        var receitaDiaria = (imovel.Valorrenda * 12 / 365m) * (1 - imposto);
        var custoDiario = (imovel.Valormensalcondo * 12 + imovel.Valoranualdespesas) / 365m;

        var lucroProporcional = (receitaDiaria - custoDiario) * dias;

        return Math.Round(imovel.Valorimovel + lucroProporcional, 2);
    }

    
    public static decimal CalcularExpectativaRendimentoAnual(Imovelarrendado imovel, Ativofinanceiro ativo)
    {
        var imposto = (ativo.Percimposto ?? 0m) / 100m; // imposto em percentagem convertida

        var receitaBruta = imovel.Valorrenda * 12;
        var receitaLiquida = receitaBruta * (1 - imposto);
        var custos = imovel.Valormensalcondo * 12 + imovel.Valoranualdespesas;

        var lucroLiquido = receitaLiquida - custos;
        return Math.Round(lucroLiquido, 2);
    }


}