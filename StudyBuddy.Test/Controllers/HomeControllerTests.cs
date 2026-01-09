using Moq;
using StudyBuddy.Web.Controllers;
using StudyBuddy.Web.Models;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Reflection;

namespace StudyBuddy.Test.Controllers
{
    public class HomeControllerTests
    {
        private readonly Mock<ILogger<HomeController>> _mockLogger;
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            _mockLogger = new Mock<ILogger<HomeController>>();
            _controller = new HomeController(_mockLogger.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        #region Index Tests

        [Fact]
        public void Index_ReturnsRedirectToAction_ToStudyTasks()
        {
            var result = _controller.Index();
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("StudyTasks", redirectResult.ControllerName);
        }

        [Fact]
        public void Index_RedirectsWithCorrectRoute()
        {
            var result = _controller.Index() as RedirectToActionResult;
            Assert.NotNull(result);
            Assert.Equal("StudyTasks", result.ControllerName);
        }

        #endregion

        #region Privacy Tests

        [Fact]
        public void Privacy_ReturnsViewResult()
        {
            var result = _controller.Privacy();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Privacy_ReturnsViewWithNoModel()
        {
            var result = _controller.Privacy() as ViewResult;
            Assert.NotNull(result);
            Assert.Null(result.Model);
        }

        [Fact]
        public void Privacy_ReturnsViewWithCorrectViewName()
        {
            var result = _controller.Privacy() as ViewResult;
            Assert.NotNull(result);
            Assert.Equal("Privacy", result.ViewName ?? "Privacy");
        }

        #endregion

        #region Error Tests

        [Fact]
        public void Error_ReturnsViewResult()
        {
            var result = _controller.Error();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Error_ReturnsViewWithErrorViewModel()
        {
            var result = _controller.Error() as ViewResult;
            Assert.NotNull(result);
            Assert.IsType<ErrorViewModel>(result.Model);
        }

        [Fact]
        public void Error_ErrorViewModelHasRequestId()
        {
            var result = _controller.Error() as ViewResult;
            var model = result.Model as ErrorViewModel;
            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.NotNull(model.RequestId);
            Assert.NotEmpty(model.RequestId);
        }

        [Fact]
        public void Error_SetsCacheHeadersToNoStore()
        {
            _controller.Error();
            var cacheAttribute = typeof(HomeController)
                .GetMethod("Error")
                ?.GetCustomAttributes(typeof(ResponseCacheAttribute), false)
                .FirstOrDefault() as ResponseCacheAttribute;

            Assert.NotNull(cacheAttribute);
            Assert.Equal(0, cacheAttribute.Duration);
            Assert.True(cacheAttribute.NoStore);
        }

        #endregion

        #region Authorization Tests

        [Fact]
        public void Index_HasAuthorizeAttribute()
        {
            var method = typeof(HomeController).GetMethod("Index");
            var authorizeAttribute = method?.GetCustomAttributes()
                .FirstOrDefault(a => a.GetType().Name == "AuthorizeAttribute");
            Assert.NotNull(authorizeAttribute);
        }

        [Fact]
        public void Privacy_DoesNotHaveAuthorizeAttribute()
        {
            var method = typeof(HomeController).GetMethod("Privacy");
            var authorizeAttribute = method?.GetCustomAttributes()
                .FirstOrDefault(a => a.GetType().Name == "AuthorizeAttribute");
            Assert.Null(authorizeAttribute);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void HomeController_CanHandleMultipleRequests()
        {
            var privacyResult = _controller.Privacy();
            var errorResult = _controller.Error();
            var indexResult = _controller.Index();

            Assert.IsType<ViewResult>(privacyResult);
            Assert.IsType<ViewResult>(errorResult);
            Assert.IsType<RedirectToActionResult>(indexResult);
        }

        #endregion
    }
}
