using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GroundHouse.Models;
using GroundHouse.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GroundHouse.Controllers
{
    [Authorize]//if to use this attribute on controller level it will be applicable to all actions inside it
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public AccountController(UserManager<ApplicationUser> userManager,//this instance is used for creating, deleteing and updating asyncly users
                                SignInManager<ApplicationUser> signInManager)//this one is for signInAsync, SignOutAsync, IsSignedId, etc
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult>//remoteError походу додає вже сам провайдер
            ExternalLoginCallback(string returnUrl = null, string remoteError = null)//метод який буде викликаний провайдером
        {
            returnUrl = returnUrl ?? Url.Content("~/");//отримуємо юрл повернення

            LoginViewModel loginViewModel = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()//отримуємо наших провайдерів
            };

            if (remoteError != null)//якщо є якась помилка, надіслана провайдером
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View("Login", loginViewModel);
            }

            var info = await signInManager.GetExternalLoginInfoAsync();//отрмуємо інформацію про спробу зайти через провайдера
            if (info == null)
            {
                ModelState.AddModelError(string.Empty, "Error loading external login information");
                return View("Login", loginViewModel);
            }

            //логінимо користувача в систему
            var signedInResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider,
                                                     info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (signedInResult.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }//it won`t succeed if the user is not in userLogins table yet
            else
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);//finding email in provider info

                if (email != null)
                {
                    var user = await userManager.FindByEmailAsync(email);

                    if (user == null)//if there is no such registered user
                    {
                        user = new ApplicationUser
                        {
                            UserName = info.Principal.FindFirstValue(ClaimTypes.Name),
                            Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                        };

                        await userManager.CreateAsync(user);//create such user
                    }

                    await userManager.AddLoginAsync(user, info);//adding login to userLogins table
                    await signInManager.SignInAsync(user, isPersistent:false);//sign the user in

                    return LocalRedirect(returnUrl);
                }

                //if we didn`t recieve email from external login provider
                ViewBag.ErrorTitle = $"Email claim not recieved from : {info.LoginProvider}";
                ViewBag.ErrorMessage = "Please contact support on khomiakmaxim@gmail.com";

                return View("Error");
            }            
        }

        //for external login providers
        [AllowAnonymous]
        [HttpPost]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            var redirecUrl = Url.Action("ExternalLoginCallback", "AccountController",
                                       new { ReturnUrl = returnUrl});//provider will sent user back to ExternalLoginCallback wiht all this things

            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirecUrl);//confusing

            return new ChallengeResult(provider, properties);

        }

        //for server side email-in-use check
        //[HttpGet][HttpPost] or
        [AcceptVerbs("Get", "Post")]//get request cause of validation is applying get request by it's own
        [AllowAnonymous]
        public async Task<IActionResult> IsEmailInUse(string email)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return Json(true);//black box(it is actually because of jquery ajax call)
            }
            else
            { 
                return Json($"Email {email} is already in use");            
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl)//will automatically bind the value from url to this parameter
        {
            LoginViewModel model = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()//all external login providers
            };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)//it will actually be mapped here from view                
        {                                                                             //it is also possibly because we've applied the
            if (ModelState.IsValid)                                                     //same name for paparameter as it is in qurey string
            {
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password,
                                                                    model.RememberMe, false);//last parameter for locking out account on failure

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        return LocalRedirect(returnUrl);//for safety - Redirect(returnUrl) is unsafe
                    }
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
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                //built-in user manager service works with IdenityUser, not RegisterViewModel
                var user = new ApplicationUser
                { 
                    UserName = model.Email,
                    Email = model.Email,
                    City = model.City
                };

                var result = await userManager.CreateAsync(user, model.Password);//adding user to DB

                if (result.Succeeded)
                {
                    if (signInManager.IsSignedIn(User) && User.IsInRole("Admin"))
                    {
                        return RedirectToAction("ListUsers", "Administration");        
                    }

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
