using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hello.Domain.Users;
using Hello.Enum;
using System.Security.Cryptography.X509Certificates;
using PermissionBasedAuthorizationIntDotNet5.Contants;
using System.Security.Claims;

namespace Hello.Domain.Seed
{
    public static class DefaultUsers
    {
        public static async Task SeedBasicUserAsync(UserManager<User> userManager)
        {
            var defaultUser = new User
            {
                UserName = "basicuser@gmail.com",
                Email = "basicuser@gmail.com",
                EmailConfirmed = true,
                Name = "defaultUser",
            };
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if(user == null)
            {
                await userManager.CreateAsync(defaultUser, "P@ssword123");
                await userManager.AddToRoleAsync(defaultUser,Roles.User.ToString());
            }
        }


        public static async Task SeedSuperAdminAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            var Super = new User
            {
                UserName = "superadmin@gmail.com",
                Email = "superadmin@gmail.com",
                EmailConfirmed = true,
                Name = "defaultUser",
            };
            var user = await userManager.FindByEmailAsync(Super.Email);
            if (user == null)
            {
                await userManager.CreateAsync(Super, "P@ssword123");
                await userManager.AddToRolesAsync(Super, new List<string> {
                    Roles.Admin.ToString(),
                    Roles.SuperAdmin.ToString(),Roles.User.ToString(),
                });

                var adminRole = await roleManager.FindByNameAsync(Roles.SuperAdmin.ToString());
                await roleManager.AddPermissionClaims(adminRole, "Products");
            } }


        public static async Task AddPermissionClaims(this RoleManager<IdentityRole> roleManager, IdentityRole role, string module)
        {
            var allClaims = await roleManager.GetClaimsAsync(role);
            var allPermissions = Permissions.GeneratePermissionsList(module);

            foreach(var permission in allPermissions)
            {
                if (!allClaims.Any(c => c.Type == "Permission" && c.Value == permission))
                    await roleManager.AddClaimAsync(role, new Claim("Permission",permission));
            }

        }

        }

    }


