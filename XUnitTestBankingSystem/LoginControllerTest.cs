using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using Website.Controllers;
using Website.DataStores.Interface;
using Website.Models;
using Website.ViewModels;
using Xunit;

namespace XUnitTestBankingSystem
{
    public class LoginControllerTest
    {
        [Fact]
        public void Login_Success()
        {
            const string loginName = "user001";
            const string password = "Password123.";

            var mockIAccountDataStore = new Mock<IAccountDataStore>();

            var authenticationServiceMock = new Mock<IAuthenticationService>();
            authenticationServiceMock
                .Setup(a => a.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(s => s.GetService(typeof(IAuthenticationService)))
                .Returns(authenticationServiceMock.Object);

            var controller = new LoginController(mockIAccountDataStore.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext() {RequestServices = serviceProviderMock.Object}
                }
            };

            var urlHelperFactory = new Mock<IUrlHelperFactory>();
            serviceProviderMock
                .Setup(s => s.GetService(typeof(IUrlHelperFactory)))
                .Returns(urlHelperFactory.Object);

            mockIAccountDataStore.Setup(m => m.IsValidCredential(It.IsAny<string>(), It.IsAny<string>())).Returns(() => new Response<bool>() { Data = true, ErrorMessage = string.Empty });

            var result = controller.Login(new LoginViewModel()
            {
                LoginName = loginName,
                Password = password
            });

            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Home", viewResult.ControllerName);

        }

        [Fact]
        public void Login_InvalidLoginName()
        {
            const string loginName = "user001";
            const string password = "Password123.";

            var mockIAccountDataStore = new Mock<IAccountDataStore>();
            var controller = new LoginController(mockIAccountDataStore.Object);

            mockIAccountDataStore.Setup(m => m.IsValidCredential(It.IsAny<string>(), It.IsAny<string>())).Returns(() => new Response<bool>() { Data = false, ErrorMessage = string.Empty });

            var result = controller.Login(new LoginViewModel()
            {
                LoginName = loginName,
                Password = password
            });

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.True(viewResult.ViewData.ModelState[""].Errors.Count == 1);
            Assert.Equal("Index", viewResult.ViewName);

        }

        [Fact]
        public void Logout_Success()
        {

            var mockIAccountDataStore = new Mock<IAccountDataStore>();

            var authenticationServiceMock = new Mock<IAuthenticationService>();
            authenticationServiceMock
                .Setup(a => a.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(s => s.GetService(typeof(IAuthenticationService)))
                .Returns(authenticationServiceMock.Object);

            var controller = new LoginController(mockIAccountDataStore.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext() { RequestServices = serviceProviderMock.Object }
                }
            };

            var urlHelperFactory = new Mock<IUrlHelperFactory>();
            serviceProviderMock
                .Setup(s => s.GetService(typeof(IUrlHelperFactory)))
                .Returns(urlHelperFactory.Object);

            var result = controller.Logout();

            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", viewResult.ControllerName);

        }
    }
}
