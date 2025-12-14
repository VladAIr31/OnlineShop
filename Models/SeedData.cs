using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;

namespace OnlineShop.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
            serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                if (context.Roles.Any()) return;

                context.Roles.AddRange(
                new IdentityRole {
                    Id = "2c5e174e-3b0e-446f-86af-483d56fd7210", Name = "Utilizator",
                    NormalizedName = "Utilizator".ToUpper() },
                
                new IdentityRole {
                    Id = "2c5e174e-3b0e-446f-86af-483d56fd7211", Name = "Colaborator",
                    NormalizedName = "Colaborator".ToUpper() },

                new IdentityRole {
                    Id = "2c5e174e-3b0e-446f-86af-483d56fd7212", Name = "Administrator",
                    NormalizedName = "Administrator".ToUpper()
                }

                );

                var hasher = new PasswordHasher<ApplicationUser>();

                context.Users.AddRange(
                    new ApplicationUser
                    {
                        Id = "8e445865-a24d-4543-a6c6-9443d048cdb0",
                        UserName = "utilizator@test.com",
                        EmailConfirmed = true,
                        NormalizedEmail = "UTILIZATOR@TEST.COM",
                        Email = "utilizator@test.com",
                        NormalizedUserName = "UTILIZATOR@TEST.COM",
                        PasswordHash = hasher.HashPassword(null, "Utilizator1!")
                    },
                    new ApplicationUser
                    {
                        Id = "8e445865-a24d-4543-a6c6-9443d048cdb1",
                        UserName = "colaborator@test.com",
                        EmailConfirmed = true,
                        NormalizedEmail = "COLABORATOR@TEST.COM",
                        Email = "colaborator@test.com",
                        NormalizedUserName = "COLABORATOR@TEST.COM",
                        PasswordHash = hasher.HashPassword(null, "Colaborator1!")
                    },
                    new ApplicationUser
                    {
                        Id = "8e445865-a24d-4543-a6c6-9443d048cdb2",
                        UserName = "administrator@test.com",
                        EmailConfirmed = true,
                        NormalizedEmail = "ADMINISTRATOR@TEST.COM",
                        Email = "administrator@test.com",
                        NormalizedUserName = "ADMINISTRATOR@TEST.COM",
                        PasswordHash = hasher.HashPassword(null, "Administrator1!")
                    }
                );

                context.UserRoles.AddRange(
                    new IdentityUserRole<string>
                    {
                        RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7210",
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0"
                    },
                    new IdentityUserRole<string>
                    {
                        RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7211",
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb1"
                    },
                    new IdentityUserRole<string>
                    {
                        RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7212",
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2"
                    }
                );

                context.SaveChanges();
            }
        }
    }
}
