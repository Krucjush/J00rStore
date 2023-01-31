using Microsoft.AspNetCore.Mvc;

namespace J00rStore.Models.ViewModels
{
    public class OrderViewModel
    {
        public Order Order { get; set; }
        public IEnumerable<Order> Orders { get; set; }
    }
}
