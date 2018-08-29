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
        #region Deposit

        [Fact]
        public void Deposit_Success()
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
        public void Deposit_AmountIsLessThanZero()
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

            mockIAccountDataStore.Setup(m => m.GetAccountDetails(It.IsAny<string>()))
                .Returns(() => new Response<AccountDetails>()
                {
                    ErrorMessage = "Amount should be greater than 0.00.",
                    Data = null
                });

            var result = controller.Transfer(new WalletViewModel()
            {
                AccountNumber = 900000000,
                Amount = 0,
                TransactionType = TransactionType.Deposit
            });

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.True(viewResult.ViewData.ModelState[""].Errors.Count == 1);
            Assert.Equal("Index", viewResult.ViewName);

        }

        [Fact]
        public void Deposit_AccountDetailsDBError()
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

            mockIAccountDataStore.Setup(m => m.GetAccountDetails(It.IsAny<string>()))
                .Returns(() => new Response<AccountDetails>()
                {
                    ErrorMessage = "Oops. something went wrong.",
                    Data = null
                });


            var httpContext = new DefaultHttpContext();

            controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
            {
                ["BalanceRowVersion"] = new byte[8]
            };

            var result = controller.Transfer(new WalletViewModel()
            {
                AccountNumber = 900000000,
                Amount = 100.00m,
                TransactionType = TransactionType.Deposit
            });

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.True(viewResult.ViewData.ModelState[""].Errors.Count == 1);
            Assert.Equal("Index", viewResult.ViewName);

        }

        [Fact]
        public void Deposit_AccountBalanceDBError()
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
                ["BalanceRowVersion"] = null
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
                ErrorMessage = "Oops. something went wrong.",
                Data = null
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

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.True(viewResult.ViewData.ModelState[""].Errors.Count == 1);
            Assert.Equal("Index", viewResult.ViewName);

        }

        [Fact]
        public void Deposit_ConcurrencyError()
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

            mockITransactionDataStore.Setup(m => m.Deposit(It.IsAny<long>(), It.IsAny<decimal>(), It.IsAny<byte[]>()))
                .Returns(() => new Response<bool>()
                {
                    ErrorMessage = "The account you are working on has been modified by another user. Changes you have made have not been committed, please resubmit.",
                    Data = false
                });

            var result = controller.Transfer(new WalletViewModel()
            {
                AccountNumber = 900000000,
                Amount = 100.00m,
                TransactionType = TransactionType.Deposit
            });

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.True(viewResult.ViewData.ModelState[""].Errors.Count == 1);
            Assert.Equal("Index", viewResult.ViewName);

        }

        #endregion

        #region Withdraw
            
        [Fact]
        public void Withdraw_Success()
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
        public void Withdraw_AmountIsLessThanZero()
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

            mockIAccountDataStore.Setup(m => m.GetAccountDetails(It.IsAny<string>()))
                .Returns(() => new Response<AccountDetails>()
                {
                    ErrorMessage = "Amount should be greater than 0.00.",
                    Data = null
                });

            var result = controller.Transfer(new WalletViewModel()
            {
                AccountNumber = 900000000,
                Amount = 0,
                TransactionType = TransactionType.Withdraw
            });

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.True(viewResult.ViewData.ModelState[""].Errors.Count == 1);
            Assert.Equal("Index", viewResult.ViewName);

        }

        [Fact]
        public void Withdraw_AccountDetailsDBError()
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

            mockIAccountDataStore.Setup(m => m.GetAccountDetails(It.IsAny<string>()))
                .Returns(() => new Response<AccountDetails>()
                {
                    ErrorMessage = "Oops. something went wrong.",
                    Data = null
                });


            var httpContext = new DefaultHttpContext();

            controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
            {
                ["BalanceRowVersion"] = new byte[8]
            };

            var result = controller.Transfer(new WalletViewModel()
            {
                AccountNumber = 900000000,
                Amount = 100.00m,
                TransactionType = TransactionType.Withdraw
            });

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.True(viewResult.ViewData.ModelState[""].Errors.Count == 1);
            Assert.Equal("Index", viewResult.ViewName);

        }

        [Fact]
        public void Withdraw_AccountBalanceDBError()
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
                ["BalanceRowVersion"] = null
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
                ErrorMessage = "Oops. something went wrong.",
                Data = null
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

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.True(viewResult.ViewData.ModelState[""].Errors.Count == 1);
            Assert.Equal("Index", viewResult.ViewName);

        }

        [Fact]
        public void Withdraw_ConcurrencyError()
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
                    ErrorMessage = "The account you are working on has been modified by another user. Changes you have made have not been committed, please resubmit.",
                    Data = false
                });

            var result = controller.Transfer(new WalletViewModel()
            {
                AccountNumber = 900000000,
                Amount = 100.00m,
                TransactionType = TransactionType.Withdraw
            });

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.True(viewResult.ViewData.ModelState[""].Errors.Count == 1);
            Assert.Equal("Index", viewResult.ViewName);

        }

        [Fact]
        public void Withdraw_InsufficientFunds()
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
                ["BalanceRowVersion"] = null
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
                    AccountNumber = 9000000000,
                    Balance = 0.00m
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

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.True(viewResult.ViewData.ModelState[""].Errors.Count == 1);
            Assert.Equal("Index", viewResult.ViewName);

        }

        #endregion

        #region FundTransfer

        [Fact]
        public void FundTransfer_Success()
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
        public void FundTransfer_AmountIsLessThanZero()
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

            mockIAccountDataStore.Setup(m => m.GetAccountDetails(It.IsAny<string>()))
                .Returns(() => new Response<AccountDetails>()
                {
                    ErrorMessage = "Amount should be greater than 0.00.",
                    Data = null
                });

            var result = controller.Transfer(new WalletViewModel()
            {
                AccountNumber = 900000000,
                Amount = 0,
                TransactionType = TransactionType.FundTransfer
            });

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.True(viewResult.ViewData.ModelState[""].Errors.Count == 1);
            Assert.Equal("Index", viewResult.ViewName);

        }

        [Fact]
        public void FundTransfer_AccountDetailsDBError()
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

            mockIAccountDataStore.Setup(m => m.GetAccountDetails(It.IsAny<string>()))
                .Returns(() => new Response<AccountDetails>()
                {
                    ErrorMessage = "Oops. something went wrong.",
                    Data = null
                });


            var httpContext = new DefaultHttpContext();

            controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
            {
                ["BalanceRowVersion"] = new byte[8]
            };

            var result = controller.Transfer(new WalletViewModel()
            {
                AccountNumber = 900000000,
                Amount = 100.00m,
                TransactionType = TransactionType.FundTransfer
            });

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.True(viewResult.ViewData.ModelState[""].Errors.Count == 1);
            Assert.Equal("Index", viewResult.ViewName);

        }

        [Fact]
        public void FundTransfer_AccountBalanceDBError()
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
                ["BalanceRowVersion"] = null
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
                ErrorMessage = "Oops. something went wrong.",
                Data = null
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

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.True(viewResult.ViewData.ModelState[""].Errors.Count == 1);
            Assert.Equal("Index", viewResult.ViewName);

        }

        [Fact]
        public void FundTransfer_ConcurrencyError()
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

            mockITransactionDataStore.Setup(m => m.FundTransfer(It.IsAny<long>(), It.IsAny<long>(),It.IsAny<decimal>(), It.IsAny<byte[]>()))
                .Returns(() => new Response<bool>()
                {
                    ErrorMessage = "The account you are working on has been modified by another user. Changes you have made have not been committed, please resubmit.",
                    Data = false
                });

            var result = controller.Transfer(new WalletViewModel()
            {
                AccountNumber = 900000000,
                Amount = 100.00m,
                TransactionType = TransactionType.FundTransfer
            });

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.True(viewResult.ViewData.ModelState[""].Errors.Count == 1);
            Assert.Equal("Index", viewResult.ViewName);

        }

        [Fact]
        public void FundTransfer_CantTransferSameAccount()
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
                ["BalanceRowVersion"] = null
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
                    AccountNumber = 9000000000,
                    Balance = 100.00m
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
                AccountNumber = 9000000000,
                Amount = 100.00m,
                TransactionType = TransactionType.FundTransfer
            });

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.True(viewResult.ViewData.ModelState[""].Errors.Count == 1);
            Assert.Equal("Index", viewResult.ViewName);

        }

        [Fact]
        public void FundTransfer_InsufficientFunds()
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
                ["BalanceRowVersion"] = null
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
                    AccountNumber = 9000000000,
                    Balance = 0.00m
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

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.True(viewResult.ViewData.ModelState[""].Errors.Count == 1);
            Assert.Equal("Index", viewResult.ViewName);

        }

        #endregion

        [Fact]
        public void Index_Success()
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
            var model = Assert.IsType<WalletViewModel>(viewResult.Model);

            Assert.Equal(2, model.Transactions.Count);
        }

        [Fact]
        public void Index_HasTransactionHistory()
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

            mockITransactionDataStore.Setup(m => m.GetAccountTransactionsHistoryList(It.IsAny<long>()))
                .Returns(() => new Response<List<WalletTransaction>>()
                {
                    ErrorMessage = string.Empty,
                    Data = new List<WalletTransaction>()
                    {
                        new WalletTransaction(),
                        new WalletTransaction(),
                        new WalletTransaction(),
                        new WalletTransaction(),
                        new WalletTransaction(),
                        new WalletTransaction(),
                        new WalletTransaction(),
                        new WalletTransaction(),
                        new WalletTransaction(),
                        new WalletTransaction(),
                    }
                });



            var result = controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WalletViewModel>(viewResult.Model);

            Assert.Equal(10, model.Transactions.Count);
        }

        [Fact]
        public void Index_AccountDetailsDBError()
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
                    ErrorMessage = "Oops. something went wrong.",
                    Data = null
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
            Assert.True(viewResult.ViewData.ModelState[""].Errors.Count == 1);
        }

        [Fact]
        public void Index_AccountBalanceDBError()
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
                ErrorMessage = "Oops. something went wrong.",
                Data = null
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
            Assert.True(viewResult.ViewData.ModelState[""].Errors.Count == 1);
        }

        [Fact]
        public void Index_TransactionHistoryDBError()
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

            mockITransactionDataStore.Setup(m => m.GetAccountTransactionsHistoryList(It.IsAny<long>()))
                .Returns(() => new Response<List<WalletTransaction>>()
                {
                    ErrorMessage = "Oops. something went wrong.",
                    Data = null
                });



            var result = controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(viewResult.ViewData.ModelState[""].Errors.Count == 1);
        }
    }


}
