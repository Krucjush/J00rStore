using J00rStore.Controllers;
using J00rStore.Data;
using J00rStore.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace J00rStore.Tests
{
    public class CartControllerTests
    {
        private readonly AppDBContext _dbContext;
        [Fact]
        public void Index_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var controller = new CartController(_dbContext);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }
    }
}