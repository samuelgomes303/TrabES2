using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Threading.Tasks;
using TrabalhoES2.Controllers;
using TrabalhoES2.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using MockQueryable.Moq;

namespace TrabalhoES2.Tests.Controllers
{
    [TestFixture]
    public class FundosControllerTests
    {
        private Mock<projetoPraticoDbContext> _mockContext;
        private CarteiraController _controller;
        private List<Fundoinvestimento> _fundos;
        private List<Ativofinanceiro> _ativos;
        private List<Carteira> _carteiras;
        private int _userId = 99;

        [SetUp]
        public void Setup()
        {
            _fundos = new List<Fundoinvestimento>();
            _ativos = new List<Ativofinanceiro>();
            _carteiras = new List<Carteira>
            {
                new Carteira { CarteiraId = 1, UtilizadorId = _userId, Ativofinanceiros = new List<Ativofinanceiro>() }
            };

            var fundosDbSet = _fundos.AsQueryable().BuildMockDbSet();
            var ativosDbSet = _ativos.AsQueryable().BuildMockDbSet();
            var carteirasDbSet = _carteiras.AsQueryable().BuildMockDbSet();

            var options = new DbContextOptionsBuilder<projetoPraticoDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _mockContext = new Mock<projetoPraticoDbContext>(options);
            _mockContext.Setup(c => c.Fundoinvestimentos).Returns(fundosDbSet.Object);
            _mockContext.Setup(c => c.Ativofinanceiros).Returns(ativosDbSet.Object);
            _mockContext.Setup(c => c.Carteiras).Returns(carteirasDbSet.Object);

            _controller = new CarteiraController(_mockContext.Object);
            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, _userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        [Test]
        public async Task AdicionarQuantidadeFundo_QuantidadeInvalida_DeveRedirecionarComErro()
        {
            var fundo = new Fundoinvestimento { FundoinvestimentoId = 1, Quantidade = 0, Valoratual = 100m };
            _fundos.Add(fundo);

            var result = await _controller.AdicionarQuantidadeFundo(1, 10);

            Assert.IsInstanceOf<RedirectToActionResult>(result);
            Assert.AreEqual("AtivosCatalogo", ((RedirectToActionResult)result).ActionName);
            Assert.AreEqual("Quantidade inválida para cálculo.", _controller.TempData["Erro"]);
        }

        [Test]
        public async Task AdicionarQuantidadeFundo_QuantidadeValida_DeveAtualizarValores()
        {
            var fundo = new Fundoinvestimento { FundoinvestimentoId = 2, Quantidade = 10, Valoratual = 1000m };
            _fundos.Add(fundo);

            var result = await _controller.AdicionarQuantidadeFundo(2, 5);

            Assert.IsInstanceOf<RedirectToActionResult>(result);
            Assert.AreEqual(15, fundo.Quantidade);
            Assert.AreEqual(1500, fundo.Valoratual);
        }

        [Test]
        public async Task AdicionarFundo_Valido_DeveCriarFundoNaCarteiraDoUtilizador()
        {
            var ativo = new Ativofinanceiro { AtivofinanceiroId = 3, Duracaomeses = 12, Percimposto = 0.25m };
            var fundo = new Fundoinvestimento { FundoinvestimentoId = 3, BancoId = 1, Nome = "Fundo XP", Taxajuropdefeito = 5, Montanteinvestido = 100, Valoratual = 100, Ativofinanceiro = ativo };
            _fundos.Add(fundo);

            var result = await _controller.AdicionarFundo(3, 100);

            Assert.IsInstanceOf<RedirectToActionResult>(result);
            Assert.AreEqual("AtivosCatalogo", ((RedirectToActionResult)result).ActionName);
        }

        [Test]
        public void CalcularValorAtualComJuros_DeveRetornarValorCorreto()
        {
            var fundo = new Fundoinvestimento { Valoratual = 1000m, Taxajuropdefeito = 12m };
            var ativo = new Ativofinanceiro { Datainicio = DateOnly.FromDateTime(DateTime.Now.AddMonths(-6)), Duracaomeses = 12 };

            var valorCalculado = _controller.CalcularValorAtualComJuros(fundo, ativo);

            Assert.Greater(valorCalculado, 1000m);
        }

        [Test]
        public async Task EditFundo_Valido_AtualizaFundosEAtivo()
        {
            var fundo = new Fundoinvestimento { FundoinvestimentoId = 7, BancoId = 1, Nome = "Fundo A", Taxajuropdefeito = 5, Montanteinvestido = 100, AtivofinanceiroId = 7 };
            var ativo = new Ativofinanceiro { AtivofinanceiroId = 7, Duracaomeses = 6 };
            _fundos.Add(fundo);
            _ativos.Add(ativo);

            var result = await _controller.EditFundo(fundo, 12);

            Assert.IsInstanceOf<RedirectToActionResult>(result);
            Assert.AreEqual(12, ativo.Duracaomeses);
        }

        [Test]
        public async Task DeleteFundoConfirmed_RemoveFundoEAtivo()
        {
            var fundo = new Fundoinvestimento { FundoinvestimentoId = 8, AtivofinanceiroId = 8 };
            var ativo = new Ativofinanceiro { AtivofinanceiroId = 8 };
            _fundos.Add(fundo);
            _ativos.Add(ativo);

            var result = await _controller.DeleteFundoConfirmed(8);

            Assert.IsInstanceOf<RedirectToActionResult>(result);
        }

        [Test]
        public async Task CreateFundo_Valido_DeveCriarFundoComAtivo()
        {
            var fundo = new Fundoinvestimento { BancoId = 1, Nome = "Novo Fundo", Taxajuropdefeito = 5, Montanteinvestido = 200 };
            var ativo = new Ativofinanceiro { Duracaomeses = 12, Percimposto = 0.2m };

            var result = await _controller.CreateFundo(fundo, ativo);

            Assert.IsInstanceOf<RedirectToActionResult>(result);
        }

        [Test]
        public async Task EditFundo_FundoNaoExiste_DeveRetornarNotFound()
        {
            var result = await _controller.EditFundo(999);

            Assert.IsInstanceOf<NotFoundResult>(result);
        }
    }
}
