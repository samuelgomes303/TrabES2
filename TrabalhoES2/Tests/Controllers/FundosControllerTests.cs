using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using TrabalhoES2.Controllers;
using TrabalhoES2.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TrabalhoES2.Tests.Controllers
{
    [TestFixture]
    public class CarteiraControllerTests
    {
        private projetoPraticoDbContext _context;
        private CarteiraController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<projetoPraticoDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            _context = new projetoPraticoDbContext(options);
            _controller = new CarteiraController(_context);
        }

        [Test]
        public async Task AdicionarQuantidadeFundo_QuantidadeInvalida_DeveRetornarErro()
        {
            // Arrange
            var fundo = new Fundoinvestimento
            {
                FundoinvestimentoId = 1,
                Quantidade = 0,
                Valoratual = 1000m
            };
            await _context.Fundoinvestimentos.AddAsync(fundo);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.AdicionarQuantidadeFundo(1, -5);

            // Assert
            Assert.IsInstanceOf<RedirectToActionResult>(result);
            var redirect = result as RedirectToActionResult;
            Assert.AreEqual("AtivosCatalogo", redirect.ActionName);
        }

        [Test]
        public async Task AdicionarQuantidadeFundo_QuantidadeValida_DeveAtualizarValores()
        {
            // Arrange
            var fundo = new Fundoinvestimento
            {
                FundoinvestimentoId = 2,
                Quantidade = 1,
                Valoratual = 1000m
            };
            await _context.Fundoinvestimentos.AddAsync(fundo);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.AdicionarQuantidadeFundo(2, 500);

            // Assert
            var fundoAtualizado = await _context.Fundoinvestimentos.FindAsync(2);
            Assert.IsNotNull(fundoAtualizado);
            Assert.AreEqual(1 + 500, fundoAtualizado.Quantidade);
            Assert.AreEqual(1000m + (1000m / 1) * 500, fundoAtualizado.Valoratual);

            Assert.IsInstanceOf<RedirectToActionResult>(result);
            var redirect = result as RedirectToActionResult;
            Assert.AreEqual("AtivosCatalogo", redirect.ActionName);
        }
    }
}
