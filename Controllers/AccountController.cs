using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GroundHouse.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GroundHouse.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;

        public AccountController(UserManager<IdentityUser> userManager,//this instance is used for creating, deleteing and updating asyncly users
                                SignInManager<IdentityUser> signInManager)//this one is for signInAsync, SignOutAsync, IsSignedId, etc
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password,
                                                                    model.RememberMe, false);//last parameter for locking out account on failure

                if (result.Succeeded)
                {                    
                    return RedirectToAction("index", "home");
                }

                ModelState.AddModelError("", "Invalid Login Attempt");                
            }

            return View(model);
        }


        //it is smart to logout user using post request
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("index", "home");
        }

        //if there is no either [HttpGet] or [HttpPost] attribute this method will do both
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                //built-in user manager service works with IdenityUser, not RegisterViewModel
                var user = new IdentityUser
                { 
                    UserName = model.Email,
                    Email = model.Email
                };

                var result = await userManager.CreateAsync(user, model.Password);//adding user to DB

                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(user, isPersistent: false);//signing in user with session cookies(false)
                    return RedirectToAction("index", "home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);//will be displayed by validation summary tag-helper
                }
            }

            return View(model);
        }
    }
}
