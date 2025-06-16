using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;
using TrabalhoES2.Controllers;
using TrabalhoES2.Models;

namespace TrabalhoES2.Tests
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
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new projetoPraticoDbContext(options);

            // Seed Banco entity required for navigation properties
            _context.Bancos.Add(new Banco {
                BancoId = 1,
                Nome = "Banco Teste"
            });

            // 1) Seed de Utilizador + Carteira
            var user = new Utilizador
            {
                Id = 1,
                UserName = "u1@example.com",
                Email = "u1@example.com",
                Nome = "Teste Cliente",
                TpUtilizador = Utilizador.TipoUtilizador.Cliente,
                SecurityStamp = Guid.NewGuid().ToString() // Corrige erro de security stamp null
            };
            _context.Users.Add(user);
            _context.Carteiras.Add(new Carteira
            {
                CarteiraId = 1,
                Nome = "Carteira 1",
                UtilizadorId = 1
            });

            // 2) Primeiro ativo: DepósitoPrazo com TITULAR = "ABC"
            _context.Ativofinanceiros.Add(new Ativofinanceiro
            {
                AtivofinanceiroId = 1,
                CarteiraId = 1,
                Datainicio = DateOnly.FromDateTime(DateTime.Now),
                Duracaomeses = 12
            });
            _context.Depositoprazos.Add(new Depositoprazo
            {
                DepositoprazoId = 1,
                AtivofinanceiroId = 1,
                BancoId = 1,
                Nrconta = "ACC1",
                Titular = "ABC",
                Taxajuroanual = 1.5m,
                Valorinicial = 100m,
                Valoratual = 100m
            });

            // 3) Segundo ativo: FundoInvestimento com NOME = "XYZ"
            _context.Ativofinanceiros.Add(new Ativofinanceiro
            {
                AtivofinanceiroId = 2,
                CarteiraId = 1,
                Datainicio = DateOnly.FromDateTime(DateTime.Now),
                Duracaomeses = 12
            });
            _context.Fundoinvestimentos.Add(new Fundoinvestimento
            {
                FundoinvestimentoId = 2,
                AtivofinanceiroId = 2,
                BancoId = 1,
                Nome = "XYZ",
                Montanteinvestido = 200m,
                Taxajuropdefeito = 1.2m,
                Valoratual = 200m
            });



            _context.SaveChanges();

            // Prepara o controller com HttpContext/User
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]{
       new Claim(ClaimTypes.NameIdentifier, "1")
    }, "test"));

            _controller = new CarteiraController(_context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                }
            };
        }

        [TearDown]
        public void TearDown()
        {
            if (_controller is IDisposable disposableController)
            {
                disposableController.Dispose();
            }
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private CarteiraController CreateController()
        {
            var controller = new CarteiraController(_context);
            // Mock authentication
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "TestAuth"));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            // Setup TempData
            var tempDataProvider = new Mock<ITempDataProvider>();
            controller.TempData = new TempDataDictionary(controller.ControllerContext.HttpContext, tempDataProvider.Object);
            return controller;
        }

        [Test]
        public async Task Index_WithDesignationFilter_ReturnsOnlyMatchingAsset()
        {
            // Act
            var controller = CreateController();
            var result = await controller.Index("ABC", "", default) as ViewResult;
            var model = result?.Model as Carteira;

            // Assert
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Ativofinanceiros.Count, Is.EqualTo(1));
            Assert.That(model.Ativofinanceiros.First().AtivofinanceiroId, Is.EqualTo(1));
        }

        [Test]
        public async Task Index_WithTipoFilter_ReturnsOnlyDepositoPrazoAssets()
        {
            // Act
            var controller = CreateController();
            var result = await controller.Index("", "DepositoPrazo", default) as ViewResult;
            var model = result?.Model as Carteira;

            // Assert
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Ativofinanceiros.Count, Is.EqualTo(1));
            Assert.That(model.Ativofinanceiros.First().Depositoprazo, Is.Not.Null);
        }

        [Test]
        public async Task Index_WithMontanteFilter_ReturnsOnlyAssetsAboveThreshold()
        {
            var controller = CreateController();
            // suponha que existam ativos com valores 100 e 200; filtro 150 deve retornar só 200
            var result = await controller.Index("", "", 150m) as ViewResult;
            var model = result?.Model as Carteira;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Ativofinanceiros.All(a =>
            {
                var valor = a.Depositoprazo != null
                    ? a.Depositoprazo.Valoratual
                    : a.Fundoinvestimento != null
                        ? a.Fundoinvestimento.Valoratual
                        : 0m;
                return valor >= 150m;
            }));
        }

        [Test]
        public async Task Index_WithCombinedFilters_ReturnsCorrectAssets()
        {
            var controller = CreateController();
            // designacao "ABC" e tipo "DepositoPrazo" simultâneo
            var result = await controller.Index("ABC", "DepositoPrazo", default) as ViewResult;
            var model = result?.Model as Carteira;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Ativofinanceiros.Count, Is.EqualTo(1));
            Assert.That(model.Ativofinanceiros.First().Depositoprazo!.Titular, Is.EqualTo("ABC"));
        }

        [Test]
        public async Task Remover_GetRedirectsToIndexAndRemovesAsset()
        {
            var controller = CreateController();
            // Adiciona um ativo extra
            var ativo = new Ativofinanceiro { AtivofinanceiroId = 99, CarteiraId = 1 };
            ativo.Carteira = _context.Carteiras.First(c => c.CarteiraId == 1); // Ensure navigation property is set
            _context.Ativofinanceiros.Add(ativo);
            // Adiciona navigation property obrigatória para evitar NullReference
            _context.Depositoprazos.Add(new Depositoprazo {
                DepositoprazoId = 99,
                AtivofinanceiroId = 99,
                BancoId = 1,
                Nrconta = "ACC99",
                Titular = "Test",
                Taxajuroanual = 1.0m,
                Valorinicial = 100m,
                Valoratual = 100m,
                Banco = _context.Bancos.First(b => b.BancoId == 1) // Ensure navigation property is set
            });
            _context.SaveChanges();
            // Reload ativo to check if CarteiraId is set
            var ativoReloaded = _context.Ativofinanceiros.First(a => a.AtivofinanceiroId == 99);
            Assert.That(ativoReloaded.CarteiraId, Is.EqualTo(1), "CarteiraId should be set to 1 after save");
            // Check if carteira for user 1 exists
            var carteira = _context.Carteiras.FirstOrDefault(c => c.UtilizadorId == 1);
            Assert.That(carteira, Is.Not.Null, "Carteira for user 1 should exist");
            // GET: Remover (should redirect to Index, not remove)
            var result = await controller.Remover(99) as RedirectToActionResult;
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ActionName, Is.EqualTo("Index"));
            // POST: RemoverConfirmado (actually removes)
            var postResult = await controller.RemoverConfirmado(99) as RedirectToActionResult;
            Assert.That(postResult, Is.Not.Null);
            Assert.That(postResult!.ActionName, Is.EqualTo("Index"));
            // Verificar que não existe mais o ativo
            var exists = _context.Ativofinanceiros.Any(a => a.AtivofinanceiroId == 99);
            Assert.That(exists, Is.False);
        }

        [Test]
        public async Task Index_WithoutUser_RedirectsToLogin()
        {
            var controller = new CarteiraController(_context);
            controller.ControllerContext = new ControllerContext {
                HttpContext = new DefaultHttpContext() // sem User ou sem claim id
            };
            var result = await controller.Index("", "", default);
            Assert.That(result, Is.InstanceOf(typeof(RedirectToActionResult)));
            var redirect = result as RedirectToActionResult;
            Assert.That(redirect!.ActionName, Is.EqualTo("Login"));
            Assert.That(redirect.ControllerName, Is.EqualTo("Account"));
        }


    }


    }

