using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
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
    public class UserControllerTests
    {
        private Mock<UserManager<Utilizador>> _mockUserManager;
        private Mock<projetoPraticoDbContext> _mockContext;
        private Mock<DbSet<Utilizador>> _mockDbSet;
        private UserController _controller;
        private List<Utilizador> _users;

        [SetUp]
        public void Setup()
        {
            // Setup test data
            _users = new List<Utilizador>
            {
                new Utilizador 
                { 
                    Id = 1, 
                    Nome = "Joao Santos", 
                    Email = "admin@example.com", 
                    UserName = "admin@example.com",
                    TpUtilizador = Utilizador.TipoUtilizador.Admin,
                    IsDeleted = false,
                    IsBlocked = false
                },
                new Utilizador 
                { 
                    Id = 17, 
                    Nome = "client", 
                    Email = "client@gmail.com", 
                    UserName = "client@gmail.com",
                    TpUtilizador = Utilizador.TipoUtilizador.Cliente,
                    IsDeleted = false,
                    IsBlocked = false
                },
                new Utilizador 
                { 
                    Id = 16, 
                    Nome = "usermanager", 
                    Email = "usermanager@gmail.com", 
                    UserName = "usermanager@gmail.com",
                    TpUtilizador = Utilizador.TipoUtilizador.UserManager,
                    IsDeleted = false,
                    IsBlocked = false
                },
                new Utilizador 
                { 
                    Id = 5, 
                    Nome = "tone", 
                    Email = "tone@gmail.com", 
                    UserName = "tone@gmail.com",
                    TpUtilizador = Utilizador.TipoUtilizador.Admin,
                    IsDeleted = true,
                    IsBlocked = false,
                    DeletedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            // Setup UserManager mock
            var store = new Mock<IUserStore<Utilizador>>();
            _mockUserManager = new Mock<UserManager<Utilizador>>(store.Object, null, null, null, null, null, null, null, null);

            // Setup DbContext and DbSet mocks using MockQueryable
            var options = new DbContextOptionsBuilder<projetoPraticoDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            
            _mockContext = new Mock<projetoPraticoDbContext>(options);
            
            // Use MockQueryable to create a proper mock DbSet
            _mockDbSet = _users.AsQueryable().BuildMockDbSet();
            _mockContext.Setup(c => c.Utilizadors).Returns(_mockDbSet.Object);

            // Create controller instance
            _controller = new UserController(_mockUserManager.Object, _mockContext.Object);

            // Setup TempData for success messages
            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            // Setup ControllerContext for User claims
            SetupControllerContextForRole("Admin");
        }

        private void SetupControllerContextForRole(string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };
        }

        [Test]
        public async Task Index_AsAdmin_ReturnsAllUsers()
        {
            // Arrange
            SetupControllerContextForRole("Admin");

            // Act
            var result = await _controller.Index(false);

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.IsInstanceOf<List<Utilizador>>(viewResult.Model);
            var model = viewResult.Model as List<Utilizador>;
            Assert.AreEqual(3, model.Count); // Não inclui os deleted
        }

        [Test]
        public async Task Index_AsAdmin_IncludeDeleted_ReturnsAllUsersIncludingDeleted()
        {
            // Arrange
            SetupControllerContextForRole("Admin");

            // Act
            var result = await _controller.Index(true);

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.IsInstanceOf<List<Utilizador>>(viewResult.Model);
            var model = viewResult.Model as List<Utilizador>;
            Assert.AreEqual(4, model.Count); // Inclui os deleted
            Assert.IsTrue(viewResult.ViewData["IncludeDeleted"].Equals(true));
        }

        [Test]
        public async Task Index_AsUserManager_ReturnsOnlyClientes()
        {
            // Arrange
            SetupControllerContextForRole("UserManager");

            // Act
            var result = await _controller.Index(false);

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            var model = viewResult.Model as List<Utilizador>;
            Assert.AreEqual(1, model.Count); // Só um cliente na lista
            Assert.AreEqual(Utilizador.TipoUtilizador.Cliente, model.First().TpUtilizador);
        }

        [Test]
        public async Task Details_ValidIdAsAdmin_ReturnsUserDetails()
        {
            // Arrange
            var userId = 1;
            SetupControllerContextForRole("Admin");

            // Act
            var result = await _controller.Details(userId);

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.IsInstanceOf<Utilizador>(viewResult.Model);
            var model = viewResult.Model as Utilizador;
            Assert.AreEqual("Joao Santos", model.Nome);
            Assert.IsTrue(viewResult.ViewData["IsAdmin"].Equals(true));
        }

        [Test]
        public async Task Details_UserManagerTryingToAccessAdmin_ReturnsForbid()
        {
            // Arrange
            var userId = 1; // Admin user
            SetupControllerContextForRole("UserManager");

            // Act
            var result = await _controller.Details(userId);

            // Assert
            Assert.IsInstanceOf<ForbidResult>(result);
        }

        [Test]
        public void Create_AsAdmin_ReturnsViewWithAllTipoUtilizadorOptions()
        {
            // Arrange
            SetupControllerContextForRole("Admin");

            // Act
            var result = _controller.Create();

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult.ViewData["TipoUtilizadorOptions"]);
        }

        [Test]
        public void Create_AsUserManager_ReturnsViewWithOnlyClienteOption()
        {
            // Arrange
            SetupControllerContextForRole("UserManager");

            // Act
            var result = _controller.Create();

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult.ViewData["TipoUtilizadorOptions"]);
        }

        [Test]
        public async Task Create_ValidData_CreatesUserSuccessfully()
        {
            // Arrange
            SetupControllerContextForRole("Admin");
            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<Utilizador>(), It.IsAny<string>()))
                           .ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<Utilizador>(), It.IsAny<string>()))
                           .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.Create("Test User", "test@example.com", "Test123!", "Test123!", Utilizador.TipoUtilizador.Cliente);

            // Assert
            Assert.IsInstanceOf<RedirectToActionResult>(result);
            var redirectResult = result as RedirectToActionResult;
            Assert.AreEqual("Index", redirectResult.ActionName);
            Assert.AreEqual("Utilizador criado com sucesso!", _controller.TempData["SuccessMessage"]);
        }

        [Test]
        public async Task Create_UserManagerTryingToCreateAdmin_AddsModelError()
        {
            // Arrange
            SetupControllerContextForRole("UserManager");

            // Act
            var result = await _controller.Create("Test Admin", "admin@test.com", "Test123!", "Test123!", Utilizador.TipoUtilizador.Admin);

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            Assert.IsFalse(_controller.ModelState.IsValid);
            Assert.IsTrue(_controller.ModelState.ContainsKey(""));
        }

        [Test]
        public async Task Create_PasswordMismatch_ReturnsViewWithError()
        {
            // Arrange
            SetupControllerContextForRole("Admin");

            // Act
            var result = await _controller.Create("Test User", "test@example.com", "Test123!", "DifferentPassword", Utilizador.TipoUtilizador.Cliente);

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            Assert.IsFalse(_controller.ModelState.IsValid);
        }

        [Test]
        public async Task Edit_ValidId_ReturnsEditView()
        {
            // Arrange
            var userId = 17; // Cliente
            _mockContext.Setup(c => c.Utilizadors.FindAsync(userId))
                       .ReturnsAsync(_users.First(u => u.Id == userId));
            SetupControllerContextForRole("Admin");

            // Act
            var result = await _controller.Edit(userId);

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.IsInstanceOf<Utilizador>(viewResult.Model);
        }

        [Test]
        public async Task Block_ValidClienteAsUserManager_ReturnsBlockView()
        {
            // Arrange
            var userId = 17; // Cliente
            SetupControllerContextForRole("UserManager");

            // Act
            var result = await _controller.Block(userId);

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.IsInstanceOf<Utilizador>(viewResult.Model);
            Assert.IsTrue(viewResult.ViewData["IsUserManager"].Equals(true));
        }

        [Test]
        public async Task BlockConfirmed_ValidUser_BlocksUserSuccessfully()
        {
            // Arrange
            var userId = 17;
            var user = _users.First(u => u.Id == userId);
            _mockContext.Setup(c => c.Utilizadors.FindAsync(userId)).ReturnsAsync(user);
            _mockContext.Setup(c => c.Update(It.IsAny<Utilizador>()));
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);
            SetupControllerContextForRole("UserManager");

            // Act
            var result = await _controller.BlockConfirmed(userId);

            // Assert
            Assert.IsInstanceOf<RedirectToActionResult>(result);
            Assert.IsTrue(user.IsBlocked);
            Assert.IsNotNull(user.BlockedAt);
            Assert.AreEqual("Utilizador bloqueado com sucesso!", _controller.TempData["SuccessMessage"]);
        }

        [Test]
        public async Task DeleteConfirmed_AsAdmin_SoftDeletesUser()
        {
            // Arrange
            var userId = 17;
            var user = _users.First(u => u.Id == userId);
            _mockContext.Setup(c => c.Utilizadors.FindAsync(userId)).ReturnsAsync(user);
            _mockContext.Setup(c => c.Update(It.IsAny<Utilizador>()));
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);
            SetupControllerContextForRole("Admin");

            // Act
            var result = await _controller.DeleteConfirmed(userId);

            // Assert
            Assert.IsInstanceOf<RedirectToActionResult>(result);
            Assert.IsTrue(user.IsDeleted);
            Assert.IsNotNull(user.DeletedAt);
            Assert.AreEqual("Utilizador removido com sucesso!", _controller.TempData["SuccessMessage"]);
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }
    }
}