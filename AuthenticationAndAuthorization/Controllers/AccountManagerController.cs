using AuthenticationAndAuthorization.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthenticationAndAuthorization.Controllers
{
    [Authorize(Roles ="Admin")]
    public class AccountManagerController : Controller
    {
        UserManager<IdentityUser> userManager;
        RoleManager<IdentityRole> roleManager;

        public AccountManagerController(UserManager<IdentityUser> _userManager, RoleManager<IdentityRole> _roleManager)
        {
            userManager = _userManager;
            roleManager = _roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AssignRole()
        {
            var users = userManager.Users.ToList();
            var roles = roleManager.Roles.ToList();

            List<string> listRoles = new List<string>();
            foreach (var item in roles)
            {
                listRoles.Add(item.Name);
            }

            List<AssignRoleViewModel> rolelist = new List<AssignRoleViewModel>();

            foreach (var item in users)
            {
                AssignRoleViewModel model = new AssignRoleViewModel();
                model.UserId = item.Id;
                model.UserName = item.UserName;
                model.Email = item.Email;
                model.RoleName = userManager.GetRolesAsync(item).Result.Count != 0 ? userManager.GetRolesAsync(item).Result[0] : "";
                model.Roles = listRoles;
                rolelist.Add(model);

            }
            return View(rolelist);
        }

        [HttpPost]
        public async Task<IActionResult> AssignRole(string UserId, string RoleName)
        {
            if (ModelState.IsValid)
            {
                var user = userManager.FindByIdAsync(UserId).Result;
                var roles = roleManager.Roles.ToList();

                List<string> listRoles = new List<string>();
                foreach (var item in roles)
                {
                    listRoles.Add(item.Name);
                }
                var RemoveRoles = await userManager.RemoveFromRoleAsync(user, listRoles.ToString());
                var Result = await userManager.AddToRoleAsync(user, RoleName);

                return RedirectToAction("AssignRole");
            }
            else
            {
                return View();
            }
        }
    }
}
