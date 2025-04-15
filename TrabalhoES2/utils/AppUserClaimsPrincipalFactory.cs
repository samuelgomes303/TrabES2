using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;
using TrabalhoES2.Models;

namespace TrabalhoES2.utils // ou Services, se colocares noutra pasta
{
    public class AppUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<Utilizador, IdentityRole<int>>
    {
        public AppUserClaimsPrincipalFactory(
            UserManager<Utilizador> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(Utilizador user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim("TpUtilizador", user.TpUtilizador.ToString()));

            return identity;
        }
    }
}