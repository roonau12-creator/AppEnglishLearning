using System.Security.Claims;
using AppLearningEnglish.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace AppLearningEnglish.Identity
{
    public class AppUserClaimsPrincipalFactory
        : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        public AppUserClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> options)
            : base(userManager, roleManager, options)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            if (!string.IsNullOrWhiteSpace(user.FullName))
            {
                identity.AddClaim(new Claim("FullName", user.FullName));
            }

            if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
            {
                identity.AddClaim(new Claim("AvatarUrl", user.AvatarUrl));
            }

            return identity;
        }
    }
}
