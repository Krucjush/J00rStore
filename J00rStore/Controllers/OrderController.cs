using J00rStore.Data;
using J00rStore.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace J00rStore.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly AppDBContext _dbContext;

        public OrderController(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var orders = _dbContext.Orders.ToList();
            return View(orders);
        }

        public IActionResult Details(int orderId)
        {

            var order = _dbContext.Orders.Include(q => q.ShoppingCarts).ThenInclude(q => q.Product).FirstOrDefault(oh => oh.Id == orderId);
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details()
        {
            return NotFound();
        }
    }
}
