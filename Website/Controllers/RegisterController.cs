﻿using Microsoft.AspNetCore.Mvc;
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
            ViewData["Success"] = null;
            return View("Index");
        }

        public IActionResult Create(RegistrationViewModel loginNameViewModel)
        {
            ViewData["Success"] = null;
            if (!ModelState.IsValid) return View("Index");

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

            var encryptedPassword =
                System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(loginNameViewModel.Password));

            var accountDetails = new AccountDetails()
            {
                LoginName = loginNameViewModel.LoginName,
                Password = encryptedPassword,
                FirstName = loginNameViewModel.FirstName,
                LastName = loginNameViewModel.LastName,
                Address = loginNameViewModel.Address,
                EmailAddress = loginNameViewModel.EmailAddress
            };

            var registeredAccount = _accountDataStore.Register(accountDetails);

            if (string.IsNullOrEmpty(registeredAccount.ErrorMessage))
            {
                ViewData["Success"] = "Successfully created new account.";
                return View("Index");
            }

            ModelState.AddModelError("", "Oops. Something went wrong.");
            return View("Index", loginNameViewModel);

        }
        
    }
}