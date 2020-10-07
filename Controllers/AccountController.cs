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
using Microsoft.Extensions.Logging;

namespace GroundHouse.Controllers
{
    [Authorize]//if to use this attribute on controller level it will be applicable to all actions inside it
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ILogger<AccountController> logger;

        public AccountController(UserManager<ApplicationUser> userManager,//this instance is used for creating, deleteing and updating asyncly users
                                SignInManager<ApplicationUser> signInManager,//this one is for signInAsync, SignOutAsync, IsSignedId, etc
                                ILogger<AccountController> logger)   
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> AddPassword()
        {
            var user = await userManager.GetUserAsync(User);

            var userHasPassword = await userManager.HasPasswordAsync(user);

            if (userHasPassword)
            {
                return RedirectToAction("ChangePassword");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddPassword(AddPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(User);

                var result = await userManager.AddPasswordAsync(user, model.NewPassword);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View();
                }

                await signInManager.RefreshSignInAsync(user);

                return View("AddPasswordConfirmation");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await userManager.GetUserAsync(User);

            var userHasPassword = await userManager.HasPasswordAsync(user);

            if (!userHasPassword)
            {
                return RedirectToAction("AddPassword");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                // ChangePasswordAsync changes the user password
                var result = await userManager.ChangePasswordAsync(user,
                    model.CurrentPassword, model.NewPassword);

                // The new password did not meet the complexity rules or
                // the current password is incorrect. Add these errors to
                // the ModelState and rerender ChangePassword view
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View();
                }

                // Upon successfully changing the password refresh sign-in cookie
                await signInManager.RefreshSignInAsync(user);
                return View("ChangePasswordConfirmation");
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token, string email)
        {
            // If password reset token or email is null, most likely the
            // user tried to tamper the password reset link
            if (token == null || email == null)
            {
                ModelState.AddModelError("", "Invalid password reset token");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Find the user by email
                var user = await userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    // reset the user password
                    var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);
                    if (result.Succeeded)
                    {
                        if (await userManager.IsLockedOutAsync(user))
                        {
                            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
                        }

                        return View("ResetPasswordConfirmation");
                    }
                    // Display validation errors. For example, password reset token already
                    // used to change the password or password complexity rules not met
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }

                // To avoid account enumeration and brute force attacks, don't
                // reveal that the user does not exist
                return View("ResetPasswordConfirmation");
            }
            // Display validation errors if model state is not valid
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user != null && await userManager.IsEmailConfirmedAsync(user))
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(user);
                    var confirmationLink = Url.Action("ResetPassword", "Account",
                                            new { email = model.Email, token = token}, Request.Scheme);

                    logger.Log(LogLevel.Warning, confirmationLink);

                    return View("ForgotPasswordConfirmation");
                }
                return View("ForgotPasswordConfirmation");                
            }

            return View(model);            
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return RedirectToAction("index", "home");
            }

            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"The user ID {userId} is invalid";
                return View("NotFound");
            }

            var result = await userManager.ConfirmEmailAsync(user, token);//flips flag email confirm from false to true

            if (result.Succeeded)
            {
                return View();
            }

            ViewBag.ErrorTitle = "Email cannot be confirmed";
            return View("Error");
        }

        [AllowAnonymous]
        [HttpGet]
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

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);//finding email in provider info

            ApplicationUser user = null;

            if (email != null)
            {
                user = await userManager.FindByEmailAsync(email);
                if (user != null && !user.EmailConfirmed)
                {
                    ModelState.AddModelError(string.Empty, "Email not confirmed yet");
                    return View("Login", loginViewModel);
                }
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
                

                if (email != null)
                {                    
                    if (user == null)//if there is no such registered user
                    {
                        user = new ApplicationUser
                        {
                            UserName = info.Principal.FindFirstValue(ClaimTypes.Name),
                            Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                        };

                        await userManager.CreateAsync(user);//create such user

                        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                        var confirmationLink = Url.Action("ConfirmEmail", "Account",
                                                new { userId = user.Id, token = token}, Request.Scheme);

                        logger.Log(LogLevel.Warning, confirmationLink);

                        ViewBag.ErrorTitle = "Registration successful";
                        ViewBag.ErrorMessage = "Before you can login, please confirm your " +
                            "email, by clicking on the confirmation link we have emailed you";
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
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account",
                new { ReturnUrl = returnUrl });
            var properties = signInManager
                .ConfigureExternalAuthenticationProperties(provider, redirectUrl);
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

            model.ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();//for avoiding null refferences exception
            if (ModelState.IsValid)                               //same name for paparameter as it is in qurey string
            {

                var user = await userManager.FindByEmailAsync(model.Email);

                if (user != null && !user.EmailConfirmed &&
                                    (await userManager.CheckPasswordAsync(user, model.Password)))//also checking password for security
                {
                    ModelState.AddModelError(string.Empty, "Email not confirmed yet");
                    return View(model);
                }

                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password,
                                                                    model.RememberMe, true);//last parameter for locking out account on failure

                if (result.Succeeded)
                {

                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        return LocalRedirect(returnUrl);//for safety - Redirect(returnUrl) is unsafe
                    }
                    return RedirectToAction("index", "home");
                }

                if (result.IsLockedOut)
                {
                    return View("AccountLocked");
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
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);//generation token
                    var confirmationLink = Url.Action("ConfirmEmail", "Account", 
                                            new { userId = user.Id, token = token}, Request.Scheme);//confirmation Link

                    logger.Log(LogLevel.Warning, confirmationLink);//logging confirmation link

                    if (signInManager.IsSignedIn(User) && User.IsInRole("Admin"))
                    {
                        return RedirectToAction("ListUsers", "Administration");        
                    }

                    ViewBag.ErrorTitle = "Registration successful";
                    ViewBag.ErrorMessage = "Before you can login, please confirm your " +
                        "email, by clicking on the confirmation link we have emailed you";
                    return View("Error");
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
