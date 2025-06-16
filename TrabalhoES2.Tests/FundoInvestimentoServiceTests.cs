// Projeto TrabalhoES2.Tests
using NUnit.Framework;
using TrabalhoES2.Services;
using TrabalhoES2.Models;
using System;

namespace TrabalhoES2.Tests
{
    [TestFixture]
    public class FundoInvestimentoServiceTests
    {
        [TestCase(100, 0, 1, 0)] // taxa zero gera zero rendimento
        [TestCase(100, 10, 1, 10)] // 10% 1 ano: 100*(1.1)-100 = 10
        [TestCase(200, 5, 2, 20.5)] // 5% 2 anos: 200*(1.1025)-200 = 20.5
        public void CalcularValorAtualComJuros_CasoBasico(decimal montanteInicial, decimal taxaPercent, int anos, decimal expectedJuros)
        {
            // Montar objeto-fake:
            var ativo = new Ativofinanceiro { Datainicio = DateOnly.FromDateTime(DateTime.Now), Duracaomeses = anos * 12 };
            var fundo = new Fundoinvestimento {
                Montanteinvestido = montanteInicial,
                Taxajuropdefeito = taxaPercent,
                Valoratual = montanteInicial
            };
            // Simular método: primeiro atualizar Valoratual via cálculo
            var atual = FundoInvestimentoService.CalcularValorAtualComJuros(fundo, ativo);
            // Expectativa:
            Assert.That(atual - montanteInicial, Is.EqualTo(expectedJuros));
        }

        [Test]
        public void CalcularValorAtualComJuros_DuracaoZero_RetornaMontanteAtual()
        {
            var ativo = new Ativofinanceiro { Datainicio = DateOnly.FromDateTime(DateTime.Now), Duracaomeses = 0 };
            var fundo = new Fundoinvestimento {
                Montanteinvestido = 150m,
                Taxajuropdefeito = 5m,
                Valoratual = 150m
            };
            var atual = FundoInvestimentoService.CalcularValorAtualComJuros(fundo, ativo);
            Assert.That(atual, Is.EqualTo(150m));
        }
    }

    [TestFixture]
    public class ImovelServiceTests
    {
        [Test]
        public void CalcularValorAtualComRendimentos_ImovelNulo_LancaExcecaoArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                ImovelService.CalcularValorAtualComRendimentos(null, new Ativofinanceiro()));
        }

        [Test]
        public void CalcularExpectativaRendimentoAnual_ExemploSimples()
        {
            // Crie um Imovelarrendado com valores de teste:
            var imovel = new Imovelarrendado {
                Valorimovel = 100000m,
                Valorrenda = 1000m, // mensal
                Valormensalcondo = 100m,
                Valoranualdespesas = 1200m
            };
            var ativo = new Ativofinanceiro {
                Datainicio = DateOnly.FromDateTime(DateTime.Now),
                Duracaomeses = 12
            };
            var exp = ImovelService.CalcularExpectativaRendimentoAnual(imovel, ativo);
            // Verifica que exp = (renda*12) - (condo*12 + despesas anuais)
            var esperado = (1000m * 12) - (100m * 12 + 1200m);
            Assert.That(exp, Is.EqualTo(esperado));
        }
    }
}
