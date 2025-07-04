using Hello.Contract.Interfaces;
using Hello.Domain.Users;
using Hello.Enum;
using Hello.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PermissionBasedAuthorizationIntDotNet5.Contants;
using System.Data;
using System.Security.Claims;

namespace Hello.Controllers
{

    public class UserController : Controller
    {
        #region feilds
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<User> _signIn;
        private readonly IJwt _jwt;
        #endregion

        #region ctor
        public UserController(UserManager<User> userManager, IJwt jwt, RoleManager<IdentityRole> roleManager, SignInManager<User> signIn)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signIn = signIn;
            _jwt = jwt;
        }
        #endregion



        #region First
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();

            var userVMs = new List<UserVM>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userVMs.Add(new UserVM
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles
                });
            }

            return View(userVMs);
        }



        public async Task<IActionResult> ManageRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var roles = await _roleManager.Roles.ToListAsync();

            var viewModel = new UserRolesVM
            {
                UserId = user.Id,
                UserName = user.UserName,
                Roles = roles.Select(role => new CheckBoxViewModel
                {
                    DisplayValue = role.Name,
                    IsSelected = _userManager.IsInRoleAsync(user, role.Name).Result
                }).ToList()
            };

            return View(viewModel);

        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRoles(UserRolesVM model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
                return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, userRoles);

            await _userManager.AddToRolesAsync(user, model.Roles.Where(r => r.IsSelected).Select(r => r.DisplayValue));


            return RedirectToAction(nameof(Index));


        }


        #endregion


        public async Task<IActionResult> ManagePermissions(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound();

            var allPermissions = Permissions.GenerateAllPermissions();

            var ValuesOfPermissions = allPermissions.Select(p => new CheckBoxViewModel
            {
                DisplayValue = p
            }).ToList();



            var userPermissions = _userManager.GetClaimsAsync(user).Result.Select(x => x.Value);


            foreach (var permission in ValuesOfPermissions)
            {
                if (userPermissions.Any(a => a == permission.DisplayValue))
                    permission.IsSelected = true;
            }



            var model = new UserPermissionsVM
            {
                UserId = userId,
                UserName = user.UserName,
                Permissions = ValuesOfPermissions.ToList()
            };


            return View(model);
        }





        [HttpPost]
        public async Task<IActionResult> ManagePermissions(UserPermissionsVM model)
        {

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return NotFound();

            var allClaims = await _userManager.GetClaimsAsync(user);

            foreach (var c in allClaims)
            {
                await _userManager.RemoveClaimAsync(user, c);
            }
            var TrueSelected = model.Permissions.Where(x => x.IsSelected).ToList();


            foreach (var per in TrueSelected)
            {
                await _userManager.AddClaimAsync(user, new Claim("Permission", per.DisplayValue));
            }


            return RedirectToAction(nameof(Index));
        }





        #region RegisterLogin
        public IActionResult Register()
        {
            return View(new RegisterVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM input)
        {
            if (!ModelState.IsValid)
                return View(input);

            var check_email = await _userManager.FindByEmailAsync(input.Email);
            var check_usernamse = await _userManager.FindByNameAsync(input.UserName);

            if (check_email != null || check_usernamse != null)
            {
                ModelState.AddModelError("", "This username or email is already taken.");
                return View(input);
            }

            var newUser = new User
            {
                Name = input.Name,
                Email = input.Email,
                UserName = input.UserName,
            };

            var createUser = await _userManager.CreateAsync(newUser, input.Password);

            if (!createUser.Succeeded)
            {
                foreach (var error in createUser.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(input);
            }

            await _userManager.AddToRoleAsync(newUser, Roles.User.ToString());

            var roles = await _userManager.GetRolesAsync(newUser);

            var token = await _jwt.GenerateToken(roles.ToList(), newUser.UserName, newUser.Id);


            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            //await _signIn.SignInAsync(newUser, false);

            return RedirectToAction("Index", "Home");
        }


        public IActionResult Login()
        {
            return View(new LoginVM());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM input)
        {
            if (!ModelState.IsValid)
                return View(input);

            var user = await _userManager.FindByEmailAsync(input.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Email or Password is not correct");
                return View(input);
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, input.Password);
            if (!isPasswordValid)
            {
                ModelState.AddModelError("", "Email or Password is not correct");
                return View(input);
            }

            var roles = await _userManager.GetRolesAsync(user);

            var token = await _jwt.GenerateToken(roles.ToList(), user.UserName, user.Id);


            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });


            //await _signIn.SignInAsync(user, false);
            Console.WriteLine(token);


            return RedirectToAction("Index", "Home");
        }



        public async Task<IActionResult> Logout()
        {
            //await _signIn.SignOutAsync();

            Response.Cookies.Delete("jwt");
            return RedirectToAction("Index", "Home");
        }

        #endregion





    }
}


