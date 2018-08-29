using Microsoft.AspNetCore.Mvc;
using Moq;
using Website.Controllers;
using Website.DataStores.Interface;
using Website.Models;
using Website.ViewModels;
using Xunit;

namespace XUnitTestBankingSystem
{
    public class RegistrationControllerTest
    {
        [Fact]
        public void Register_Index()
        {
            var mockIAccountDataStore = new Mock<IAccountDataStore>();
            var controller = new RegisterController(mockIAccountDataStore.Object);

            var result = controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewData["Success"]);
            Assert.Equal("Index", viewResult.ViewName);

        }

        [Fact]
        public void Register_Success()
        {
            const string loginName = "user001";
            
            var mockIAccountDataStore = new Mock<IAccountDataStore>();
            
            var controller = new RegisterController(mockIAccountDataStore.Object);

            mockIAccountDataStore.Setup(m => m.CheckLoginNameExist(loginName)).Returns(() => new Response<bool>() { Data = false, ErrorMessage = string.Empty });

            mockIAccountDataStore.Setup(m => m.Register(It.IsAny<AccountDetails>())).Returns(() => new Response<bool>() { Data = true, ErrorMessage = string.Empty });

            var result = controller.Create(new RegistrationViewModel()
            {
                LoginName = loginName,
                Address = string.Empty,
                FirstName = string.Empty,
                LastName = string.Empty,
                Password = string.Empty,
                EmailAddress = string.Empty
            });

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Successfully created new account.", viewResult.ViewData["Success"]);
            Assert.Equal("Index", viewResult.ViewName);

        }

        [Fact]
        public void Register_LoginNameAlreadyExist()
        {
            const string loginName = "user001";

            var mockIAccountDataStore = new Mock<IAccountDataStore>();

            var controller = new RegisterController(mockIAccountDataStore.Object);

            mockIAccountDataStore.Setup(m => m.CheckLoginNameExist(loginName)).Returns(() => new Response<bool>() { Data = true, ErrorMessage = string.Empty });

            var result = controller.Create(new RegistrationViewModel()
            {
                LoginName = loginName,
                Address = string.Empty,
                FirstName = string.Empty,
                LastName = string.Empty,
                Password = string.Empty,
                EmailAddress = string.Empty
            });

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(viewResult.ViewData.ModelState[""].Errors.Count == 1);
            Assert.Equal("Index", viewResult.ViewName);

        }
    }
}
