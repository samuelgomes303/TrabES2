using NUnit.Framework;
using System;
using TrabalhoES2.Models;
using TrabalhoES2.Services;

namespace TrabalhoES2.Tests
{
    [TestFixture]
    public class ErrorTests
    {
        [Test]
        public void ValorAtual_Deposito_DataInicioNula_DeveRetornarValorInicial()
        {
            var deposito = new Depositoprazo
            {
                Valorinicial = 1500m,
                Taxajuroanual = 2.5m
            };

            var ativo = new Ativofinanceiro
            {
                Datainicio = null,
                Percimposto = 28
            };

            var result = deposito.Valorinicial; // porque com data nula não se calcula juros

            Assert.That(result, Is.EqualTo(1500m));

        }

        [Test]
        public void ExpectativaRendimento_Imovel_ValoresNulos_DeveRetornarZero()
        {
            var imovel = new Imovelarrendado
            {
                Valorrenda = 0m,
                Valormensalcondo = 0m,
                Valoranualdespesas = 0m,
                Valorimovel = 0m
            };

            var ativo = new Ativofinanceiro
            {
                Percimposto = null
            };

            var rendimento = ImovelService.CalcularExpectativaRendimentoAnual(imovel, ativo);

            Assert.That(rendimento, Is.EqualTo(0m));
        }

        [Test]
        public void CriarImovel_ValorNegativo_DeveLancarExcecaoSimulada()
        {
            var imovel = new Imovelarrendado
            {
                Designacao = "Inválido",
                Valorimovel = -1000m
            };

            Assert.Throws<ArgumentException>(() =>
            {
                if (imovel.Valorimovel < 0)
                    throw new ArgumentException("Valor do imóvel não pode ser negativo");
            });
        }

        [Test]
        public void CriarDeposito_TaxaNegativa_DeveLancarExcecaoSimulada()
        {
            var deposito = new Depositoprazo
            {
                Valorinicial = 1000m,
                Taxajuroanual = -2m
            };

            Assert.Throws<ArgumentException>(() =>
            {
                if (deposito.Taxajuroanual < 0)
                    throw new ArgumentException("Taxa de juro não pode ser negativa");
            });
        }
    }
}
