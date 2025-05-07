namespace TrabalhoES2.Services.Relatorios;
using System;
using System.Collections.Generic;

public interface ICalculadoraLucroAtivo
{
    bool AtivoRelevante(DateTime dataInicio, DateTime dataFim);
    object CalcularLucro(DateTime dataInicio, DateTime dataFim);
    IEnumerable<object> CalcularImpostosMensais(DateTime dataInicio, DateTime dataFim);
}