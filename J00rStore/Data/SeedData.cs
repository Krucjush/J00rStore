using J00rStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace J00rStore.Data
{
	public static class SeedData
	{
		public static async Task EnsurePopulated(IApplicationBuilder app)
		{
			var scope = app.ApplicationServices.CreateScope();
			var context = scope.ServiceProvider.GetRequiredService<AppDBContext>();
			var storeContext = scope.ServiceProvider.GetRequiredService<AppStoreContext>();
			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
			var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

			var pendingMigrations = context.Database.GetPendingMigrations();
			if (pendingMigrations.Any())
			{
				context.Database.Migrate();
			}
			context.Database.EnsureCreated();

			var identityPendingMigrations = storeContext.Database.GetPendingMigrations();
			if (identityPendingMigrations.Any())
			{
				storeContext.Database.Migrate();
			}

			storeContext.Database.EnsureCreated();

			if (!await context.Products.AnyAsync())
			{
				var brands = new List<Brand>
				{
					new()
					{
						Name = "j00r",
					},
				};
				context.Brands.AddRange(brands);
				context.SaveChanges();
			}
			if (!await context.Products.AnyAsync())
			{
				var products = new List<Product>
				{
					new()
					{
						Name = "Nienorzyca",
						Description =
							"Piękna historia dwójki nieznajomych, którzy próbują stworzyć trwałą miłość, wbrew wszelkim trudnościom.",
						Brand = context.Brands.FirstOrDefault(q => q.Name == "j00r"),
						Price = 0.99,
						ImageUrl = "\\img\\products\\nie-no-rzyca.jpg"
					},
				};
				context.Products.AddRange(products);
				context.SaveChanges();
			}


			await roleManager.CreateAsync(new IdentityRole(StaticDetails.ROLE_ADMIN));
			await roleManager.CreateAsync(new IdentityRole(StaticDetails.ROLE_USER_INDIVIDUAL));

			var adminUser = new User()
			{
				UserName = "admin@wsei.pl",
				FirstName = "Marcin",
				Surname = "Krasucki",
				NormalizedUserName = "ADMIN@WSEI.PL",
				PhoneNumber = "1234567890",
				Email = "admin@wsei.pl",
				NormalizedEmail = "ADMIN@WSEI.PL",
				EmailConfirmed = true,
				Street = "Admin Street",
				City = "Admin City",
				State = "Admin State",
				ZipCode = "Admin Zip"
			};

			if (!await context.Users.AnyAsync(u => u.FirstName == adminUser.FirstName))
			{
				var result = await userManager.CreateAsync(adminUser, "Wsei@1");

				if (result.Succeeded)
					await userManager.AddToRoleAsync(adminUser, StaticDetails.ROLE_ADMIN);
			}

			var individualUser = new User()
			{
				FirstName = "Andzej",
				Surname = "Woda",
				UserName = "individual@wsei.pl",
				NormalizedUserName = "INDIVIDUAL@WSEI.PL",
				PhoneNumber = "1234567890",
				Email = "individual@wsei.pl",
				NormalizedEmail = "INDIVIDUAL@WSEI.PL",
				EmailConfirmed = true,
				Street = "User Street",
				City = "User City",
				State = "User State",
				ZipCode = "User Zip"
			};

			if (!await context.Users.AnyAsync(u => u.FirstName == individualUser.FirstName))
			{
				var result = await userManager.CreateAsync(individualUser, "Wsei@1");

				if (result.Succeeded)
					await userManager.AddToRoleAsync(individualUser, StaticDetails.ROLE_USER_COMPANY);
			}

			context.SaveChanges();
			storeContext.SaveChanges();
		}
	}
}
