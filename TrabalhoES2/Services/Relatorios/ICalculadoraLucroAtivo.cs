namespace TrabalhoES2.Services.Relatorios;

public interface ICalculadoraLucroAtivo
{
    bool AtivoRelevante(DateTime dataInicio, DateTime dataFim);
    object CalcularLucro(DateTime dataInicio, DateTime dataFim);
}