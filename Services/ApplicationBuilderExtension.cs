using ASPMMA;
using ASPMMA.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ASPMMA.Services
{
    public static class ApplicationBuilderExtension
    {
        public static async Task<IApplicationBuilder> PrepareDataBase(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();

            var services = scope.ServiceProvider;
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();

            try
            {
                var dbContext = services.GetRequiredService<ApplicationDbContext>();
                var userManager = services.GetRequiredService<UserManager<Client>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                await dbContext.Database.MigrateAsync();
                await EnsureOrderStatusColumnAsync(dbContext);
                await SeedRolesAsync(roleManager);
                await SeedSuperAdminAsync(userManager);
            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger<Program>();
                logger.LogError(ex, "An error occurred seeding the DB.");
            }

            return app;
        }

        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            var roleNames = new[] { "Admin", "Client", "User" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        public static async Task SeedSuperAdminAsync(UserManager<Client> userManager)
        {
            var defaultUser = new Client
            {
                UserName = "superadmin",
                Email = "superadmin@gmail.com",
                FirstName = "Ivan",
                LastName = "Ivanov",
                PhoneNumber = "0899999999",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                var result = await userManager.CreateAsync(defaultUser, "123!@#Qwe");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(defaultUser, "Admin");
                }
            }
        }

        private static async Task EnsureOrderStatusColumnAsync(ApplicationDbContext dbContext)
        {
            const string sql = """
                IF OBJECT_ID(N'[Orders]', N'U') IS NOT NULL AND COL_LENGTH(N'Orders', N'Status') IS NULL
                BEGIN
                    ALTER TABLE [Orders] ADD [Status] int NOT NULL CONSTRAINT [DF_Orders_Status] DEFAULT 0;
                END
                """;

            await dbContext.Database.ExecuteSqlRawAsync(sql);
        }
    }
}
