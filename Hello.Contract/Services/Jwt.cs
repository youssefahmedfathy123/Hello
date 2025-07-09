using Hello.Contract.Interfaces;
using Hello.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PermissionBasedAuthorizationIntDotNet5.Contants;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Hello.Contract.Services
{
    public class Jwt : IJwt
    {
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;

        public Jwt(IConfiguration configuration, RoleManager<IdentityRole> roleManager, UserManager<User> UserManager)
        {
            _configuration = configuration;
            _roleManager = roleManager;
            _userManager = UserManager;
        }


        public async Task<string> GenerateToken(List<string> roles, string? username, string Id)
        {               

            var Claims = new List<Claim>()
            {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.NameIdentifier, Id),
            };

            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
                return "User not found";

            var res = await _userManager.GetClaimsAsync(user);


            foreach (var claim in res.Where(c => c.Type == "Permission"))
            {
                Claims.Add(claim);
            }



            foreach (var role in roles)
            {
                Claims.Add(new Claim(ClaimTypes.Role, role));

                var identityRole = await _roleManager.FindByNameAsync(role);
                if (identityRole == null)
                    continue;


                var roleClaims = await _roleManager.GetClaimsAsync(identityRole);


                foreach (var claim in roleClaims.Where(c => c.Type == "Permission"))
                {
                    if(!res.Any(x => x.Value == claim.Value))
                      Claims.Add(claim);
                }

            }




            var Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var Credentials = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256);


            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:audience"],
                claims: Claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["JWT:ExpireMinutes"])),
                signingCredentials: Credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);

        }
    }
}

