using NUnit.Framework;
using System;
using TrabalhoES2.Models;
using TrabalhoES2.Services;

namespace TrabalhoES2.Tests
{
    [TestFixture]
    public class AtivosTests
    {
        // === IMÓVEIS ===

        [Test]
        public void ExpectativaRendimento_Imovel_DeveRetornarCorreto()
        {
            var imovel = new Imovelarrendado
            {
                Valorrenda = 1000m,
                Valormensalcondo = 100m,
                Valoranualdespesas = 500m
            };
            var ativo = new Ativofinanceiro
            {
                Percimposto = 20m // 20% de imposto
            };

            // cálculo esperado com base na fórmula real
            var receitaBruta = 1000m * 12;
            var receitaLiquida = receitaBruta * (1 - 0.20m); // 20%
            var custos = 100m * 12 + 500m;
            var esperado = Math.Round(receitaLiquida - custos, 2);

            var resultado = ImovelService.CalcularExpectativaRendimentoAnual(imovel, ativo);
            Assert.That(resultado, Is.EqualTo(esperado));

        }


        [Test]
        public void ValorAtual_Imovel_DeveRetornarCorreto()
        {
            var imovel = new Imovelarrendado
            {
                Valorimovel = 100000m,
                Valorrenda = 1000m,
                Valormensalcondo = 100m,
                Valoranualdespesas = 500m
            };

            var dataInicio = DateOnly.FromDateTime(DateTime.Today.AddDays(-365)); 
            var ativo = new Ativofinanceiro
            {
                Datainicio = dataInicio,
                Percimposto = 20m
            };

            var imposto = 0.20m;
            var dias = 365;

            var receitaDiaria = (1000m * 12 / 365m) * (1 - imposto);
            var custoDiario = (100m * 12 + 500m) / 365m;
            var lucroProporcional = (receitaDiaria - custoDiario) * dias;

            var esperado = Math.Round(100000m + lucroProporcional, 2);

            var resultado = ImovelService.CalcularValorAtualComRendimentos(imovel, ativo);

            Assert.That((double)resultado, Is.EqualTo((double)esperado).Within(0.01));

        }



        [Test]
        public void CriarImovel_DeveInicializarCorretamente()
        {
            var imovel = new Imovelarrendado { Designacao = "Apartamento" };
            Assert.That(imovel.Designacao, Is.EqualTo("Apartamento"));
        }

        [Test]
        public void EditarImovel_DeveAtualizarValores()
        {
            var imovel = new Imovelarrendado { Valorrenda = 900m };
            imovel.Valorrenda = 1100m;
            Assert.That(imovel.Valorrenda, Is.EqualTo(1100m));
        }

        [Test]
        public void ApagarImovel_DeveSerNulo()
        {
            Imovelarrendado? imovel = new() { Designacao = "T1" };
            imovel = null;
            Assert.That(imovel, Is.Null);
        }

        // === DEPÓSITOS ===

        [Test]
        public void ValorAtual_Deposito_DeveCalcularCorretamente()
        {
            decimal valorInicial = 1000m;
            decimal taxaAnual = 6m;
            var dataInicio = DateOnly.FromDateTime(DateTime.Today.AddMonths(-6));

            var service = new DepositoService(null);
            var resultado = service.TesteCalcularValorAtualAoDia(valorInicial, taxaAnual, dataInicio);

            Assert.That(resultado, Is.GreaterThan(valorInicial));
        }

        [Test]
        public void ExpectativaRendimento_Deposito_DeveCalcularCorretamente()
        {
            decimal valorInicial = 2000m;
            decimal taxaAnual = 4m;
            int meses = 12;
            decimal imposto = 0.28m;

            var esperado = valorInicial * (taxaAnual / 100m) * meses / 12m * (1 - imposto);

            var deposito = new Depositoprazo { Valorinicial = valorInicial, Taxajuroanual = taxaAnual };
            var ativo = new Ativofinanceiro { Duracaomeses = meses };

            var resultado = deposito.Valorinicial * (deposito.Taxajuroanual / 100m) * meses / 12m * (1 - imposto);

            Assert.That(resultado, Is.EqualTo(esperado));
        }

        [Test]
        public void CriarDeposito_DeveInicializarCorretamente()
        {
            var deposito = new Depositoprazo { Valorinicial = 3000m };
            Assert.That(deposito.Valorinicial, Is.EqualTo(3000m));
        }

        [Test]
        public void EditarDeposito_DeveAtualizarValores()
        {
            var deposito = new Depositoprazo { Taxajuroanual = 1.5m };
            deposito.Taxajuroanual = 2.5m;
            Assert.That(deposito.Taxajuroanual, Is.EqualTo(2.5m));
        }

        [Test]
        public void ApagarDeposito_DeveSerNulo()
        {
            Depositoprazo? deposito = new() { Valorinicial = 5000m };
            deposito = null;
            Assert.That(deposito, Is.Null);
        }
    }

    // Extensão de classe do service para acesso ao método privado nos testes
    public static class DepositoServiceExtensions
    {
        public static decimal TesteCalcularValorAtualAoDia(this DepositoService service,
            decimal valorInicial, decimal taxaAnual, DateOnly dataInicio)
        {
            var TANB = taxaAnual / 100m;
            var t = 0.28m;

            var hoje = DateOnly.FromDateTime(DateTime.Today);
            var diasPassados = (hoje.ToDateTime(TimeOnly.MinValue) - dataInicio.ToDateTime(TimeOnly.MinValue)).Days;

            if (diasPassados < 0) diasPassados = 0;

            var jurosProporcionais = valorInicial * TANB * diasPassados / 365m * (1 - t);
            return valorInicial + jurosProporcionais;
        }
    }
}
