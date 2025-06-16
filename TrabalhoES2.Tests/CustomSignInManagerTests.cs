using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using TrabalhoES2.Models;
using TrabalhoES2.Services;

namespace TrabalhoES2.Tests
{
    [TestFixture]
    public class CustomSignInManagerTests
    {
        private CustomSignInManager _signInManager;
        private Mock<UserManager<Utilizador>> _userMgr;
        private Mock<IHttpContextAccessor> _httpCtx;
        private Mock<IUserClaimsPrincipalFactory<Utilizador>> _claimsFactory;
        private Mock<IOptions<IdentityOptions>> _opts;
        private Mock<ILogger<SignInManager<Utilizador>>> _logger;
        private Mock<IAuthenticationSchemeProvider> _schemes;
        private Mock<IUserConfirmation<Utilizador>> _confirmation;

        [SetUp]
        public void Setup()
        {
            _userMgr       = new Mock<UserManager<Utilizador>>(Mock.Of<IUserStore<Utilizador>>(),
                                    null, null, null, null, null, null, null, null);
            _httpCtx       = new Mock<IHttpContextAccessor>();
            _claimsFactory = new Mock<IUserClaimsPrincipalFactory<Utilizador>>();
            _opts          = new Mock<IOptions<IdentityOptions>>();
            _logger        = new Mock<ILogger<SignInManager<Utilizador>>>();
            _schemes       = new Mock<IAuthenticationSchemeProvider>();
            _confirmation  = new Mock<IUserConfirmation<Utilizador>>();

            _signInManager = new CustomSignInManager(
                _userMgr.Object,
                _httpCtx.Object,
                _claimsFactory.Object,
                _opts.Object,
                _logger.Object,
                _schemes.Object,
                _confirmation.Object
            );
        }

        [Test]
        public async Task PasswordSignInAsync_WhenUserIsBlocked_ReturnsLockedOut()
        {
            // Arrange
            var blocked = new Utilizador { UserName = "test", IsBlocked = true };
            _userMgr
              .Setup(m => m.FindByNameAsync("test"))
              .ReturnsAsync(blocked);

            // Act
            var result = await _signInManager.PasswordSignInAsync("test", "whatever", false, false);

            // Assert
            Assert.That(result.IsLockedOut, Is.True);
        }
    }
}
