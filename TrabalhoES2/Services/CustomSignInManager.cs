using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TrabalhoES2.Models;

namespace TrabalhoES2.Services
{
    public class CustomSignInManager : SignInManager<Utilizador>
    {
        public CustomSignInManager(
            UserManager<Utilizador> userManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<Utilizador> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<Utilizador>> logger,
            IAuthenticationSchemeProvider schemes,
            IUserConfirmation<Utilizador> confirmation)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
        }

        public override async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            var user = await UserManager.FindByNameAsync(userName);
            if (user == null)
            {
                return SignInResult.Failed;
            }

            // Verificar se o usuário está bloqueado ou removido
            if (user.IsDeleted)
            {
                return SignInResult.NotAllowed; // Usuário foi removido
            }
            
            if (user.IsBlocked)
            {
                return SignInResult.LockedOut; // Usuário está bloqueado
            }

            return await base.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);
        }
    }
}