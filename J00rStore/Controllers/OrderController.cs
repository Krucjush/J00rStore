using J00rStore.Data;
using J00rStore.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace J00rStore.Controllers
{
    public class OrderController : Controller
    {
        public OrderViewModel OrderViewModel { get; set; }
        private readonly AppDBContext _dbContext;

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            if (_dbContext.Orders.FirstOrDefault(oh => oh.Id == orderId) == null)
                return NotFound();

            OrderViewModel = new OrderViewModel();
            OrderViewModel.Order = _dbContext.Orders.FirstOrDefault(oh => oh.Id == orderId);

            return View(OrderViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details()
        {
            OrderViewModel.Order = _dbContext.Orders.FirstOrDefault(oh => oh.Id == OrderViewModel.Order.Id);
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>()
                {
                    "card"
                },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };
            var details = OrderViewModel.Orders.ToList();

            foreach (var detail in OrderViewModel.Orders)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(detail.Price * 100),
                        Currency = "pln",
                    },
                    Quantity = detail.Count,
                };

                options.LineItems.Add(sessionLineItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);

            _dbContext.Orders.UpdateStripeIds(OrderViewModel.OrderHeader.Id, session.Id,
                session.PaymentIntentId);
            _unitOfWork.OrderHeaderRepository.UpdateStatus(OrderViewModel.OrderHeader.Id,
                OrderViewModel.OrderHeader.OrderStatus, StaticDetails.PAYMENT_STATUS_APPROVED);
            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
    }
}
