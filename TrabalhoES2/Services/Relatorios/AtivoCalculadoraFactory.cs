namespace TrabalhoES2.Services.Relatorios;
using TrabalhoES2.Models;

public static class AtivoCalculadoraFactory
{
    public static ICalculadoraLucroAtivo Criar(Ativofinanceiro ativo)
    {
        return new CalculadoraLucroAtivo(ativo);
    }
}
