using Microsoft.AspNetCore.Mvc;
using Website.DataStores.Interface;
using Website.Models;
using Website.ViewModels;

namespace Website.Controllers
{
    public class RegisterController : Controller
    {
        private readonly IAccountDataStore _accountDataStore;
        public RegisterController(IAccountDataStore accountDataStore)
        {
            _accountDataStore = accountDataStore;
        }

        public IActionResult Index()
        {
            return View("Index");
        }

        public IActionResult Create(RegistrationViewModel loginNameViewModel)
        {
            if (!ModelState.IsValid) return View("Index");

            var accountDetails = new AccountDetails()
            {
                LoginName = loginNameViewModel.LoginName,
                Password = loginNameViewModel.Password,
                FirstName = loginNameViewModel.FirstName,
                LastName = loginNameViewModel.LastName,
                Address = loginNameViewModel.Address
            };

            var isLoginNameAlreadyExistsResponse = _accountDataStore.CheckLoginNameExist(loginNameViewModel.LoginName);

            if (isLoginNameAlreadyExistsResponse.Data)
            {
                ModelState.AddModelError("", "LoginName already exist.");
                return View("Index", loginNameViewModel);
            }

            if (!string.IsNullOrEmpty(isLoginNameAlreadyExistsResponse.ErrorMessage))
            {
                ModelState.AddModelError("", "Oops. Something went wrong.");
                return View("Index", loginNameViewModel);
            }

            var response = _accountDataStore.Register(accountDetails);

            if (string.IsNullOrEmpty(response.ErrorMessage)) return RedirectToAction("Index", "Login");

            ModelState.AddModelError("", "Oops. Something went wrong.");
            return View("Index");

        }
        
    }
}