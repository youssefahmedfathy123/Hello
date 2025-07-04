using Hello.Contract.Interfaces;
using Hello.Domain.Users;
using Hello.Enum;
using Hello.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Hello.Controllers
{

    public class UserController : Controller
    {
        #region feilds
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        //private readonly IJwt _jwt;
        #endregion

        #region ctor
        public UserController(UserManager<User> userManager/*, IJwt jwt*/, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            //_jwt = jwt;
        }
        #endregion


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
            await _userManager.RemoveFromRolesAsync(user,userRoles);

            await _userManager.AddToRolesAsync(user, model.Roles.Where(r => r.IsSelected).Select(r => r.DisplayValue));


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

            //var token = _jwt.GenerateToken(roles.ToList(), newUser.UserName, newUser.Id);

            // ✅ Store token in cookie
            //Response.Cookies.Append("jwt", token, new CookieOptions
            //{
            //    HttpOnly = true,
            //    Secure = true,
            //    SameSite = SameSiteMode.Strict,
            //    Expires = DateTimeOffset.UtcNow.AddHours(1)
            //});

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

            //var token = _jwt.GenerateToken(roles.ToList(), user.UserName, user.Id);

            // ✅ Store token in cookie
            //Response.Cookies.Append("jwt", token, new CookieOptions
            //{
            //    HttpOnly = true,
            //    Secure = true,
            //    SameSite = SameSiteMode.Strict,
            //    Expires = DateTimeOffset.UtcNow.AddHours(1)
            //});

            return RedirectToAction("Index", "Home");
        }


        public IActionResult Logout()
        {
            // ✅ Delete token from cookie
            Response.Cookies.Delete("jwt");
            return RedirectToAction("Index", "Home");
        }

        #endregion



    }
}

