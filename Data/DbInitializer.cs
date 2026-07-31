using Microsoft.AspNetCore.Identity;

namespace EWasteSolutions.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

            var userManager = serviceProvider
                .GetRequiredService<UserManager<IdentityUser>>();

            const string adminRole = "Admin";
            const string adminEmail = "admin@ewastesolutions.com";
            const string adminPassword = "Admin@12345";

            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                var roleResult = await roleManager.CreateAsync(
                    new IdentityRole(adminRole));

                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        roleResult.Errors.Select(error => error.Description));

                    throw new InvalidOperationException(
                        $"Unable to create Admin role: {errors}");
                }
            }

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var userResult = await userManager.CreateAsync(
                    adminUser,
                    adminPassword);

                if (!userResult.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        userResult.Errors.Select(error => error.Description));

                    throw new InvalidOperationException(
                        $"Unable to create admin user: {errors}");
                }
            }

            if (!await userManager.IsInRoleAsync(adminUser, adminRole))
            {
                var addToRoleResult = await userManager.AddToRoleAsync(
                    adminUser,
                    adminRole);

                if (!addToRoleResult.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        addToRoleResult.Errors.Select(error => error.Description));

                    throw new InvalidOperationException(
                        $"Unable to assign Admin role: {errors}");
                }
            }
        }
    }
}