using J00rStore.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using J00rStore.Models;
using J00rStore.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;

namespace J00rStore.Controllers
{
	public class CartController : Controller
	{
		private readonly AppDBContext _dbContext;
		[BindProperty] public ShoppingCartViewModel ShoppingCartViewModel { get; set; }
		public int TotalPrice { get; set; }

		public CartController(AppDBContext dbContext)
		{
			_dbContext = dbContext;
		}
		[Authorize]
        public IActionResult Index()
		{
			var claims = User.Identity as ClaimsIdentity;
			var idClaim = claims?.FindFirst(ClaimTypes.NameIdentifier);
			if (idClaim == null)
            {
				return Unauthorized();
            }

			ShoppingCartViewModel = new ShoppingCartViewModel()
			{
				ListCart = _dbContext.ShoppingCarts.Include(x => x.Product).Where(sc => sc.UserId == idClaim.Value),
				Order = new Order()
			};

			foreach (var cart in ShoppingCartViewModel.ListCart)
			{

                ShoppingCartViewModel.Order.TotalPrice += (double)cart.Product.Price * cart.Count;
			}

			return View(ShoppingCartViewModel);
		}
		[HttpGet]
		[ActionName("Summary")]
		public IActionResult ShowSummary()
		{
			var claims = User.Identity as ClaimsIdentity;
			var idClaim = claims?.FindFirst(ClaimTypes.NameIdentifier);
			if (idClaim == null) { return Unauthorized(); }

			ShoppingCartViewModel = new ShoppingCartViewModel()
			{
				ListCart = _dbContext.ShoppingCarts.Include(x => x.Product).Where(sc => sc.UserId == idClaim.Value),
				Order = new Order()
			};

			foreach (var cart in ShoppingCartViewModel.ListCart)
			{
				ShoppingCartViewModel.Order.TotalPrice += (double)cart.Product.Price * cart.Count;
			}

			return View(ShoppingCartViewModel);
		}
		[HttpPost]
		[ActionName("Summary")]
		[ValidateAntiForgeryToken]
		public IActionResult PostSummary(ShoppingCartViewModel shoppingCartViewModel)
		{
			var claims = User.Identity as ClaimsIdentity;
			var idClaim = claims?.FindFirst(ClaimTypes.NameIdentifier);
			if (idClaim == null) { return Unauthorized(); }

			ShoppingCartViewModel.ListCart = _dbContext.ShoppingCarts.Include(x => x.Product).Where(sc => sc.UserId == idClaim.Value && sc.OrderId == null).ToList();

			ShoppingCartViewModel.Order.UserId = idClaim.Value;

            foreach (var cart in ShoppingCartViewModel.ListCart)
            {
                ShoppingCartViewModel.Order.TotalPrice += (double)cart.Product.Price * cart.Count;
            }

            List<Product> products = new List<Product>();
			int count = 0;
			double price = 0;
			foreach (var cart in ShoppingCartViewModel.ListCart)
			{
				products.Add(cart.Product);
				price += cart.Price;
				count += cart.Count;
			}
			var orderDetails = new Order
			{
				Count = count,
				City = ShoppingCartViewModel.Order.City,
				ShoppingCarts = ShoppingCartViewModel.ListCart.ToList(),
				Name = ShoppingCartViewModel.Order.Name,
				Id = ShoppingCartViewModel.Order.Id,
				ZipCode = ShoppingCartViewModel.Order.ZipCode,
				PhoneNumber = ShoppingCartViewModel.Order.PhoneNumber,
				Street = ShoppingCartViewModel.Order.Street,
				State = ShoppingCartViewModel.Order.State,
				TotalPrice = ShoppingCartViewModel.Order.TotalPrice,
				UserId = ShoppingCartViewModel.Order.UserId 
            };
			_dbContext.Orders.Add(orderDetails);
			_dbContext.SaveChanges();

			return RedirectToAction(nameof(OrderConfirmation), new { id = orderDetails.Id });
		}
		public IActionResult OrderConfirmation(int id)
		{
			var orderHeader = _dbContext.Orders.FirstOrDefault(oh => oh.Id == id);

			if (orderHeader == null) throw new ArgumentNullException(nameof(orderHeader));

			var shoppingCarts = _dbContext.ShoppingCarts.Where(sc => sc.UserId == orderHeader.UserId).ToList();

			_dbContext.SaveChanges();

			return View(id);
		}
        public IActionResult Plus(int cartId)
        {
            var cart = _dbContext.ShoppingCarts.FirstOrDefault(q => q.Id == cartId);
            cart.Count++;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Minus(int cartId)
        {
            var cart = _dbContext.ShoppingCarts.FirstOrDefault(sc => sc.Id == cartId);

            if (cart.Count <= 1)
            {
                _dbContext.ShoppingCarts.Remove(cart);
            }
            else
            {
                cart.Count--;
            }

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
		public IActionResult Remove(int cartId)
		{
			var cart = _dbContext.ShoppingCarts.FirstOrDefault(q => q.Id == cartId);
			_dbContext.ShoppingCarts.Remove(cart);
			_dbContext.SaveChanges();

			return RedirectToAction(nameof(Index));
		}
	}
}
