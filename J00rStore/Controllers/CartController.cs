using J00rStore.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using J00rStore.Models;
using J00rStore.Data;
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
				ListCart = _dbContext.ShoppingCarts.Where(sc => sc.ApplicationUserId == idClaim.Value),
				OrderHeader = new OrderHeader()
			};

			foreach (var cart in ShoppingCartViewModel.ListCart)
			{
				cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price);

				ShoppingCartViewModel.OrderHeader.TotalPrice += cart.Price * cart.Count;
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
				ListCart = _dbContext.ShoppingCarts.Where(sc => sc.ApplicationUserId == idClaim.Value),
				OrderHeader = new OrderHeader()
			};

			ShoppingCartViewModel.OrderHeader.User =
				_dbContext.Users.FirstOrDefault(u => u.Id == idClaim.Value);

			ShoppingCartViewModel.OrderHeader.Name = ShoppingCartViewModel.OrderHeader.User.FirstName;
			ShoppingCartViewModel.OrderHeader.PhoneNumber = ShoppingCartViewModel.OrderHeader.User.PhoneNumber;
			ShoppingCartViewModel.OrderHeader.Street = ShoppingCartViewModel.OrderHeader.User.Street;
			ShoppingCartViewModel.OrderHeader.City = ShoppingCartViewModel.OrderHeader.User.City;
			ShoppingCartViewModel.OrderHeader.State = ShoppingCartViewModel.OrderHeader.User.State;
			ShoppingCartViewModel.OrderHeader.PostalCode = ShoppingCartViewModel.OrderHeader.User.ZipCode;

			foreach (var cart in ShoppingCartViewModel.ListCart)
			{
				cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price);

				ShoppingCartViewModel.OrderHeader.TotalPrice += cart.Price * cart.Count;
			}

			return View(ShoppingCartViewModel);
		}
		[HttpPost]
		[ActionName("Summary")]
		[ValidateAntiForgeryToken]
		public IActionResult PostSummary()
		{
			var claims = User.Identity as ClaimsIdentity;
			var idClaim = claims?.FindFirst(ClaimTypes.NameIdentifier);
			if (idClaim == null) { return Unauthorized(); }

			ShoppingCartViewModel.ListCart = _dbContext.ShoppingCarts.Where(sc => sc.ApplicationUserId == idClaim.Value);

			ShoppingCartViewModel.OrderHeader.OrderDate = DateTime.Now;
			ShoppingCartViewModel.OrderHeader.ApplicationUserId = idClaim.Value;

			foreach (var cart in ShoppingCartViewModel.ListCart)
			{
				cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price);

				ShoppingCartViewModel.OrderHeader.TotalPrice += cart.Price * cart.Count;
			}

			var user = _dbContext.Users.FirstOrDefault(u => u.Id == idClaim.Value);

			if (user == null) { return Unauthorized(); }

			
			ShoppingCartViewModel.OrderHeader.PaymentStatus = StaticDetails.PAYMENT_STATUS_DELAYED_PAYMENT;
			ShoppingCartViewModel.OrderHeader.OrderStatus = StaticDetails.STATUS_APPROVED;

		 	_dbContext.OrderHeaders.Add(ShoppingCartViewModel.OrderHeader);
			_dbContext.SaveChanges();

			foreach (var cart in ShoppingCartViewModel.ListCart)
			{
				var orderDetails = new OrderDetails
				{
					ProductId = cart.ProductId,
					OrderId = ShoppingCartViewModel.OrderHeader.Id,
					Price = cart.Price,
					Count = cart.Count
				};

				_dbContext.OrderDetails.Add(orderDetails);
				_dbContext.SaveChanges();
			}

			var domain = "https://localhost:7009/";
			var options = new SessionCreateOptions
			{
				PaymentMethodTypes = new List<string>()
				{
					"card"
				},
				LineItems = new List<SessionLineItemOptions>(),
				Mode = "payment",
				SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartViewModel.OrderHeader.Id}",
				CancelUrl = domain + "customer/cart/index",
			};

			foreach (var cart in ShoppingCartViewModel.ListCart)
			{
				var sessionLineItem = new SessionLineItemOptions
				{
					PriceData = new SessionLineItemPriceDataOptions
					{
						UnitAmount = (long)(cart.Price * 100),
						Currency = "usd",
						ProductData = new SessionLineItemPriceDataProductDataOptions
						{
							Name = cart.Product.Name
						},
					},
					Quantity = cart.Count,
				};

				options.LineItems.Add(sessionLineItem);
			}

			var service = new SessionService();
			var session = service.Create(options);

			var dbOrder = _dbContext.OrderHeaders.FirstOrDefault(q => q.Id == ShoppingCartViewModel.OrderHeader.Id);
			if (dbOrder != null)
			{
				dbOrder.PaymentDate = DateTime.Now;
				dbOrder.SessionId = session.Id;
				dbOrder.PaymentIntentId = session.PaymentIntentId;
				_dbContext.SaveChanges();
			}

			Response.Headers.Add("Location", session.Url);
			return new StatusCodeResult(303);
		}
		public IActionResult OrderConfirmation(int id)
		{
			var orderHeader = _dbContext.OrderHeaders.FirstOrDefault(oh => oh.Id == id);

			if (orderHeader == null) throw new ArgumentNullException(nameof(orderHeader));

			if (orderHeader.PaymentStatus != StaticDetails.PAYMENT_STATUS_DELAYED_PAYMENT)
			{
				var service = new SessionService();
				Session session = service.Get(orderHeader.SessionId);

				if (session.PaymentStatus.ToLower() == "paid")
				{
					var dbOrder = _dbContext.OrderHeaders.FirstOrDefault(q => q.Id == id);
					dbOrder.PaymentDate = DateTime.Now;
					dbOrder.SessionId = orderHeader.SessionId;
					dbOrder.PaymentIntentId = session.PaymentIntentId;
					dbOrder.OrderStatus = StaticDetails.STATUS_APPROVED;
					dbOrder.PaymentStatus = StaticDetails.PAYMENT_STATUS_APPROVED;
					_dbContext.SaveChanges();
				}
			}

			var shoppingCarts = _dbContext.ShoppingCarts.Where(sc => sc.ApplicationUserId == orderHeader.ApplicationUserId).ToList();

			HttpContext.Session.Clear();

			_dbContext.ShoppingCarts.RemoveRange(shoppingCarts);
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

            var count = _dbContext.ShoppingCarts.Where(sc => sc.ApplicationUserId == cart.ApplicationUserId).ToList().Count();
            HttpContext.Session.SetInt32(StaticDetails.SESSION_CART, count);

            return RedirectToAction(nameof(Index));
        }

        public static double GetPriceBasedOnQuantity(double quantity, double price)
		{
			if (quantity <= 0 || price <= 0)
				throw new ArgumentException();

			return quantity switch
			{
				<= 50 => price,
				<= 100 => price * 0.85,
				_ => price * 0.75
			};
		}
	}
}
