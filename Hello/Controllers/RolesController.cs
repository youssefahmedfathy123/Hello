using Hello.Domain.Users;
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

    public class RolesController : Controller
    {

        #region fields
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;
        #endregion

        #region ctor
        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<User> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }
        #endregion


        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles.ToListAsync();

            return View(roles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(RoleFormVM model)
        {
            if (!ModelState.IsValid)
                return View("Index", await _roleManager.Roles.ToListAsync());

            if(await _roleManager.RoleExistsAsync(model.Name))
            {
                ModelState.AddModelError("Name", "Role is exists!");
                return View("Index", await _roleManager.Roles.ToListAsync());
            }
            await _roleManager.CreateAsync(new IdentityRole(model.Name.Trim()));

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> ManagePermissions(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);

            if (role == null)
                return NotFound();

            var roleClaims =  _roleManager.GetClaimsAsync(role).Result.Select(c => c.Value).ToList();

            var allClaims = Permissions.GenerateAllPermissions();

            var allPermissions = allClaims.Select(p => new CheckBoxViewModel { DisplayValue = p }).ToList();

            foreach(var per in allPermissions)
            {
                if (roleClaims.Any(c => c == per.DisplayValue))
                    per.IsSelected = true;
            }


            var viewModel = new PermissionsVM
            {
                RoleId = roleId,
                RoleName = role.Name,
                RoleCalims = allPermissions
            };

            return View(viewModel);


        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManagePermissions(PermissionsVM model)
        {
            var role = await _roleManager.FindByIdAsync(model.RoleId);

            if (role == null)
                return NotFound();


            var roleClaims = await _roleManager.GetClaimsAsync(role);


            foreach (var claim in roleClaims)
                await _roleManager.RemoveClaimAsync(role, claim);


            var selectedClaims = model.RoleCalims.Where(c => c.IsSelected).ToList();


            foreach (var claim in selectedClaims)
                await _roleManager.AddClaimAsync(role, new Claim("Permission", claim.DisplayValue));


            return RedirectToAction(nameof(Index));


        }


    }
  }


