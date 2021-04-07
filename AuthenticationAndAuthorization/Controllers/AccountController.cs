using AuthenticationAndAuthorization.Services;
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
    [AllowAnonymous]
    public class AccountController : Controller
    {
        UserManager<IdentityUser> userManager;
        SignInManager<IdentityUser> signInManager;
        public AccountController(UserManager<IdentityUser> _userManager, SignInManager<IdentityUser> _signInManager)
        {
            userManager = _userManager;
            signInManager = _signInManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost ]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            IdentityUser user = new IdentityUser()
            {
                UserName = model.Username,
                Email = model.Email
            };

            var result=await userManager.CreateAsync(user, model.Password);
            var resultRole = await userManager.AddToRoleAsync(user, "User");

            var confirmToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
            string confirmationLink = Url.Action("ConfirmEmail", "Account", new { userid = user.Id, token = confirmToken }, protocol: HttpContext.Request.Scheme);
            //EmailService.Send(user.Email, "Confirm Your Email", "Click Here To Confim you Email Address" + confirmationLink);
            //userManager.CreateAsync(user, model.Password).Wait();
            System.IO.File.WriteAllText(@"C:\temp\ConfirmEmail.txt", confirmationLink);
            if (result.Succeeded && resultRole.Succeeded)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await userManager.FindByIdAsync(userId);
            var result = await userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                return RedirectToAction("Login","Account");
            }

            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginView)
        {
            //IdentityUser user = new IdentityUser()
            //{
            //    UserName = loginView.UserName,
            //    PasswordHash = loginView.Password
            //};
            var result = await signInManager.PasswordSignInAsync(loginView.UserName, loginView.Password, loginView.RememberMe, true);

            if (result.Succeeded)
            {
                var user = await userManager.FindByNameAsync(loginView.UserName);
                if (user.EmailConfirmed)
                {
                    return RedirectToAction("Index", "Home");
                }
                else {
                    await signInManager.SignOutAsync();
                }
               
            }
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel forgot)
        {
            var user = await userManager.FindByEmailAsync(forgot.UserEmail);
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            string resetLink = Url.Action("ResetPassword", "Account", new { userid = user.Id, token = resetToken }, protocol: HttpContext.Request.Scheme);

            System.IO.File.WriteAllText(@"C:\temp\ResetPassword.txt", resetLink);

            ViewBag.Msg = "Reset Password Link Has Been Emailed";
            return View();
        }


        [HttpGet]
        public IActionResult ResetPassword(string userId, string token)
        {
            var obj = new ResetPasswordViewModel()
            {
                UserId = userId,
                Token = token
            };
            return View(obj);
        }

        [HttpPost]
        public async  Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPass)
        {
            var user = await userManager.FindByIdAsync(resetPass.UserId);
            var result = await userManager.ResetPasswordAsync(user, resetPass.Token, resetPass.Password);

            if (result.Succeeded)
            {
                ViewBag.Msg = "Password Reset Succeede!";
            }
            else
            {
                ViewBag.Msg = "Password Reset Failed";
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login","Account"); 
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
