using J00rStore.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using J00rStore.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace J00rStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDBContext _dbContext;

        public HomeController(ILogger<HomeController> logger, AppDBContext appDbContext)
        {
            _logger = logger;
            _dbContext = appDbContext;
        }

        public async Task<IActionResult> Index()
        {
			var products = await _dbContext.Products.ToListAsync();
			return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
	        var product = await _dbContext.Products.FirstOrDefaultAsync(q => q.Id == id);

            if (product == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var shoppingCart = new ShoppingCart() { ProductId = id, Product = product, Count = 1 };
            
	        return View(shoppingCart);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
		public IActionResult Details(ShoppingCart cart)
        {
	        var claims = User.Identity as ClaimsIdentity;
	        var idClaim = claims?.FindFirst(ClaimTypes.NameIdentifier);
	        if (idClaim == null)
	        {
		        return Unauthorized();
	        }
            cart.ApplicationUserId = idClaim.Value;

            var dbCart = _dbContext.ShoppingCarts.FirstOrDefault(s => s.ApplicationUserId == idClaim.Value && s.ProductId == cart.ProductId);

            if (dbCart == null)
            {
                _dbContext.ShoppingCarts.Add(cart);
                _dbContext.SaveChanges();
                var count = _dbContext.ShoppingCarts.Where(s => s.ApplicationUserId == idClaim.Value).ToList().Count();
            }
            else
            {
                dbCart.Count += cart.Count;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}