using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Security.Claims;
using Website.Controllers;
using Website.DataStores.Interface;
using Website.Enum;
using Website.Models;
using Website.ViewModels;
using Xunit;

namespace XUnitTestBankingSystem
{
    public class HomeControllerTest
    {
        [Fact]
        public void DepositSuccess()
        {

            var mockIAccountDataStore = new Mock<IAccountDataStore>();
            var mockITransactionDataStore = new Mock<ITransactionDataStore>();

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "user001"),
            }));

            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user,
                }
                
            };


            var controller = new HomeController(mockIAccountDataStore.Object, mockITransactionDataStore.Object);

            controller.ControllerContext = context;

            var httpContext = new DefaultHttpContext();

            controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            controller.TempData["BalanceRowVersion"] = new byte[8];

            mockIAccountDataStore.Setup(m => m.GetAccountDetails(It.IsAny<string>()))
                .Returns(() => new Response<AccountDetails>()
                {
                    ErrorMessage = string.Empty,
                    Data = new AccountDetails()
                    {
                        AccountNumber = 9000000000,
                        AccountId = 1,
                        LoginName = "user001",
                    }
                });

            mockIAccountDataStore.Setup(m => m.GetAccountBalance(It.IsAny<long>())).Returns(() => new Response<Wallet>()
            {
                ErrorMessage = string.Empty,
                Data = new Wallet()
                {
                    AccountNumber = 900000000,
                    RowVersion = new byte[8],
                    Balance = 100
                }
            });

            mockITransactionDataStore.Setup(m => m.Deposit(It.IsAny<long>(), It.IsAny<decimal>(), It.IsAny<byte[]>()))
                .Returns(() => new Response<bool>()
                {
                    ErrorMessage = string.Empty,
                    Data = true
                });

            var result = controller.Transfer(new WalletViewModel()
            {
                AccountNumber = 900000000,
                Amount = 100.00m,
                TransactionType = TransactionType.Deposit
            });

            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Home", viewResult.ControllerName);

        }

        [Fact]
        public void WithdrawSuccess()
        {

            var mockIAccountDataStore = new Mock<IAccountDataStore>();
            var mockITransactionDataStore = new Mock<ITransactionDataStore>();
       

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "user001"),
            }));

            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user,
                }

            };


            var controller = new HomeController(mockIAccountDataStore.Object, mockITransactionDataStore.Object)
            {
                ControllerContext = context
            };


            var httpContext = new DefaultHttpContext();

            controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
            {
                ["BalanceRowVersion"] = new byte[8]
            };

            mockIAccountDataStore.Setup(m => m.GetAccountDetails(It.IsAny<string>()))
                .Returns(() => new Response<AccountDetails>()
                {
                    ErrorMessage = string.Empty,
                    Data = new AccountDetails()
                    {
                        AccountNumber = 9000000000,
                        AccountId = 1,
                        LoginName = "user001",
                    }
                });

            mockIAccountDataStore.Setup(m => m.GetAccountBalance(It.IsAny<long>())).Returns(() => new Response<Wallet>()
            {
                ErrorMessage = string.Empty,
                Data = new Wallet()
                {
                    AccountNumber = 900000000,
                    RowVersion = new byte[8],
                    Balance = 100
                }
            });

            mockITransactionDataStore.Setup(m => m.Withdraw(It.IsAny<long>(), It.IsAny<decimal>(), It.IsAny<byte[]>()))
                .Returns(() => new Response<bool>()
                {
                    ErrorMessage = string.Empty,
                    Data = true
                });

            var result = controller.Transfer(new WalletViewModel()
            {
                AccountNumber = 900000000,
                Amount = 100.00m,
                TransactionType = TransactionType.Withdraw
            });

            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Home", viewResult.ControllerName);

        }

        [Fact]
        public void FundTransferSuccess()
        {

            var mockIAccountDataStore = new Mock<IAccountDataStore>();
            var mockITransactionDataStore = new Mock<ITransactionDataStore>();

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "user001"),
            }));

            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user,
                }

            };


            var controller = new HomeController(mockIAccountDataStore.Object, mockITransactionDataStore.Object);

            controller.ControllerContext = context;

            var httpContext = new DefaultHttpContext();

            controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            controller.TempData["BalanceRowVersion"] = new byte[8];

            mockIAccountDataStore.Setup(m => m.GetAccountDetails(It.IsAny<string>()))
                .Returns(() => new Response<AccountDetails>()
                {
                    ErrorMessage = string.Empty,
                    Data = new AccountDetails()
                    {
                        AccountNumber = 9000000000,
                        AccountId = 1,
                        LoginName = "user001",
                    }
                });

            mockIAccountDataStore.Setup(m => m.GetAccountBalance(It.IsAny<long>())).Returns(() => new Response<Wallet>()
            {
                ErrorMessage = string.Empty,
                Data = new Wallet()
                {
                    AccountNumber = 900000000,
                    RowVersion = new byte[8],
                    Balance = 100
                }
            });

            mockITransactionDataStore.Setup(m => m.FundTransfer(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<decimal>(), It.IsAny<byte[]>()))
                .Returns(() => new Response<bool>()
                {
                    ErrorMessage = string.Empty,
                    Data = true
                });

            var result = controller.Transfer(new WalletViewModel()
            {
                AccountNumber = 900000000,
                Amount = 100.00m,
                TransactionType = TransactionType.FundTransfer
            });

            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Home", viewResult.ControllerName);

        }

        [Fact]
        public void IndexSuccess()
        {

            var mockIAccountDataStore = new Mock<IAccountDataStore>();
            var mockITransactionDataStore = new Mock<ITransactionDataStore>();

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "user001"),
            }));

            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user,
                }

            };


            var controller = new HomeController(mockIAccountDataStore.Object, mockITransactionDataStore.Object)
            {
                ControllerContext = context
            };


            var httpContext = new DefaultHttpContext();

            controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
            {
                ["BalanceRowVersion"] = new byte[8]
            };

            mockIAccountDataStore.Setup(m => m.GetAccountDetails(It.IsAny<string>()))
                .Returns(() => new Response<AccountDetails>()
                {
                    ErrorMessage = string.Empty,
                    Data = new AccountDetails()
                    {
                        AccountNumber = 9000000000,
                        AccountId = 1,
                        LoginName = "user001",
                    }
                });

            mockIAccountDataStore.Setup(m => m.GetAccountBalance(It.IsAny<long>())).Returns(() => new Response<Wallet>()
            {
                ErrorMessage = string.Empty,
                Data = new Wallet()
                {
                    AccountNumber = 900000000,
                    RowVersion = new byte[8],
                    Balance = 100
                }
            });

            mockITransactionDataStore.Setup(m => m.Withdraw(It.IsAny<long>(), It.IsAny<decimal>(), It.IsAny<byte[]>()))
                .Returns(() => new Response<bool>()
                {
                    ErrorMessage = string.Empty,
                    Data = true
                });

            mockITransactionDataStore.Setup(m => m.GetAccountTransactionsHistoryList(It.IsAny<long>()))
                .Returns(() => new Response<List<WalletTransaction>>()
                {
                    ErrorMessage = string.Empty,
                    Data = new List<WalletTransaction>()
                    {
                        new WalletTransaction(),
                        new WalletTransaction()
                    }
                });

            

            var result = controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = viewResult.Model as WalletViewModel;

            if (model != null) Assert.Equal(2, model.Transactions.Count);
        }
    }


}
