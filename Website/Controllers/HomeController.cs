using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Website.DataStores.Interface;
using Website.Enum;
using Website.Models;
using Website.ViewModels;

namespace Website.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IAccountDataStore _accountDataStore;
        private readonly ITransactionDataStore _transactionDataStore;

        public HomeController(IAccountDataStore accountDataStore, ITransactionDataStore transactionDataStore)
        {
            _accountDataStore = accountDataStore;
            _transactionDataStore = transactionDataStore;
        }
        public IActionResult Index()
        {
            var loginName = User.Identity.Name;

            var accountDetails = _accountDataStore.GetAccountDetails(loginName);

            if (!string.IsNullOrEmpty(accountDetails.ErrorMessage))
            {
                ModelState.AddModelError("", "Oops. something went wrong.");
                return View("Index");
            }

            var wallet = _accountDataStore.GetAccountBalance(accountDetails.Data.AccountNumber);

            if (!string.IsNullOrEmpty(wallet.ErrorMessage))
            {
                ModelState.AddModelError("", "Oops. something went wrong.");
                return View("Index");
            }
            
            TempData["BalanceRowVersion"] = wallet.Data.RowVersion;

            var transactionHistory =
                _transactionDataStore.GetAccountTransactionsHistoryList(accountDetails.Data.AccountNumber);

            if (!string.IsNullOrWhiteSpace(transactionHistory.ErrorMessage))
            {
                ModelState.AddModelError("", "Oops. something went wrong.");
                return View("Index");
            }

            var walletTransaction = transactionHistory.Data.Select(t => new TransactionViewModel()
            {
                TransactionDateTime = t.TransactionDate,
                Description = t.TransactionType.ToString(),
                Reference = t.TransactionReference,
                Credit = t.Credit,
                Debit = t.Debit,
                Balance = t.Balance

            }).ToList();

            var viewModel = new WalletViewModel()
            {
                Balance = wallet.Data.Balance,
                Transactions = walletTransaction
            };

            return View(viewModel);
        }

        public IActionResult Transfer(WalletViewModel walletViewModel)
        {
            var response = new Response<bool>();

            if (walletViewModel.Amount <= 0)
            {
                ModelState.AddModelError("", "Amount should be greater than 0.00.");
                return View("Index", walletViewModel);
            }

            var loginName = User.Identity.Name;
            var accountDetails = _accountDataStore.GetAccountDetails(loginName);

            if (!string.IsNullOrEmpty(accountDetails.ErrorMessage))
            {
                ModelState.AddModelError("", "Oops. something went wrong.");
                return View("Index", walletViewModel);
            }

            var accountNumber = accountDetails.Data.AccountNumber;
            var accountNumberReceiver = walletViewModel.AccountNumber;

            var rowVersion = TempData["BalanceRowVersion"] as byte[];

            if (rowVersion == null)
            {
                var wallet = _accountDataStore.GetAccountBalance(accountDetails.Data.AccountNumber);
                rowVersion = wallet.Data.RowVersion;
            }

            switch (walletViewModel.TransactionType)
            {
                case TransactionType.Deposit:
                    response = _transactionDataStore.Deposit(accountNumber, walletViewModel.Amount, rowVersion);
                    break;
                case TransactionType.Withdraw:
                    response = _transactionDataStore.Withdraw(accountNumber, walletViewModel.Amount, rowVersion);
                    break;
                case TransactionType.FundTransfer:

                    if (accountNumber.Equals(accountNumberReceiver))
                    {
                        ModelState.AddModelError("", "You can't transfer using your own account.");
                        return View("Index", walletViewModel);
                    }

                    response = _transactionDataStore.FundTransfer(accountNumber, accountNumberReceiver.GetValueOrDefault(0), walletViewModel.Amount, rowVersion);
                    break;
                case TransactionType.FundReceived:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (string.IsNullOrEmpty(response.ErrorMessage))
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", response.ErrorMessage);

            return View("Index", walletViewModel);

        }


    }
}
