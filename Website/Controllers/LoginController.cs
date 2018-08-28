using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Website.DataStores.Interface;
using Website.ViewModels;

namespace Website.Controllers
{
    public class LoginController : Controller
    {

        private readonly IAccountDataStore _accountDataStore;

        public LoginController(IAccountDataStore accountDataStore)
        {
            _accountDataStore = accountDataStore;
        }

        public IActionResult Index()
        {
            return View();
        }


        public IActionResult Login(LoginViewModel loginViewModel)
        {
            if (!ModelState.IsValid) return View("Index");

            var encryptedPassword =
                System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(loginViewModel.Password));
            var response = _accountDataStore.IsValidCredential(loginViewModel.LoginName, encryptedPassword);

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                ModelState.AddModelError("", response.ErrorMessage);
                return View("Index", loginViewModel);
            }

            if (!response.Data)
            {
                ModelState.AddModelError("", "Invalid Login Name or Password.");
                return View("Index", loginViewModel);
            }

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, loginViewModel.LoginName));
            identity.AddClaim(new Claim(ClaimTypes.Name, loginViewModel.LoginName));
            var principal = new ClaimsPrincipal(identity);
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = loginViewModel.RememberMe });

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();

            return RedirectToAction("Index", "Login");
        }
    }
}