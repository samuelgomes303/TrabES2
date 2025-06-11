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
    
    
    public List<(DateTime Mes, decimal Saldo)> CalcularEvolucaoDepositoMensal(
        Depositoprazo deposito,
        DateTime dataInicio,
        int duracaoMeses
    )
    {
        var resultado = new List<(DateTime, decimal)>();
        decimal saldo = deposito.Valorinicial;
        decimal taxaMensal = deposito.Taxajuroanual / 12 / 100;

        var mesCorrente = new DateTime(dataInicio.Year, dataInicio.Month, 1);

        for (int i = 0; i <= duracaoMeses; i++)
        {
            resultado.Add((mesCorrente, saldo));
            saldo *= (1 + taxaMensal);
            mesCorrente = mesCorrente.AddMonths(1);
        }

        return resultado;
    }
    
    public List<(DateTime Mes, decimal Saldo)> CalcularEvolucaoImovelMensal(
        Imovelarrendado imovel,
        DateTime dataInicio,
        int duracaoMeses)
    {
        var resultado = new List<(DateTime, decimal)>();
        decimal saldo = 0;
        decimal despesasMensais = (imovel.Valoranualdespesas / 12M) + imovel.Valormensalcondo;
        decimal lucroMensal = imovel.Valorrenda - despesasMensais;

        var mesCorrente = new DateTime(dataInicio.Year, dataInicio.Month, 1);

        for (int i = 0; i <= duracaoMeses; i++)
        {
            resultado.Add((mesCorrente, saldo));
            saldo += lucroMensal;
            mesCorrente = mesCorrente.AddMonths(1);
        }

        return resultado;
    }
    
    public List<(DateTime Mes, decimal Saldo)> CalcularEvolucaoFundoMensal(
        Fundoinvestimento fundo,
        DateTime dataInicio,
        int duracaoMeses)
    {
        var resultado = new List<(DateTime, decimal)>();
        decimal saldo = fundo.Montanteinvestido;
        decimal taxaMensal = fundo.Taxajuropdefeito / 12M / 100M;

        var mesCorrente = new DateTime(dataInicio.Year, dataInicio.Month, 1);

        for (int i = 0; i <= duracaoMeses; i++)
        {
            resultado.Add((mesCorrente, saldo));
            saldo *= (1 + taxaMensal);
            mesCorrente = mesCorrente.AddMonths(1);
        }

        return resultado;
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

        public decimal CalcularImpostoMensalDeposito(Depositoprazo ativoDeposito, decimal percImposto, DateTime dt)
        {
            // A diferença entre o valor atual e o valor inicial será o rendimento
            decimal rendimento = ativoDeposito.Valoratual - ativoDeposito.Valorinicial;

            // O imposto mensal será o rendimento multiplicado pela percentagem de imposto
            decimal impostoMensal = rendimento * (percImposto / 100);

            // Retorna o imposto mensal calculado
            return impostoMensal;
        }

        public decimal CalcularImpostoMensalImovel(Imovelarrendado ativoImovel, decimal percImposto, DateTime dt)
        {
            throw new NotImplementedException();
            
        }

        public decimal CalcularImpostoMensalFundo(Fundoinvestimento ativoFundo, decimal percImposto, DateTime dt)
        {
            // O rendimento do fundo é a diferença entre o valor atual e o valor inicial
            // FALTA ACABAR
            decimal rendimento = ativoFundo.Montanteinvestido;

            // O imposto mensal será o rendimento multiplicado pela percentagem de imposto
            decimal impostoMensal = rendimento * (percImposto / 100);

            // Retorna o imposto mensal calculado
            return impostoMensal;
        }
}
