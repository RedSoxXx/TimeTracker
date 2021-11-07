using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Data.Identity;

namespace TimeTracker.DAL
{
    public static class DbInitializer
    {
        private const string SuperAdminEmail = "noreply@rts.com";
        private const string SuperAdminPassword = "123";

        public static void Seed(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            CreateRoleIfNotExists(UserRoleName.SuperAdmin);
            CreateRoleIfNotExists(UserRoleName.User);

            if (userManager.FindByEmailAsync(SuperAdminEmail).Result == null)
            {
                var hasher = new PasswordHasher<User>();
                var user = new User
                {
                    UserName = SuperAdminEmail,
                    Email = SuperAdminEmail,
                    PasswordHash = hasher.HashPassword(null, SuperAdminPassword),
                    EmailConfirmed = true,
                    SecurityStamp = string.Empty,
                };

                var result = userManager.CreateAsync(user).Result;

                if (result.Succeeded)
                    userManager.AddToRoleAsync(user, UserRoleName.SuperAdmin).Wait();
            }

            void CreateRoleIfNotExists(string roleName)
            {
                if (roleManager.FindByNameAsync(roleName).Result == null)
                {
                    roleManager.CreateAsync(new Role
                    {
                        Name = roleName,
                        NormalizedName = roleName.ToUpper(),
                    }).Wait();
                }
            }
        }
    }
}
