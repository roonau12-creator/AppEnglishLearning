using AppLearningEnglish.Models;
using Microsoft.AspNetCore.Identity;

namespace AppLearningEnglish.Identity
{
    public static class IdentityDataSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var roleManager = scope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();
            var config = scope.ServiceProvider
                .GetRequiredService<IConfiguration>();

            string[] roles =
            {
                AppRoles.Admin,
                AppRoles.User
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminEmail = config["AdminAccount:Email"];
            var adminPassword = config["AdminAccount:Password"];
            var adminName = config["AdminAccount:FullName"] ?? "Administrator";

            if (!string.IsNullOrWhiteSpace(adminEmail) &&
                !string.IsNullOrWhiteSpace(adminPassword))
            {
                var admin = await userManager.FindByEmailAsync(adminEmail);

                if (admin == null)
                {
                    admin = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FullName = adminName,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var createResult =
                        await userManager.CreateAsync(admin, adminPassword);

                    if (!createResult.Succeeded)
                    {
                        throw new InvalidOperationException(
                            string.Join("; ", createResult.Errors.Select(x => x.Description)));
                    }
                }

                if (!await userManager.IsInRoleAsync(admin, AppRoles.Admin))
                {
                    await userManager.AddToRoleAsync(admin, AppRoles.Admin);
                }
            }

            var users = userManager.Users.ToList();

            foreach (var user in users)
            {
                if (!user.LockoutEnabled)
                {
                    await userManager.SetLockoutEnabledAsync(user, true);
                }

                var currentRoles = await userManager.GetRolesAsync(user);

                if (currentRoles.Count == 0)
                {
                    await userManager.AddToRoleAsync(user, AppRoles.User);
                }
            }
        }
    }
}
