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


        public IActionResult Login(LoginViewModel login)
        {
            if (!ModelState.IsValid) return View("Index");

            var encryptedPassword =
                System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(login.Password));
            var response = _accountDataStore.IsValidCredential(login.LoginName, encryptedPassword);

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                ModelState.AddModelError("", response.ErrorMessage);
                return View("Index");
            }

            if (!response.Data)
            {
                ModelState.AddModelError("", "Invalid Login Name or Password.");
                return View("Index");
            }

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, login.LoginName));
            identity.AddClaim(new Claim(ClaimTypes.Name, login.LoginName));
            var principal = new ClaimsPrincipal(identity);
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = login.RememberMe });

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();

            return RedirectToAction("Index", "Login");
        }
    }
}