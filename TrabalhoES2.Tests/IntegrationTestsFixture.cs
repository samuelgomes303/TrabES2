using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TrabalhoES2.Models;

namespace TrabalhoES2.Tests
{
    public class IntegrationTestsFixture
    {
        protected WebApplicationFactory<Program> _factory = null!;
        protected HttpClient _client = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder => {
                    builder.UseSetting(Microsoft.AspNetCore.Hosting.WebHostDefaults.EnvironmentKey, "Test");
                    builder.ConfigureServices(services => {
                        // Remove DbContext original
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<projetoPraticoDbContext>));
                        if (descriptor != null) services.Remove(descriptor);

                        // Registra InMemory para testes
                        services.AddDbContext<projetoPraticoDbContext>(opts =>
                            opts.UseInMemoryDatabase("IntegrationTestDb"));

                        // Build para fazer seed
                        var sp = services.BuildServiceProvider();
                        using var scope = sp.CreateScope();
                        var ctx = scope.ServiceProvider.GetRequiredService<projetoPraticoDbContext>();
                        ctx.Database.EnsureDeleted();
                        ctx.Database.EnsureCreated();
                        // Seed de usuário Id=1 e carteira
                        ctx.Users.Add(new Utilizador {
                        Id = 1,
                        UserName = "test@example.com",
                        Email = "test@example.com",
                        Nome = "Teste",
                        TpUtilizador = Utilizador.TipoUtilizador.Cliente,
                        SecurityStamp = Guid.NewGuid().ToString() // Corrige erro de security stamp null
                    });
                        ctx.Carteiras.Add(new Carteira {
                            CarteiraId = 1,
                            Nome = "CarteiraTeste",
                            UtilizadorId = 1
                        });
                        ctx.SaveChanges();
                    });
                });
            _client = _factory.CreateClient();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }
    }

    [TestFixture]
    public class CarteiraIntegrationTests : IntegrationTestsFixture
    {
        [Test]
        public async Task Get_Index_Unauthenticated_RedirectsToLogin()
        {
            var unauthClient = _factory.WithWebHostBuilder(builder => {
                builder.UseSetting("environment", "Test");
                builder.ConfigureServices(services => {
                    // Remove TestAuthHandler to simulate unauthenticated
                    var descriptor = services.SingleOrDefault(d => d.ServiceType.Name == "TestAuthHandler");
                    if (descriptor != null) services.Remove(descriptor);
                    // Set default scheme to Identity's application cookie
                    services.PostConfigure<Microsoft.AspNetCore.Authentication.AuthenticationOptions>(options => {
                        options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme;
                        options.DefaultChallengeScheme = Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme;
                    });
                });
            }).CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions {
                AllowAutoRedirect = false
            });
            var response = await unauthClient.GetAsync("/Carteira/Index");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
            Assert.That(response.Headers.Location?.ToString() ?? string.Empty, Does.Contain("/Account/Login").Or.Contain("/Identity/Account/Login"));
        }

        [Test]
        public async Task Get_Index_AfterLogin_Returns200()
        {
            var response = await _client.GetAsync("/Carteira/Index");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content = await response.Content.ReadAsStringAsync();
            Assert.That(content, Does.Contain("Minha Carteira"));
        }
    }
}
