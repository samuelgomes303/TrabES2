namespace TrabalhoES2.Services.Relatorios;
using TrabalhoES2.Models;

public class RelatorioLucroHelper
{
    public void CalcularLucroDeposito(
            Depositoprazo deposito,
            decimal percImposto,
            DateOnly dataInicioAtivo,
            int duracaoMeses,
            DateTime inicioPeriodo,
            DateTime fimPeriodo,
            out decimal lucroBruto,
            out decimal lucroDepoisImpostos,
            out decimal impostos,
            out decimal lucroMensalMedioDepoisImpostos, out decimal lucroMedioMensalBruto)
        {
            // Inicializar valores de retorno
            lucroBruto = 0;
            impostos = 0;
            lucroDepoisImpostos = 0;
            lucroMedioMensalBruto = 0;
            lucroMensalMedioDepoisImpostos = 0;

            // 1. Converter datas e verificar período válido
            DateTime dtInicioAtivo = dataInicioAtivo.ToDateTime(TimeOnly.MinValue);
            DateTime dtFimAtivo = dtInicioAtivo.AddMonths(duracaoMeses);

            // 2. Calcular interseção de períodos
            DateTime inicioCalculo = dtInicioAtivo < inicioPeriodo ? inicioPeriodo : dtInicioAtivo;
            DateTime fimCalculo = dtFimAtivo > fimPeriodo ? fimPeriodo : dtFimAtivo;

            if (fimCalculo <= inicioCalculo) return;

            // 3. Calcular meses no período
            int mesesNoPeriodo = ((fimCalculo.Year - inicioCalculo.Year) * 12) + fimCalculo.Month - inicioCalculo.Month;
            if (mesesNoPeriodo <= 0) return;
            
            // 4. calculo do lucro de deposito a prazo
            decimal taxaMensal = deposito.Taxajuroanual / 12 / 100;
            decimal valorFinal = deposito.Valorinicial * (decimal)Math.Pow(1 + (double)taxaMensal, mesesNoPeriodo);
            
            lucroBruto = valorFinal - deposito.Valorinicial;
            impostos = lucroBruto * ( percImposto / 100);
            lucroDepoisImpostos = lucroBruto - impostos; // depois de impostos
                // periodo mensal médio
            lucroMedioMensalBruto = lucroBruto / mesesNoPeriodo;
            lucroMensalMedioDepoisImpostos = lucroDepoisImpostos / mesesNoPeriodo; // depois de impostos
        }


    public void CalcularLucroImovel(Imovelarrendado imovel, decimal percImposto, DateOnly dataInicioAtivo, DateTime inicioPeriodo, DateTime fimPeriodo,
            out decimal lucroBruto, 
            out decimal lucroDepoisImpostos, 
            out decimal impostos, 
            out decimal lucroMensalMedioDepoisImpostos, out decimal lucroMedioMensalBruto)
        {
            lucroBruto = 0;
            lucroDepoisImpostos = 0;
            impostos = 0;
            lucroMensalMedioDepoisImpostos = 0;
            lucroMedioMensalBruto = 0;

            // 1. Converter datas e verificar período válido
            DateTime dtInicioAtivo = dataInicioAtivo.ToDateTime(TimeOnly.MinValue);
            DateTime dtFimAtivo = fimPeriodo; // Imóveis geralmente não têm data fim fixa

            // 2. Calcular interseção de períodos
            DateTime inicioCalculo = dtInicioAtivo < inicioPeriodo ? inicioPeriodo : dtInicioAtivo;
            DateTime fimCalculo = dtFimAtivo > fimPeriodo ? fimPeriodo : dtFimAtivo;

            if (fimCalculo <= inicioCalculo) return;

            // 3. Calcular meses no período
            int mesesNoPeriodo = ((fimCalculo.Year - inicioCalculo.Year) * 12) + 
                fimCalculo.Month - inicioCalculo.Month;
            if (mesesNoPeriodo <= 0) return;

            // 4. Cálculo do lucro bruto (rendas - despesas)
            decimal rendaBruta = imovel.Valorrenda * mesesNoPeriodo;
            decimal despesaAnualRespetiva = (imovel.Valoranualdespesas / 12) * mesesNoPeriodo;
            decimal despesaTotal = (imovel.Valormensalcondo * mesesNoPeriodo) + despesaAnualRespetiva;
    
            lucroBruto = rendaBruta - despesaTotal;
            lucroMedioMensalBruto = lucroBruto / mesesNoPeriodo;
            impostos = lucroBruto * (percImposto / 100);
            
            lucroDepoisImpostos = lucroBruto - impostos;
            lucroMensalMedioDepoisImpostos = lucroDepoisImpostos / mesesNoPeriodo;
        }

        public (decimal bruto, decimal imposto) CalcularLucroFundo(Fundoinvestimento fundo, decimal percImposto, DateTime inicio, DateTime fim)
        {
            // Implementar cálculo real baseado nas datas
            decimal rendimento = fundo.Montanteinvestido * (fundo.Taxajuropdefeito / 100);
            decimal imposto = rendimento * (percImposto / 100);
            return (rendimento, imposto);
        }
}