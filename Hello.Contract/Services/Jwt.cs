//using Hello.Contract.Interfaces;
//using Microsoft.Extensions.Configuration;
//using Microsoft.IdentityModel.Tokens;
//using PermissionBasedAuthorizationIntDotNet5.Contants;
//using System;
//using System.Collections.Generic;
//using System.IdentityModel.Tokens.Jwt;
//using System.Linq;
//using System.Security.Claims;
//using System.Text;
//using System.Threading.Tasks;

//namespace Hello.Contract.Services
//{
//    public class Jwt : IJwt
//    {
//        private readonly IConfiguration _configuration;
//        public Jwt(IConfiguration configuration)
//        {
//            _configuration = configuration;
//        }

//        public string GenerateToken(List<string> roles, string username, string Id)
//        {
//            var Claims = new List<Claim>()
//            {
//            new Claim(ClaimTypes.Name, username),
//            new Claim(ClaimTypes.NameIdentifier, Id)
//            };


//            foreach (var role in roles)
//            {
//                Claims.Add(new Claim(ClaimTypes.Role, role));

//             //   Generate permissions based on role
//               var permissions = Permissions.GeneratePermissionsList(role);
//                foreach (var permission in permissions)
//                {
//                    Claims.Add(new Claim("Permission", permission, ClaimValueTypes.String, "LOCAL AUTHORITY"));
//                }

//            }


//            var Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
//            var Credentials = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256);


//            var token = new JwtSecurityToken(
//                issuer: _configuration["JWT:Issuer"],
//                audience: _configuration["JWT:audience"],
//                claims: Claims,
//                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["JWT:ExpireMinutes"])),
//                signingCredentials: Credentials
//                );

//            return new JwtSecurityTokenHandler().WriteToken(token);

//        }
//    }
//}

